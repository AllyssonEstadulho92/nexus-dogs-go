using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace NexusDogsGo.Gameplay.World;

public interface ILocationProvider
{
    bool IsRunning { get; }
    Task<bool> StartAsync(CancellationToken cancellationToken);
    bool TryGetLocation(out GeoCoordinate coordinate);
    void Stop();
}

public sealed class UnityLocationProvider : ILocationProvider
{
    public bool IsRunning => Input.location.status == LocationServiceStatus.Running;

    public async Task<bool> StartAsync(CancellationToken cancellationToken)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            var permissionWait = 0;
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && permissionWait < 80)
            {
                cancellationToken.ThrowIfCancellationRequested();
                permissionWait++;
                await Task.Delay(100, cancellationToken);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) return false;
        }
#endif
        if (!Input.location.isEnabledByUser) return false;
        Input.location.Start(5f, 5f);

        var attempts = 0;
        while (Input.location.status == LocationServiceStatus.Initializing && attempts < 100)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempts++;
            await Task.Delay(100, cancellationToken);
        }

        return Input.location.status == LocationServiceStatus.Running;
    }

    public bool TryGetLocation(out GeoCoordinate coordinate)
    {
        if (!IsRunning)
        {
            coordinate = default;
            return false;
        }

        var data = Input.location.lastData;
        coordinate = new GeoCoordinate(data.latitude, data.longitude);
        return true;
    }

    public void Stop()
    {
        if (Input.location.status == LocationServiceStatus.Running) Input.location.Stop();
    }
}
