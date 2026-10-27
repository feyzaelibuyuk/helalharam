"""Black-box HTTP checks against an already-running local development server.
Does not load or run the Windows-blocked PreferenceChecks assembly.
Uses isolated in-memory cookie jars and synthetic inputs only.
"""
import html
from html.parser import HTMLParser
import http.cookiejar
import json
import sys
import urllib.request
import urllib.parse
import urllib.error
base=sys.argv[1] if len(sys.argv)>1 else "http://127.0.0.1:5095"
if base not in ("http://127.0.0.1:5095","http://localhost:5095"):
    raise SystemExit("Use the isolated local verification server on port 5095.")
class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self,*args,**kwargs): return None
class TokenParser(HTMLParser):
    token=None
    def handle_starttag(self,tag,attrs):
        values=dict(attrs)
        if tag=="input" and values.get("name")=="__RequestVerificationToken": self.token=values["value"]
def client(): return urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()),NoRedirect())
def call(c,path,data=None,headers=None):
    req=urllib.request.Request(base+path,data=None if data is None else urllib.parse.urlencode(data).encode(),headers=headers or {})
    try: response=c.open(req,timeout=30)
    except urllib.error.HTTPError as error: response=error
    with response: return response.code,html.unescape(response.read().decode()),response.headers
def check(ok,label):
    if not ok: raise AssertionError(label)
def token(c):
    code,body,_=call(c,"/food-preferences")
    check(code==200,"Preferences GET")
    p=TokenParser();p.feed(body);check(bool(p.token),"Antiforgery token")
    return p.token
c=client();other=client();csrf=token(c)
choices=[("allergens","milk"),("allergens","peanuts"),("diet","vegan"),("lactose","true"),("gluten","true"),("custom","киви")]
code,_,_=call(c,"/food-preferences/save",choices);check(code==400,"Save requires CSRF")
code,_,headers=call(c,"/food-preferences/save",choices+[("__RequestVerificationToken",csrf)])
check(code==302,"Save redirect")
cookie=headers.get("Set-Cookie","")
check("helal-food-preferences-v1=" in cookie and "httponly" in cookie.lower() and "samesite=strict" in cookie.lower(),"Protected cookie flags")
check("киви" not in cookie and '"allergens"' not in cookie,"No plaintext preference payload")
def evaluate(text,language="en",who=c):
    code,body,headers=call(who,"/ingredients?"+urllib.parse.urlencode({"text":text,"language":language}))
    check(code==200,"Ingredients GET")
    check("no-store" in headers.get("Cache-Control","").lower(),"Personal result is noncacheable")
    return body
body=evaluate("milk-free; whey powder; may contain peanuts; wheat flour; honey")
for expected in ["İçermez beyanında eşleşme", "whey powder", "Olası içerik / eser miktar", "Yer fıstığı", "Gluten içeren tahıllar", "Vegan tercihi"]:
    check(expected in body,"Missing compound warning: "+expected)
check("Laktozdan kaçınma</h3>" in evaluate("lactose"),"Explicit lactose ingredient")
check("Kişisel filtre seçilmedi" in evaluate("milk",who=other),"Preferences leaked across cookie jars")
check("güvenli/alerjensiz/uygun sonucu verilmedi" in evaluate("rice, water"),"No-match safety")
check("İçindekiler metni eksik" in evaluate(""),"Missing text")
check("İçermez beyanında eşleşme" in evaluate("SÜT içermez; peyniraltı suyu; eser miktarda yer fıstığı içerebilir","tr"),"TR declaration")
check("Süt</h3>" in evaluate("İNEK SÜTÜ","tr"),"Common Turkish milk inflection")
check("Vegan tercihi · Yumurta" in evaluate("œuf et farine","fr"),"French ligature")
check("Vegan tercihi</h3>" not in evaluate("sucre et farine","fr"),"French conjunction false meat")
body=evaluate("КИВИ, вода","other");check("Ek kaçınılan içerik: киви" in body and "metin kontrolü kısmi" in body,"Unsupported language/custom Unicode")
check("food-finding" not in evaluate("milkshake, peanutsomething"),"Word boundary")
code,_,_=call(c,"/food-preferences/save",[("diet","invalid"),("__RequestVerificationToken",csrf)]);check(code==400,"Unknown diet rejected")
code,_,_=call(c,"/food-preferences/save",[("diet","none"),("custom",','.join(['test']*9)),("__RequestVerificationToken",csrf)]);check(code==400,"Custom limit")
ids=["milk","eggs","gluten","peanuts","nuts","soybeans","sesame-seeds","fish","crustaceans","molluscs","celery","mustard","lupin","sulphites"]
code,_,_=call(c,"/food-preferences/save",[("allergens",x) for x in ids]+[("diet","none"),("__RequestVerificationToken",csrf)]);check(code==302,"Save all groups")
body=evaluate("milk, egg, wheat, peanut, almond, soy, sesame, fish, shrimp, mussel, celery, mustard, lupin, E220")
check(body.count('class="food-finding"')==14,"All fourteen representative groups")
body=evaluate(';'.join('milk '+str(i) for i in range(130))+"; peanuts");check("Yer fıstığı</h3>" in body,"Repeated group suppressed another allergen")
code,_,_=call(c,"/food-preferences/clear",[]);check(code==400,"Clear CSRF")
code,_,_=call(c,"/food-preferences/clear",[("__RequestVerificationToken",csrf)]);check(code==302,"Clear saved preferences")
check("Kişisel filtre seçilmedi" in evaluate("milk"),"Clear did not remove selections")
print("PASS HTTP: save/clear CSRF, cookie flags, isolated clients, private cache headers, declarations/traces/negation, TR/FR, unsupported/custom Unicode, no-match/missing text, boundaries, 14 examples, group caps and validation.")
