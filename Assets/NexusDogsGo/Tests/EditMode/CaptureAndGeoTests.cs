using NexusDogsGo.Config;
using NexusDogsGo.Gameplay.Capture;
using NexusDogsGo.Gameplay.World;
using NUnit.Framework;

namespace NexusDogsGo.Tests;

public sealed class CaptureAndGeoTests
{
    private sealed class FixedRandom : IRandomSource
    {
        private readonly double _value;
        public FixedRandom(double value) => _value = value;
        public double Next01() => _value;
    }

    [Test]
    public void ExcellentSuperBallHasHigherChanceThanNormalPokeBall()
    {
        var dog = StarterCatalog.Dogs[1];
        var normal = new CaptureContext(dog, 10, CaptureBallType.PokeBall, ThrowQuality.Normal, false);
        var boosted = new CaptureContext(dog, 10, CaptureBallType.SuperBall, ThrowQuality.Excellent, true);
        Assert.That(CaptureCalculator.CalculateProbability(boosted), Is.GreaterThan(CaptureCalculator.CalculateProbability(normal)));
    }

    [Test]
    public void CaptureResolutionUsesProbabilityBoundary()
    {
        var dog = StarterCatalog.Dogs[0];
        var context = new CaptureContext(dog, 1, CaptureBallType.SuperBall, ThrowQuality.Excellent, true);
        var result = CaptureCalculator.Resolve(context, new FixedRandom(0.01));
        Assert.That(result.Captured, Is.True);
    }

    [Test]
    public void GeoDistanceSamePointIsZero()
    {
        var point = new GeoCoordinate(38.7223, -9.1393);
        Assert.That(GeoMath.DistanceMeters(point, point), Is.EqualTo(0d).Within(0.001d));
    }

    [Test]
    public void OffsetCreatesExpectedApproximateDistance()
    {
        var origin = new GeoCoordinate(38.7223, -9.1393);
        var moved = GeoMath.OffsetMeters(origin, 100d, 0d);
        Assert.That(GeoMath.DistanceMeters(origin, moved), Is.EqualTo(100d).Within(0.5d));
    }
}
