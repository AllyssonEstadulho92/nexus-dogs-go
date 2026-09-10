using System;
using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public static class GeoSceneProjection
    {
        private const double EarthRadiusMeters = 6371000d;

        public static Vector3 ToWorldOffset(in GeoCoordinate origin, in GeoCoordinate target, float worldUnitsPerMeter = 1f)
        {
            var lat0 = DegreesToRadians(origin.Latitude);
            var dLat = DegreesToRadians(target.Latitude - origin.Latitude);
            var dLon = DegreesToRadians(target.Longitude - origin.Longitude);

            var northMeters = dLat * EarthRadiusMeters;
            var eastMeters = dLon * EarthRadiusMeters * Math.Cos(lat0);
            return new Vector3(
                (float)(eastMeters * worldUnitsPerMeter),
                0f,
                (float)(northMeters * worldUnitsPerMeter));
        }

        private static double DegreesToRadians(double value)
        {
            return value * Math.PI / 180d;
        }
    }
}
