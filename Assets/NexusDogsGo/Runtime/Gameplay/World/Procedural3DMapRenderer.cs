using System;
using System.Collections.Generic;
using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class Procedural3DMapRenderer : MonoBehaviour
    {
        [SerializeField] private MapExplorationController exploration;
        [SerializeField] private Transform environmentRoot;
        [SerializeField] private Transform playerMarker;
        [SerializeField, Min(0.001f)] private float worldUnitsPerMeter = 0.04f;
        [SerializeField, Range(5, 17)] private int gridSize = 11;
        [SerializeField, Min(20f)] private float blockSizeMeters = 54f;
        [SerializeField, Min(4f)] private float roadWidthMeters = 11f;
        [SerializeField, Min(5f)] private float rebuildDistanceMeters = 120f;
        [SerializeField, Min(4f)] private float maximumBuildingHeightMeters = 42f;

        private readonly List<GameObject> _generated = new List<GameObject>();
        private GeoCoordinate _origin;
        private bool _hasOrigin;
        private Material _groundMaterial;
        private Material _roadMaterial;
        private Material _buildingMaterial;
        private Material _buildingAccentMaterial;
        private Material _parkMaterial;
        private Material _poiMaterial;

        public GeoCoordinate Origin => _origin;
        public float WorldUnitsPerMeter => worldUnitsPerMeter;

        private void Awake()
        {
            EnsureMaterials();
        }

        private void OnEnable()
        {
            if (exploration == null) return;
            exploration.LocationChanged += HandleLocation;
            if (exploration.LocationReady) HandleLocation(exploration.PlayerCoordinate);
        }

        private void OnDisable()
        {
            if (exploration != null) exploration.LocationChanged -= HandleLocation;
        }

        private void HandleLocation(GeoCoordinate coordinate)
        {
            if (!_hasOrigin || GeoMath.DistanceMeters(_origin, coordinate) >= rebuildDistanceMeters)
            {
                _origin = coordinate;
                _hasOrigin = true;
                BuildEnvironment(coordinate);
            }

            if (playerMarker != null) playerMarker.localPosition = Vector3.up * 0.22f;
        }

        public Vector3 Project(in GeoCoordinate coordinate)
        {
            if (!_hasOrigin) return Vector3.zero;
            return GeoSceneProjection.ToWorldOffset(_origin, coordinate, worldUnitsPerMeter);
        }

        private void BuildEnvironment(GeoCoordinate coordinate)
        {
            ClearGenerated();
            EnsureMaterials();

            var parent = environmentRoot != null ? environmentRoot : transform;
            var size = Mathf.Max(5, gridSize);
            if (size % 2 == 0) size++;
            var half = size / 2;
            var blockWorld = blockSizeMeters * worldUnitsPerMeter;
            var roadWorld = roadWidthMeters * worldUnitsPerMeter;
            var cellWorld = blockWorld + roadWorld;
            var totalWorld = size * cellWorld;

            CreateBox(parent, "Ground", new Vector3(0f, -0.10f, 0f), new Vector3(totalWorld + cellWorld, 0.16f, totalWorld + cellWorld), _groundMaterial);

            for (var i = -half; i <= half; i++)
            {
                var offset = i * cellWorld;
                CreateBox(parent, "Road_NS_" + i, new Vector3(offset, 0f, 0f), new Vector3(roadWorld, 0.035f, totalWorld + cellWorld), _roadMaterial);
                CreateBox(parent, "Road_EW_" + i, new Vector3(0f, 0.002f, offset), new Vector3(totalWorld + cellWorld, 0.035f, roadWorld), _roadMaterial);
            }

            var seed = StableSeed(coordinate);
            var random = new MapRandom(seed);
            for (var x = -half; x < half; x++)
            {
                for (var z = -half; z < half; z++)
                {
                    var center = new Vector3((x + 0.5f) * cellWorld, 0f, (z + 0.5f) * cellWorld);
                    var roll = random.Next01();
                    if (roll < 0.18f)
                    {
                        CreatePark(parent, center, blockWorld, ref random);
                    }
                    else
                    {
                        CreateCityBlock(parent, center, blockWorld, ref random);
                    }
                }
            }

            CreatePoi(parent, new Vector3(cellWorld * 1.7f, 0f, cellWorld * 0.9f), 1.4f, "POI_Park");
            CreatePoi(parent, new Vector3(-cellWorld * 1.8f, 0f, -cellWorld * 0.7f), 1.9f, "POI_Landmark");
        }

        private void CreateCityBlock(Transform parent, Vector3 center, float blockWorld, ref MapRandom random)
        {
            var margin = blockWorld * 0.08f;
            var lot = (blockWorld - margin * 3f) * 0.5f;
            for (var ix = 0; ix < 2; ix++)
            {
                for (var iz = 0; iz < 2; iz++)
                {
                    if (random.Next01() < 0.16f) continue;
                    var width = lot * Mathf.Lerp(0.60f, 0.95f, random.Next01());
                    var depth = lot * Mathf.Lerp(0.60f, 0.95f, random.Next01());
                    var heightMeters = Mathf.Lerp(7f, maximumBuildingHeightMeters, Mathf.Pow(random.Next01(), 1.55f));
                    var height = heightMeters * worldUnitsPerMeter;
                    var localX = (ix == 0 ? -1f : 1f) * (lot * 0.5f + margin * 0.5f);
                    var localZ = (iz == 0 ? -1f : 1f) * (lot * 0.5f + margin * 0.5f);
                    var position = center + new Vector3(localX, height * 0.5f, localZ);
                    var material = random.Next01() > 0.74f ? _buildingAccentMaterial : _buildingMaterial;
                    CreateBox(parent, "Building", position, new Vector3(width, height, depth), material);

                    if (height > 0.55f && random.Next01() > 0.55f)
                    {
                        var roofSize = Mathf.Min(width, depth) * 0.34f;
                        CreateBox(parent, "Roof", position + Vector3.up * (height * 0.5f + 0.05f), new Vector3(roofSize, 0.08f, roofSize), _roadMaterial);
                    }
                }
            }
        }

        private void CreatePark(Transform parent, Vector3 center, float blockWorld, ref MapRandom random)
        {
            CreateBox(parent, "Park", center + Vector3.up * 0.015f, new Vector3(blockWorld * 0.92f, 0.04f, blockWorld * 0.92f), _parkMaterial);
            var treeCount = 3 + (int)(random.Next01() * 6f);
            for (var i = 0; i < treeCount; i++)
            {
                var x = Mathf.Lerp(-blockWorld * 0.38f, blockWorld * 0.38f, random.Next01());
                var z = Mathf.Lerp(-blockWorld * 0.38f, blockWorld * 0.38f, random.Next01());
                var trunkHeight = Mathf.Lerp(0.12f, 0.24f, random.Next01());
                CreateBox(parent, "TreeTrunk", center + new Vector3(x, trunkHeight * 0.5f + 0.04f, z), new Vector3(0.055f, trunkHeight, 0.055f), _buildingAccentMaterial);
                var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.name = "TreeCrown";
                crown.transform.SetParent(parent, false);
                crown.transform.localPosition = center + new Vector3(x, trunkHeight + 0.14f, z);
                crown.transform.localScale = Vector3.one * Mathf.Lerp(0.18f, 0.30f, random.Next01());
                ApplyMaterial(crown, _parkMaterial);
                RemoveCollider(crown);
                _generated.Add(crown);
            }
        }

        private void CreatePoi(Transform parent, Vector3 position, float height, string objectName)
        {
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = objectName;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = position + Vector3.up * height * 0.5f;
            cylinder.transform.localScale = new Vector3(0.20f, height * 0.5f, 0.20f);
            ApplyMaterial(cylinder, _poiMaterial);
            RemoveCollider(cylinder);
            _generated.Add(cylinder);

            var beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beacon.name = objectName + "_Beacon";
            beacon.transform.SetParent(parent, false);
            beacon.transform.localPosition = position + Vector3.up * (height + 0.18f);
            beacon.transform.localScale = Vector3.one * 0.34f;
            ApplyMaterial(beacon, _poiMaterial);
            RemoveCollider(beacon);
            _generated.Add(beacon);
        }

        private void CreateBox(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            ApplyMaterial(box, material);
            RemoveCollider(box);
            _generated.Add(box);
        }

        private static void ApplyMaterial(GameObject obj, Material material)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null && material != null) renderer.sharedMaterial = material;
        }

        private static void RemoveCollider(GameObject obj)
        {
            var collider = obj.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private void EnsureMaterials()
        {
            if (_groundMaterial != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            _groundMaterial = CreateMaterial(shader, new Color(0.045f, 0.075f, 0.095f, 1f));
            _roadMaterial = CreateMaterial(shader, new Color(0.085f, 0.12f, 0.14f, 1f));
            _buildingMaterial = CreateMaterial(shader, new Color(0.17f, 0.25f, 0.29f, 1f));
            _buildingAccentMaterial = CreateMaterial(shader, new Color(0.10f, 0.38f, 0.43f, 1f));
            _parkMaterial = CreateMaterial(shader, new Color(0.10f, 0.30f, 0.22f, 1f));
            _poiMaterial = CreateMaterial(shader, new Color(0.10f, 0.82f, 0.90f, 1f));
        }

        private static Material CreateMaterial(Shader shader, Color color)
        {
            var material = new Material(shader);
            material.color = color;
            return material;
        }

        private void ClearGenerated()
        {
            for (var i = 0; i < _generated.Count; i++)
            {
                if (_generated[i] != null) Destroy(_generated[i]);
            }
            _generated.Clear();
        }

        private static uint StableSeed(in GeoCoordinate coordinate)
        {
            unchecked
            {
                var lat = (uint)Math.Round((coordinate.Latitude + 90d) * 1000d);
                var lon = (uint)Math.Round((coordinate.Longitude + 180d) * 1000d);
                uint hash = 2166136261u;
                hash = (hash ^ lat) * 16777619u;
                hash = (hash ^ lon) * 16777619u;
                return hash;
            }
        }

        private struct MapRandom
        {
            private uint _state;

            public MapRandom(uint seed)
            {
                _state = seed == 0u ? 0xA341316Cu : seed;
            }

            public float Next01()
            {
                var x = _state;
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;
                _state = x;
                return (x & 0x00FFFFFFu) / 16777216f;
            }
        }
    }
}
