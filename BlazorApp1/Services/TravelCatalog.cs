using System.Globalization;
using System.Text;
using System.Text.Json;
using HelalRehber.Shared;
using Microsoft.Extensions.Caching.Memory;
namespace BlazorApp1.Services;

public sealed class TravelCatalog(IWebHostEnvironment env,IHttpClientFactory clients,IConfiguration config,IMemoryCache cache)
{
    private readonly SemaphoreSlim gate=new(1,1);
    private DateTimeOffset nextRequest=DateTimeOffset.MinValue;
    private DateOnly day=DateOnly.FromDateTime(DateTime.UtcNow);
    private int requests;
    private long downloaded;
    private readonly Lazy<List<CityRow>> cities=new(()=>JsonSerializer.Deserialize<List<CityRow>>(File.ReadAllText(Path.Combine(env.ContentRootPath,"Data","travel-cities.json")),new JsonSerializerOptions(JsonSerializerDefaults.Web))??[]);
    private sealed class CityRow
    {
        public int Id { get; set; } public string Name { get; set; }=""; public string Country { get; set; }="";
        public double Latitude { get; set; } public double Longitude { get; set; } public string Aliases { get; set; }="";
        private string? search;
        public string Search => search??=Normalize(Name+","+Aliases);
        public TravelCity Pick()=>new(Id,Name,Country,Latitude,Longitude);
    }
    private static string Normalize(string text)=>string.Concat(text.Replace('ı','i').Replace('İ','i').Normalize(NormalizationForm.FormD).Where(c=>CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark)).ToLowerInvariant();
    public TravelCity? City(int id)=>cities.Value.FirstOrDefault(c=>c.Id==id)?.Pick();
    public List<TravelCity> Cities(string? query)
    {
        if(string.IsNullOrWhiteSpace(query)||query.Trim().Length<2||query.Length>80)return [];
        var search=Normalize(query.Trim());
        return cities.Value.Where(c=>c.Search.Contains(search,StringComparison.Ordinal)).OrderByDescending(c=>Normalize(c.Name)==search).ThenBy(c=>c.Name).Take(20).Select(c=>c.Pick()).ToList();
    }
    public async Task<TravelResult> SearchAsync(TravelSearch input,CancellationToken token=default)
    {
        double? mapLatitude=null,mapLongitude=null;
        TravelResult Fail(int status,string message)=>new(status,message,mapLatitude??0,mapLongitude??0,null,[],mapLatitude is not null);
        if(!input.Consent)return Fail(400,"Arama bölgesinin Overpass'a, harita görüntüsünün OpenStreetMap'e iletilmesini onaylayın.");
        if(!bool.TryParse(config["Maps:Enabled"],out var enabled)||!enabled)return Fail(503,"Harita veri servisi bu ortamda kapalı. Yayın için veri sağlayıcısı yapılandırılmalı.");
        var city=input.UseCoordinates?null:cities.Value.FirstOrDefault(c=>c.Id==input.CityId);
        var latitude=input.UseCoordinates?input.Latitude:city?.Latitude;
        var longitude=input.UseCoordinates?input.Longitude:city?.Longitude;
        if(latitude is null||longitude is null||!TravelRules.Valid(latitude.Value,longitude.Value))return Fail(400,"Bir şehir seçin veya geçerli enlem/boylam girin (enlem -85–85, boylam -180–180).");
        var lat=TravelRules.Coarse(latitude.Value);var lon=TravelRules.Coarse(longitude.Value);mapLatitude=lat;mapLongitude=lon;
        var key=FormattableString.Invariant($"travel:{lat:F2}:{lon:F2}");
        if(cache.TryGetValue<TravelResult>(key,out var cached))return cached!;
        if(!await gate.WaitAsync(0,token))return Fail(429,"Harita servisi başka aramayı işliyor. Biraz bekleyip tekrar deneyin.");
        try
        {
            if(cache.TryGetValue<TravelResult>(key,out cached))return cached!;
            var now=DateTimeOffset.UtcNow;var today=DateOnly.FromDateTime(now.UtcDateTime);if(today!=day){day=today;requests=0;downloaded=0;}
            if(now<nextRequest||requests>=50||downloaded>6*1024*1024)return Fail(429,"Ücretsiz demo harita isteği sınırına ulaşıldı. Bir süre sonra tekrar deneyin.");
            nextRequest=now.AddSeconds(10);requests++;
            var query=FormattableString.Invariant($"[out:json][timeout:20][maxsize:4194304];nwr(around:3000,{lat:F2},{lon:F2})[amenity~\"^(restaurant|cafe|fast_food|food_court)$\"][\"diet:halal\"~\"^(yes|only)$\"];out center 100;");
            using var request=new HttpRequestMessage(HttpMethod.Post,""){Content=new FormUrlEncodedContent(new Dictionary<string,string>{{"data",query}})};
            using var response=await clients.CreateClient("Overpass").SendAsync(request,HttpCompletionOption.ResponseHeadersRead,token);
            if((int)response.StatusCode is 429 or 406 or 504){nextRequest=DateTimeOffset.UtcNow.AddSeconds(30);return Fail((int)response.StatusCode==406?429:(int)response.StatusCode,"Ücretsiz harita veri servisi yoğun. Otomatik tekrar denenmez; en az 30 saniye sonra yeniden deneyin.");}
            if(!response.IsSuccessStatusCode)return Fail(502,"Harita verisi alınamadı. Bu, bölgede restoran olmadığı anlamına gelmez.");
            await response.Content.LoadIntoBufferAsync(4*1024*1024,token);
            var payload=await response.Content.ReadAsByteArrayAsync(token);downloaded+=payload.LongLength;
            using var json=JsonDocument.Parse(payload);
            if(json.RootElement.TryGetProperty("remark",out _))return Fail(502,"Harita servisi eksik sonuç bildirdi. Sonuçlar gösterilemiyor; sonra tekrar deneyin.");
            var places=Parse(json.RootElement,lat,lon);
            var result=new TravelResult(200,places.Count==0?"Bu bölgede bu sorguya uyan OSM kaydı bulunamadı. Bu, restoran veya helal seçenek olmadığı anlamına gelmez.":"OSM topluluk beyanları gösteriliyor; sertifika ve güncel işletme durumu doğrulanmadı. En fazla 100 kaynak kaydı işlenir; tüm işletmeler listelenmez.",lat,lon,now,places,true);
            cache.Set(key,result,TimeSpan.FromMinutes(30));return result;
        }
        catch(OperationCanceledException) when(!token.IsCancellationRequested){return Fail(504,"Harita isteği zaman aşımına uğradı. Sonra tekrar deneyin.");}
        catch(Exception ex) when(ex is HttpRequestException or JsonException or InvalidOperationException or KeyNotFoundException or FormatException){return Fail(502,"Harita verisi okunamadı. Bu hata uygunluk sonucu değildir.");}
        finally {gate.Release();}
    }
    public static List<TravelPlace> Parse(JsonElement root,double latitude,double longitude)
    {
        var places=new List<TravelPlace>();
        foreach(var item in root.GetProperty("elements").EnumerateArray().Take(100))
        {
            var type=item.GetProperty("type").GetString();if(type is not ("node" or "way" or "relation"))continue;
            if(!item.TryGetProperty("tags",out var tags))continue;
            string Tag(string key)=>tags.TryGetProperty(key,out var v)?(v.GetString()??"")[..Math.Min(v.GetString()?.Length??0,300)]:"";
            var halal=Tag("diet:halal");if(halal is not ("yes" or "only"))continue;
            if(Tag("amenity") is not ("restaurant" or "cafe" or "fast_food" or "food_court"))continue;
            if(!item.TryGetProperty("id",out var idValue)||!idValue.TryGetInt64(out var id)||id<=0)continue;
            var point=item.TryGetProperty("center",out var center)?center:item;
            if(!point.TryGetProperty("lat",out var la)||!point.TryGetProperty("lon",out var lo)||!la.TryGetDouble(out var lat)||!lo.TryGetDouble(out var lon)||!TravelRules.Valid(lat,lon))continue;
            var distance=TravelRules.Distance(latitude,longitude,lat,lon);if(distance>3)continue;
            var address=string.Join(" ",new[]{Tag("addr:street"),Tag("addr:housenumber"),Tag("addr:city")}.Where(x=>x.Length>0));
            places.Add(new(type!,id,Tag("name") is {Length:>0} name?name:"Adı belirtilmemiş işletme",lat,lon,address,Tag("cuisine"),halal,Tag("check_date"),Math.Round(distance,2)));
        }
        return places.DistinctBy(x=>(x.OsmType,x.OsmId)).OrderBy(x=>x.DistanceKm).ToList();
    }
}
