"""Isolated OPUS-MT EN/TR comparison using synthetic fixtures only.
Explicit --download fetches official model files; otherwise loading is local-only.
Does not modify the LibreTranslate installation or app configuration.
"""
import argparse, json, os, time, unicodedata
from pathlib import Path
parser=argparse.ArgumentParser()
parser.add_argument("--download",action="store_true")
args=parser.parse_args()
root=Path(__file__).resolve().parents[2]
work=root/".verification"/"opus-evaluation"
work.mkdir(parents=True,exist_ok=True)
os.environ["HF_HOME"]=str(work/"hf-cache")
from huggingface_hub import HfApi, snapshot_download
from transformers import MarianTokenizer, MarianMTModel
import torch
model_id="Helsinki-NLP/opus-mt-tc-big-en-tr"
revision_file=work/"revision.txt"
if args.download:
    revision=revision_file.read_text(encoding="utf-8").strip() if revision_file.exists() else HfApi().model_info(model_id).sha
    snapshot=snapshot_download(model_id,revision=revision,local_dir=str(work/"model"),allow_patterns=["config.json","generation_config.json","pytorch_model.bin","source.spm","target.spm","vocab.json","tokenizer_config.json","special_tokens_map.json","README.md"])
    revision_file.write_text(revision,encoding="utf-8")
else:
    revision=revision_file.read_text(encoding="utf-8").strip()
    snapshot=str(work/"model")
    if not Path(snapshot,"pytorch_model.bin").is_file(): raise SystemExit("Model missing; run explicitly with --download first.")
# SentencePiece requires a Windows short path for user names with non-ASCII characters.
if os.name=="nt":
    import ctypes
    buffer=ctypes.create_unicode_buffer(32768)
    if ctypes.windll.kernel32.GetShortPathNameW(snapshot,buffer,len(buffer)): snapshot=buffer.value
print("Loading official model revision",revision,flush=True)
tokenizer=MarianTokenizer.from_pretrained(snapshot,local_files_only=True)
model=MarianMTModel.from_pretrained(snapshot,local_files_only=True)
model.eval(); torch.set_num_threads(min(4,os.cpu_count() or 1))
cases=json.loads((root/"tests"/"translation-quality-cases.json").read_text(encoding="utf-8"))
results=[]
def norm(text): return unicodedata.normalize("NFC",text.lower().replace("i\u0307","i"))
for case in cases:
    start=time.perf_counter()
    inputs=tokenizer(case["text"],return_tensors="pt",truncation=False)
    with torch.inference_mode(): output=model.generate(**inputs,max_new_tokens=256,num_beams=4)
    text=tokenizer.decode(output[0],skip_special_tokens=True)
    missing=[group for group in case["concepts"] if not any(norm(term) in norm(text) for term in group)]
    forbidden=[term for term in case.get("forbidden",[]) if norm(term) in norm(text)]
    result=dict(id=case["id"],source=case["text"],translation=text,missing=missing,wrong_context=forbidden,seconds=round(time.perf_counter()-start,2),passed=not(missing or forbidden))
    results.append(result);print(json.dumps(result,ensure_ascii=False),flush=True)
report=dict(model=model_id,revision=revision,results=results,limitations="Only three synthetic lexical regression examples. Human review and end-to-end integration still required. Not a general accuracy or allergy/halal certification.")
(work/"results.json").write_text(json.dumps(report,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("Report:",work/"results.json",flush=True)
raise SystemExit(0 if all(x["passed"] for x in results) else 1)
