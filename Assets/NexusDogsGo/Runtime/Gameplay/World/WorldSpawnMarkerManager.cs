using System.Collections.Generic;
using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class WorldSpawnMarkerManager : MonoBehaviour
    {
        [SerializeField] private MapExplorationController exploration;
        [SerializeField] private Transform markerRoot;
        [SerializeField] private GameObject markerPrefab;
        [SerializeField, Min(0.001f)] private float worldUnitsPerMeter = 0.04f;
        [SerializeField, Min(0f)] private float markerHeight = 0.25f;

        private readonly List<GameObject> _markers = new List<GameObject>();
        private GeoCoordinate _origin;
        private bool _hasOrigin;

        private void OnEnable()
        {
            if (exploration == null) return;
            exploration.LocationChanged += HandleLocation;
            exploration.SpawnsChanged += HandleSpawns;
        }

        private void OnDisable()
        {
            if (exploration == null) return;
            exploration.LocationChanged -= HandleLocation;
            exploration.SpawnsChanged -= HandleSpawns;
        }

        private void HandleLocation(GeoCoordinate coordinate)
        {
            _origin = coordinate;
            _hasOrigin = true;
            Reposition(exploration != null ? exploration.CurrentSpawns : null);
        }

        private void HandleSpawns(IReadOnlyList<DogSpawn> spawns)
        {
            Rebuild(spawns);
        }

        private void Rebuild(IReadOnlyList<DogSpawn> spawns)
        {
            Clear();
            if (!_hasOrigin || markerPrefab == null || spawns == null) return;

            var parent = markerRoot != null ? markerRoot : transform;
            for (var i = 0; i < spawns.Count; i++)
            {
                var spawn = spawns[i];
                if (spawn == null || spawn.Dog == null) continue;

                var marker = Instantiate(markerPrefab, parent);
                marker.name = "Spawn_" + spawn.SpawnId;
                marker.transform.localPosition = GeoSceneProjection.ToWorldOffset(_origin, spawn.Coordinate, worldUnitsPerMeter) + Vector3.up * markerHeight;

                var binding = marker.GetComponent<DogSpawnMarker>();
                if (binding != null) binding.Bind(spawn, exploration);
                _markers.Add(marker);
            }
        }

        private void Reposition(IReadOnlyList<DogSpawn> spawns)
        {
            if (!_hasOrigin || spawns == null || _markers.Count != spawns.Count)
            {
                Rebuild(spawns);
                return;
            }

            for (var i = 0; i < _markers.Count; i++)
            {
                var marker = _markers[i];
                var spawn = spawns[i];
                if (marker == null || spawn == null) continue;
                marker.transform.localPosition = GeoSceneProjection.ToWorldOffset(_origin, spawn.Coordinate, worldUnitsPerMeter) + Vector3.up * markerHeight;
            }
        }

        private void Clear()
        {
            for (var i = 0; i < _markers.Count; i++)
            {
                if (_markers[i] != null) Destroy(_markers[i]);
            }
            _markers.Clear();
        }
    }

    public sealed class DogSpawnMarker : MonoBehaviour
    {
        private DogSpawn _spawn;
        private MapExplorationController _exploration;

        public DogSpawn Spawn => _spawn;

        public void Bind(DogSpawn spawn, MapExplorationController exploration)
        {
            _spawn = spawn;
            _exploration = exploration;
        }

        public void Select()
        {
            if (_spawn != null && _exploration != null) _exploration.SelectSpawn(_spawn);
        }

        private void OnMouseUpAsButton()
        {
            Select();
        }
    }
}
