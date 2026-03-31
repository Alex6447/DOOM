using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using DOOM.Game;
using DOOM.Core;
using DOOM.UI;

namespace DOOM.Editor
{
    /// <summary>
    /// Автоматическая сборка сцен DOOM через меню DOOM > Setup.
    /// </summary>
    public static class SceneSetupTool
    {
        // ─────────────────────────────────────────────────────────────
        //  GAME SCENE
        // ─────────────────────────────────────────────────────────────
        [MenuItem("DOOM/Setup GameScene")]
        public static void SetupGameScene()
        {
            // Открываем GameScene
            string scenePath = "Assets/Scenes/GameScene.unity";
            EditorSceneManager.OpenScene(scenePath);

            var scene = SceneManager.GetActiveScene();
            ClearScene(scene);

            // ── Camera ──────────────────────────────────────────────
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            camGO.tag = "MainCamera";
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0, 0, -10);

            // Global Light
            var lightGO = new GameObject("Global Light 2D");
            var light2d = lightGO.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light2d.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light2d.intensity = 1f;

            // ── Background ──────────────────────────────────────────
            var bgParent = CreateEmpty("Background", null);
            CreateScrollingBackground("BG_Back",   bgParent, -4f, 0.6f, new Color(0.4f, 0.55f, 0.7f),  -3);
            CreateScrollingBackground("BG_Mid",    bgParent, -2f, 0.9f, new Color(0.55f, 0.45f, 0.35f), -2);
            CreateScrollingBackground("BG_Road",   bgParent,  0f, 1.2f, new Color(0.3f, 0.3f, 0.32f),  -1);

            // Стены коридора (левая и правая)
            CreateWall("Wall_Left",  -1.8f);
            CreateWall("Wall_Right",  1.8f);

            // ── Managers ────────────────────────────────────────────
            var managers = CreateEmpty("_Managers", null);
            managers.AddComponent<GameManager>();
            managers.AddComponent<SaveSystem>();
            managers.AddComponent<GameStateManager>();
            managers.AddComponent<CountryDatabase>();
            managers.AddComponent<PerformanceManager>();

            var waveCtrl = managers.AddComponent<WaveController>();
            var upgradeSystem = managers.AddComponent<UpgradeSystem>();
            var bossSpawner = managers.AddComponent<BossSpawner>();

            // ObjectPoolManager — с пулами
            var pool = managers.AddComponent<ObjectPoolManager>();

            // ── EnemySpawner ────────────────────────────────────────
            var spawnerGO = CreateEmpty("EnemySpawner", null);
            spawnerGO.AddComponent<EnemySpawner>();

            // ── PlayerSquad ─────────────────────────────────────────
            var squadGO = CreateEmpty("PlayerSquad", null);
            squadGO.transform.position = new Vector3(0, -3.5f, 0);
            squadGO.AddComponent<PlayerSquad>();
            squadGO.AddComponent<WeaponSystem>();

            // ── HUD Canvas ──────────────────────────────────────────
            var canvas = CreateCanvas("HUD_Canvas");
            canvas.AddComponent<HUDController>();
            canvas.AddComponent<VictoryScreen>();
            canvas.AddComponent<SafeAreaAdapter>();

            // Счётчик отряда
            var squadLabel = CreateTMPLabel(canvas.transform, "SquadLabel",
                "Отряд: 5", 14, TextAlignmentOptions.TopLeft,
                new Vector2(10, -10), new Vector2(200, 40));

            // Волна
            var waveLabel = CreateTMPLabel(canvas.transform, "WaveLabel",
                "Волна: 1", 14, TextAlignmentOptions.TopRight,
                new Vector2(-210, -10), new Vector2(200, 40));

            // Счёт
            var scoreLabel = CreateTMPLabel(canvas.transform, "ScoreLabel",
                "Счёт: 0", 16, TextAlignmentOptions.Top,
                new Vector2(-100, -10), new Vector2(200, 40));

