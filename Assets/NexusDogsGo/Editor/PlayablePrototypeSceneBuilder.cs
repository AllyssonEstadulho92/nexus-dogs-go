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
            Debug.Log("NEXUS DOGS GO playable vertical slice ready with procedural 3D GPS map.");
        }

        private static void ConfigureMapUiFor3D(Transform mapRoot)
        {
            var rootImage = mapRoot.GetComponent<Image>();
            if (rootImage != null)
            {
                rootImage.color = new Color(0.02f, 0.05f, 0.07f, 0.08f);
                rootImage.raycastTarget = false;
            }

            var panel = mapRoot.Find("Panel");
            if (panel != null)
            {
                var panelImage = panel.GetComponent<Image>();
                if (panelImage != null)
                {
                    panelImage.color = new Color(0.025f, 0.07f, 0.09f, 0.18f);
                    panelImage.raycastTarget = false;
                }
            }

            var labels = mapRoot.GetComponentsInChildren<Text>(true);
            for (var i = 0; i < labels.Length; i++)
            {
                var label = labels[i];
                label.raycastTarget = false;
                if (label.text.Contains("MAPA REAL"))
                {
                    label.text = "MAPA 3D • GPS\nArrasta para rodar • pinça para zoom";
                    label.fontSize = 28;
                    var rect = label.rectTransform;
                    rect.sizeDelta = new Vector2(720f, 100f);
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
            var spawnRoot = new GameObject("DogSpawns");
            spawnRoot.transform.SetParent(world.transform, false);

            var player = CreatePlayerMarker(world.transform);
            var companion = CreateCompanionMarker(world.transform, player.transform);
            companion.transform.localPosition = new Vector3(-0.55f, 0.18f, -0.65f);

            var renderer = world.AddComponent<Procedural3DMapRenderer>();
            SetObjectReference(renderer, "exploration", exploration);
            SetObjectReference(renderer, "environmentRoot", environment.transform);
            SetObjectReference(renderer, "playerMarker", player.transform);

            var markerTemplate = CreateDogSpawnTemplate(world.transform);
            var markerManager = world.AddComponent<WorldSpawnMarkerManager>();
            SetObjectReference(markerManager, "exploration", exploration);
            SetObjectReference(markerManager, "markerRoot", spawnRoot.transform);
            SetObjectReference(markerManager, "markerPrefab", markerTemplate);

            ConfigureMainCamera(player.transform);
            ConfigureLighting();
        }

        private static GameObject CreatePlayerMarker(Transform parent)
        {
            var root = new GameObject("PlayerMarker");
            root.transform.SetParent(parent, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "PlayerBody";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            body.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
            RemoveCollider(body);
            SetRendererColor(body, NexusTheme.Cyan);

            var direction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            direction.name = "Direction";
            direction.transform.SetParent(root.transform, false);
            direction.transform.localPosition = new Vector3(0f, 0.18f, 0.34f);
            direction.transform.localScale = new Vector3(0.08f, 0.06f, 0.42f);
            RemoveCollider(direction);
            SetRendererColor(direction, Color.white);

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "AccuracyRing";
            ring.transform.SetParent(root.transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            ring.transform.localScale = new Vector3(1.15f, 0.018f, 1.15f);
            RemoveCollider(ring);
            SetRendererColor(ring, new Color(0.10f, 0.75f, 0.90f, 0.33f));

            return root;
        }

        private static GameObject CreateCompanionMarker(Transform parent, Transform target)
        {
            var root = new GameObject("CompanionDog");
            root.transform.SetParent(parent, false);
            var follower = root.AddComponent<CompanionFollower>();
            SetObjectReference(follower, "target", target);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "DogBodyPlaceholder";
            body.transform.SetParent(root.transform, false);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.transform.localScale = new Vector3(0.28f, 0.38f, 0.24f);
            RemoveCollider(body);
            SetRendererColor(body, new Color(0.92f, 0.72f, 0.26f, 1f));

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "DogHeadPlaceholder";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.14f, 0.32f);
            head.transform.localScale = Vector3.one * 0.30f;
            RemoveCollider(head);
            SetRendererColor(head, new Color(0.96f, 0.79f, 0.34f, 1f));
            return root;
        }

        private static GameObject CreateDogSpawnTemplate(Transform parent)
        {
            var root = new GameObject("DogSpawnTemplate", typeof(SphereCollider), typeof(DogSpawnMarker));
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, -100f, 0f);
            var collider = root.GetComponent<SphereCollider>();
            collider.radius = 0.55f;

            var visual = new GameObject("Visual", typeof(Map3DMarkerEffects));
            visual.transform.SetParent(root.transform, false);

            var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = "Beacon";
            beacon.transform.SetParent(visual.transform, false);
            beacon.transform.localPosition = new Vector3(0f, 0.38f, 0f);
            beacon.transform.localScale = new Vector3(0.16f, 0.38f, 0.16f);
            RemoveCollider(beacon);
            SetRendererColor(beacon, NexusTheme.Green);

            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "DogEncounter";
            orb.transform.SetParent(visual.transform, false);
            orb.transform.localPosition = new Vector3(0f, 0.92f, 0f);
            orb.transform.localScale = Vector3.one * 0.48f;
            RemoveCollider(orb);
            SetRendererColor(orb, NexusTheme.CyanSoft);

            var halo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            halo.name = "Halo";
            halo.transform.SetParent(visual.transform, false);
            halo.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            halo.transform.localScale = new Vector3(0.72f, 0.025f, 0.72f);
            RemoveCollider(halo);
            SetRendererColor(halo, new Color(0.15f, 0.95f, 0.65f, 0.55f));

            root.SetActive(false);
            return root;
        }

        private static void ConfigureMainCamera(Transform target)
        {
            var cameraObject = GameObject.Find("Main Camera");
            if (cameraObject == null) return;

            var camera = cameraObject.GetComponent<Camera>();
            if (camera != null)
            {
                camera.orthographic = false;
                camera.fieldOfView = 48f;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 500f;
                camera.backgroundColor = new Color(0.025f, 0.055f, 0.075f, 1f);
            }

            cameraObject.transform.position = new Vector3(-5f, 15f, -16f);
            var controller = cameraObject.GetComponent<Map3DCameraController>();
            if (controller == null) controller = cameraObject.AddComponent<Map3DCameraController>();
            controller.SetTarget(target);
        }

        private static void ConfigureLighting()
        {
            var lightObject = GameObject.Find("Map Sun");
            if (lightObject == null)
            {
                lightObject = new GameObject("Map Sun", typeof(Light));
                var light = lightObject.GetComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                light.color = new Color(0.86f, 0.94f, 1f, 1f);
                light.shadows = LightShadows.Soft;
                lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            }

            RenderSettings.ambientLight = new Color(0.22f, 0.30f, 0.34f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.025f, 0.055f, 0.075f, 1f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 58f;
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
            CreateActionButton(root, "POKÉ BOLA", new Vector2(-260f, -555f), new Vector2(240f, 80f), NexusTheme.Cyan, encounter.SelectPokeBall);
            CreateActionButton(root, "SUPER BOLA", new Vector2(0f, -555f), new Vector2(240f, 80f), NexusTheme.SurfaceElevated, encounter.SelectSuperBall);
            CreateActionButton(root, "RAÇÃO", new Vector2(260f, -555f), new Vector2(240f, 80f), NexusTheme.Green, encounter.ToggleFood);
            CreateActionButton(root, "FUGIR", new Vector2(0f, -665f), new Vector2(300f, 75f), NexusTheme.SurfaceElevated, encounter.Flee);
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
