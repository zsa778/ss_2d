using System;
using System.Collections;
using UnityEngine;
using SliceShoot.Input;

namespace SliceShoot.Core
{
    public enum GameMode { Adventure, Endless }

    [System.Serializable]
    public class StageData
    {
        public int bossScoreThreshold;
    }

    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

        [SerializeField] private GameMode _gameMode = GameMode.Endless;
        [SerializeField] private StageData[] _stages;
        [SerializeField] private TouchInputManager _inputManager;
        [SerializeField] private float _bossZoomSize = 2f;
        [SerializeField] private float _cameraMoveTime = 1f;
        [SerializeField] private float _spawnAnimDuration = 3f;
        [SerializeField] private float _cameraReturnTime = 1f;

        public GameMode Mode { get { return _gameMode; } }
        public int CurrentStage { get { return _currentStageIndex + 1; } }
        public bool BossActive { get { return _bossActive; } }

        public event Action<int> OnStageCleared;
        public event Action OnBossSpawning;

        private int _currentStageIndex = 0;
        private bool _bossActive = false;
        private bool _bossSequenceRunning = false;
        private SliceShoot.Monster.Monster _activeBoss = null;
        private CameraAnimator _cameraAnimator;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _cameraAnimator = GetComponent<CameraAnimator>();
            if (_cameraAnimator == null) _cameraAnimator = gameObject.AddComponent<CameraAnimator>();
        }

        private void Start()
        {
            if (_gameMode == GameMode.Adventure)
                ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(int score)
        {
            if (_bossActive || _bossSequenceRunning) return;
            if (_currentStageIndex >= _stages.Length) return;

            StageData stage = _stages[_currentStageIndex];
            if (score >= stage.bossScoreThreshold)
            {
                _bossSequenceRunning = true;
                StartCoroutine(BossSpawnSequence(stage));
            }
        }

        private IEnumerator BossSpawnSequence(StageData stage)
        {
            _bossActive = true;
            ScoreManager.Instance.ScoringEnabled = false;
            SliceShoot.Monster.MonsterSpawner.Instance.BlockRegularSpawning = true;
            OnBossSpawning?.Invoke();

            // Block all player input during cinematic
            if (_inputManager != null) _inputManager.InputEnabled = false;
            if (SliceShoot.Character.Character.Instance != null) SliceShoot.Character.Character.Instance.InputEnabled = false;

            byte bossNum = (byte)(_currentStageIndex + 1);
            SliceShoot.Monster.MonsterData bossData = SliceShoot.Monster.MonsterSpawner.Instance.GetBossData(bossNum);
            if (bossData == null)
            {
                _bossActive = false;
                _bossSequenceRunning = false;
                SliceShoot.Monster.MonsterSpawner.Instance.BlockRegularSpawning = false;
                ScoreManager.Instance.ScoringEnabled = true;
                if (_inputManager != null) _inputManager.InputEnabled = true;
                if (SliceShoot.Character.Character.Instance != null) SliceShoot.Character.Character.Instance.InputEnabled = true;
                yield break;
            }

            // Freeze movement and stop animations on all current monsters
            float stunDuration = _cameraMoveTime + _spawnAnimDuration;
            SliceShoot.Monster.MonsterSpawner.Instance.FreezeAllMonsters(stunDuration);
            SliceShoot.Monster.MonsterSpawner.Instance.SetAllMonstersAnimSpeed(0f);

            // Spawn boss — wait one frame so body-part colliders can compute their
            // rest-pose positions from the skeleton before we freeze them.
            _activeBoss = SliceShoot.Monster.MonsterSpawner.Instance.SpawnBoss(bossData, bossData.spawnPosition);
            if (_activeBoss != null)
            {
                _activeBoss.SetCombatVisualsVisible(false);
                _activeBoss.OnDied += OnBossDied;
            }
            yield return null; // let MonsterBodyPart.LateUpdate run once at rest pose

            if (_activeBoss != null)
                _activeBoss.FreezePosition(stunDuration);

            // Camera moves to boss position and zooms in (1s)
            yield return StartCoroutine(_cameraAnimator.LerpTo(bossData.spawnPosition, _bossZoomSize, _cameraMoveTime));

            // Boss plays spawn animation while camera holds (3s)
            if (_activeBoss != null)
            {
                string trigger = bossData.spawnAnimTrigger;
                if (!string.IsNullOrEmpty(trigger))
                    _activeBoss.TriggerSpawnAnimation(trigger);
            }
            yield return new WaitForSeconds(_spawnAnimDuration);

            // Camera returns, restore monster animations and player input
            yield return StartCoroutine(_cameraAnimator.LerpTo(_cameraAnimator.OriginalPos, _cameraAnimator.OriginalSize, _cameraReturnTime));

            SliceShoot.Monster.MonsterSpawner.Instance.SetAllMonstersAnimSpeed(1f);
            SliceShoot.Monster.MonsterSpawner.Instance.BlockRegularSpawning = false;
            SliceShoot.Monster.MonsterSpawner.Instance.RefillToTarget();
            if (_activeBoss != null) _activeBoss.SetCombatVisualsVisible(true);
            if (_inputManager != null) _inputManager.InputEnabled = true;
            if (SliceShoot.Character.Character.Instance != null) SliceShoot.Character.Character.Instance.InputEnabled = true;

            _bossSequenceRunning = false;
        }

        private void OnBossDied(SliceShoot.Monster.Monster boss)
        {
            boss.OnDied -= OnBossDied;
            _activeBoss = null;
            _bossActive = false;
            ScoreManager.Instance.ScoringEnabled = true;

            int clearedStage = _currentStageIndex + 1;
            _currentStageIndex++;
            OnStageCleared?.Invoke(clearedStage);
        }
    }
}