            // Кнопка паузы
            var pauseBtn = CreateButton(canvas.transform, "PauseBtn", "⏸",
                new Vector2(-40, -40), new Vector2(60, 60), TextAlignmentOptions.Center);

            // Панель паузы
            var pausePanel = CreatePanel(canvas.transform, "PauseMenu", new Color(0,0,0,0.7f));
            pausePanel.SetActive(false);
            CreateButton(pausePanel.transform, "ResumeBtn",   "Продолжить",  new Vector2(0,  80), new Vector2(300, 60), TextAlignmentOptions.Center);
            CreateButton(pausePanel.transform, "RestartBtn",  "Рестарт",     new Vector2(0,   0), new Vector2(300, 60), TextAlignmentOptions.Center);
            CreateButton(pausePanel.transform, "MainMenuBtn", "Главное меню",new Vector2(0, -80), new Vector2(300, 60), TextAlignmentOptions.Center);

            // Экран победы
            var victoryPanel = CreatePanel(canvas.transform, "VictoryPanel", new Color(0,0,0,0.85f));
            victoryPanel.SetActive(false);
            CreateTMPLabel(victoryPanel.transform, "VictoryScore",   "Счёт: 0",   28, TextAlignmentOptions.Center, new Vector2(0,  60), new Vector2(500, 60));
            CreateTMPLabel(victoryPanel.transform, "VictoryCountry", "Победа!",   22, TextAlignmentOptions.Center, new Vector2(0,   0), new Vector2(500, 60));
            CreateButton(victoryPanel.transform,   "PlayAgainBtn",   "Играть снова", new Vector2(0, -80), new Vector2(300, 60), TextAlignmentOptions.Center);

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[DOOM] GameScene собрана успешно!");
        }

        // ─────────────────────────────────────────────────────────────
        //  COUNTRY SELECT SCENE
        // ─────────────────────────────────────────────────────────────
        [MenuItem("DOOM/Setup CountrySelectScene")]
        public static void SetupCountrySelectScene()
        {
            string scenePath = "Assets/Scenes/CountrySelectScene.unity";
            EditorSceneManager.OpenScene(scenePath);
            var scene = SceneManager.GetActiveScene();
            ClearScene(scene);

            // Camera
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            camGO.tag = "MainCamera";
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0, 0, -10);

            // Managers (нужны для CountryDatabase)
            var managers = CreateEmpty("_Managers", null);
            managers.AddComponent<GameManager>();
            managers.AddComponent<SaveSystem>();
            managers.AddComponent<CountryDatabase>();

            // Canvas
            var canvas = CreateCanvas("CountrySelect_Canvas");
            canvas.AddComponent<CountrySelectScreen>();
            canvas.AddComponent<SafeAreaAdapter>();

            // Заголовок
            CreateTMPLabel(canvas.transform, "Title", "🌍 Выбери страну", 26,
                TextAlignmentOptions.Top, new Vector2(0, -30), new Vector2(900, 60));

            // ScrollView
            var scrollGO = new GameObject("CountryScrollView");
            scrollGO.transform.SetParent(canvas.transform, false);
            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            var scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRT.anchorMax = new Vector2(0.95f, 0.88f);
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;

            // Viewport
            var viewport = CreateEmpty("Viewport", scrollGO.transform);
            var vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();
            scrollRect.viewport = vpRT;

            // Content
            var content = CreateEmpty("Content", viewport.transform);
            var contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRT;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            // Search field
            CreateSearchField(canvas.transform);

