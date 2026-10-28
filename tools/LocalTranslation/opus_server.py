"""Loopback-only OPUS EN/TR gateway; optional other routes via local LibreTranslate.
No model download at startup, no user text logging, no remote translation requests.
"""
import ctypes, json, os, re, threading, urllib.request, urllib.error
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
MODEL=ROOT/".verification/opus-evaluation/model"
if not (MODEL/"pytorch_model.bin").is_file(): raise SystemExit("OPUS model missing. See docs/OPUS_ENTEGRASYON.md")
model_path=str(MODEL)
if os.name=="nt":
    buffer=ctypes.create_unicode_buffer(32768)
    if ctypes.windll.kernel32.GetShortPathNameW(model_path,buffer,len(buffer)): model_path=buffer.value
from transformers import MarianTokenizer, MarianMTModel
import torch
tokenizer=MarianTokenizer.from_pretrained(model_path,local_files_only=True)
model=MarianMTModel.from_pretrained(model_path,local_files_only=True)
model.eval();torch.set_num_threads(min(4,os.cpu_count() or 1))
gate=threading.BoundedSemaphore(1)
class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self,*args,**kwargs): return None
opener=urllib.request.build_opener(urllib.request.ProxyHandler({}),NoRedirect())
def legacy(path,data=None):
    req=urllib.request.Request("http://127.0.0.1:5000/"+path,data=None if data is None else json.dumps(data).encode(),headers={"Content-Type":"application/json"})
    with opener.open(req,timeout=10 if path in ("languages","detect") else 110) as response:
        raw=response.read(512001)
        if len(raw)>512000: raise ValueError("Oversized local response")
        return json.loads(raw)
def translate(text):
    parts=re.split(r"(\r\n|\n|\r)",text);result=[];chunks=0
    for part in parts:
        if not part.strip(): result.append(part);continue
        ids=tokenizer(part,add_special_tokens=False,truncation=False)["input_ids"]
        translated=[]
        for start in range(0,len(ids),192):
            chunks+=1
            if chunks>128: raise ValueError("Too many segments; split text manually")
            batch=tokenizer.prepare_for_model(ids[start:start+192],return_attention_mask=True)
            batch={key:torch.tensor([value]) for key,value in batch.items()}
            with torch.inference_mode(): output=model.generate(**batch,max_new_tokens=512,num_beams=4)
            generated=output[0].tolist()
            if not generated or generated[-1]!=tokenizer.eos_token_id: raise ValueError("Incomplete translation; split text manually")
            value=tokenizer.decode(generated,skip_special_tokens=True)
            if not value.strip(): raise ValueError("Empty translated segment")
            translated.append(value)
        result.append(" ".join(translated))
    return "".join(result)
class Handler(BaseHTTPRequestHandler):
    def log_message(self,*args): pass
    def reply(self,code,data):
        payload=json.dumps(data,ensure_ascii=False).encode("utf-8")
        self.send_response(code);self.send_header("Content-Type","application/json; charset=utf-8");self.send_header("Content-Length",str(len(payload)));self.send_header("Cache-Control","no-store");self.end_headers()
        try:self.wfile.write(payload)
        except (BrokenPipeError,ConnectionResetError):pass
    def do_GET(self):
        if self.path!="/languages": self.reply(404,{"error":"Unknown endpoint"});return
        languages={"en":{"code":"en","name":"English","targets":["en","tr"]},"tr":{"code":"tr","name":"Turkish","targets":["tr"]}}
        try:
            for item in legacy("languages"):
                code=item["code"]
                if code not in languages: languages[code]=item
                else: languages[code]["targets"]=sorted(set(languages[code]["targets"]+item["targets"]))
        except (OSError,ValueError,KeyError):pass
        self.reply(200,list(languages.values()))
    def do_POST(self):
        if self.path not in ("/translate","/detect"):self.reply(404,{"error":"Unknown endpoint"});return
        try:
            self.connection.settimeout(15)
            size=int(self.headers.get("Content-Length","0"))
            if not 0<size<=192000:self.reply(413,{"error":"Request too large or missing length"});return
            data=json.loads(self.rfile.read(size));text=data.get("q")
            if not isinstance(text,str) or not 0<len(text)<=12000:self.reply(400,{"error":"Text must be 1-12000 characters"});return
            if self.path=="/detect":
                try:self.reply(200,legacy("detect",{"q":text}))
                except (OSError,ValueError):self.reply(503,{"error":"Select source language manually; local detection unavailable"})
                return
            if data.get("source")=="en" and data.get("target")=="tr":
                if not gate.acquire(blocking=False):self.reply(429,{"error":"Local model busy"});return
                try:
                    value=translate(text)
                    self.reply(200,{"translatedText":value,"provider":"OPUS-MT EN→TR (yerel)"})
                finally:gate.release()
            else:
                value=legacy("translate",data);value["provider"]="LibreTranslate (yerel)";self.reply(200,value)
        except (OSError,ValueError,TypeError,KeyError):self.reply(502,{"error":"Local translation failed; no partial result returned"})
        except Exception:self.reply(502,{"error":"Local model failed; no partial result returned"})
print("OPUS ready: http://127.0.0.1:5002 (EN -> TR). No automatic model download.",flush=True)
ThreadingHTTPServer(("127.0.0.1",5002),Handler).serve_forever()
