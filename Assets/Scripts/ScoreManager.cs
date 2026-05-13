using System;
using UnityEngine;

namespace SliceShoot.Core
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public int Score { get; private set; }
        public int ComboCount { get; private set; }
        public bool ScoringEnabled { get; set; } = true;

        public float ComboMultiplier
        {
            get { return 1.0f + Mathf.Max(0, ComboCount - 1) * 0.1f; }
        }

        public event Action<int> OnScoreChanged;
        public event Action<int, Vector3> OnComboHit;
        public event Action OnComboBreak;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Playing)
                ResetAll();
        }

        public void RegisterHit(Vector3 monsterPosition)
        {
            if (!ScoringEnabled) return;
            ComboCount++;
            int points = Mathf.RoundToInt(10 * ComboMultiplier);
            Score += points;
            OnScoreChanged?.Invoke(Score);
            OnComboHit?.Invoke(ComboCount, monsterPosition);
        }

        public void RegisterMiss()
        {
            if (ComboCount > 1)
                OnComboBreak?.Invoke();
            ComboCount = 1;
        }

        private void ResetAll()
        {
            Score = 0;
            ComboCount = 0;
            OnScoreChanged?.Invoke(Score);
        }
    }
}