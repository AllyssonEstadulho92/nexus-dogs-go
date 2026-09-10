namespace NexusDogsGo.Api.Services;

public sealed record SpawnDto(string SpawnId, string DogId, double Latitude, double Longitude, int Level, DateTimeOffset ExpiresAt);

public sealed class SpawnService
{
    private const double EarthRadiusMeters = 6_371_000d;
    private const int WorldSeed = 92417;

    private struct DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(uint seed) => _state = seed == 0 ? 0x6D2B79F5u : seed;

        private uint NextUInt()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public double NextDouble() => (NextUInt() & 0x00FFFFFFu) / 16777216d;
        public int NextInt(int minInclusive, int maxExclusive) => maxExclusive <= minInclusive ? minInclusive : minInclusive + (int)(NextDouble() * (maxExclusive - minInclusive));
    }

    public IReadOnlyList<SpawnDto> Generate(double latitude, double longitude, int count, IReadOnlyList<DogDto> dogs)
    {
        var now = DateTimeOffset.UtcNow;
        var bucket = now.ToUnixTimeSeconds() / 900;
        var random = new DeterministicRandom(BuildStableSeed(WorldSeed, bucket, latitude, longitude));
        var result = new List<SpawnDto>(count);

        for (var i = 0; i < count; i++)
        {
            var angle = random.NextDouble() * Math.PI * 2;
            var distance = 35 + random.NextDouble() * 415;
            var north = Math.Cos(angle) * distance;
            var east = Math.Sin(angle) * distance;
            var dLat = north / EarthRadiusMeters;
            var cos = Math.Cos(latitude * Math.PI / 180);
            var safeCos = Math.Abs(cos) < 0.000001 ? 0.000001 : cos;
            var dLon = east / (EarthRadiusMeters * safeCos);
            var dog = PickDog(ref random, dogs);
            result.Add(new SpawnDto(
                $"{bucket}-{i}-{dog.Id}",
                dog.Id,
                latitude + dLat * 180 / Math.PI,
                longitude + dLon * 180 / Math.PI,
                random.NextInt(1, 21),
                DateTimeOffset.FromUnixTimeSeconds((bucket + 1) * 900)));
        }

        return result;
    }

    private static DogDto PickDog(ref DeterministicRandom random, IReadOnlyList<DogDto> dogs)
    {
        var roll = random.NextDouble();
        var rarity = roll < 0.60 ? "Common" : roll < 0.87 ? "Rare" : roll < 0.98 ? "Epic" : "Legendary";
        var candidates = dogs.Where(x => string.Equals(x.Rarity, rarity, StringComparison.Ordinal)).ToArray();
        if (candidates.Length == 0) return dogs[random.NextInt(0, dogs.Count)];
        return candidates[random.NextInt(0, candidates.Length)];
    }

    private static uint BuildStableSeed(int worldSeed, long bucket, double latitude, double longitude)
    {
        unchecked
        {
            uint hash = 2166136261u;
            hash = Mix(hash, (uint)worldSeed);
            hash = Mix(hash, (uint)bucket);
            hash = Mix(hash, (uint)(bucket >> 32));
            hash = Mix(hash, (uint)Math.Round((latitude + 90d) * 10000d));
            hash = Mix(hash, (uint)Math.Round((longitude + 180d) * 10000d));
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
}
