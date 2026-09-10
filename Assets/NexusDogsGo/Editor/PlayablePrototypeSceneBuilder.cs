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

            RewireMapEncounterButton(mapRoot, exploration);
            RewireCaptureButton(captureRoot, encounter);
            AddCaptureControls(captureRoot, encounter);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Selection.activeObject = mapRoot.gameObject;
            Debug.Log("NEXUS DOGS GO playable vertical slice ready. In Editor, GPS is simulated; Android uses device GPS.");
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
