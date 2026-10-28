"""Real loopback gateway checks; no C# assembly is loaded or executed here.
Requires the installed OPUS gateway on 127.0.0.1:5002. Only synthetic text is used.
"""
import json, urllib.request, urllib.error
from pathlib import Path
BASE="http://127.0.0.1:5002"
def call(path,data=None):
    req=urllib.request.Request(BASE+path,data=None if data is None else json.dumps(data).encode(),headers={"Content-Type":"application/json"})
    try:r=urllib.request.urlopen(req,timeout=120)
    except urllib.error.HTTPError as error:r=error
    with r:return r.code,json.load(r),r.headers
code,languages,_=call("/languages")
assert code==200 and any(x["code"]=="en" and "tr" in x["targets"] for x in languages)
for case in json.loads(Path(__file__).with_name("translation-quality-cases.json").read_text(encoding="utf-8")):
    code,result,headers=call("/translate",dict(q=case["text"],source="en",target="tr",format="text"))
    assert code==200,result
    text=result["translatedText"].lower()
    assert all(any(term in text for term in group) for group in case["concepts"]),result
    assert not any(term in text for term in case.get("forbidden",[])),result
    assert "OPUS" in result["provider"] and "no-store" in headers["Cache-Control"]
    print("PASS:",case["id"])
source=("Ingredients: milk, wheat flour, soy. "*30)+"\r\n\r\nNutrition: fat 12 g; salt 0.5 g.\nFinal code E471 987654."
code,result,_=call("/translate",dict(q=source,source="en",target="tr"))
assert code==200,result
text=result["translatedText"]
assert text.count("\r\n")==2 and text.count("\n")==3,text
assert "E471" in text and "987654" in text and "12" in text,text
assert "süt" in text and "yağ" in text,text
print("PASS: multi-segment text, blank lines, CRLF/LF, final marker, E code and numbers")
code,_,_=call("/translate",dict(q="x"*12001,source="en",target="tr"));assert code==400
code,_,_=call("/translate",dict(q=123,source="en",target="tr"));assert code==400
code,_,_=call("/unknown");assert code==404
print("PASS: length/type validation and unknown endpoint")
