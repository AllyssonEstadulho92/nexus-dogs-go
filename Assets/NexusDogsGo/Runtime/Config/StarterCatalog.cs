using System.Collections.Generic;
using NexusDogsGo.Domain;

namespace NexusDogsGo.Config;

public static class StarterCatalog
{
    public static IReadOnlyList<DogDefinition> Dogs { get; } = new List<DogDefinition>
    {
        new() { Id = "luna-husky", DisplayName = "Luna", Breed = "Siberian Husky", Rarity = DogRarity.Rare, BaseAttack = 58, BaseDefense = 53, BaseStamina = 61, CaptureDifficulty = 1.15f },
        new() { Id = "max-rottweiler", DisplayName = "Max", Breed = "Rottweiler", Rarity = DogRarity.Epic, BaseAttack = 72, BaseDefense = 68, BaseStamina = 70, CaptureDifficulty = 1.45f },
        new() { Id = "thor-german-shepherd", DisplayName = "Thor", Breed = "German Shepherd", Rarity = DogRarity.Rare, BaseAttack = 66, BaseDefense = 62, BaseStamina = 66, CaptureDifficulty = 1.25f },
        new() { Id = "mel-pitbull", DisplayName = "Mel", Breed = "American Pit Bull Terrier", Rarity = DogRarity.Epic, BaseAttack = 70, BaseDefense = 60, BaseStamina = 67, CaptureDifficulty = 1.40f },
        new() { Id = "rocky-beagle", DisplayName = "Rocky", Breed = "Beagle", Rarity = DogRarity.Common, BaseAttack = 46, BaseDefense = 44, BaseStamina = 55, CaptureDifficulty = 0.90f },
        new() { Id = "shadow-shepherd", DisplayName = "Shadow", Breed = "Belgian Shepherd", Rarity = DogRarity.Legendary, BaseAttack = 82, BaseDefense = 76, BaseStamina = 78, CaptureDifficulty = 1.85f }
    };

    public static DogDefinition? Find(string id)
    {
        for (var i = 0; i < Dogs.Count; i++)
        {
            if (Dogs[i].Id == id) return Dogs[i];
        }
        return null;
    }
}
