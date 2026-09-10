using System;

namespace NexusDogsGo.Gameplay.World;

public readonly struct GeoCoordinate
{
    public readonly double Latitude;
    public readonly double Longitude;

    public GeoCoordinate(double latitude, double longitude)
    {
        Latitude = Math.Clamp(latitude, -90d, 90d);
        Longitude = NormalizeLongitude(longitude);
    }

    private static double NormalizeLongitude(double longitude)
    {
        while (longitude > 180d) longitude -= 360d;
        while (longitude < -180d) longitude += 360d;
        return longitude;
    }
}

public static class GeoMath
{
    private const double EarthRadiusMeters = 6371000d;

    public static double DistanceMeters(in GeoCoordinate a, in GeoCoordinate b)
    {
        var lat1 = DegreesToRadians(a.Latitude);
        var lat2 = DegreesToRadians(b.Latitude);
        var dLat = DegreesToRadians(b.Latitude - a.Latitude);
        var dLon = DegreesToRadians(b.Longitude - a.Longitude);
        var sinLat = Math.Sin(dLat / 2d);
        var sinLon = Math.Sin(dLon / 2d);
        var h = sinLat * sinLat + Math.Cos(lat1) * Math.Cos(lat2) * sinLon * sinLon;
        var c = 2d * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1d - h));
        return EarthRadiusMeters * c;
    }

    public static GeoCoordinate OffsetMeters(in GeoCoordinate origin, double northMeters, double eastMeters)
    {
        var dLat = northMeters / EarthRadiusMeters;
        var dLon = eastMeters / (EarthRadiusMeters * Math.Cos(DegreesToRadians(origin.Latitude)));
        return new GeoCoordinate(origin.Latitude + RadiansToDegrees(dLat), origin.Longitude + RadiansToDegrees(dLon));
    }

    private static double DegreesToRadians(double value) => value * Math.PI / 180d;
    private static double RadiansToDegrees(double value) => value * 180d / Math.PI;
}
