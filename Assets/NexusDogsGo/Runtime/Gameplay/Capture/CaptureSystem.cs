using System;
using NexusDogsGo.Domain;

namespace NexusDogsGo.Gameplay.Capture;

public enum CaptureBallType
{
    PokeBall,
    SuperBall
}

public enum ThrowQuality
{
    Miss,
    Normal,
    Nice,
    Great,
    Excellent
}

public readonly struct CaptureContext
{
    public readonly DogDefinition Dog;
    public readonly int DogLevel;
    public readonly CaptureBallType Ball;
    public readonly ThrowQuality ThrowQuality;
    public readonly bool UsedFood;

    public CaptureContext(DogDefinition dog, int dogLevel, CaptureBallType ball, ThrowQuality throwQuality, bool usedFood)
    {
        Dog = dog;
        DogLevel = Math.Max(1, dogLevel);
        Ball = ball;
        ThrowQuality = throwQuality;
        UsedFood = usedFood;
    }
}

public readonly struct CaptureResult
{
    public readonly bool Captured;
    public readonly double Probability;
    public readonly double Roll;

    public CaptureResult(bool captured, double probability, double roll)
    {
        Captured = captured;
        Probability = probability;
        Roll = roll;
    }
}

public interface IRandomSource
{
    double Next01();
}

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random _random = new();
    public double Next01() => _random.NextDouble();
}

public static class CaptureCalculator
{
    public static double CalculateProbability(in CaptureContext context)
    {
        if (context.ThrowQuality == ThrowQuality.Miss) return 0d;

        var ballMultiplier = context.Ball == CaptureBallType.SuperBall ? 1.45d : 1d;
        var throwMultiplier = context.ThrowQuality switch
        {
            ThrowQuality.Normal => 1d,
            ThrowQuality.Nice => 1.10d,
            ThrowQuality.Great => 1.25d,
            ThrowQuality.Excellent => 1.50d,
            _ => 0d
        };
        var foodMultiplier = context.UsedFood ? 1.20d : 1d;
        var levelPenalty = 1d + Math.Max(0, context.DogLevel - 1) * 0.025d;
        var difficulty = Math.Max(0.25d, context.Dog.CaptureDifficulty) * levelPenalty;
        var baseChance = 0.48d / difficulty;
        return Math.Clamp(baseChance * ballMultiplier * throwMultiplier * foodMultiplier, 0.03d, 0.95d);
    }

    public static CaptureResult Resolve(in CaptureContext context, IRandomSource random)
    {
        if (random == null) throw new ArgumentNullException(nameof(random));
        var probability = CalculateProbability(context);
        var roll = random.Next01();
        return new CaptureResult(roll < probability, probability, roll);
    }
}
