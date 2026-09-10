using System.Collections.Generic;
using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class WorldSpawnMarkerManager : MonoBehaviour
    {
        [SerializeField] private MapExplorationController exploration;
        [SerializeField] private Procedural3DMapRenderer mapRenderer;
        [SerializeField] private Transform markerRoot;
        [SerializeField] private GameObject markerPrefab;
        [SerializeField, Min(0.001f)] private float worldUnitsPerMeter = 0.04f;
        [SerializeField, Min(0f)] private float markerHeight = 0.08f;

        private readonly List<GameObject> _markers = new List<GameObject>();
        private GeoCoordinate _fallbackOrigin;
        private bool _hasFallbackOrigin;

        private void OnEnable()
        {
            if (exploration != null)
            {
                exploration.LocationChanged += HandleLocation;
                exploration.SpawnsChanged += HandleSpawns;
            }

            if (mapRenderer != null) mapRenderer.WorldRebuilt += HandleWorldRebuilt;
        }

        private void OnDisable()
        {
            if (exploration != null)
            {
                exploration.LocationChanged -= HandleLocation;
                exploration.SpawnsChanged -= HandleSpawns;
            }

            if (mapRenderer != null) mapRenderer.WorldRebuilt -= HandleWorldRebuilt;
        }

        private void HandleWorldRebuilt(GeoCoordinate origin)
        {
            _fallbackOrigin = origin;
            _hasFallbackOrigin = true;
            Reposition(exploration != null ? exploration.CurrentSpawns : null);
        }

        private void HandleLocation(GeoCoordinate coordinate)
        {
            if (mapRenderer == null || !mapRenderer.HasOrigin)
            {
                _fallbackOrigin = coordinate;
                _hasFallbackOrigin = true;
            }

            Reposition(exploration != null ? exploration.CurrentSpawns : null);
        }

        private void HandleSpawns(IReadOnlyList<DogSpawn> spawns)
        {
            Rebuild(spawns);
        }

        private void Rebuild(IReadOnlyList<DogSpawn> spawns)
        {
            Clear();
            if (markerPrefab == null || spawns == null || !CanProject()) return;

            var parent = markerRoot != null ? markerRoot : transform;
            for (var i = 0; i < spawns.Count; i++)
            {
                var spawn = spawns[i];
                if (spawn == null || spawn.Dog == null) continue;

                var marker = Instantiate(markerPrefab, parent);
                marker.name = "WildDog_" + spawn.SpawnId;
                marker.transform.localPosition = Project(spawn.Coordinate) + Vector3.up * markerHeight;
                marker.SetActive(true);

                var binding = marker.GetComponent<DogSpawnMarker>();
                if (binding != null) binding.Bind(spawn, exploration);

                var actor = marker.GetComponent<WildDogMapActor>();
                if (actor != null) actor.Bind(spawn);

                _markers.Add(marker);
            }
        }

        private void Reposition(IReadOnlyList<DogSpawn> spawns)
        {
            if (!CanProject() || spawns == null)
            {
                return;
            }

            if (_markers.Count != spawns.Count)
            {
                Rebuild(spawns);
                return;
            }

            for (var i = 0; i < _markers.Count; i++)
            {
                var marker = _markers[i];
                var spawn = spawns[i];
                if (marker == null || spawn == null) continue;
                marker.transform.localPosition = Project(spawn.Coordinate) + Vector3.up * markerHeight;
            }
        }

        private bool CanProject()
        {
            return (mapRenderer != null && mapRenderer.HasOrigin) || _hasFallbackOrigin;
        }

        private Vector3 Project(in GeoCoordinate coordinate)
        {
            if (mapRenderer != null && mapRenderer.HasOrigin) return mapRenderer.Project(coordinate);
            return GeoSceneProjection.ToWorldOffset(_fallbackOrigin, coordinate, worldUnitsPerMeter);
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
