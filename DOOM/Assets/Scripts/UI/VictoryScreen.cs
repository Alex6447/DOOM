using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DOOM.Game;
using DOOM.Core;

namespace DOOM.UI
{
    /// <summary>
    /// DOOM-5.6 — Экран победы: итоговый счёт, кнопка "Играть снова".
    /// </summary>
    public class VictoryScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI countryText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (panel != null) panel.SetActive(false);
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += OnStateChanged;
            playAgainButton?.onClick.AddListener(OnPlayAgain);
            mainMenuButton?.onClick.AddListener(OnMainMenu);
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.Victory) return;

            if (panel != null) panel.SetActive(true);
            var session = GameManager.Instance?.CurrentSession;
            if (session == null) return;
            if (scoreText != null) scoreText.text = $"Счёт: {session.score}";
            if (countryText != null)
            {
                var country = CountryDatabase.Instance?.GetById(session.selectedCountryId);
                countryText.text = country != null
                    ? $"{country.flag} {country.name} — {country.targetName} уничтожен!"
                    : "Победа!";
            }
        }

        private void OnPlayAgain()
        {
            GameManager.Instance?.SaveGame();
            UnityEngine.SceneManagement.SceneManager.LoadScene("CountrySelectScene");
        }

        private void OnMainMenu() => OnPlayAgain();
    }
}
