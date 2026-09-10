using System;
using System.Collections.Generic;
using System.Threading;
using NexusDogsGo.Core;
using NexusDogsGo.Services;
using NexusDogsGo.UI.Navigation;
using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class MapExplorationController : MonoBehaviour
    {
        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private MonoBehaviour mapServiceBehaviour;
        [SerializeField, Min(1f)] private float refreshIntervalSeconds = 5f;
        [SerializeField, Min(10f)] private float respawnDistanceMeters = 35f;
        [SerializeField, Range(1, 25)] private int nearbySpawnCount = 8;
        [SerializeField, Min(50f)] private float spawnRadiusMeters = 450f;
        [SerializeField] private float mapZoom = 16f;

        private CancellationTokenSource _lifetime;
        private IMapService _mapService;
        private GeoCoordinate _lastSpawnOrigin;
        private bool _hasSpawnOrigin;
        private float _nextRefreshTime;
        private IReadOnlyList<DogSpawn> _currentSpawns = Array.Empty<DogSpawn>();

        public bool LocationReady { get; private set; }
        public GeoCoordinate PlayerCoordinate { get; private set; }
        public IReadOnlyList<DogSpawn> CurrentSpawns => _currentSpawns;
        public DogSpawn SelectedSpawn { get; private set; }

        public event Action<GeoCoordinate> LocationChanged;
        public event Action<IReadOnlyList<DogSpawn>> SpawnsChanged;
        public event Action<DogSpawn> SpawnSelected;
        public event Action<string> StatusChanged;

        private void Awake()
        {
            _mapService = mapServiceBehaviour as IMapService;
            if (mapServiceBehaviour != null && _mapService == null)
                Debug.LogError(mapServiceBehaviour.name + " must implement IMapService.", mapServiceBehaviour);
        }

        private void OnEnable()
        {
            _lifetime = new CancellationTokenSource();
            BeginLocation(_lifetime.Token);
        }

        private void OnDisable()
        {
            if (_lifetime == null) return;
            _lifetime.Cancel();
            _lifetime.Dispose();
            _lifetime = null;
        }

        private async void BeginLocation(CancellationToken cancellationToken)
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                SetStatus("GameBootstrap indisponível.");
                return;
            }

            try
            {
                SetStatus("A obter localização...");
                LocationReady = await bootstrap.Location.StartAsync(cancellationToken);
                if (!LocationReady)
                {
                    SetStatus("Localização indisponível. Ativa o GPS e a permissão de localização.");
                    return;
                }

                SetStatus("GPS ativo.");
                RefreshLocation(forceSpawns: true);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                LocationReady = false;
                SetStatus("Falha ao iniciar localização.");
                Debug.LogException(exception, this);
            }
        }

        private void Update()
        {
            if (!LocationReady || Time.unscaledTime < _nextRefreshTime) return;
            _nextRefreshTime = Time.unscaledTime + refreshIntervalSeconds;
            RefreshLocation(forceSpawns: false);
        }

        public void RefreshNow()
        {
            RefreshLocation(forceSpawns: true);
        }

        private void RefreshLocation(bool forceSpawns)
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null || !bootstrap.Location.TryGetLocation(out var coordinate)) return;

            PlayerCoordinate = coordinate;
            _mapService?.SetPlayerLocation(coordinate);
            LocationChanged?.Invoke(coordinate);

            var movedEnough = !_hasSpawnOrigin || GeoMath.DistanceMeters(_lastSpawnOrigin, coordinate) >= respawnDistanceMeters;
            var expired = _currentSpawns.Count == 0 || _currentSpawns[0].ExpiresAt <= DateTimeOffset.UtcNow;
            if (!forceSpawns && !movedEnough && !expired) return;

            _currentSpawns = bootstrap.Spawns.GenerateNearby(
                coordinate,
                DateTimeOffset.UtcNow,
                nearbySpawnCount,
                spawnRadiusMeters);

            _lastSpawnOrigin = coordinate;
            _hasSpawnOrigin = true;
            _mapService?.SetDogSpawns(_currentSpawns);
            _mapService?.Focus(coordinate, mapZoom);
            SpawnsChanged?.Invoke(_currentSpawns);
        }

        public void SelectFirstSpawn()
        {
            if (_currentSpawns.Count > 0) SelectSpawn(_currentSpawns[0]);
        }

        public void SelectSpawnById(string spawnId)
        {
            if (string.IsNullOrWhiteSpace(spawnId)) return;

            for (var i = 0; i < _currentSpawns.Count; i++)
            {
                var spawn = _currentSpawns[i];
                if (!string.Equals(spawn.SpawnId, spawnId, StringComparison.Ordinal)) continue;
                SelectSpawn(spawn);
                return;
            }
        }

        public void SelectSpawn(DogSpawn spawn)
        {
            if (spawn == null || spawn.Dog == null) return;
            SelectedSpawn = spawn;
            SpawnSelected?.Invoke(spawn);
            if (navigator != null) navigator.Show(ScreenId.Capture);
        }

        public void ClearSelectedSpawn()
        {
            SelectedSpawn = null;
        }

        private void SetStatus(string message)
        {
            StatusChanged?.Invoke(message);
            Debug.Log("[Map] " + message, this);
        }
    }
}
