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

    internal struct DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(uint seed)
        {
            _state = seed == 0 ? 0x6D2B79F5u : seed;
        }

        public uint NextUInt()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public double NextDouble()
        {
            return (NextUInt() & 0x00FFFFFFu) / 16777216d;
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            return minInclusive + (int)(NextDouble() * (maxExclusive - minInclusive));
        }
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
            var random = new DeterministicRandom(BuildStableSeed(_worldSeed, bucket, player));
            var result = new List<DogSpawn>(count);

            for (var i = 0; i < count; i++)
            {
                var angle = random.NextDouble() * Math.PI * 2d;
                var distance = 35d + random.NextDouble() * (radiusMeters - 35d);
                var north = Math.Cos(angle) * distance;
                var east = Math.Sin(angle) * distance;
                var dog = PickDog(ref random);
                var level = random.NextInt(1, 21);
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

        private static uint BuildStableSeed(int worldSeed, long bucket, in GeoCoordinate player)
        {
            unchecked
            {
                uint hash = 2166136261u;
                hash = Mix(hash, (uint)worldSeed);
                hash = Mix(hash, (uint)bucket);
                hash = Mix(hash, (uint)(bucket >> 32));
                hash = Mix(hash, (uint)Math.Round((player.Latitude + 90d) * 10000d));
                hash = Mix(hash, (uint)Math.Round((player.Longitude + 180d) * 10000d));
                return hash;
            }
        }

        private static uint Mix(uint hash, uint value)
        {
            unchecked
            {
                hash ^= value;
                hash *= 16777619u;
                return hash;
            }
        }

        private static DogDefinition PickDog(ref DeterministicRandom random)
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

            if (candidates.Count == 0) return StarterCatalog.Dogs[random.NextInt(0, StarterCatalog.Dogs.Count)];
            return candidates[random.NextInt(0, candidates.Count)];
        }
    }
}
