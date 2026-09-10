using System;
using NexusDogsGo.Domain;
using NexusDogsGo.Gameplay.Missions;
using NexusDogsGo.Gameplay.Progression;
using NexusDogsGo.Gameplay.World;

namespace NexusDogsGo.Gameplay.Capture
{
    public enum CaptureAttemptStatus
    {
        Captured,
        Escaped,
        MissingBall,
        MissingFood,
        InvalidSpawn
    }

    public readonly struct CaptureAttemptOutcome
    {
        public readonly CaptureAttemptStatus Status;
        public readonly CaptureResult Result;
        public readonly DogInstance CapturedDog;

        public CaptureAttemptOutcome(CaptureAttemptStatus status, CaptureResult result, DogInstance capturedDog)
        {
            Status = status;
            Result = result;
            CapturedDog = capturedDog;
        }
    }

    public sealed class CaptureGameService
    {
        private readonly PlayerProfile _profile;
        private readonly MissionTracker _missions;
        private readonly IRandomSource _random;

        public CaptureGameService(PlayerProfile profile, MissionTracker missions, IRandomSource random = null)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _missions = missions ?? throw new ArgumentNullException(nameof(missions));
            _random = random ?? new SystemRandomSource();
        }

        public CaptureAttemptOutcome Attempt(DogSpawn spawn, CaptureBallType ball, ThrowQuality quality, bool useFood)
        {
            if (spawn == null || spawn.Dog == null)
                return new CaptureAttemptOutcome(CaptureAttemptStatus.InvalidSpawn, default, null);

            var ballItem = ball == CaptureBallType.SuperBall ? InventoryItemId.SuperBall : InventoryItemId.PokeBall;
            if (_profile.Inventory.GetQuantity(ballItem) < 1)
                return new CaptureAttemptOutcome(CaptureAttemptStatus.MissingBall, default, null);

            if (useFood && _profile.Inventory.GetQuantity(InventoryItemId.PremiumFood) < 1)
                return new CaptureAttemptOutcome(CaptureAttemptStatus.MissingFood, default, null);

            _profile.Inventory.TryConsume(ballItem);
            if (useFood) _profile.Inventory.TryConsume(InventoryItemId.PremiumFood);

            var context = new CaptureContext(spawn.Dog, spawn.Level, ball, quality, useFood);
            var result = CaptureCalculator.Resolve(context, _random);
            if (!result.Captured)
                return new CaptureAttemptOutcome(CaptureAttemptStatus.Escaped, result, null);

            var dog = new DogInstance
            {
                DefinitionId = spawn.Dog.Id,
                Level = Math.Max(1, spawn.Level),
                CurrentHp = ProgressionSystem.CalculateMaxHp(spawn.Level)
            };
            _profile.Dogs.Add(dog);
            _profile.Coins = checked(_profile.Coins + 25);
            ProgressionSystem.AddPlayerExperience(_profile, 100);
            _missions.Report(MissionMetric.CaptureDogs, 1);
            return new CaptureAttemptOutcome(CaptureAttemptStatus.Captured, result, dog);
        }
    }
}
