using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DOOM.Game;
using DOOM.Core;

namespace DOOM.UI
{
    /// <summary>
    /// DOOM-3.9 — HUD: счётчик отряда, номер волны, счёт.
    /// DOOM-3.8 — Кнопка паузы.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI squadSizeText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button pauseButton;

        [Header("Pause Menu")]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private PlayerSquad _squad;

        private void Start()
        {
            _squad = FindFirstObjectByType<PlayerSquad>();

            // Авто-поиск по имени если не задано в Inspector
            var t = transform;
            if (squadSizeText == null) squadSizeText = FindTMP(t, "SquadLabel");
            if (waveText      == null) waveText      = FindTMP(t, "WaveLabel");
            if (scoreText     == null) scoreText     = FindTMP(t, "ScoreLabel");
            if (pauseButton   == null) pauseButton   = FindBtn(t, "PauseBtn");
            if (pauseMenu     == null) pauseMenu     = FindGO(t,  "PauseMenu");
            if (resumeButton  == null) resumeButton  = FindBtn(t, "ResumeBtn");
            if (restartButton == null) restartButton = FindBtn(t, "RestartBtn");
            if (mainMenuButton== null) mainMenuButton= FindBtn(t, "MainMenuBtn");

            pauseButton?.onClick.AddListener(() => GameStateManager.Instance?.TogglePause());
            resumeButton?.onClick.AddListener(() => GameStateManager.Instance?.SetState(GameState.Playing));
            restartButton?.onClick.AddListener(OnRestart);
            mainMenuButton?.onClick.AddListener(OnMainMenu);

            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += OnStateChanged;

            if (pauseMenu != null) pauseMenu.SetActive(false);
        }

        private TextMeshProUGUI FindTMP(Transform root, string n)
        {
            var go = root.Find(n);
            return go != null ? go.GetComponent<TextMeshProUGUI>() : null;
        }
        private Button FindBtn(Transform root, string n)
        {
            // Ищем рекурсивно (кнопки могут быть внутри PauseMenu)
            var found = root.GetComponentsInChildren<Button>(true);
            foreach (var b in found)
                if (b.name == n) return b;
            return null;
        }
        private GameObject FindGO(Transform root, string n)
        {
            var t = root.Find(n);
            return t != null ? t.gameObject : null;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= OnStateChanged;
        }

        private void Update()
        {
            if (_squad == null) _squad = FindFirstObjectByType<PlayerSquad>();

            if (_squad != null && squadSizeText != null)
                squadSizeText.text = $"Отряд: {_squad.SquadSize}";

            var session = GameManager.Instance?.CurrentSession;
            if (session == null) return;
            if (scoreText != null) scoreText.text = $"Счёт: {session.score}";
            if (waveText  != null) waveText.text  = $"Волна: {session.currentWave}";
        }

        private void OnStateChanged(GameState state)
        {
            if (pauseMenu != null) pauseMenu.SetActive(state == GameState.Paused);
        }

        public void RefreshAfterUpgrade()
        {
            // Принудительное обновление HUD после получения улучшения
        }

        private void OnRestart()
        {
            GameStateManager.Instance?.SetState(GameState.Playing);
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void OnMainMenu()
        {
            GameStateManager.Instance?.SetState(GameState.Playing);
            UnityEngine.SceneManagement.SceneManager.LoadScene("CountrySelectScene");
        }
    }
}
