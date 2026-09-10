using System;
using System.Threading;
using System.Threading.Tasks;
using NexusDogsGo.Domain;
using NexusDogsGo.Gameplay.Missions;
using NexusDogsGo.Gameplay.World;
using NexusDogsGo.Services;
using UnityEngine;

namespace NexusDogsGo.Core;

public sealed class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap? Instance { get; private set; }
    public PlayerProfile Profile { get; private set; } = new();
    public GameStateMachine StateMachine { get; } = new();
    public MissionTracker Missions { get; private set; } = new();
    public DogSpawnService Spawns { get; private set; } = new();
    public ILocationProvider Location { get; private set; } = new UnityLocationProvider();
    public IAuthService Auth { get; private set; } = new LocalGuestAuthService();
    public IDataStore DataStore { get; private set; } = null!;

    private CancellationTokenSource? _lifetime;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _lifetime = new CancellationTokenSource();
        DataStore = new JsonFileDataStore();

        try
        {
            await InitializeAsync(_lifetime.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var session = await Auth.SignInAsync(cancellationToken);
        Profile = await DataStore.LoadProfileAsync(session.UserId, cancellationToken) ?? new PlayerProfile
        {
            PlayerId = session.UserId,
            DisplayName = session.DisplayName
        };
        StateMachine.Set(GameState.Home);
    }

    public Task SaveAsync()
    {
        return DataStore.SaveProfileAsync(Profile, _lifetime?.Token ?? CancellationToken.None);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && !string.IsNullOrWhiteSpace(Profile.PlayerId)) _ = SaveAsync();
    }

    private void OnApplicationQuit()
    {
        if (!string.IsNullOrWhiteSpace(Profile.PlayerId)) _ = SaveAsync();
        Location.Stop();
        _lifetime?.Cancel();
        _lifetime?.Dispose();
        _lifetime = null;
    }
}
