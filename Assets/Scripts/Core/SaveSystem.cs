using System;
using UnityEngine;
using DOOM.Data;

namespace DOOM.Core
{
    /// <summary>
    /// DOOM-1.3 — Сохранение/загрузка GameSession через PlayerPrefs (JSON).
    /// Автосохранение на OnApplicationPause(true).
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        private const string SaveKey = "doom_game_session";

        public static SaveSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                Save(GameManager.Instance?.CurrentSession);
        }

        public void Save(GameSession session)
        {
            if (session == null) return;
            string json = JsonUtility.ToJson(session);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public GameSession Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
                return new GameSession();

            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<GameSession>(json) ?? new GameSession();
        }

        public bool HasSave() => PlayerPrefs.HasKey(SaveKey);

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }
    }
}
