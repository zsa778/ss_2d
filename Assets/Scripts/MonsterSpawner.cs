using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SliceShoot.Monster
{
    public class MonsterSpawner : MonoBehaviour
    {
        public static MonsterSpawner Instance { get; private set; }
        public bool BlockRegularSpawning { get; set; }

        [Header("Monster Pool")]
        [SerializeField] private MonsterData[] _monsterPool;
        [SerializeField] private MonsterData[] _bossPool;

        [Header("Stage Settings")]
        [SerializeField] private int _baseMonsters = 2;
        [SerializeField] private int _monstersPerStage = 1;
        [SerializeField] private int _maxMonstersCapacity = 10;
        [SerializeField] private float _respawnDelay = 0.5f;
        [SerializeField] private float _bossRespawnDelay = 1f;

        // Zone 1 = Ring 1, full circle
        // Zone 2 = Ring 2, top sector    (aTR → aTL)
        // Zone 3 = Ring 2, left sector   (aTL → aBL)
        // Zone 4 = Ring 2, bottom sector (aBL → aBR)
        // Zone 5 = Ring 2, right sector  (aBR → aTR, wraps)
        // Zone 6 = Ring 3, full circle
        [Header("Spawn Rings (camera-relative units)")]
        [SerializeField] private float _ring1Inner = 0.2f;
        [SerializeField] private float _ring1Outer = 0.7f;
        [SerializeField] private float _ring2Inner = 0.85f;
        [SerializeField] private float _ring2Outer = 1.55f;
        [SerializeField] private float _ring3Inner = 1.7f;
        [SerializeField] private float _ring3Outer = 2.2f;

        [Header("Sector Angles (degrees, CCW from right)")]
        [SerializeField] private float _angleTopRight = 60f;
        [SerializeField] private float _angleTopLeft  = 120f;
        [SerializeField] private float _angleBotLeft  = 240f;
        [SerializeField] private float _angleBotRight = 300f;

        private int _maxMonsters;
        private readonly List<Monster> _activeMonsters = new List<Monster>();
        private Camera _cam;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _maxMonsters = _baseMonsters;
        }

        private void Start()
        {
            _cam = Camera.main;
            RefillToTarget();
        }

        public void SetStage(int stage)
        {
            _maxMonsters = Mathf.Min(_baseMonsters + (stage - 1) * _monstersPerStage, _maxMonstersCapacity);
            RefillToTarget();
        }

        public void RefillToTarget()
        {
            if (BlockRegularSpawning || _monsterPool == null || _monsterPool.Length == 0) return;
            int toSpawn = _maxMonsters - _activeMonsters.Count;
            for (int i = 0; i < toSpawn; i++)
                SpawnRandom();
        }

        private void SpawnRandom()
        {
            MonsterData data = _monsterPool[Random.Range(0, _monsterPool.Length)];
            int zone = (data.spawnZones != null && data.spawnZones.Length > 0)
                ? data.spawnZones[Random.Range(0, data.spawnZones.Length)]
                : 1;
            SpawnAt(data, PositionInZone(zone));
        }

        private Monster SpawnAt(MonsterData data, Vector3 position)
        {
            if (data.prefab == null) return null;
            Monster m = Instantiate(data.prefab, position, Quaternion.identity);
            m.Initialize(data);
            m.OnDied += OnMonsterDied;
            _activeMonsters.Add(m);
            return m;
        }

        private void OnMonsterDied(Monster m)
        {
            _activeMonsters.Remove(m);
            if (m.Data.bossNum > 0 || BlockRegularSpawning) return;
            float delay = IsBossFightActive() ? _bossRespawnDelay : _respawnDelay;
            StartCoroutine(DelayedSpawn(delay));
        }

        private IEnumerator DelayedSpawn(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!BlockRegularSpawning && _activeMonsters.Count < _maxMonsters)
                SpawnRandom();
        }

        private bool IsBossFightActive()
        {
            foreach (var m in _activeMonsters)
                if (m != null && m.Data != null && m.Data.bossNum > 0) return true;
            return false;
        }

        private Vector3 PositionInZone(int zone)
        {
            if (_cam == null) _cam = Camera.main;
            float unit = _cam.orthographicSize * 0.5f;

            float rMin, rMax, aMin, aMax;
            bool wrap = false;

            switch (zone)
            {
                case 1:  rMin = _ring1Inner; rMax = _ring1Outer; aMin = 0f;            aMax = 360f;           break;
                case 2:  rMin = _ring2Inner; rMax = _ring2Outer; aMin = _angleTopRight; aMax = _angleTopLeft;  break;
                case 3:  rMin = _ring2Inner; rMax = _ring2Outer; aMin = _angleTopLeft;  aMax = _angleBotLeft;  break;
                case 4:  rMin = _ring2Inner; rMax = _ring2Outer; aMin = _angleBotLeft;  aMax = _angleBotRight; break;
                case 5:  rMin = _ring2Inner; rMax = _ring2Outer; aMin = _angleBotRight; aMax = _angleTopRight; wrap = true; break;
                case 6:  rMin = _ring3Inner; rMax = _ring3Outer; aMin = _angleTopLeft;  aMax = _angleBotLeft;  break;
                case 7:  rMin = _ring3Inner; rMax = _ring3Outer; aMin = _angleBotRight; aMax = _angleTopRight; wrap = true; break;
                default: rMin = _ring1Inner; rMax = _ring1Outer; aMin = 0f;            aMax = 360f;           break;
            }

            float r = Random.Range(rMin, rMax) * unit;
            float angle;
            if (wrap)
            {
                float span = (360f - aMin) + aMax;
                float raw  = aMin + Random.Range(0f, span);
                angle = raw >= 360f ? raw - 360f : raw;
            }
            else if (aMax - aMin >= 359.9f)
            {
                angle = Random.Range(0f, 360f);
            }
            else
            {
                angle = Random.Range(aMin, aMax);
            }

            float rad = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad) * r, Mathf.Sin(rad) * r, 0f);
        }

        public void StunAllMonsters(float duration)
        {
            foreach (var m in _activeMonsters) if (m != null) m.Stun(duration);
        }

        public void FreezeAllMonsters(float duration)
        {
            foreach (var m in _activeMonsters) if (m != null) m.FreezePosition(duration);
        }

        public void SetAllMonstersAnimSpeed(float speed)
        {
            foreach (var m in _activeMonsters) if (m != null) m.SetAnimSpeed(speed);
        }

        public IReadOnlyList<Monster> GetActiveMonsters() => _activeMonsters;

        public float GetSpawnZone1Outer()
        {
            if (_cam == null) _cam = Camera.main;
            return _ring1Outer * (_cam != null ? _cam.orthographicSize * 0.5f : 1f);
        }

        public Monster SpawnBoss(MonsterData data, Vector3 position) => SpawnAt(data, position);

        public MonsterData GetBossData(byte bossNum)
        {
            if (_bossPool == null) return null;
            foreach (var d in _bossPool)
                if (d != null && d.bossNum == bossNum) return d;
            return null;
        }
    }
}
