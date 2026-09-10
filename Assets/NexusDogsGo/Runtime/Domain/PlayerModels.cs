using System;
using System.Collections.Generic;

namespace NexusDogsGo.Domain;

[Serializable]
public sealed class PlayerProfile
{
    public string PlayerId = string.Empty;
    public string DisplayName = "Treinador";
    public int Level = 1;
    public int Experience;
    public int Coins = 1250;
    public int Gems = 85;
    public double TotalDistanceMeters;
    public string ActiveCompanionInstanceId = string.Empty;
    public List<DogInstance> Dogs = new();
    public InventoryState Inventory = InventoryState.CreateStarter();
}

public enum InventoryItemId
{
    PokeBall,
    SuperBall,
    PremiumFood,
    HealPotion,
    Incense,
    Revive
}

[Serializable]
public sealed class InventoryEntry
{
    public InventoryItemId ItemId;
    public int Quantity;
}

[Serializable]
public sealed class InventoryState
{
    public List<InventoryEntry> Entries = new();

    public static InventoryState CreateStarter()
    {
        return new InventoryState
        {
            Entries = new List<InventoryEntry>
            {
                new() { ItemId = InventoryItemId.PokeBall, Quantity = 25 },
                new() { ItemId = InventoryItemId.SuperBall, Quantity = 5 },
                new() { ItemId = InventoryItemId.PremiumFood, Quantity = 3 },
                new() { ItemId = InventoryItemId.HealPotion, Quantity = 10 },
                new() { ItemId = InventoryItemId.Incense, Quantity = 2 },
                new() { ItemId = InventoryItemId.Revive, Quantity = 1 }
            }
        };
    }

    public int GetQuantity(InventoryItemId itemId)
    {
        var entry = Entries.Find(x => x.ItemId == itemId);
        return entry?.Quantity ?? 0;
    }

    public bool TryConsume(InventoryItemId itemId, int amount = 1)
    {
        if (amount <= 0) return true;
        var entry = Entries.Find(x => x.ItemId == itemId);
        if (entry == null || entry.Quantity < amount) return false;
        entry.Quantity -= amount;
        return true;
    }

    public void Add(InventoryItemId itemId, int amount)
    {
        if (amount <= 0) return;
        var entry = Entries.Find(x => x.ItemId == itemId);
        if (entry == null)
        {
            Entries.Add(new InventoryEntry { ItemId = itemId, Quantity = amount });
            return;
        }

        checked { entry.Quantity += amount; }
    }
}
