const {test}=require('node:test');const assert=require('node:assert/strict');const vm=require('node:vm');const fs=require('node:fs');
const code=fs.readFileSync('BlazorApp1/wwwroot/travel-map.js','utf8');
function setup({secure=true,denied=false,map=false}={}){
 const events={},lat={},lon={},use={},status={},mapStatus={},questions={},translate={};let requests=0,tiles=0,popup;
 const element=map?{dataset:{lat:'41.01',lon:'28.97',tiles:'https://tile.openstreetmap.org/{z}/{x}/{y}.png',places:JSON.stringify([{Name:'<img onerror=alert(1)>',Latitude:41.01,Longitude:28.97}])}}:null;
 const document={getElementById:id=>({'travel-lat':lat,'travel-lon':lon,'travel-use-coordinates':use}[id]),querySelector:s=>({'[data-location-status]':status,'[data-travel-map]':element,'[data-map-status]':mapStatus,'[data-travel-questions]':questions,'[data-travel-translate]':translate}[s]||null),addEventListener:(n,f)=>events[n]=f,createElement:()=>({textContent:''})};
 const L={map:()=>({setView(){return this;}}),tileLayer:()=>{tiles++;return{on(){return this;},addTo(){return this;}};},circle:()=>({addTo(){}}),circleMarker:()=>({bindPopup(node){popup=node;return this;},addTo(){}})};
 const navigator={geolocation:{getCurrentPosition(ok,fail){requests++;denied?fail():ok({coords:{latitude:41.014999,longitude:28.974999}});}}};
 const window={isSecureContext:secure,L};vm.runInNewContext(code,{window,document,navigator,L});
 return {events,window,lat,lon,use,status,questions,translate,requests:()=>requests,tiles:()=>tiles,popup:()=>popup};
}
test('no automatic location or tile fetch without consent/result',()=>{const s=setup();assert.equal(s.requests(),0);assert.equal(s.tiles(),0);});
test('explicit location fills coarse coordinates and never auto-submits',()=>{const s=setup();s.events.click({target:{closest:()=>true}});assert.equal(s.requests(),1);assert.equal(s.lat.value,'41.01');assert.equal(s.lon.value,'28.97');assert.equal(s.use.checked,true);assert.match(s.status.textContent,/onay|Onay/);});
test('denied permission and insecure context allow manual fallback',()=>{const s=setup({denied:true});s.events.click({target:{closest:()=>true}});assert.match(s.status.textContent,/izin verilmedi/);const t=setup({secure:false});t.events.click({target:{closest:()=>true}});assert.equal(t.requests(),0);assert.match(t.status.textContent,/kullanılamıyor/);});
test('map uses text popup instead of injecting OSM names as HTML',()=>{const s=setup({map:true});assert.equal(s.tiles(),1);assert.match(s.popup().textContent,/<img onerror/);assert.equal(s.popup().innerHTML,undefined);});
test('travel language updates question/translation links without location sharing',()=>{const s=setup();s.events.change({target:{id:'travel-language',value:'ja'}});assert.equal(s.questions.href,'/questions?language=ja');assert.equal(s.translate.href,'/translate?target=ja');assert.equal(s.requests(),0);});
