#if UNITY_EDITOR
using NexusDogsGo.Gameplay.Capture;
using NexusDogsGo.Gameplay.World;
using NexusDogsGo.UI.Navigation;
using NexusDogsGo.UI.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace NexusDogsGo.EditorTools
{
    public static class PlayablePrototypeSceneBuilder
    {
        [MenuItem("NEXUS DOGS GO/Build Playable Vertical Slice")]
        public static void Build()
        {
            PrototypeSceneBuilder.Build();

            var ui = GameObject.Find("UI");
            if (ui == null)
            {
                Debug.LogError("UI root not found after prototype build.");
                return;
            }

            var navigator = ui.GetComponent<ScreenNavigator>();
            var mapRoot = ui.transform.Find("Map");
            var captureRoot = ui.transform.Find("Capture");
            if (navigator == null || mapRoot == null || captureRoot == null)
            {
                Debug.LogError("Required prototype roots were not found.");
                return;
            }

            var exploration = mapRoot.gameObject.GetComponent<MapExplorationController>();
            if (exploration == null) exploration = mapRoot.gameObject.AddComponent<MapExplorationController>();
            SetObjectReference(exploration, "navigator", navigator);

            var encounter = captureRoot.gameObject.GetComponent<CaptureEncounterController>();
            if (encounter == null) encounter = captureRoot.gameObject.AddComponent<CaptureEncounterController>();
            SetObjectReference(encounter, "mapController", exploration);
            SetObjectReference(encounter, "navigator", navigator);

            ConfigureMapUiFor3D(mapRoot);
            Build3DMapWorld(exploration);
            RewireMapEncounterButton(mapRoot, exploration);
            RewireCaptureButton(captureRoot, encounter);
            AddCaptureControls(captureRoot, encounter);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Selection.activeObject = mapRoot.gameObject;
            Debug.Log("NEXUS DOGS GO playable slice ready with detailed 3D world, avatar, companion and wild dogs.");
        }

        private static void ConfigureMapUiFor3D(Transform mapRoot)
        {
            var rootImage = mapRoot.GetComponent<Image>();
            if (rootImage != null)
            {
                rootImage.color = new Color(0.02f, 0.05f, 0.07f, 0.04f);
                rootImage.raycastTarget = false;
            }

            var panel = mapRoot.Find("Panel");
            if (panel != null)
            {
                var panelImage = panel.GetComponent<Image>();
                if (panelImage != null)
                {
                    panelImage.color = new Color(0.025f, 0.07f, 0.09f, 0.10f);
                    panelImage.raycastTarget = false;
                }
            }

            var labels = mapRoot.GetComponentsInChildren<Text>(true);
            for (var i = 0; i < labels.Length; i++)
            {
                var label = labels[i];
                label.raycastTarget = false;
                if (label.text.Contains("MAPA REAL") || label.text.Contains("MAPA 3D"))
                {
                    label.text = "NEXUS WORLD 3D • GPS\nArrasta para rodar • pinça para zoom";
                    label.fontSize = 27;
                    var rect = label.rectTransform;
                    rect.sizeDelta = new Vector2(760f, 96f);
                    rect.anchoredPosition = new Vector2(0f, 565f);
                }
            }
        }

        private static void Build3DMapWorld(MapExplorationController exploration)
        {
            var oldWorld = GameObject.Find("NEXUS_3D_MAP");
            if (oldWorld != null) Object.DestroyImmediate(oldWorld);

            var world = new GameObject("NEXUS_3D_MAP");
            var environment = new GameObject("Environment");
            environment.transform.SetParent(world.transform, false);
            var spawnRoot = new GameObject("WildDogs");
            spawnRoot.transform.SetParent(world.transform, false);

            var player = CreatePlayerMarker(world.transform);
            var companion = CreateCompanionMarker(world.transform, player.transform);
            companion.transform.localPosition = new Vector3(-0.52f, 0.10f, -0.64f);

            var renderer = world.AddComponent<Procedural3DMapRenderer>();
            SetObjectReference(renderer, "exploration", exploration);
            SetObjectReference(renderer, "environmentRoot", environment.transform);
            SetObjectReference(renderer, "playerMarker", player.transform);

            var markerTemplate = CreateDogSpawnTemplate(world.transform);
            var markerManager = world.AddComponent<WorldSpawnMarkerManager>();
            SetObjectReference(markerManager, "exploration", exploration);
            SetObjectReference(markerManager, "mapRenderer", renderer);
            SetObjectReference(markerManager, "markerRoot", spawnRoot.transform);
            SetObjectReference(markerManager, "markerPrefab", markerTemplate);

            var camera = ConfigureMainCamera(player.transform);
            var sun = ConfigureLighting();
            var atmosphere = world.AddComponent<Map3DWorldAtmosphere>();
            atmosphere.Configure(sun, camera);
        }

        private static GameObject CreatePlayerMarker(Transform parent)
        {
            var root = new GameObject("PlayerAvatar");
            root.transform.SetParent(parent, false);

            CreatePrimitivePart(root.transform, PrimitiveType.Capsule, "Torso", new Vector3(0f, 0.48f, 0f), new Vector3(0.22f, 0.25f, 0.16f), Quaternion.identity, new Color(0.08f, 0.64f, 0.88f, 1f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 0.82f, 0f), Vector3.one * 0.23f, Quaternion.identity, new Color(0.76f, 0.56f, 0.42f, 1f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "Hair", new Vector3(0f, 0.89f, -0.015f), new Vector3(0.23f, 0.12f, 0.23f), Quaternion.identity, new Color(0.08f, 0.07f, 0.06f, 1f));

            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "LeftLeg", new Vector3(-0.09f, 0.20f, 0f), new Vector3(0.13f, 0.35f, 0.13f), Quaternion.identity, new Color(0.10f, 0.14f, 0.18f, 1f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "RightLeg", new Vector3(0.09f, 0.20f, 0f), new Vector3(0.13f, 0.35f, 0.13f), Quaternion.identity, new Color(0.10f, 0.14f, 0.18f, 1f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "LeftShoe", new Vector3(-0.09f, 0.035f, 0.055f), new Vector3(0.15f, 0.07f, 0.24f), Quaternion.identity, new Color(0.035f, 0.04f, 0.05f, 1f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "RightShoe", new Vector3(0.09f, 0.035f, 0.055f), new Vector3(0.15f, 0.07f, 0.24f), Quaternion.identity, new Color(0.035f, 0.04f, 0.05f, 1f));

            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "LeftArm", new Vector3(-0.25f, 0.50f, 0f), new Vector3(0.10f, 0.34f, 0.10f), Quaternion.Euler(0f, 0f, -8f), new Color(0.10f, 0.52f, 0.74f, 1f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "RightArm", new Vector3(0.25f, 0.50f, 0f), new Vector3(0.10f, 0.34f, 0.10f), Quaternion.Euler(0f, 0f, 8f), new Color(0.10f, 0.52f, 0.74f, 1f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Backpack", new Vector3(0f, 0.49f, -0.17f), new Vector3(0.31f, 0.36f, 0.13f), Quaternion.identity, new Color(0.10f, 0.18f, 0.22f, 1f));

            var ring = CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "GpsAccuracyRing", new Vector3(0f, 0.018f, 0f), new Vector3(0.82f, 0.015f, 0.82f), Quaternion.identity, new Color(0.10f, 0.82f, 0.95f, 0.46f));
            ring.transform.SetAsFirstSibling();
            return root;
        }

        private static GameObject CreateCompanionMarker(Transform parent, Transform target)
        {
            var root = new GameObject("CompanionDog");
            root.transform.SetParent(parent, false);
            var follower = root.AddComponent<CompanionFollower>();
            SetObjectReference(follower, "target", target);

            var visual = new GameObject("Dog");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * 0.92f;
            CreateDogVisual(visual.transform, new Color(0.86f, 0.61f, 0.25f, 1f), true);
            return root;
        }

        private static GameObject CreateDogSpawnTemplate(Transform parent)
        {
            var root = new GameObject("DogSpawnTemplate", typeof(SphereCollider), typeof(DogSpawnMarker), typeof(WildDogMapActor));
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, -100f, 0f);
            var collider = root.GetComponent<SphereCollider>();
            collider.radius = 0.68f;
            collider.center = new Vector3(0f, 0.26f, 0f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            var aura = new GameObject("Aura", typeof(Map3DMarkerEffects));
            aura.transform.SetParent(visual.transform, false);
            var halo = CreatePrimitivePart(aura.transform, PrimitiveType.Cylinder, "RarityHalo", new Vector3(0f, 0.035f, 0f), new Vector3(0.68f, 0.018f, 0.68f), Quaternion.identity, new Color(0.18f, 0.92f, 0.54f, 1f));
            var beacon = CreatePrimitivePart(aura.transform, PrimitiveType.Cylinder, "RarityBeacon", new Vector3(0f, 0.30f, 0f), new Vector3(0.035f, 0.30f, 0.035f), Quaternion.identity, new Color(0.18f, 0.92f, 0.54f, 1f));
            halo.name = "AuraHalo";
            beacon.name = "AuraBeacon";

            var dog = new GameObject("Dog");
            dog.transform.SetParent(visual.transform, false);
            dog.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            CreateDogVisual(dog.transform, new Color(0.62f, 0.45f, 0.30f, 1f), false);

            root.SetActive(false);
            return root;
        }

        private static void CreateDogVisual(Transform root, Color bodyColor, bool companion)
        {
            var secondary = Color.Lerp(bodyColor, Color.white, 0.34f);
            var dark = Color.Lerp(bodyColor, Color.black, 0.45f);

            CreatePrimitivePart(root, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.23f, 0f), new Vector3(0.22f, 0.34f, 0.18f), Quaternion.Euler(90f, 0f, 0f), bodyColor);
            CreatePrimitivePart(root, PrimitiveType.Sphere, "Chest", new Vector3(0f, 0.25f, 0.23f), new Vector3(0.26f, 0.30f, 0.24f), Quaternion.identity, secondary);

            var headPivot = new GameObject("HeadPivot");
            headPivot.transform.SetParent(root, false);
            headPivot.transform.localPosition = new Vector3(0f, 0.38f, 0.36f);
            CreatePrimitivePart(headPivot.transform, PrimitiveType.Sphere, "Head", Vector3.zero, new Vector3(0.30f, 0.28f, 0.29f), Quaternion.identity, bodyColor);
            CreatePrimitivePart(headPivot.transform, PrimitiveType.Sphere, "Muzzle", new Vector3(0f, -0.035f, 0.20f), new Vector3(0.20f, 0.13f, 0.18f), Quaternion.identity, secondary);
            CreatePrimitivePart(headPivot.transform, PrimitiveType.Sphere, "Nose", new Vector3(0f, -0.02f, 0.30f), Vector3.one * 0.075f, Quaternion.identity, new Color(0.045f, 0.04f, 0.04f, 1f));
            CreatePrimitivePart(headPivot.transform, PrimitiveType.Cube, "LeftEar", new Vector3(-0.17f, 0.16f, 0.01f), new Vector3(0.11f, 0.22f, 0.08f), Quaternion.Euler(0f, 0f, -24f), dark);
            CreatePrimitivePart(headPivot.transform, PrimitiveType.Cube, "RightEar", new Vector3(0.17f, 0.16f, 0.01f), new Vector3(0.11f, 0.22f, 0.08f), Quaternion.Euler(0f, 0f, 24f), dark);
            CreatePrimitivePart(headPivot.transform, PrimitiveType.Sphere, "LeftEye", new Vector3(-0.09f, 0.05f, 0.25f), Vector3.one * 0.045f, Quaternion.identity, Color.black);
            CreatePrimitivePart(headPivot.transform, PrimitiveType.Sphere, "RightEye", new Vector3(0.09f, 0.05f, 0.25f), Vector3.one * 0.045f, Quaternion.identity, Color.black);

            var legColor = Color.Lerp(bodyColor, dark, 0.12f);
            CreatePrimitivePart(root, PrimitiveType.Cube, "Leg_FL", new Vector3(-0.14f, 0.04f, 0.22f), new Vector3(0.10f, 0.30f, 0.10f), Quaternion.identity, legColor);
            CreatePrimitivePart(root, PrimitiveType.Cube, "Leg_FR", new Vector3(0.14f, 0.04f, 0.22f), new Vector3(0.10f, 0.30f, 0.10f), Quaternion.identity, legColor);
            CreatePrimitivePart(root, PrimitiveType.Cube, "Leg_BL", new Vector3(-0.14f, 0.04f, -0.21f), new Vector3(0.10f, 0.30f, 0.10f), Quaternion.identity, legColor);
            CreatePrimitivePart(root, PrimitiveType.Cube, "Leg_BR", new Vector3(0.14f, 0.04f, -0.21f), new Vector3(0.10f, 0.30f, 0.10f), Quaternion.identity, legColor);

            var tail = CreatePrimitivePart(root, PrimitiveType.Cube, "Tail", new Vector3(0f, 0.31f, -0.42f), new Vector3(0.09f, 0.09f, 0.42f), Quaternion.Euler(-22f, 0f, 0f), bodyColor);
            tail.transform.localRotation = Quaternion.Euler(-22f, 0f, 18f);

            if (companion)
            {
                CreatePrimitivePart(root, PrimitiveType.Cube, "Collar", new Vector3(0f, 0.34f, 0.25f), new Vector3(0.32f, 0.055f, 0.08f), Quaternion.identity, NexusTheme.Cyan);
            }
        }

        private static Camera ConfigureMainCamera(Transform target)
        {
            var cameraObject = GameObject.Find("Main Camera");
            if (cameraObject == null) return null;

            var camera = cameraObject.GetComponent<Camera>();
            if (camera != null)
            {
                camera.orthographic = false;
                camera.fieldOfView = 46f;
                camera.nearClipPlane = 0.04f;
                camera.farClipPlane = 650f;
                camera.backgroundColor = new Color(0.30f, 0.58f, 0.75f, 1f);
            }

            cameraObject.transform.position = new Vector3(-4.5f, 12f, -14f);
            var controller = cameraObject.GetComponent<Map3DCameraController>();
            if (controller == null) controller = cameraObject.AddComponent<Map3DCameraController>();
            controller.SetTarget(target);
            return camera;
        }

        private static Light ConfigureLighting()
        {
            var lightObject = GameObject.Find("Map Sun");
            if (lightObject == null) lightObject = new GameObject("Map Sun", typeof(Light));

            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.10f;
            light.color = new Color(1f, 0.94f, 0.82f, 1f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.72f;
            lightObject.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

            RenderSettings.ambientLight = new Color(0.36f, 0.41f, 0.39f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.42f, 0.58f, 0.65f, 1f);
            RenderSettings.fogStartDistance = 24f;
            RenderSettings.fogEndDistance = 84f;
            return light;
        }

        private static void RewireMapEncounterButton(Transform mapRoot, MapExplorationController exploration)
        {
            var buttonTransform = mapRoot.Find("ENCONTRO");
            if (buttonTransform == null) return;
            var button = buttonTransform.GetComponent<Button>();
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(exploration.SelectFirstSpawn);
        }

        private static void RewireCaptureButton(Transform captureRoot, CaptureEncounterController encounter)
        {
            var buttonTransform = captureRoot.Find("●  CAPTURAR");
            if (buttonTransform == null) return;
            var button = buttonTransform.GetComponent<Button>();
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(encounter.AttemptGreat);
        }

        private static void AddCaptureControls(Transform root, CaptureEncounterController encounter)
        {
            var throwArea = new GameObject("ThrowArea", typeof(RectTransform), typeof(Image), typeof(SwipeThrowController));
            throwArea.transform.SetParent(root, false);
            var throwRect = throwArea.GetComponent<RectTransform>();
            throwRect.sizeDelta = new Vector2(820f, 720f);
            throwRect.anchoredPosition = new Vector2(0f, 120f);
            var image = throwArea.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.01f);
            image.raycastTarget = true;
            SetObjectReference(throwArea.GetComponent<SwipeThrowController>(), "encounter", encounter);

            CreateLabel(root, "DESLIZA PARA CIMA PARA LANÇAR", new Vector2(0f, -265f), new Vector2(820f, 55f), 24, NexusTheme.CyanSoft);
            CreateActionButton(root, "BOLA", new Vector2(-260f, -555f), new Vector2(240f, 80f), NexusTheme.Cyan, encounter.SelectPokeBall);
            CreateActionButton(root, "SUPER BOLA", new Vector2(0f, -555f), new Vector2(240f, 80f), NexusTheme.SurfaceElevated, encounter.SelectSuperBall);
            CreateActionButton(root, "RAÇÃO", new Vector2(260f, -555f), new Vector2(240f, 80f), NexusTheme.Green, encounter.ToggleFood);
            CreateActionButton(root, "FUGIR", new Vector2(0f, -665f), new Vector2(300f, 75f), NexusTheme.SurfaceElevated, encounter.Flee);
        }

        private static GameObject CreatePrimitivePart(Transform parent, PrimitiveType type, string objectName, Vector3 position, Vector3 scale, Quaternion rotation, Color color)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = objectName;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            obj.transform.localRotation = rotation;
            RemoveCollider(obj);
            SetRendererColor(obj, color);
            return obj;
        }

        private static void CreateActionButton(Transform parent, string text, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction action)
        {
            var obj = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            obj.GetComponent<Image>().color = color;
            obj.GetComponent<Button>().onClick.AddListener(action);
            CreateLabel(obj.transform, text, Vector2.zero, size, 22, Color.white);
        }

        private static void CreateLabel(Transform parent, string text, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var labelObject = new GameObject("Label_" + text, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var label = labelObject.GetComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.raycastTarget = false;
        }

        private static void RemoveCollider(GameObject obj)
        {
            var collider = obj.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }

        private static void SetRendererColor(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.30f);
            renderer.sharedMaterial = material;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError("Serialized property not found: " + target.GetType().Name + "." + propertyName);
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
