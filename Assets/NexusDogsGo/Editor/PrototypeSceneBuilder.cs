#if UNITY_EDITOR
using System.Collections.Generic;
using NexusDogsGo.Core;
using NexusDogsGo.UI.Navigation;
using NexusDogsGo.UI.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NexusDogsGo.EditorTools;

public static class PrototypeSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Prototype.unity";

    [MenuItem("NEXUS DOGS GO/Build Prototype Scene")]
    public static void Build()
    {
        EnsureFolder("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraObject = new GameObject("Main Camera", typeof(Camera));
        cameraObject.tag = "MainCamera";
        cameraObject.GetComponent<Camera>().backgroundColor = NexusTheme.Background;
        cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

        var bootstrapObject = new GameObject("GameBootstrap", typeof(GameBootstrap));
        _ = bootstrapObject;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var canvasObject = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var navigator = canvasObject.AddComponent<ScreenNavigator>();
        var roots = new Dictionary<ScreenId, GameObject>();
        foreach (var id in new[] { ScreenId.Home, ScreenId.Map, ScreenId.Dogs, ScreenId.Missions, ScreenId.Inventory, ScreenId.Events })
        {
            roots[id] = CreateScreen(canvasObject.transform, id);
        }

        var serialized = new SerializedObject(navigator);
        serialized.FindProperty("initialScreen").enumValueIndex = (int)ScreenId.Home;
        var screens = serialized.FindProperty("screens");
        screens.arraySize = roots.Count;
        var index = 0;
        foreach (var pair in roots)
        {
            var item = screens.GetArrayElementAtIndex(index++);
            item.FindPropertyRelative("Id").enumValueIndex = (int)pair.Key;
            item.FindPropertyRelative("Root").objectReferenceValue = pair.Value;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();

        BuildHome(roots[ScreenId.Home], navigator);
        foreach (var id in new[] { ScreenId.Map, ScreenId.Dogs, ScreenId.Missions, ScreenId.Inventory, ScreenId.Events })
        {
            BuildStandardScreen(roots[id], navigator, id);
        }

        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeObject = canvasObject;
        Debug.Log($"NEXUS DOGS GO prototype scene created: {ScenePath}");
    }

    private static GameObject CreateScreen(Transform parent, ScreenId id)
    {
        var root = new GameObject(id.ToString(), typeof(RectTransform), typeof(Image));
        root.transform.SetParent(parent, false);
        Stretch(root.GetComponent<RectTransform>());
        root.GetComponent<Image>().color = NexusTheme.Background;
        root.SetActive(false);
        return root;
    }

    private static void BuildHome(GameObject root, ScreenNavigator navigator)
    {
        CreateLabel(root.transform, "NEXUS\nDOGS GO", 170, new Vector2(0, 420), 86, NexusTheme.CyanSoft, FontStyle.Bold);
        CreateLabel(root.transform, "EXPLORA • CAPTURA • EVOLUI", 80, new Vector2(0, 250), 32, NexusTheme.TextPrimary, FontStyle.Bold);
        CreateButton(root.transform, "JOGAR", new Vector2(0, -40), new Vector2(620, 130), NexusTheme.Green, navigator.ShowMap);
        CreateButton(root.transform, "CONFIGURAÇÕES", new Vector2(0, -210), new Vector2(620, 105), NexusTheme.SurfaceElevated, () => Debug.Log("Settings placeholder"));
        CreateButton(root.transform, "INFORMAÇÕES", new Vector2(0, -345), new Vector2(620, 105), NexusTheme.SurfaceElevated, () => Debug.Log("Info placeholder"));
    }

    private static void BuildStandardScreen(GameObject root, ScreenNavigator navigator, ScreenId id)
    {
        CreateLabel(root.transform, id.ToString().ToUpperInvariant(), 110, new Vector2(0, 730), 52, NexusTheme.TextPrimary, FontStyle.Bold);
        CreatePanel(root.transform, new Vector2(0, 70), new Vector2(920, 1180));
        CreateLabel(root.transform, PlaceholderText(id), 420, new Vector2(0, 100), 35, NexusTheme.TextSecondary, FontStyle.Normal);

        var labels = new[] { "MAPA", "CÃES", "MISSÕES", "INVENTÁRIO", "EVENTOS" };
        var actions = new UnityEngine.Events.UnityAction[] { navigator.ShowMap, navigator.ShowDogs, navigator.ShowMissions, navigator.ShowInventory, navigator.ShowEvents };
        for (var i = 0; i < labels.Length; i++)
        {
            var x = -400 + i * 200;
            CreateButton(root.transform, labels[i], new Vector2(x, -820), new Vector2(180, 95), id == (ScreenId)(i == 0 ? (int)ScreenId.Map : i == 1 ? (int)ScreenId.Dogs : i == 2 ? (int)ScreenId.Missions : i == 3 ? (int)ScreenId.Inventory : (int)ScreenId.Events) ? NexusTheme.Cyan : NexusTheme.SurfaceElevated, actions[i]);
        }
    }

    private static string PlaceholderText(ScreenId id) => id switch
    {
        ScreenId.Map => "GPS + mapa + jogador + cão companheiro + spawns + POIs\n\nO serviço de localização e geração de spawns já está implementado.",
        ScreenId.Dogs => "Coleção filtrável de cães\n\nLuna • Max • Thor • Mel • Rocky • Shadow",
        ScreenId.Missions => "Diárias • Semanais • Todas\n\nCaminhar • Capturar • Explorar • Boss",
        ScreenId.Inventory => "Bolas • Comida • Poções • Outros\n\nO inventário inicial já é persistido no perfil.",
        ScreenId.Events => "Festival • Raid • Dia Comunitário • Temporada\n\nOs eventos online serão servidos pelo backend.",
        _ => id.ToString()
    };

    private static void CreatePanel(Transform parent, Vector2 position, Vector2 size)
    {
        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        panel.GetComponent<Image>().color = NexusTheme.Surface;
    }

    private static void CreateLabel(Transform parent, string text, float height, Vector2 position, int fontSize, Color color, FontStyle style)
    {
        var obj = new GameObject("Label", typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(900, height);
        rect.anchoredPosition = position;
        var label = obj.GetComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
    }

    private static void CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction action)
    {
        var obj = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        obj.GetComponent<Image>().color = color;
        obj.GetComponent<Button>().onClick.AddListener(action);
        CreateLabel(obj.transform, label, size.y, Vector2.zero, 27, Color.white, FontStyle.Bold);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
