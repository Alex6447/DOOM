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
            pauseButton.onClick.AddListener(() => GameStateManager.Instance.TogglePause());
            resumeButton.onClick.AddListener(() => GameStateManager.Instance.SetState(GameState.Playing));
            restartButton.onClick.AddListener(OnRestart);
            mainMenuButton.onClick.AddListener(OnMainMenu);

            GameStateManager.Instance.OnStateChanged += OnStateChanged;
            pauseMenu.SetActive(false);
        }

        private void OnDestroy() =>
            GameStateManager.Instance.OnStateChanged -= OnStateChanged;

        private void Update()
        {
            if (_squad != null)
                squadSizeText.text = $"Отряд: {_squad.SquadSize}";

            if (GameManager.Instance != null)
            {
                scoreText.text = $"Счёт: {GameManager.Instance.CurrentSession.score}";
                waveText.text  = $"Волна: {GameManager.Instance.CurrentSession.currentWave}";
            }
        }

        private void OnStateChanged(GameState state)
        {
            pauseMenu.SetActive(state == GameState.Paused);
        }

        public void RefreshAfterUpgrade()
        {
            // Принудительное обновление HUD после получения улучшения
        }

        private void OnRestart()
        {
            GameStateManager.Instance.SetState(GameState.Playing);
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void OnMainMenu()
        {
            GameStateManager.Instance.SetState(GameState.Playing);
            UnityEngine.SceneManagement.SceneManager.LoadScene("CountrySelectScene");
        }
    }
}
