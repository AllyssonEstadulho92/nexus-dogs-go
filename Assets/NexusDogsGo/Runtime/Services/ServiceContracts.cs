using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexusDogsGo.Domain;
using NexusDogsGo.Gameplay.World;

namespace NexusDogsGo.Services;

public readonly struct AuthSession
{
    public readonly string UserId;
    public readonly string DisplayName;

    public AuthSession(string userId, string displayName)
    {
        UserId = userId;
        DisplayName = displayName;
    }
}

public interface IAuthService
{
    Task<AuthSession> SignInAsync(CancellationToken cancellationToken);
    Task SignOutAsync(CancellationToken cancellationToken);
}

public interface IDataStore
{
    Task<PlayerProfile?> LoadProfileAsync(string playerId, CancellationToken cancellationToken);
    Task SaveProfileAsync(PlayerProfile profile, CancellationToken cancellationToken);
}

public interface IMapService
{
    void SetPlayerLocation(in GeoCoordinate coordinate);
    void SetDogSpawns(IReadOnlyList<DogSpawn> spawns);
    void Focus(in GeoCoordinate coordinate, float zoom);
}

public interface IArService
{
    bool IsSupported { get; }
    Task<bool> StartAsync(CancellationToken cancellationToken);
    void Stop();
}

public interface INetworkService
{
    bool IsConnected { get; }
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
}
