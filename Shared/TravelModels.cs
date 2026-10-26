using System.Globalization;
namespace HelalRehber.Shared;
public sealed class TravelSearch
{
    public int CityId { get; set; }
    public bool UseCoordinates { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool Consent { get; set; }
    public string CardLanguage { get; set; } = "en";
}
public sealed record TravelCity(int Id,string Name,string Country,double Latitude,double Longitude);
public sealed record TravelPlace(string OsmType,long OsmId,string Name,double Latitude,double Longitude,string Address,string Cuisine,string HalalTag,string SourceDate,double DistanceKm)
{
    public string SourceUrl => $"https://www.openstreetmap.org/{OsmType}/{OsmId}";
    public string MapUrl => "https://www.openstreetmap.org/?mlat="+Latitude.ToString(CultureInfo.InvariantCulture)+"&mlon="+Longitude.ToString(CultureInfo.InvariantCulture)+"#map=17/"+Latitude.ToString(CultureInfo.InvariantCulture)+"/"+Longitude.ToString(CultureInfo.InvariantCulture);
    public string DirectionsUrl => "https://www.google.com/maps/dir/?api=1&destination="+Latitude.ToString(CultureInfo.InvariantCulture)+","+Longitude.ToString(CultureInfo.InvariantCulture);
}
public sealed record TravelResult(int HttpStatus,string Message,double Latitude,double Longitude,DateTimeOffset? FetchedUtc,List<TravelPlace> Places,bool HasCenter=false);
public static class TravelRules
{
    public static bool Valid(double latitude,double longitude) => double.IsFinite(latitude)&&double.IsFinite(longitude)&&latitude>=-85&&latitude<=85&&longitude>=-180&&longitude<=180;
    public static double Coarse(double coordinate) => Math.Round(coordinate,2,MidpointRounding.AwayFromZero);
    public static double Distance(double a,double b,double c,double d)
    {
        double Rad(double x)=>x*Math.PI/180;
        var h=Math.Pow(Math.Sin(Rad(c-a)/2),2)+Math.Cos(Rad(a))*Math.Cos(Rad(c))*Math.Pow(Math.Sin(Rad(d-b)/2),2);
        return 6371*2*Math.Asin(Math.Sqrt(Math.Clamp(h,0,1)));
    }
}
