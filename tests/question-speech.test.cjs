const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
function setup(voices = []) {
    const events = {}, windowEvents = {}, speechEvents = {}, spoken = [], status = { textContent: '' };
    let cancelled = 0;
    const synthesis = { getVoices: () => voices, cancel: () => cancelled++, speak: x => spoken.push(x), addEventListener: (n,f) => speechEvents[n] = f };
    const document = { hidden: false, querySelector: s => s === '[data-question-status]' ? status : {}, addEventListener: (n,f) => events[n] = f };
    const window = { speechSynthesis: synthesis, SpeechSynthesisUtterance: function(text) { this.text = text; }, addEventListener: (n,f) => windowEvents[n] = f };
    vm.runInNewContext(fs.readFileSync('BlazorApp1/wwwroot/question-speech.js', 'utf8'), { window, document, SpeechSynthesisUtterance: window.SpeechSynthesisUtterance });
    const click = (language, text = 'Question?') => {
        const card = { dataset: { language }, querySelector: () => ({ textContent: text }) };
        const button = { hasAttribute: () => false, closest: () => card };
        events.click({ target: { closest: s => s === 'a[href]' ? null : button } });
    };
    return { click, status, spoken, synthesis, events, windowEvents, speechEvents, document, cancelled: () => cancelled };
}
test('speaks exact card text using matching local language, without fallback', () => {
    const s = setup([{localService:true,lang:'tr-TR'},{localService:true,lang:'ja-JP'}]);
    s.click('ja', 'この料理の材料は？');
    assert.equal(s.spoken[0].text, 'この料理の材料は？');assert.equal(s.spoken[0].lang, 'ja-JP');
    s.click('ar');assert.equal(s.spoken.length,1);assert.match(s.status.textContent,/yerel ses bulunamadı/);
});
test('never sends speech to a network voice', () => {
    const s=setup([{localService:false,lang:'en-US'}]);s.click('en');assert.equal(s.spoken.length,0);
});
test('voice list can arrive late; no automatic speech', () => {
    const voices=[];const s=setup(voices);s.click('en');assert.equal(s.spoken.length,0);
    voices.push({localService:true,lang:'en-US'});s.speechEvents.voiceschanged();assert.equal(s.spoken.length,0);
    s.click('en');assert.equal(s.spoken.length,1);
});
test('switching cards cancels old speech and ignores stale completion', () => {
    const s=setup([{localService:true,lang:'en-US'}]);s.click('en','first');const old=s.spoken[0];s.click('en','second');
    old.onend();assert.equal(s.status.textContent,'Seslendiriliyor…');s.spoken[1].onend();assert.match(s.status.textContent,/tamamlandı/);
    assert.equal(s.cancelled(),2);
});
test('navigation, hiding page, and stop cancel speech', () => {
    const s=setup();s.windowEvents.pagehide();s.windowEvents.popstate();s.events.submit();s.document.hidden=true;s.events.visibilitychange();
    s.events.click({target:{closest:q=>q==='a[href]'?null:{hasAttribute:()=>true}}});assert.equal(s.cancelled(),5);assert.match(s.status.textContent,/durduruldu/);
});
test('browser without speech support leaves readable cards and explains failure', () => {
    const handlers={}, status={textContent:''};const window={addEventListener:()=>{}};
    const document={querySelector:()=>status,addEventListener:(n,f)=>handlers[n]=f};
    vm.runInNewContext(fs.readFileSync('BlazorApp1/wwwroot/question-speech.js','utf8'),{window,document});
    handlers.click({target:{closest:q=>q==='a[href]'?null:{hasAttribute:()=>false}}});assert.match(status.textContent,/desteklemiyor/);
});
