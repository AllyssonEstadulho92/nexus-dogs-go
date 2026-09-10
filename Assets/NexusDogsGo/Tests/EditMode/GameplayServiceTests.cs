using System;
using NexusDogsGo.Config;
using NexusDogsGo.Domain;
using NexusDogsGo.Gameplay.Capture;
using NexusDogsGo.Gameplay.Missions;
using NexusDogsGo.Gameplay.World;
using NUnit.Framework;

namespace NexusDogsGo.Tests
{
    public sealed class GameplayServiceTests
    {
        private sealed class FixedRandom : IRandomSource
        {
            public double Next01() => 0.001d;
        }

        [Test]
        public void SuccessfulCaptureConsumesBallAndAddsDog()
        {
            var profile = new PlayerProfile();
            var missions = new MissionTracker();
            var service = new CaptureGameService(profile, missions, new FixedRandom());
            var spawn = new DogSpawn
            {
                SpawnId = "test",
                Dog = StarterCatalog.Dogs[0],
                Level = 3,
                Coordinate = new GeoCoordinate(38.72, -9.14),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
            };
            var ballsBefore = profile.Inventory.GetQuantity(InventoryItemId.PokeBall);

            var outcome = service.Attempt(spawn, CaptureBallType.PokeBall, ThrowQuality.Excellent, false);

            Assert.That(outcome.Status, Is.EqualTo(CaptureAttemptStatus.Captured));
            Assert.That(profile.Dogs.Count, Is.EqualTo(1));
            Assert.That(profile.Inventory.GetQuantity(InventoryItemId.PokeBall), Is.EqualTo(ballsBefore - 1));
        }

        [Test]
        public void SpawnGenerationIsStableForSameBucketAndCoordinate()
        {
            var service = new DogSpawnService();
            var coordinate = new GeoCoordinate(38.7223, -9.1393);
            var time = DateTimeOffset.FromUnixTimeSeconds(1_800_000_000);
            var a = service.GenerateNearby(coordinate, time, 5);
            var b = service.GenerateNearby(coordinate, time, 5);

            for (var i = 0; i < a.Count; i++)
            {
                Assert.That(a[i].SpawnId, Is.EqualTo(b[i].SpawnId));
                Assert.That(a[i].Dog.Id, Is.EqualTo(b[i].Dog.Id));
                Assert.That(a[i].Level, Is.EqualTo(b[i].Level));
                Assert.That(a[i].Coordinate.Latitude, Is.EqualTo(b[i].Coordinate.Latitude).Within(0.0000001d));
                Assert.That(a[i].Coordinate.Longitude, Is.EqualTo(b[i].Coordinate.Longitude).Within(0.0000001d));
            }
        }
    }
}
