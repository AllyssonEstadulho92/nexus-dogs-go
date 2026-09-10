using System;
using System.Collections.Generic;
using UnityEngine;

namespace NexusDogsGo.UI.Navigation;

public enum ScreenId
{
    Home,
    DogSelect,
    Map,
    Capture,
    Social,
    Dogs,
    Missions,
    Battle,
    Inventory,
    Events,
    Ar
}

[Serializable]
public sealed class ScreenBinding
{
    public ScreenId Id;
    public GameObject Root = null!;
}

public sealed class ScreenNavigator : MonoBehaviour
{
    [SerializeField] private ScreenId initialScreen = ScreenId.Home;
    [SerializeField] private List<ScreenBinding> screens = new();

    public ScreenId Current { get; private set; }
    public event Action<ScreenId>? Changed;

    private void Start() => Show(initialScreen);

    public void Show(ScreenId id)
    {
        var found = false;
        foreach (var screen in screens)
        {
            if (screen.Root == null) continue;
            var active = screen.Id == id;
            screen.Root.SetActive(active);
            found |= active;
        }

        if (!found)
        {
            Debug.LogWarning($"Screen {id} is not registered in {name}.");
            return;
        }

        Current = id;
        Changed?.Invoke(id);
    }

    public void ShowMap() => Show(ScreenId.Map);
    public void ShowDogs() => Show(ScreenId.Dogs);
    public void ShowMissions() => Show(ScreenId.Missions);
    public void ShowInventory() => Show(ScreenId.Inventory);
    public void ShowEvents() => Show(ScreenId.Events);
}
