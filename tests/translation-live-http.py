"""Checks installed local translation via C# API; never downloads models or starts services.
Run with the isolated verification server on 127.0.0.1:5095 and local motor on port 5000.
This verifies transport/preservation, not linguistic accuracy.
"""
import json
import urllib.request
import urllib.error
BASE = "http://127.0.0.1:5095"
def call(path, data=None):
    req = urllib.request.Request(BASE+path, data=None if data is None else json.dumps(data).encode(), headers={"Content-Type":"application/json"})
    try: response = urllib.request.urlopen(req, timeout=150)
    except urllib.error.HTTPError as error: response = error
    with response: return response.code, json.load(response), response.headers
code, options, _ = call("/api/translation/options")
assert code == 200 and options["configured"], "C# translation configuration unavailable"
assert {"en","tr"}.issubset({x["code"] for x in options["languages"]}), "Start the installed local EN/TR motor first; no success claimed"
source = "Ingredients: water, sugar, MILK, WHEAT, SOY, E471.\nNutrition per 100 g: fat 12 g; salt 0.5 g."
request = dict(text=source,sourceLanguage="en",targetLanguage="tr",mode="label",reviewedSource=True,cloudConsent=True)
code, result, headers = call("/api/translation", request)
assert code == 200, result
assert "no-store" in headers.get("Cache-Control", ""), "Translation must not be browser-cached"
doc = result["document"]
assert doc["originalText"] == source, "Original source changed"
assert doc["translatedText"].strip(), "Empty translation"
assert doc["sourceLanguage"] == "en" and doc["targetLanguage"] == "tr"
assert "E471" in doc["translatedText"], "E code was lost"
print("PASS: real C# -> local motor -> result; original preserved; E471 retained; no-store")
unchanged = [word for word in ("milk", "wheat", "soy") if word in doc["translatedText"].lower()]
if unchanged:
    assert any("Eksik çeviri olasılığı" in warning and all(word in warning for word in unchanged) for warning in doc["warnings"]), "Retained food terms must be flagged"
print("PASS: retained English food terms carry a specific warning when present")
print("Synthetic translation for HUMAN quality review:")
print(doc["translatedText"])
print("Quality is not certified by these checks.")
code, _, _ = call("/api/translation", dict(request,cloudConsent=False))
assert code == 400, "Consent required"
code, _, _ = call("/api/translation", dict(request,targetLanguage="ja"))
assert code == 400, "Uninstalled language must fail explicitly"
print("PASS: consent and unsupported language rejection")
