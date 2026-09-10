using System;
using NexusDogsGo.Domain;
using NexusDogsGo.Gameplay.Missions;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class ExplorationTracker
    {
        private const double MinimumMovementMeters = 3d;
        private const double MaximumAcceptedSegmentMeters = 500d;

        private readonly PlayerProfile _profile;
        private readonly MissionTracker _missions;
        private bool _hasPrevious;
        private GeoCoordinate _previous;

        public ExplorationTracker(PlayerProfile profile, MissionTracker missions)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _missions = missions ?? throw new ArgumentNullException(nameof(missions));
        }

        public double ReportLocation(in GeoCoordinate coordinate)
        {
            if (!_hasPrevious)
            {
                _previous = coordinate;
                _hasPrevious = true;
                return 0d;
            }

            var distance = GeoMath.DistanceMeters(_previous, coordinate);
            _previous = coordinate;

            if (distance < MinimumMovementMeters || distance > MaximumAcceptedSegmentMeters) return 0d;
            _profile.TotalDistanceMeters += distance;
            _missions.Report(MissionMetric.WalkMeters, distance);
            return distance;
        }

        public void ReportPoiVisit()
        {
            _missions.Report(MissionMetric.VisitPoi, 1d);
        }
    }
}
