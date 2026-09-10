using System;
using System.Collections.Generic;
using NexusDogsGo.Config;
using NexusDogsGo.Domain;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class DogSpawn
    {
        public string SpawnId = string.Empty;
        public DogDefinition Dog;
        public GeoCoordinate Coordinate;
        public int Level;
        public DateTimeOffset ExpiresAt;
    }

    public sealed class DogSpawnService
    {
        private readonly int _worldSeed;

        public DogSpawnService(int worldSeed = 92417)
        {
            _worldSeed = worldSeed;
        }

        public IReadOnlyList<DogSpawn> GenerateNearby(in GeoCoordinate player, DateTimeOffset now, int count = 8, double radiusMeters = 450d)
        {
            count = Math.Max(1, Math.Min(25, count));
            radiusMeters = Math.Max(50d, Math.Min(2000d, radiusMeters));
            var bucket = now.ToUnixTimeSeconds() / 900;
            var seed = HashCode.Combine(_worldSeed, bucket, Math.Round(player.Latitude, 3), Math.Round(player.Longitude, 3));
            var random = new Random(seed);
            var result = new List<DogSpawn>(count);

            for (var i = 0; i < count; i++)
            {
                var angle = random.NextDouble() * Math.PI * 2d;
                var distance = 35d + random.NextDouble() * (radiusMeters - 35d);
                var north = Math.Cos(angle) * distance;
                var east = Math.Sin(angle) * distance;
                var dog = PickDog(random);
                var level = random.Next(1, 21);
                result.Add(new DogSpawn
                {
                    SpawnId = bucket + "-" + i + "-" + dog.Id,
                    Dog = dog,
                    Coordinate = GeoMath.OffsetMeters(player, north, east),
                    Level = level,
                    ExpiresAt = DateTimeOffset.FromUnixTimeSeconds((bucket + 1) * 900)
                });
            }

            return result;
        }

        private static DogDefinition PickDog(Random random)
        {
            var roll = random.NextDouble();
            DogRarity rarity;
            if (roll < 0.60) rarity = DogRarity.Common;
            else if (roll < 0.87) rarity = DogRarity.Rare;
            else if (roll < 0.98) rarity = DogRarity.Epic;
            else rarity = DogRarity.Legendary;

            var candidates = new List<DogDefinition>();
            foreach (var dog in StarterCatalog.Dogs)
            {
                if (dog.Rarity == rarity) candidates.Add(dog);
            }

            if (candidates.Count == 0) return StarterCatalog.Dogs[random.Next(StarterCatalog.Dogs.Count)];
            return candidates[random.Next(candidates.Count)];
        }
    }
}
