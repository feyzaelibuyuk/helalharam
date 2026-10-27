"""Read/edit integration checks against the isolated local development server."""
import html,json,sys,urllib.parse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
# Reuse standard-library HTTP primitives without importing a test module with side effects.
import http.cookiejar,urllib.request,urllib.error
from html.parser import HTMLParser
base=sys.argv[1] if len(sys.argv)>1 else 'http://127.0.0.1:5095'
if base not in ('http://127.0.0.1:5095','http://localhost:5095'):raise SystemExit('Use the isolated local verification server on 5095.')
class NoRedirect(urllib.request.HTTPRedirectHandler):
 def redirect_request(self,*args,**kwargs):return None
client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()),NoRedirect())
def call(path,data=None):
 req=urllib.request.Request(base+path,data=None if data is None else urllib.parse.urlencode(data).encode())
 try:r=client.open(req,timeout=150)
 except urllib.error.HTTPError as e:r=e
 with r:return r.code,r.read().decode()
def check(value,why):
 if not value:raise AssertionError(why)
class Form(HTMLParser):
 def __init__(self):super().__init__();self.token=None;self.text='';self.in_text=False
 def handle_starttag(self,tag,attrs):
  a=dict(attrs)
  if tag=='input' and a.get('name')=='__RequestVerificationToken':self.token=a.get('value')
  if tag=='textarea' and a.get('id')=='translation-text':self.in_text=True
 def handle_endtag(self,tag):
  if tag=='textarea':self.in_text=False
 def handle_data(self,data):
  if self.in_text:self.text+=data
code,raw=call('/api/products/1');check(code==200,'Demo fixture product');p=json.loads(raw)['product']
original=p.get('originalIngredients') or p['ingredients']
code,page=call('/products/1');check(code==200 and '/translate/catalog/1' in page,'Local product translation entry')
check('Otomatik çeviri henüz bağlı değil' not in html.unescape(page),'Stale translation message')
code,page=call('/translate/catalog/1');f=Form();f.feed(page);check(code==200 and f.text==original,'Complete original text transfer')
check(f.token is not None,'CSRF token')
changed='TEST ONLY — edited source text, not a real product claim.'
data={'_handler':'translation','Input.Text':changed,'Input.SourceLanguage':'en','Input.TargetLanguage':'tr','Input.Mode':'label','Input.ReviewedSource':'true','Input.CloudConsent':'true'}
code,_=call('/translate/catalog/1',data);check(code==400,'Translation POST CSRF')
data['__RequestVerificationToken']=f.token
code,page=call('/translate/catalog/1',data);f=Form();f.feed(page);check(code==200 and f.text==changed,'POST overwrote edited source with catalog text')
code,page=call('/translate/catalog/2147483647');check(code==200 and 'Yerel ürün bulunamadı' in html.unescape(page),'Missing product fallback')
print('PASS HTTP flow: local product link, full original text transfer, edited source retained on POST, CSRF, missing product fallback, stale-message removal.')
