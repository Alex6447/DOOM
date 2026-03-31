using System;
using UnityEngine;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-3.7 — Менеджер состояний игры.
    /// </summary>
    public enum GameState
    {
        Playing,
        Paused,
        WaveRestart,
        WaveComplete,
        GameOver,
        Victory
    }

    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Playing;

        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;

            // DOOM-3.8 — пауза через TimeScale
            Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;

            OnStateChanged?.Invoke(newState);
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)  SetState(GameState.Paused);
            else if (CurrentState == GameState.Paused) SetState(GameState.Playing);
        }

        public bool IsPlaying => CurrentState == GameState.Playing;
    }
}
