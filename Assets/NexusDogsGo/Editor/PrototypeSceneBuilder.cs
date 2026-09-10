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

namespace NexusDogsGo.EditorTools
{
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

            new GameObject("GameBootstrap", typeof(GameBootstrap));
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
            foreach (ScreenId id in System.Enum.GetValues(typeof(ScreenId))) roots[id] = CreateScreen(canvasObject.transform, id);
            BindScreens(navigator, roots);

            BuildHome(roots[ScreenId.Home], navigator);
            BuildDogSelect(roots[ScreenId.DogSelect], navigator);
            BuildMap(roots[ScreenId.Map], navigator);
            BuildCapture(roots[ScreenId.Capture], navigator);
            BuildSocial(roots[ScreenId.Social], navigator);
            BuildMainTab(roots[ScreenId.Dogs], navigator, ScreenId.Dogs, "COLEÇÃO", "Filtros: Todos • Comuns • Raros • Épicos • Lendários\n\nLuna • Max • Thor • Mel • Rocky • Shadow");
            BuildMainTab(roots[ScreenId.Missions], navigator, ScreenId.Missions, "MISSÕES", "Diárias • Semanais • Todas\n\nCaminhar 1 km\nCapturar 3 cães\nExplorar 2 pontos\nDerrotar 1 boss\nAndar 2 km");
            BuildBattle(roots[ScreenId.Battle], navigator);
            BuildMainTab(roots[ScreenId.Inventory], navigator, ScreenId.Inventory, "INVENTÁRIO", "Poké Bola ×25\nSuper Poké Bola ×5\nRação Premium ×3\nPoção de cura ×10\nIncenso ×2\nRevive ×1");
            BuildMainTab(roots[ScreenId.Events], navigator, ScreenId.Events, "EVENTOS", "Festival da Colheita\nMega Raid\nDia Comunitário\nRaids de Cinco Estrelas");
            BuildAr(roots[ScreenId.Ar], navigator);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeObject = canvasObject;
            Debug.Log("NEXUS DOGS GO prototype scene created: " + ScenePath);
        }

        private static void BindScreens(ScreenNavigator navigator, Dictionary<ScreenId, GameObject> roots)
        {
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
            CreateLabel(root.transform, "NEXUS\nDOGS GO", 190, new Vector2(0, 460), 86, NexusTheme.CyanSoft, FontStyle.Bold);
            CreateLabel(root.transform, "EXPLORA • CAPTURA • EVOLUI", 80, new Vector2(0, 270), 32, NexusTheme.TextPrimary, FontStyle.Bold);
            CreateButton(root.transform, "JOGAR", new Vector2(0, -20), new Vector2(620, 130), NexusTheme.Green, delegate { navigator.Show(ScreenId.DogSelect); });
            CreateButton(root.transform, "CONFIGURAÇÕES", new Vector2(0, -190), new Vector2(620, 105), NexusTheme.SurfaceElevated, delegate { Debug.Log("Settings placeholder"); });
            CreateButton(root.transform, "INFORMAÇÕES", new Vector2(0, -325), new Vector2(620, 105), NexusTheme.SurfaceElevated, delegate { Debug.Log("Info placeholder"); });
        }

        private static void BuildDogSelect(GameObject root, ScreenNavigator navigator)
        {
            CreateHeader(root.transform, "ESCOLHE O TEU CÃO", "Cada cão tem habilidades únicas");
            var names = new[] { "LUNA\nEquilibrado", "MAX\nForte", "THOR\nRápido", "MEL\nDefensor" };
            for (var i = 0; i < names.Length; i++)
            {
                var x = i % 2 == 0 ? -235 : 235;
                var y = i < 2 ? 290 : -40;
                var color = i == 1 ? NexusTheme.Red : i == 2 ? NexusTheme.Green : NexusTheme.SurfaceElevated;
                CreateButton(root.transform, names[i], new Vector2(x, y), new Vector2(400, 260), color, delegate { });
            }
            CreateButton(root.transform, "CONFIRMAR", new Vector2(0, -420), new Vector2(720, 120), NexusTheme.Cyan, delegate { navigator.Show(ScreenId.Map); });
            CreateBack(root.transform, navigator, ScreenId.Home);
        }

        private static void BuildMap(GameObject root, ScreenNavigator navigator)
        {
            CreateHeader(root.transform, "MAPA", "GPS • spawns • POIs • companheiro");
            CreatePanel(root.transform, new Vector2(0, 80), new Vector2(940, 1210), NexusTheme.Surface);
            CreateLabel(root.transform, "MAPA REAL\n\nJogador + cão companheiro\n\n● PokéStop    ● cão selvagem    ● evento", 520, new Vector2(0, 100), 38, NexusTheme.CyanSoft, FontStyle.Bold);
            CreateButton(root.transform, "ENCONTRO", new Vector2(300, -430), new Vector2(300, 100), NexusTheme.Green, delegate { navigator.Show(ScreenId.Capture); });
            CreateButton(root.transform, "SOCIAL", new Vector2(-300, -430), new Vector2(300, 100), NexusTheme.Cyan, delegate { navigator.Show(ScreenId.Social); });
            CreateBottomNav(root.transform, navigator, ScreenId.Map);
        }

        private static void BuildCapture(GameObject root, ScreenNavigator navigator)
        {
            CreateHeader(root.transform, "GOLDEN RETRIEVER", "Raro • CP 432");
            CreatePanel(root.transform, new Vector2(0, 120), new Vector2(930, 1180), NexusTheme.SurfaceElevated);
            CreateLabel(root.transform, "CÃO 3D\n\n◯ zona de lançamento", 450, new Vector2(0, 210), 46, NexusTheme.TextPrimary, FontStyle.Bold);
            CreateButton(root.transform, "●  CAPTURAR", new Vector2(0, -390), new Vector2(500, 130), NexusTheme.Red, delegate { navigator.Show(ScreenId.Dogs); });
            CreateBack(root.transform, navigator, ScreenId.Map);
        }

        private static void BuildSocial(GameObject root, ScreenNavigator navigator)
        {
            CreateHeader(root.transform, "AMIGOS", "Perfil • amigos • social");
            CreatePanel(root.transform, new Vector2(0, 150), new Vector2(900, 1070), Color.white);
            CreateLabel(root.transform, "NUNO\nNível 12\n\n1 250 moedas     85 gemas\n\nEventos e recompensas sociais", 600, new Vector2(0, 160), 36, NexusTheme.Background, FontStyle.Bold);
            CreateBack(root.transform, navigator, ScreenId.Map);
        }

        private static void BuildMainTab(GameObject root, ScreenNavigator navigator, ScreenId active, string title, string body)
        {
            CreateHeader(root.transform, title, string.Empty);
            CreatePanel(root.transform, new Vector2(0, 100), new Vector2(930, 1220), NexusTheme.Surface);
            CreateLabel(root.transform, body, 800, new Vector2(0, 120), 34, NexusTheme.TextPrimary, FontStyle.Normal);
            if (active == ScreenId.Dogs)
                CreateButton(root.transform, "BATALHAR", new Vector2(0, -410), new Vector2(380, 100), NexusTheme.Red, delegate { navigator.Show(ScreenId.Battle); });
            CreateBottomNav(root.transform, navigator, active);
        }

        private static void BuildBattle(GameObject root, ScreenNavigator navigator)
        {
            CreateHeader(root.transform, "BATALHA", "Thor vs Rottweiler");
            CreatePanel(root.transform, new Vector2(0, 160), new Vector2(930, 1050), NexusTheme.SurfaceElevated);
            CreateLabel(root.transform, "THOR                         ROTTWEILER\nHP 320/310                    HP 180/180\n\n                VS", 420, new Vector2(0, 300), 34, NexusTheme.TextPrimary, FontStyle.Bold);
            CreateButton(root.transform, "ATAQUE", new Vector2(-300, -340), new Vector2(250, 120), NexusTheme.Red, delegate { });
            CreateButton(root.transform, "DEFESA", new Vector2(0, -340), new Vector2(250, 120), NexusTheme.Cyan, delegate { });
            CreateButton(root.transform, "ITEM", new Vector2(300, -340), new Vector2(250, 120), NexusTheme.Yellow, delegate { });
            CreateBack(root.transform, navigator, ScreenId.Dogs);
        }

        private static void BuildAr(GameObject root, ScreenNavigator navigator)
        {
            CreateHeader(root.transform, "AR / CÂMARA", "Vê o teu cão no mundo real");
            CreatePanel(root.transform, new Vector2(0, 120), new Vector2(930, 1190), NexusTheme.SurfaceElevated);
            CreateLabel(root.transform, "CÂMARA\n\nAR Foundation + ARCore\n\nColocação do cão • escala • fotografia", 520, new Vector2(0, 160), 38, NexusTheme.TextPrimary, FontStyle.Bold);
            CreateBack(root.transform, navigator, ScreenId.Map);
        }

        private static void CreateHeader(Transform parent, string title, string subtitle)
        {
            CreateLabel(parent, title, 100, new Vector2(0, 780), 50, NexusTheme.TextPrimary, FontStyle.Bold);
            if (!string.IsNullOrEmpty(subtitle)) CreateLabel(parent, subtitle, 60, new Vector2(0, 710), 26, NexusTheme.TextSecondary, FontStyle.Normal);
        }

        private static void CreateBottomNav(Transform parent, ScreenNavigator navigator, ScreenId active)
        {
            var labels = new[] { "MAPA", "CÃES", "MISSÕES", "INVENTÁRIO", "EVENTOS" };
            var screenIds = new[] { ScreenId.Map, ScreenId.Dogs, ScreenId.Missions, ScreenId.Inventory, ScreenId.Events };
            var actions = new UnityEngine.Events.UnityAction[] { navigator.ShowMap, navigator.ShowDogs, navigator.ShowMissions, navigator.ShowInventory, navigator.ShowEvents };
            for (var i = 0; i < labels.Length; i++)
            {
                var x = -400 + i * 200;
                CreateButton(parent, labels[i], new Vector2(x, -820), new Vector2(180, 95), active == screenIds[i] ? NexusTheme.Cyan : NexusTheme.SurfaceElevated, actions[i]);
            }
        }

        private static void CreateBack(Transform parent, ScreenNavigator navigator, ScreenId target)
        {
            CreateButton(parent, "‹", new Vector2(-450, 790), new Vector2(90, 90), NexusTheme.SurfaceElevated, delegate { navigator.Show(target); });
        }

        private static void CreatePanel(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            panel.GetComponent<Image>().color = color;
        }

        private static void CreateLabel(Transform parent, string text, float height, Vector2 position, int fontSize, Color color, FontStyle style)
        {
            var obj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(920, height);
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
            var obj = new GameObject(label.Replace("\n", " "), typeof(RectTransform), typeof(Image), typeof(Button));
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
}
#endif
