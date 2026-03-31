using UnityEngine;
using DOOM.Data;

namespace DOOM.Core
{
    /// <summary>
    /// Центральный менеджер игры. Хранит текущую сессию, загружает/сохраняет её.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameSession CurrentSession { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Инициализируем сессию сразу в Awake чтобы другие Start() не получили null
            CurrentSession = new GameSession();
        }

        private void Start()
        {
            // Перезаписываем сохранённой сессией если есть
            CurrentSession = SaveSystem.Instance?.Load() ?? CurrentSession;
        }

        public void StartNewGame(string countryId)
        {
            CurrentSession = new GameSession { selectedCountryId = countryId };
        }

        public void SaveGame() => SaveSystem.Instance?.Save(CurrentSession);

        public void AddScore(int amount)
        {
            CurrentSession.score += amount;
        }
    }
}
