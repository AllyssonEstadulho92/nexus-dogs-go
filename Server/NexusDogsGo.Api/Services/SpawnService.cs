namespace NexusDogsGo.Api.Services;

public sealed record SpawnDto(string SpawnId, string DogId, double Latitude, double Longitude, int Level, DateTimeOffset ExpiresAt);

public sealed class SpawnService
{
    private const double EarthRadiusMeters = 6_371_000d;

    public IReadOnlyList<SpawnDto> Generate(double latitude, double longitude, int count, IReadOnlyList<DogDto> dogs)
    {
        var now = DateTimeOffset.UtcNow;
        var bucket = now.ToUnixTimeSeconds() / 900;
        var seed = HashCode.Combine(bucket, Math.Round(latitude, 3), Math.Round(longitude, 3));
        var random = new Random(seed);
        var result = new List<SpawnDto>(count);

        for (var i = 0; i < count; i++)
        {
            var angle = random.NextDouble() * Math.PI * 2;
            var distance = 35 + random.NextDouble() * 415;
            var north = Math.Cos(angle) * distance;
            var east = Math.Sin(angle) * distance;
            var dLat = north / EarthRadiusMeters;
            var dLon = east / (EarthRadiusMeters * Math.Cos(latitude * Math.PI / 180));
            var dog = dogs[random.Next(dogs.Count)];
            result.Add(new SpawnDto(
                $"{bucket}-{i}-{dog.Id}",
                dog.Id,
                latitude + dLat * 180 / Math.PI,
                longitude + dLon * 180 / Math.PI,
                random.Next(1, 21),
                DateTimeOffset.FromUnixTimeSeconds((bucket + 1) * 900)));
        }

        return result;
    }
}