            // Info panel (выбранная страна)
            var infoPanel = CreatePanel(canvas.transform, "SelectionInfo", new Color(0.15f, 0.15f, 0.2f, 0.95f));
            var infoPanelRT = infoPanel.GetComponent<RectTransform>();
            infoPanelRT.anchorMin = new Vector2(0.05f, 0.02f);
            infoPanelRT.anchorMax = new Vector2(0.95f, 0.14f);
            infoPanelRT.offsetMin = Vector2.zero;
            infoPanelRT.offsetMax = Vector2.zero;
            infoPanel.SetActive(false);
            CreateTMPLabel(infoPanel.transform, "SelectedFlag",    "🇷🇺",           32, TextAlignmentOptions.MidlineLeft,  new Vector2(-380, 0), new Vector2(60, 60));
            CreateTMPLabel(infoPanel.transform, "SelectedCapital", "Столица: ...",  16, TextAlignmentOptions.MidlineLeft,  new Vector2(-300, 10), new Vector2(400, 30));
            CreateTMPLabel(infoPanel.transform, "SelectedTarget",  "Цель: ...",     14, TextAlignmentOptions.MidlineLeft,  new Vector2(-300, -15), new Vector2(400, 30));

            // Кнопка Играть
            var playBtn = CreateButton(canvas.transform, "PlayButton", "▶  ИГРАТЬ",
                new Vector2(200, 0), new Vector2(220, 60), TextAlignmentOptions.Center);
            playBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.6f, 0.02f);
            playBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.14f);

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[DOOM] CountrySelectScene собрана успешно!");
        }

        // ─────────────────────────────────────────────────────────────
        //  BUILD SETTINGS
        // ─────────────────────────────────────────────────────────────
        [MenuItem("DOOM/Add Scenes to Build Settings")]
        public static void AddScenesToBuild()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/CountrySelectScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true),
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[DOOM] Сцены добавлены в Build Settings: CountrySelectScene(0), GameScene(1)");
        }

        // ─────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────
        static void ClearScene(Scene scene)
        {
            foreach (var go in scene.GetRootGameObjects())
                Object.DestroyImmediate(go);
        }

        static GameObject CreateEmpty(string name, Transform parent)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        static void CreateScrollingBackground(string name, GameObject parent, float y, float speed, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = new Vector3(0, y, 0);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite   = CreateSolidSprite(color, 512, 512);
            sr.sortingOrder = order;
            go.transform.localScale = new Vector3(4f, 12f, 1f);

            var scroller = go.AddComponent<BackgroundScroller>();
        }

        static void CreateWall(string name, float x)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(x, 0, 0);
            go.transform.localScale = new Vector3(0.3f, 20f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSolidSprite(new Color(0.2f, 0.2f, 0.25f), 32, 512);
            sr.sortingOrder = 0;
            var col = go.AddComponent<BoxCollider2D>();
        }

        static Sprite CreateSolidSprite(Color color, int w, int h)
        {
            var tex = new Texture2D(w, h);
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        static GameObject CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 1f; // Height

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        static GameObject CreateTMPLabel(Transform parent, string name, string text,
            float fontSize, TextAlignmentOptions align, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return go;
        }

        static GameObject CreateButton(Transform parent, string name, string label,
            Vector2 anchoredPos, Vector2 size, TextAlignmentOptions align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.5f, 0.9f);
            var btn = go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var txtGO = new GameObject("Label");
            txtGO.transform.SetParent(go.transform, false);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 18;
            tmp.alignment = align;
            tmp.color = Color.white;
            var trt = txtGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            return go;
        }

        static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        static void CreateSearchField(Transform parent)
        {
            var go = new GameObject("SearchField");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.89f);
            rt.anchorMax = new Vector2(0.95f, 0.95f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var field = go.AddComponent<TMP_InputField>();
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 16;
            tmp.color = Color.white;
            var trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10, 0);
            trt.offsetMax = Vector2.zero;
            field.textComponent = tmp;

            var placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(go.transform, false);
            var ph = placeholder.AddComponent<TextMeshProUGUI>();
            ph.text = "🔍 Поиск страны...";
            ph.fontSize = 16;
            ph.color = new Color(0.6f, 0.6f, 0.6f);
            var prt = placeholder.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(10, 0);
            prt.offsetMax = Vector2.zero;
            field.placeholder = ph;
        }
    }
}
