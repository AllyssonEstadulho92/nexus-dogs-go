using System.Threading;
using System.Threading.Tasks;
using NexusDogsGo.Gameplay.World;
using NUnit.Framework;

namespace NexusDogsGo.Tests
{
    public sealed class VerticalSliceTests
    {
        [Test]
        public async Task SimulatedLocationProvider_StartsAndMoves()
        {
            var origin = new GeoCoordinate(38.7369d, -9.1427d);
            var provider = new SimulatedLocationProvider(origin);

            Assert.That(await provider.StartAsync(CancellationToken.None), Is.True);
            Assert.That(provider.TryGetLocation(out var before), Is.True);

            provider.MoveMeters(100d, 0d);
            Assert.That(provider.TryGetLocation(out var after), Is.True);
            Assert.That(GeoMath.DistanceMeters(before, after), Is.InRange(99d, 101d));
        }

        [Test]
        public void GeoSceneProjection_ProjectsNorthToPositiveZ()
        {
            var origin = new GeoCoordinate(38.7369d, -9.1427d);
            var north = GeoMath.OffsetMeters(origin, 100d, 0d);
            var projected = GeoSceneProjection.ToWorldOffset(origin, north, 1f);

            Assert.That(projected.z, Is.InRange(99f, 101f));
            Assert.That(projected.x, Is.InRange(-1f, 1f));
        }

        [Test]
        public void GeoSceneProjection_ProjectsEastToPositiveX()
        {
            var origin = new GeoCoordinate(38.7369d, -9.1427d);
            var east = GeoMath.OffsetMeters(origin, 0d, 100d);
            var projected = GeoSceneProjection.ToWorldOffset(origin, east, 1f);

            Assert.That(projected.x, Is.InRange(99f, 101f));
            Assert.That(projected.z, Is.InRange(-1f, 1f));
        }
    }
}
