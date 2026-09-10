namespace NexusDogsGo.Api.Services;

public sealed record DogDto(string Id, string Name, string Breed, string Rarity, int Attack, int Defense, int Stamina, double CaptureDifficulty);

public sealed class DogCatalogService
{
    public IReadOnlyList<DogDto> All { get; } = new[]
    {
        new DogDto("luna-husky", "Luna", "Siberian Husky", "Rare", 58, 53, 61, 1.15),
        new DogDto("max-rottweiler", "Max", "Rottweiler", "Epic", 72, 68, 70, 1.45),
        new DogDto("thor-german-shepherd", "Thor", "German Shepherd", "Rare", 66, 62, 66, 1.25),
        new DogDto("mel-pitbull", "Mel", "American Pit Bull Terrier", "Epic", 70, 60, 67, 1.40),
        new DogDto("rocky-beagle", "Rocky", "Beagle", "Common", 46, 44, 55, 0.90),
        new DogDto("shadow-shepherd", "Shadow", "Belgian Shepherd", "Legendary", 82, 76, 78, 1.85)
    };
}
