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
        [SerializeField, Range(7, 15)] private int gridSize = 11;
        [SerializeField, Min(20f)] private float blockSizeMeters = 54f;
        [SerializeField, Min(4f)] private float roadWidthMeters = 11f;
        [SerializeField, Min(40f)] private float rebuildDistanceMeters = 120f;
        [SerializeField, Min(4f)] private float maximumBuildingHeightMeters = 46f;
        [SerializeField, Min(1f)] private float playerMoveSmoothness = 7.5f;

        private readonly List<GameObject> _generated = new List<GameObject>();
        private GeoCoordinate _origin;
        private bool _hasOrigin;
        private Vector3 _desiredPlayerPosition;
        private Vector3 _playerVelocity;
        private Vector3 _previousPlayerPosition;

        private Material _groundMaterial;
        private Material _roadMaterial;
        private Material _laneMaterial;
        private Material _sidewalkMaterial;
        private Material _buildingMaterial;
        private Material _buildingAccentMaterial;
        private Material _roofMaterial;
        private Material _windowMaterial;
        private Material _parkMaterial;
        private Material _pathMaterial;
        private Material _waterMaterial;
        private Material _woodMaterial;
        private Material _metalMaterial;
        private Material _poiMaterial;
        private Material _warmGlowMaterial;

        public GeoCoordinate Origin => _origin;
        public float WorldUnitsPerMeter => worldUnitsPerMeter;
        public bool HasOrigin => _hasOrigin;

        public event Action<GeoCoordinate> WorldRebuilt;

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

        private void Update()
        {
            if (playerMarker == null || !_hasOrigin) return;

            var before = playerMarker.localPosition;
            playerMarker.localPosition = Vector3.SmoothDamp(
                before,
                _desiredPlayerPosition,
                ref _playerVelocity,
                1f / Mathf.Max(1f, playerMoveSmoothness));

            var movement = playerMarker.localPosition - _previousPlayerPosition;
            movement.y = 0f;
            if (movement.sqrMagnitude > 0.00002f)
            {
                var desiredRotation = Quaternion.LookRotation(movement.normalized, Vector3.up);
                playerMarker.localRotation = Quaternion.Slerp(
                    playerMarker.localRotation,
                    desiredRotation,
                    Time.deltaTime * 8f);
            }

            _previousPlayerPosition = playerMarker.localPosition;
        }

        private void HandleLocation(GeoCoordinate coordinate)
        {
            var rebuilt = false;
            if (!_hasOrigin || GeoMath.DistanceMeters(_origin, coordinate) >= rebuildDistanceMeters)
            {
                _origin = coordinate;
                _hasOrigin = true;
                BuildEnvironment(coordinate);
                rebuilt = true;
            }

            _desiredPlayerPosition = Project(coordinate) + Vector3.up * 0.10f;
            if (playerMarker != null && rebuilt)
            {
                playerMarker.localPosition = _desiredPlayerPosition;
                _previousPlayerPosition = _desiredPlayerPosition;
                _playerVelocity = Vector3.zero;
            }

            if (rebuilt)
            {
                var handler = WorldRebuilt;
                if (handler != null) handler(_origin);
            }
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
            var size = Mathf.Max(7, gridSize);
            if (size % 2 == 0) size++;
            var half = size / 2;
            var blockWorld = blockSizeMeters * worldUnitsPerMeter;
            var roadWorld = roadWidthMeters * worldUnitsPerMeter;
            var cellWorld = blockWorld + roadWorld;
            var totalWorld = size * cellWorld;

            CreateBox(parent, "Ground", new Vector3(0f, -0.14f, 0f), new Vector3(totalWorld + cellWorld, 0.22f, totalWorld + cellWorld), _groundMaterial);
            BuildRoadNetwork(parent, size, half, blockWorld, roadWorld, cellWorld, totalWorld);

            var random = new MapRandom(StableSeed(coordinate));
            for (var x = -half; x < half; x++)
            {
                for (var z = -half; z < half; z++)
                {
                    var center = new Vector3((x + 0.5f) * cellWorld, 0f, (z + 0.5f) * cellWorld);
                    var roll = random.Next01();
                    if (roll < 0.15f)
                    {
                        CreatePark(parent, center, blockWorld, ref random);
                    }
                    else if (roll < 0.22f)
                    {
                        CreatePlaza(parent, center, blockWorld, ref random);
                    }
                    else if (roll < 0.27f)
                    {
                        CreateWaterBlock(parent, center, blockWorld, ref random);
                    }
                    else
                    {
                        CreateCityBlock(parent, center, blockWorld, ref random);
                    }

                    if (((x + z) & 1) == 0) CreateStreetFurniture(parent, center, blockWorld, ref random);
                }
            }

            CreateCrosswalks(parent, roadWorld, cellWorld);
            CreatePoiHub(parent, new Vector3(cellWorld * 1.7f, 0f, cellWorld * 0.9f), "Nexus_Hub", 1.50f);
            CreatePoiHub(parent, new Vector3(-cellWorld * 1.8f, 0f, -cellWorld * 0.7f), "Challenge_Arena", 1.95f);
            CreatePoiHub(parent, new Vector3(cellWorld * 0.1f, 0f, -cellWorld * 2.0f), "Dog_Park_Hub", 1.30f);
        }

        private void BuildRoadNetwork(Transform parent, int size, int half, float blockWorld, float roadWorld, float cellWorld, float totalWorld)
        {
            for (var i = -half; i <= half; i++)
            {
                var offset = i * cellWorld;
                CreateBox(parent, "Road_NS_" + i, new Vector3(offset, 0f, 0f), new Vector3(roadWorld, 0.040f, totalWorld + cellWorld), _roadMaterial);
                CreateBox(parent, "Road_EW_" + i, new Vector3(0f, 0.002f, offset), new Vector3(totalWorld + cellWorld, 0.040f, roadWorld), _roadMaterial);

                var dashCount = Mathf.Max(4, size * 3);
                var dashSpacing = (totalWorld + cellWorld) / dashCount;
                for (var d = 0; d < dashCount; d++)
                {
                    if ((d & 1) == 1) continue;
                    var along = -totalWorld * 0.5f + d * dashSpacing;
                    CreateBox(parent, "Lane_NS", new Vector3(offset, 0.026f, along), new Vector3(0.024f, 0.012f, dashSpacing * 0.48f), _laneMaterial);
                    CreateBox(parent, "Lane_EW", new Vector3(along, 0.028f, offset), new Vector3(dashSpacing * 0.48f, 0.012f, 0.024f), _laneMaterial);
                }
            }
        }

        private void CreateCityBlock(Transform parent, Vector3 center, float blockWorld, ref MapRandom random)
        {
            CreateBox(parent, "Sidewalk", center + Vector3.up * 0.032f, new Vector3(blockWorld * 0.98f, 0.065f, blockWorld * 0.98f), _sidewalkMaterial);

            var margin = blockWorld * 0.075f;
            var lot = (blockWorld - margin * 3f) * 0.5f;
            for (var ix = 0; ix < 2; ix++)
            {
                for (var iz = 0; iz < 2; iz++)
                {
                    if (random.Next01() < 0.12f) continue;

                    var width = lot * Mathf.Lerp(0.62f, 0.95f, random.Next01());
                    var depth = lot * Mathf.Lerp(0.62f, 0.95f, random.Next01());
                    var heightMeters = Mathf.Lerp(7f, maximumBuildingHeightMeters, Mathf.Pow(random.Next01(), 1.48f));
                    var height = heightMeters * worldUnitsPerMeter;
                    var localX = (ix == 0 ? -1f : 1f) * (lot * 0.5f + margin * 0.5f);
                    var localZ = (iz == 0 ? -1f : 1f) * (lot * 0.5f + margin * 0.5f);
                    var position = center + new Vector3(localX, height * 0.5f + 0.075f, localZ);
                    var material = random.Next01() > 0.70f ? _buildingAccentMaterial : _buildingMaterial;

                    CreateBox(parent, "Building", position, new Vector3(width, height, depth), material);
                    CreateBuildingDetails(parent, position, width, height, depth, ref random);
                }
            }
        }

        private void CreateBuildingDetails(Transform parent, Vector3 position, float width, float height, float depth, ref MapRandom random)
        {
            var roofHeight = Mathf.Clamp(height * 0.08f, 0.035f, 0.11f);
            var roofWidth = width * Mathf.Lerp(0.30f, 0.62f, random.Next01());
            var roofDepth = depth * Mathf.Lerp(0.30f, 0.62f, random.Next01());
            CreateBox(parent, "RoofUnit", position + Vector3.up * (height * 0.5f + roofHeight * 0.5f), new Vector3(roofWidth, roofHeight, roofDepth), _roofMaterial);

            var doorWidth = Mathf.Min(width * 0.18f, 0.18f);
            var doorHeight = Mathf.Min(height * 0.22f, 0.22f);
            CreateBox(parent, "Entrance", position + new Vector3(0f, -height * 0.5f + doorHeight * 0.5f + 0.004f, depth * 0.5f + 0.006f), new Vector3(doorWidth, doorHeight, 0.014f), _metalMaterial);

            if (height < 0.30f) return;
            var bands = height > 1.05f ? 3 : 2;
            for (var floor = 0; floor < bands; floor++)
            {
                var y = position.y - height * 0.26f + floor * (height * 0.22f);
                CreateBox(parent, "WindowBandFront", new Vector3(position.x, y, position.z + depth * 0.5f + 0.006f), new Vector3(width * 0.66f, Mathf.Min(0.045f, height * 0.08f), 0.012f), _windowMaterial);
                CreateBox(parent, "WindowBandSide", new Vector3(position.x + width * 0.5f + 0.006f, y, position.z), new Vector3(0.012f, Mathf.Min(0.045f, height * 0.08f), depth * 0.66f), _windowMaterial);
            }
        }

        private void CreatePark(Transform parent, Vector3 center, float blockWorld, ref MapRandom random)
        {
            CreateBox(parent, "Park", center + Vector3.up * 0.025f, new Vector3(blockWorld * 0.96f, 0.052f, blockWorld * 0.96f), _parkMaterial);
            CreateBox(parent, "ParkPath_NS", center + Vector3.up * 0.057f, new Vector3(blockWorld * 0.15f, 0.012f, blockWorld * 0.86f), _pathMaterial);
            CreateBox(parent, "ParkPath_EW", center + Vector3.up * 0.059f, new Vector3(blockWorld * 0.86f, 0.012f, blockWorld * 0.15f), _pathMaterial);

            var treeCount = 6 + (int)(random.Next01() * 7f);
            for (var i = 0; i < treeCount; i++)
            {
                var x = Mathf.Lerp(-blockWorld * 0.40f, blockWorld * 0.40f, random.Next01());
                var z = Mathf.Lerp(-blockWorld * 0.40f, blockWorld * 0.40f, random.Next01());
                if (Mathf.Abs(x) < blockWorld * 0.12f || Mathf.Abs(z) < blockWorld * 0.12f) continue;
                CreateTree(parent, center + new Vector3(x, 0f, z), ref random);
            }

            var fountain = CreateCylinder(parent, "ParkFountain", center + Vector3.up * 0.08f, new Vector3(0.32f, 0.08f, 0.32f), _sidewalkMaterial);
            var water = CreateCylinder(parent, "ParkFountainWater", center + Vector3.up * 0.135f, new Vector3(0.25f, 0.016f, 0.25f), _waterMaterial);
            fountain.transform.localRotation = Quaternion.identity;
            water.transform.localRotation = Quaternion.identity;

            CreateBench(parent, center + new Vector3(blockWorld * 0.27f, 0f, blockWorld * 0.18f), Quaternion.Euler(0f, 90f, 0f));
            CreateBench(parent, center + new Vector3(-blockWorld * 0.25f, 0f, -blockWorld * 0.20f), Quaternion.identity);
        }

        private void CreatePlaza(Transform parent, Vector3 center, float blockWorld, ref MapRandom random)
        {
            CreateBox(parent, "Plaza", center + Vector3.up * 0.030f, new Vector3(blockWorld * 0.96f, 0.060f, blockWorld * 0.96f), _pathMaterial);

            var monumentHeight = Mathf.Lerp(0.55f, 1.10f, random.Next01());
            CreateCylinder(parent, "PlazaMonumentBase", center + Vector3.up * 0.10f, new Vector3(0.40f, 0.10f, 0.40f), _sidewalkMaterial);
            CreateCylinder(parent, "PlazaMonument", center + Vector3.up * (monumentHeight * 0.5f + 0.16f), new Vector3(0.10f, monumentHeight * 0.5f, 0.10f), _metalMaterial);
            CreateSphere(parent, "PlazaMonumentTop", center + Vector3.up * (monumentHeight + 0.22f), Vector3.one * 0.22f, _poiMaterial);

            for (var i = 0; i < 4; i++)
            {
                var angle = i * Mathf.PI * 0.5f;
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * blockWorld * 0.33f;
                CreateTree(parent, center + offset, ref random);
            }
        }

        private void CreateWaterBlock(Transform parent, Vector3 center, float blockWorld, ref MapRandom random)
        {
            CreateBox(parent, "Promenade", center + Vector3.up * 0.018f, new Vector3(blockWorld * 0.98f, 0.040f, blockWorld * 0.98f), _sidewalkMaterial);
            CreateBox(parent, "Water", center + Vector3.up * 0.042f, new Vector3(blockWorld * 0.76f, 0.018f, blockWorld * 0.76f), _waterMaterial);

            CreateBench(parent, center + new Vector3(blockWorld * 0.39f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f));
            CreateBench(parent, center + new Vector3(-blockWorld * 0.39f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
            if (random.Next01() > 0.45f)
                CreateTree(parent, center + new Vector3(0f, 0f, blockWorld * 0.40f), ref random);
        }

        private void CreateStreetFurniture(Transform parent, Vector3 center, float blockWorld, ref MapRandom random)
        {
            var corner = blockWorld * 0.46f;
            CreateStreetLamp(parent, center + new Vector3(corner, 0f, corner));
            if (random.Next01() > 0.35f) CreateStreetLamp(parent, center + new Vector3(-corner, 0f, -corner));
        }

        private void CreateStreetLamp(Transform parent, Vector3 position)
        {
            CreateCylinder(parent, "StreetLampPole", position + Vector3.up * 0.30f, new Vector3(0.022f, 0.30f, 0.022f), _metalMaterial);
            CreateSphere(parent, "StreetLampGlow", position + Vector3.up * 0.62f, Vector3.one * 0.065f, _warmGlowMaterial);
        }

        private void CreateTree(Transform parent, Vector3 position, ref MapRandom random)
        {
            var trunkHeight = Mathf.Lerp(0.16f, 0.30f, random.Next01());
            CreateCylinder(parent, "TreeTrunk", position + Vector3.up * (trunkHeight * 0.5f + 0.05f), new Vector3(0.040f, trunkHeight * 0.5f, 0.040f), _woodMaterial);
            var crownScale = Mathf.Lerp(0.22f, 0.38f, random.Next01());
            var crown = CreateSphere(parent, "TreeCrown", position + Vector3.up * (trunkHeight + 0.18f), new Vector3(crownScale, crownScale * 1.15f, crownScale), _parkMaterial);
            crown.transform.localRotation = Quaternion.Euler(0f, random.Next01() * 360f, 0f);
        }

        private void CreateBench(Transform parent, Vector3 position, Quaternion rotation)
        {
            var seat = CreateBox(parent, "BenchSeat", position + Vector3.up * 0.10f, new Vector3(0.36f, 0.045f, 0.12f), _woodMaterial);
            seat.transform.localRotation = rotation;
            var back = CreateBox(parent, "BenchBack", position + Vector3.up * 0.19f + rotation * new Vector3(0f, 0f, 0.045f), new Vector3(0.36f, 0.16f, 0.035f), _woodMaterial);
            back.transform.localRotation = rotation;
        }

        private void CreateCrosswalks(Transform parent, float roadWorld, float cellWorld)
        {
            var stripeWidth = roadWorld * 0.11f;
            for (var i = -3; i <= 3; i++)
            {
                var offset = i * stripeWidth * 1.55f;
                CreateBox(parent, "Crosswalk_NS", new Vector3(offset, 0.034f, roadWorld * 0.68f), new Vector3(stripeWidth, 0.012f, roadWorld * 0.34f), _laneMaterial);
                CreateBox(parent, "Crosswalk_EW", new Vector3(roadWorld * 0.68f, 0.036f, offset), new Vector3(roadWorld * 0.34f, 0.012f, stripeWidth), _laneMaterial);
            }

            var ringSize = Mathf.Max(0.04f, roadWorld * 0.10f);
            CreateCylinder(parent, "CentralCompass", new Vector3(0f, 0.040f, 0f), new Vector3(ringSize, 0.010f, ringSize), _poiMaterial);
        }

        private void CreatePoiHub(Transform parent, Vector3 position, string objectName, float height)
        {
            CreateCylinder(parent, objectName + "_Base", position + Vector3.up * 0.06f, new Vector3(0.54f, 0.06f, 0.54f), _sidewalkMaterial);
            CreateCylinder(parent, objectName + "_Core", position + Vector3.up * (height * 0.5f + 0.10f), new Vector3(0.12f, height * 0.5f, 0.12f), _poiMaterial);
            CreateSphere(parent, objectName + "_Beacon", position + Vector3.up * (height + 0.22f), Vector3.one * 0.34f, _poiMaterial);

            for (var i = 0; i < 8; i++)
            {
                var angle = i / 8f * Mathf.PI * 2f;
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.50f;
                CreateBox(parent, objectName + "_Ring", position + offset + Vector3.up * 0.10f, new Vector3(0.11f, 0.035f, 0.11f), _warmGlowMaterial);
            }
        }

        private GameObject CreateBox(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            ApplyMaterial(box, material);
            RemoveCollider(box);
            _generated.Add(box);
            return box;
        }

        private GameObject CreateSphere(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = objectName;
            sphere.transform.SetParent(parent, false);
            sphere.transform.localPosition = position;
            sphere.transform.localScale = scale;
            ApplyMaterial(sphere, material);
            RemoveCollider(sphere);
            _generated.Add(sphere);
            return sphere;
        }

        private GameObject CreateCylinder(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material)
        {
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = objectName;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = position;
            cylinder.transform.localScale = scale;
            ApplyMaterial(cylinder, material);
            RemoveCollider(cylinder);
            _generated.Add(cylinder);
            return cylinder;
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

            _groundMaterial = CreateMaterial(shader, new Color(0.075f, 0.10f, 0.095f, 1f), 0.02f, 0.12f);
            _roadMaterial = CreateMaterial(shader, new Color(0.105f, 0.12f, 0.125f, 1f), 0.02f, 0.22f);
            _laneMaterial = CreateMaterial(shader, new Color(0.84f, 0.82f, 0.70f, 1f), 0.00f, 0.28f);
            _sidewalkMaterial = CreateMaterial(shader, new Color(0.43f, 0.45f, 0.43f, 1f), 0.02f, 0.24f);
            _buildingMaterial = CreateMaterial(shader, new Color(0.34f, 0.39f, 0.40f, 1f), 0.05f, 0.22f);
            _buildingAccentMaterial = CreateMaterial(shader, new Color(0.24f, 0.43f, 0.46f, 1f), 0.05f, 0.28f);
            _roofMaterial = CreateMaterial(shader, new Color(0.18f, 0.22f, 0.23f, 1f), 0.12f, 0.30f);
            _windowMaterial = CreateMaterial(shader, new Color(0.18f, 0.46f, 0.60f, 1f), 0.24f, 0.72f);
            _parkMaterial = CreateMaterial(shader, new Color(0.18f, 0.48f, 0.25f, 1f), 0.00f, 0.12f);
            _pathMaterial = CreateMaterial(shader, new Color(0.62f, 0.58f, 0.48f, 1f), 0.00f, 0.16f);
            _waterMaterial = CreateMaterial(shader, new Color(0.08f, 0.42f, 0.62f, 1f), 0.05f, 0.84f);
            _woodMaterial = CreateMaterial(shader, new Color(0.36f, 0.22f, 0.12f, 1f), 0.00f, 0.20f);
            _metalMaterial = CreateMaterial(shader, new Color(0.16f, 0.19f, 0.22f, 1f), 0.58f, 0.48f);
            _poiMaterial = CreateEmissiveMaterial(shader, new Color(0.08f, 0.82f, 0.95f, 1f), 1.6f);
            _warmGlowMaterial = CreateEmissiveMaterial(shader, new Color(1f, 0.68f, 0.22f, 1f), 1.8f);
        }

        private static Material CreateMaterial(Shader shader, Color color, float metallic, float smoothness)
        {
            var material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        private static Material CreateEmissiveMaterial(Shader shader, Color color, float emissionStrength)
        {
            var material = CreateMaterial(shader, color, 0.05f, 0.52f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emissionStrength);
            }
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
