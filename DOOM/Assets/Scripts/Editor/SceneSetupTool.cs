using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace DOOM.Editor
{
    public static class SceneSetupTool
    {
        // ══════════════════════════════════════════════════════════════
        //  GAME SCENE
        // ══════════════════════════════════════════════════════════════
        [MenuItem("DOOM/1. Setup GameScene")]
        public static void SetupGameScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
            var scene = SceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
                Object.DestroyImmediate(go);

            // ── Camera ──────────────────────────────────────────────
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic    = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.18f);
            cam.transform.position = new Vector3(0, 0, -10);
            camGO.AddComponent<AudioListener>();

            // Global Light 2D — добавим через GameObject menu вручную,
            // здесь создадим пустышку-placeholder
            var lightGO = new GameObject("Global Light 2D");
            // Компонент Light2D добавь руками: Add Component → Rendering → Light 2D → Global

            // ── Background слои ─────────────────────────────────────
            var bgRoot = new GameObject("--- BACKGROUND ---");

            MakeSprite("BG_Sky",    bgRoot, new Color(0.35f, 0.50f, 0.70f), new Vector3(0,  3f, 0), new Vector3(4.5f, 6f, 1), -5);
            MakeSprite("BG_City",   bgRoot, new Color(0.45f, 0.40f, 0.38f), new Vector3(0,  0f, 0), new Vector3(4.5f, 6f, 1), -4);
            MakeSprite("BG_Road",   bgRoot, new Color(0.28f, 0.28f, 0.30f), new Vector3(0, -2f, 0), new Vector3(3.0f, 8f, 1), -3);
            MakeSprite("Wall_Left", bgRoot, new Color(0.18f, 0.18f, 0.22f), new Vector3(-1.65f, 0, 0), new Vector3(0.25f, 14f, 1), -2);
            MakeSprite("Wall_Right",bgRoot, new Color(0.18f, 0.18f, 0.22f), new Vector3( 1.65f, 0, 0), new Vector3(0.25f, 14f, 1), -2);

            // BackgroundScroller — добавь к BG_Road, BG_City вручную после запуска

            // ── Managers ────────────────────────────────────────────
            var managers = new GameObject("--- MANAGERS ---");
            managers.AddComponent<Core.GameManager>();
            managers.AddComponent<Core.SaveSystem>();
            managers.AddComponent<Core.CountryDatabase>();
            managers.AddComponent<Core.ObjectPoolManager>();
            managers.AddComponent<Core.PerformanceManager>();
            managers.AddComponent<Game.GameStateManager>();
            managers.AddComponent<Game.WaveController>();
            managers.AddComponent<Game.UpgradeSystem>();
            managers.AddComponent<Game.BossSpawner>();

            // ── EnemySpawner ────────────────────────────────────────
            var spawner = new GameObject("EnemySpawner");
            spawner.AddComponent<Game.EnemySpawner>();

            // ── PlayerSquad ─────────────────────────────────────────
            var squad = new GameObject("PlayerSquad");
            squad.transform.position = new Vector3(0, -3.8f, 0);
            squad.AddComponent<Game.PlayerSquad>();
            squad.AddComponent<Game.WeaponSystem>();

            // Визуальный placeholder убран — бойцы спавнятся из пула

            // ── HUD Canvas ──────────────────────────────────────────
            var canvasGO = new GameObject("HUD_Canvas");
            var canvas   = canvasGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight  = 1f;
            canvasGO.AddComponent<GraphicRaycaster>();
            canvasGO.AddComponent<UI.HUDController>();
            canvasGO.AddComponent<UI.VictoryScreen>();
            canvasGO.AddComponent<Core.SafeAreaAdapter>();

            // HUD элементы
            MakeLabel(canvasGO.transform, "SquadLabel",  "Отряд: 5",  36, new Vector2(-300, -80), new Vector2(400, 70), TextAlignmentOptions.MidlineLeft);
            MakeLabel(canvasGO.transform, "WaveLabel",   "Волна: 1",  36, new Vector2( 300, -80), new Vector2(400, 70), TextAlignmentOptions.MidlineRight);
            MakeLabel(canvasGO.transform, "ScoreLabel",  "Счёт: 0",   48, new Vector2(   0, -80), new Vector2(400, 70), TextAlignmentOptions.Midline);

            // Кнопка паузы
            var pauseBtn = MakeButton(canvasGO.transform, "PauseBtn", "||", new Vector2(460, -50), new Vector2(70, 70));

            // Панель паузы
            var pausePanel = MakePanel(canvasGO.transform, "PauseMenu", new Color(0, 0, 0, 0.75f));
            pausePanel.SetActive(false);
            var resumeBtn  = MakeButton(pausePanel.transform, "ResumeBtn",   "Продолжить",  new Vector2(0,  120), new Vector2(380, 80));
            var restartBtn = MakeButton(pausePanel.transform, "RestartBtn",  "Рестарт",     new Vector2(0,    0), new Vector2(380, 80));
            var menuBtn    = MakeButton(pausePanel.transform, "MainMenuBtn", "Главное меню",new Vector2(0, -120), new Vector2(380, 80));

            // Экран победы
            var victoryPanel = MakePanel(canvasGO.transform, "VictoryPanel", new Color(0, 0, 0, 0.85f));
            victoryPanel.SetActive(false);
            MakeLabel(victoryPanel.transform, "VictoryTitle",   "ПОБЕДА!",          42, new Vector2(0, 200), new Vector2(700, 80), TextAlignmentOptions.Midline);
            var countryLbl  = MakeLabel(victoryPanel.transform, "VictoryCountry", "Цель уничтожена!", 24, new Vector2(0,  80), new Vector2(700, 60), TextAlignmentOptions.Midline);
            var scoreLbl    = MakeLabel(victoryPanel.transform, "VictoryScore",   "Счёт: 0",          22, new Vector2(0, -20), new Vector2(700, 60), TextAlignmentOptions.Midline);
            var playAgainBtn = MakeButton(victoryPanel.transform, "PlayAgainBtn", "Играть снова", new Vector2(0,-140), new Vector2(400, 80));

            // ── Привязка SerializeField: HUDController ───────────────
            var hud   = canvasGO.GetComponent<UI.HUDController>();
            var hudSO = new UnityEditor.SerializedObject(hud);
            hudSO.FindProperty("squadSizeText").objectReferenceValue  = canvasGO.transform.Find("SquadLabel") .GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("waveText")     .objectReferenceValue  = canvasGO.transform.Find("WaveLabel")  .GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("scoreText")    .objectReferenceValue  = canvasGO.transform.Find("ScoreLabel") .GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("pauseButton")  .objectReferenceValue  = pauseBtn  .GetComponent<Button>();
            hudSO.FindProperty("pauseMenu")    .objectReferenceValue  = pausePanel;
            hudSO.FindProperty("resumeButton") .objectReferenceValue  = resumeBtn .GetComponent<Button>();
            hudSO.FindProperty("restartButton").objectReferenceValue  = restartBtn.GetComponent<Button>();
            hudSO.FindProperty("mainMenuButton").objectReferenceValue = menuBtn   .GetComponent<Button>();
            hudSO.ApplyModifiedProperties();

            // ── Привязка SerializeField: VictoryScreen ───────────────
            var vs   = canvasGO.GetComponent<UI.VictoryScreen>();
            var vsSO = new UnityEditor.SerializedObject(vs);
            vsSO.FindProperty("panel")          .objectReferenceValue = victoryPanel;
            vsSO.FindProperty("scoreText")      .objectReferenceValue = scoreLbl    .GetComponent<TMPro.TextMeshProUGUI>();
            vsSO.FindProperty("countryText")    .objectReferenceValue = countryLbl  .GetComponent<TMPro.TextMeshProUGUI>();
            vsSO.FindProperty("playAgainButton").objectReferenceValue = playAgainBtn.GetComponent<Button>();
            vsSO.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green>[DOOM] ✅ GameScene собрана!</color>");

            // Автоматически прописываем конфиги и префабы
            DefaultConfigsCreator.CreateAll();
        }

        // ══════════════════════════════════════════════════════════════
        //  COUNTRY SELECT SCENE
        // ══════════════════════════════════════════════════════════════
        [MenuItem("DOOM/2. Setup CountrySelectScene")]
        public static void SetupCountrySelectScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/CountrySelectScene.unity");
            var scene = SceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
                Object.DestroyImmediate(go);

            // Camera
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic    = true;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.13f);
            cam.transform.position = new Vector3(0, 0, -10);
            camGO.AddComponent<AudioListener>();

            // Managers
            var managers = new GameObject("--- MANAGERS ---");
            managers.AddComponent<Core.GameManager>();
            managers.AddComponent<Core.SaveSystem>();
            managers.AddComponent<Core.CountryDatabase>();

            // Canvas
            var canvasGO = new GameObject("CountrySelect_Canvas");
            var canvas   = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution  = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight   = 1f;
            canvasGO.AddComponent<GraphicRaycaster>();
            canvasGO.AddComponent<UI.CountrySelectScreen>();
            canvasGO.AddComponent<Core.SafeAreaAdapter>();

            // Фон
            var bg = new GameObject("Background");
            bg.transform.SetParent(canvasGO.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.09f, 0.14f);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            // Заголовок
            MakeLabel(canvasGO.transform, "Title", "🌍 Выбери страну", 38,
                new Vector2(0, -80), new Vector2(900, 80), TextAlignmentOptions.Midline);

            // Поиск
            var searchGO = new GameObject("SearchField");
            searchGO.transform.SetParent(canvasGO.transform, false);
            var searchImg = searchGO.AddComponent<Image>();
            searchImg.color = new Color(0.18f, 0.18f, 0.24f);
            var searchRT = searchGO.GetComponent<RectTransform>();
            searchRT.anchorMin = new Vector2(0.05f, 0.86f);
            searchRT.anchorMax = new Vector2(0.95f, 0.92f);
            searchRT.offsetMin = Vector2.zero; searchRT.offsetMax = Vector2.zero;
            var inputField = searchGO.AddComponent<TMP_InputField>();

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(searchGO.transform, false);
            var taRT = textArea.AddComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(15, 0); taRT.offsetMax = new Vector2(-15, 0);
            textArea.AddComponent<RectMask2D>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(textArea.transform, false);
            var textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.fontSize = 22; textTMP.color = Color.white;
            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;

            var phGO = new GameObject("Placeholder");
            phGO.transform.SetParent(textArea.transform, false);
            var phTMP = phGO.AddComponent<TextMeshProUGUI>();
            phTMP.text = "🔍 Поиск по названию...";
            phTMP.fontSize = 22; phTMP.color = new Color(0.5f, 0.5f, 0.5f);
            var phRT = phGO.GetComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
            phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero;

            inputField.textComponent = textTMP;
            inputField.placeholder   = phTMP;

            // ScrollView
            var scrollGO = new GameObject("CountryScrollView");
            scrollGO.transform.SetParent(canvasGO.transform, false);
            var scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.03f, 0.18f);
            scrollRT.anchorMax = new Vector2(0.97f, 0.85f);
            scrollRT.offsetMin = Vector2.zero; scrollRT.offsetMax = Vector2.zero;
            scrollGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f);
            var scrollRect = scrollGO.AddComponent<ScrollRect>();

            var vpGO = new GameObject("Viewport");
            vpGO.transform.SetParent(scrollGO.transform, false);
            var vpRT = vpGO.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = Vector2.zero;
            vpGO.AddComponent<RectMask2D>();

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(vpGO.transform, false);
            var contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot     = new Vector2(0.5f, 1);
            contentRT.offsetMin = Vector2.zero; contentRT.offsetMax = Vector2.zero;
            var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6; vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlHeight = false; vlg.childControlWidth = true;
            var csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport  = vpRT;
            scrollRect.content   = contentRT;
            scrollRect.vertical  = true;
            scrollRect.horizontal = false;

            // Info panel
            var infoPanel = MakePanel(canvasGO.transform, "SelectionInfo", new Color(0.12f, 0.14f, 0.20f, 0.97f));
            var infoPanelRT = infoPanel.GetComponent<RectTransform>();
            infoPanelRT.anchorMin = new Vector2(0.03f, 0.02f);
            infoPanelRT.anchorMax = new Vector2(0.65f, 0.17f);
            infoPanelRT.offsetMin = Vector2.zero; infoPanelRT.offsetMax = Vector2.zero;
            infoPanel.SetActive(false);
            MakeLabel(infoPanel.transform, "SelectedFlag",    "🏳",              36, new Vector2(-200, 15),  new Vector2(60,  50), TextAlignmentOptions.Midline);
            MakeLabel(infoPanel.transform, "SelectedCapital", "Столица: —",      18, new Vector2(  20, 20),  new Vector2(320, 35), TextAlignmentOptions.MidlineLeft);
            MakeLabel(infoPanel.transform, "SelectedTarget",  "Цель: —",         15, new Vector2(  20, -18), new Vector2(320, 30), TextAlignmentOptions.MidlineLeft);

            // Кнопка ИГРАТЬ
            var playBtn = MakeButton(canvasGO.transform, "PlayButton", "▶  ИГРАТЬ", new Vector2(0, 0), new Vector2(300, 100));
            var playRT  = playBtn.GetComponent<RectTransform>();
            playRT.anchorMin = new Vector2(0.67f, 0.02f);
            playRT.anchorMax = new Vector2(0.97f, 0.17f);
            playRT.offsetMin = Vector2.zero; playRT.offsetMax = Vector2.zero;
            playBtn.GetComponent<Image>().color = new Color(0.1f, 0.65f, 0.2f);

            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green>[DOOM] ✅ CountrySelectScene собрана!</color>");
        }

        // ══════════════════════════════════════════════════════════════
        //  BUILD SETTINGS
        // ══════════════════════════════════════════════════════════════
        [MenuItem("DOOM/3. Add Scenes to Build Settings")]
        public static void AddScenesToBuild()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/CountrySelectScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity",          true),
            };
            Debug.Log("<color=green>[DOOM] ✅ Сцены в Build Settings: 0=CountrySelect, 1=GameScene</color>");
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════
        static void MakeSprite(string name, GameObject parent, Color color,
            Vector3 localPos, Vector3 scale, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeTex(color);
            sr.sortingOrder = order;
        }

        static void MakeSprite(string name, GameObject parent, Color color,
            Vector3 localPos, Vector3 scale, int order, out GameObject result)
        {
            MakeSprite(name, parent, color, localPos, scale, order);
            result = parent.transform.Find(name)?.gameObject;
        }

        static Sprite MakeTex(Color color)
        {
            var tex = new Texture2D(4, 4);
            var px  = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = color;
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 1f);
        }

        static GameObject MakePanel(Transform parent, string name, Color color)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = color;
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return go;
        }

        static GameObject MakeLabel(Transform parent, string name, string text,
            float size, Vector2 anchoredPos, Vector2 sizeDelta, TextAlignmentOptions align)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = size;
            tmp.alignment = align;
            tmp.color     = Color.white;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin      = new Vector2(0.5f, 0.5f);
            rt.anchorMax      = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta      = sizeDelta;
            return go;
        }

        static GameObject MakeButton(Transform parent, string name, string label,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = new Color(0.15f, 0.45f, 0.85f);
            go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = sizeDelta;

            var lbl = new GameObject("Label");
            lbl.transform.SetParent(go.transform, false);
            var tmp = lbl.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 22;
            tmp.alignment = TextAlignmentOptions.Midline;
            tmp.color     = Color.white;
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            return go;
        }
    }
}
