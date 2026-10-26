// Browser map/geolocation bridge. Search and data validation live in C#.
window.helalTravelSetPosition = (latitude, longitude) => {
    const lat = document.getElementById('travel-lat'), lon = document.getElementById('travel-lon');
    const use = document.getElementById('travel-use-coordinates');
    if (!lat || !lon || !use || !Number.isFinite(latitude) || !Number.isFinite(longitude)) return false;
    lat.value = (Math.round(latitude * 100) / 100).toFixed(2);
    lon.value = (Math.round(longitude * 100) / 100).toFixed(2);
    use.checked = true;
    const status = document.querySelector('[data-location-status]');
    if (status) status.textContent = 'Yaklaşık konum forma geldi. Onay kutusunu kontrol edip Bu bölgede ara düğmesine basın.';
    return true;
};
document.addEventListener('click', event => {
    if (!event.target.closest?.('[data-travel-locate]')) return;
    const status = document.querySelector('[data-location-status]');
    if (!navigator.geolocation || !window.isSecureContext) { status.textContent = 'Bu ortamda tarayıcı konumu kullanılamıyor. Mobilde üstteki cihaz konumu düğmesini, şehir veya koordinat girişini kullanın.'; return; }
    status.textContent = 'Konum izni bekleniyor…';
    navigator.geolocation.getCurrentPosition(position => window.helalTravelSetPosition(position.coords.latitude, position.coords.longitude),
        () => { status.textContent = 'Konum alınamadı veya izin verilmedi. Şehir ya da koordinat girebilirsiniz.'; },
        { enableHighAccuracy: false, timeout: 10000, maximumAge: 0 });
});
function draw() {
    const element = document.querySelector('[data-travel-map]');
    if (!element || element.dataset.ready || !window.L) return;
    element.dataset.ready = 'true';
    const status = document.querySelector('[data-map-status]');
    try {
        const lat = Number(element.dataset.lat), lon = Number(element.dataset.lon);
        const places = JSON.parse(element.dataset.places);
        const template = element.dataset.tiles;
        if (!Number.isFinite(lat) || !Number.isFinite(lon) || !template.startsWith('https://')) throw new Error('Invalid map');
        const map = L.map(element).setView([lat, lon], 14);
        L.tileLayer(template, { maxZoom: 19, keepBuffer: 0, updateWhenIdle: true,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap contributors</a>' }).on('tileerror', () => { status.textContent = 'Harita görüntüsü alınamadı. Restoran listesini ve kaynak bağlantılarını kullanabilirsiniz.'; }).addTo(map);
        L.circle([lat, lon], { radius: 3000, fillOpacity: 0.03, weight: 1 }).addTo(map);
        for (const place of places) {
            const text = document.createElement('span');
            text.textContent = `${place.Name} — OSM beyanı; sertifika doğrulanmadı`;
            L.circleMarker([place.Latitude, place.Longitude], { radius: 7, color: '#166456' }).bindPopup(text).addTo(map);
        }
        status.textContent = places.length > 0 ? 'İşaretler OSM topluluk kayıtlarıdır; sertifika doğrulanmadı. Metin listesi aşağıdadır.' : 'Harita bölgesi gösteriliyor. Restoran verisi için yukarıdaki arama durumuna bakın.';
    } catch { status.textContent = 'Harita gösterilemiyor. Aşağıdaki metin listesini kullanabilirsiniz.'; }
}
draw();
if (window.Blazor) Blazor.addEventListener('enhancedload', draw);

document.addEventListener('change', event => {
    if (event.target.id !== 'travel-language') return;
    const language = encodeURIComponent(event.target.value);
    const questions = document.querySelector('[data-travel-questions]');
    const translate = document.querySelector('[data-travel-translate]');
    if (questions) questions.href = '/questions?language=' + language;
    if (translate) translate.href = '/translate?target=' + language;
});
