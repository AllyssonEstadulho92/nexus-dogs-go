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

        private readonly HashSet<string> _consumedSpawnIds = new HashSet<string>(StringComparer.Ordinal);
        private CancellationTokenSource _lifetime;
        private IMapService _mapService;
        private GeoCoordinate _lastSpawnOrigin;
        private bool _hasSpawnOrigin;
        private float _nextRefreshTime;
        private List<DogSpawn> _currentSpawns = new List<DogSpawn>();

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

            var generated = bootstrap.Spawns.GenerateNearby(
                coordinate,
                DateTimeOffset.UtcNow,
                nearbySpawnCount,
                spawnRadiusMeters);

            _currentSpawns = new List<DogSpawn>(generated.Count);
            for (var i = 0; i < generated.Count; i++)
            {
                var spawn = generated[i];
                if (spawn != null && !_consumedSpawnIds.Contains(spawn.SpawnId)) _currentSpawns.Add(spawn);
            }

            _lastSpawnOrigin = coordinate;
            _hasSpawnOrigin = true;
            PublishSpawns();
            _mapService?.Focus(coordinate, mapZoom);
        }

        public void SelectFirstSpawn()
        {
            if (_currentSpawns.Count > 0) SelectSpawn(_currentSpawns[0]);
            else SetStatus("Não existem cães disponíveis neste ponto neste momento.");
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
            if (spawn == null || spawn.Dog == null || _consumedSpawnIds.Contains(spawn.SpawnId)) return;
            SelectedSpawn = spawn;
            SpawnSelected?.Invoke(spawn);
            if (navigator != null) navigator.Show(ScreenId.Capture);
        }

        public void ConsumeSelectedSpawn()
        {
            if (SelectedSpawn == null) return;
            _consumedSpawnIds.Add(SelectedSpawn.SpawnId);
            for (var i = _currentSpawns.Count - 1; i >= 0; i--)
            {
                if (string.Equals(_currentSpawns[i].SpawnId, SelectedSpawn.SpawnId, StringComparison.Ordinal))
                    _currentSpawns.RemoveAt(i);
            }
            SelectedSpawn = null;
            PublishSpawns();
        }

        public void ClearSelectedSpawn()
        {
            SelectedSpawn = null;
        }

        private void PublishSpawns()
        {
            _mapService?.SetDogSpawns(_currentSpawns);
            SpawnsChanged?.Invoke(_currentSpawns);
        }

        private void SetStatus(string message)
        {
            StatusChanged?.Invoke(message);
            Debug.Log("[Map] " + message, this);
        }
    }
}
