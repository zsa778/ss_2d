using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SliceShoot.Core
{
    public enum GameState { Playing, Paused, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState CurrentState { get; private set; } = GameState.Playing;
        public event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void PauseGame()   { SetState(GameState.Paused); }
        public void ResumeGame()  { SetState(GameState.Playing); }
        public void TriggerGameOver() { SetState(GameState.GameOver); }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void SetState(GameState state)
        {
            CurrentState = state;
            OnGameStateChanged?.Invoke(state);
        }
    }
}
