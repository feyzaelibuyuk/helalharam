"""Small EN/TR food-domain regression sample, not a translation accuracy certification.
Uses only synthetic fixture text and the isolated C# API. Requires services already running.
Separate server start from other translation checks to avoid the 5/minute rate limit.
"""
import json, unicodedata, urllib.request, urllib.error
from pathlib import Path

def normalize(text):
    return unicodedata.normalize("NFC",text.lower().replace("İ","i").replace("i\u0307","i"))
failures=0
for case in json.loads(Path(__file__).with_name("translation-quality-cases.json").read_text(encoding="utf-8")):
    payload=dict(text=case["text"],sourceLanguage="en",targetLanguage="tr",mode="label",reviewedSource=True,cloudConsent=True)
    request=urllib.request.Request("http://127.0.0.1:5095/api/translation",data=json.dumps(payload).encode(),headers={"Content-Type":"application/json"})
    try:
        with urllib.request.urlopen(request,timeout=150) as response: result=json.load(response)
    except (urllib.error.URLError,TimeoutError) as error:
        print("UNVERIFIED:",case["id"],type(error).__name__)
        raise SystemExit(2)
    doc=result.get("document")
    if not doc or doc.get("originalText")!=case["text"]:
        print("UNVERIFIED: missing result or modified original",case["id"])
        raise SystemExit(2)
    text=doc["translatedText"]; normalized=normalize(text)
    missing=[group for group in case["concepts"] if not any(normalize(term) in normalized for term in group)]
    forbidden=[term for term in case.get("forbidden",[]) if normalize(term) in normalized]
    bad=bool(missing or forbidden); failures+=bad
    print(("FAIL" if bad else "PASS"),case["id"],json.dumps(dict(translation=text,missing=missing,wrong_context=forbidden),ensure_ascii=False))
print("Only three lexical/context regression samples; passing does not establish full translation, allergen or halal accuracy.")
raise SystemExit(1 if failures else 0)
