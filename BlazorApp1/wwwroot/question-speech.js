// Minimal browser bridge: fixed C# question cards, local voices only, no remote API.
const supported = 'speechSynthesis' in window && 'SpeechSynthesisUtterance' in window;
let current = null;
function status(message) {
    const node = document.querySelector('[data-question-status]');
    if (node) node.textContent = message;
}
function stop() {
    current = null;
    if (supported) window.speechSynthesis.cancel();
}
function voiceFor(language) {
    return window.speechSynthesis.getVoices().find(v => v.localService === true && v.lang.toLowerCase().replaceAll('_', '-').split('-')[0] === language);
}
document.addEventListener('click', event => {
    if (event.target.closest?.('a[href]')) stop();
    const button = event.target.closest?.('[data-question-speak], [data-question-stop]');
    if (!button) return;
    stop();
    if (button.hasAttribute('data-question-stop')) { status('Ses durduruldu.'); return; }
    if (!supported) { status('Bu tarayıcı seslendirmeyi desteklemiyor. Kartı metin olarak gösterebilirsiniz.'); return; }
    const card = button.closest('[data-question-card]');
    const voice = voiceFor(card.dataset.language);
    if (!voice) { status('Bu dilde yerel ses bulunamadı veya sesler henüz yüklenmedi. Cihazın ses ayarlarını kontrol edip tekrar deneyin; kartı metin olarak gösterebilirsiniz.'); return; }
    const utterance = new SpeechSynthesisUtterance(card.querySelector('.question-phrase').textContent);
    utterance.lang = voice.lang;
    utterance.voice = voice;
    current = utterance;
    utterance.onend = () => { if (current === utterance) { current = null; status('Seslendirme tamamlandı.'); } };
    utterance.onerror = () => { if (current === utterance) { current = null; status('Seslendirme tamamlanamadı. Kartı metin olarak gösterebilirsiniz.'); } };
    status('Seslendiriliyor…');
    window.speechSynthesis.speak(utterance);
});
if (supported) {
    window.speechSynthesis.getVoices();
    window.speechSynthesis.addEventListener('voiceschanged', () => {
        if (!current && document.querySelector('[data-question-cards]')) status('Ses listesi güncellendi. Seslendir düğmesini kullanabilirsiniz.');
    });
}
window.addEventListener('pagehide', stop);
window.addEventListener('popstate', stop);
document.addEventListener('submit', stop);
document.addEventListener('visibilitychange', () => { if (document.hidden) stop(); });
