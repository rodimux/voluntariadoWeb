namespace Volun.Core.ValueObjects;

public sealed record GeoLocation(double Latitude, double Longitude)
{
    public static GeoLocation From(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
        }

        return new GeoLocation(latitude, longitude);
    }
}
