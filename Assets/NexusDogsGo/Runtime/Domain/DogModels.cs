using System;

namespace NexusDogsGo.Domain;

public enum DogRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[Serializable]
public sealed class DogDefinition
{
    public string Id = string.Empty;
    public string DisplayName = string.Empty;
    public string Breed = string.Empty;
    public DogRarity Rarity;
    public int BaseAttack;
    public int BaseDefense;
    public int BaseStamina;
    public float CaptureDifficulty = 1f;

    public int CalculateCp(int level)
    {
        level = Math.Max(1, level);
        return Math.Max(10, (BaseAttack * 2 + BaseDefense + BaseStamina) * level / 10);
    }
}

[Serializable]
public sealed class DogInstance
{
    public string InstanceId = Guid.NewGuid().ToString("N");
    public string DefinitionId = string.Empty;
    public int Level = 1;
    public int Experience;
    public int CurrentHp = 100;
    public bool IsFavorite;
}
