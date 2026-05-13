using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CharacterClass = SliceShoot.Character.Character;
using MonsterClass = SliceShoot.Monster.Monster;

namespace SliceShoot.Input
{
    public class TouchInputManager : MonoBehaviour
    {
        public static TouchInputManager Instance { get; private set; }

        private const float TAP_MAX_PX        = 10f;
        private const float TAP_MAX_TIME       = 0.3f;
        private const float SLIDE_MIN_PX       = 30f;
        private const float PATH_INTERVAL      = 0.1f;
        private const float HOLD_MIN_TIME      = 0.5f;
        private const float SLIDE_AREA_MIN_PX  = 50f;
        private const float CHAR_TOUCH_RADIUS  = 0.5f;

        private class Pointer
        {
            public Vector2 startScreen;
            public Vector2 currentScreen;
            public Vector3 startWorld;
            public float startTime;
            public readonly List<Vector3> path = new List<Vector3>();
            public Vector3 lastRecorded;
            public bool holdFired;
            public bool isOnCharacter;
            public MonsterClass touchedMonster;
        }

        public bool InputEnabled { get; set; } = true;

        private readonly Dictionary<int, Pointer> _pointers = new Dictionary<int, Pointer>();
        private Camera _cam;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start() => _cam = Camera.main;

        private void Update()
        {
            if (!InputEnabled) return;
            HandleMouse();
            HandleTouch();
            CheckHolds();
        }

        private void HandleMouse()
        {
            if (Mouse.current == null) return;
            var pos = Mouse.current.position.ReadValue();
            if (Mouse.current.leftButton.wasPressedThisFrame)       Begin(0, pos);
            else if (Mouse.current.leftButton.isPressed)             Move(0, pos);
            else if (Mouse.current.leftButton.wasReleasedThisFrame)  End(0, pos);
        }

        private void HandleTouch()
        {
            if (Touchscreen.current == null) return;
            foreach (var t in Touchscreen.current.touches)
            {
                int id = t.touchId.ReadValue() + 1;
                var pos = t.position.ReadValue();
                if (t.press.wasPressedThisFrame)       Begin(id, pos);
                else if (t.press.isPressed)             Move(id, pos);
                else if (t.press.wasReleasedThisFrame)  End(id, pos);
            }
        }

        private void Begin(int id, Vector2 screen)
        {
            var world = ToWorld(screen);
            var p = new Pointer
            {
                startScreen   = screen,
                currentScreen = screen,
                startWorld    = world,
                startTime     = Time.time,
                lastRecorded  = world
            };
            p.path.Add(world);

            if (CharacterClass.Instance != null &&
                Vector3.Distance(world, CharacterClass.Instance.transform.position) < CHAR_TOUCH_RADIUS)
            {
                p.isOnCharacter = true;
            }
            else
            {
                var col = Physics2D.OverlapPoint(new Vector2(world.x, world.y));
                p.touchedMonster = col?.GetComponent<MonsterClass>();
            }

            _pointers[id] = p;
        }

        private void Move(int id, Vector2 screen)
        {
            if (!_pointers.TryGetValue(id, out var p)) return;
            p.currentScreen = screen;
            var world = ToWorld(screen);
            if (Vector3.Distance(p.lastRecorded, world) >= PATH_INTERVAL)
            {
                p.path.Add(world);
                p.lastRecorded = world;
            }
        }

        private void End(int id, Vector2 screen)
        {
            if (!_pointers.TryGetValue(id, out var p)) return;
            _pointers.Remove(id);

            p.currentScreen = screen;
            p.path.Add(ToWorld(screen));

            if (p.isOnCharacter)
            {
                OnCharacterFire(p);
                return;
            }

            if (p.holdFired) return;

            float px  = Vector2.Distance(p.startScreen, screen);
            float dur = Time.time - p.startTime;

            if (px < TAP_MAX_PX && dur < TAP_MAX_TIME)
                OnTap(p);
            else if (px >= SLIDE_MIN_PX)
                OnSlide(p);
        }

        private void CheckHolds()
        {
            foreach (var kvp in _pointers)
            {
                var p = kvp.Value;
                if (p.holdFired || p.isOnCharacter || p.touchedMonster == null) continue;
                if (Time.time - p.startTime < HOLD_MIN_TIME) continue;
                if (Vector2.Distance(p.startScreen, p.currentScreen) >= SLIDE_MIN_PX) continue;

                p.holdFired = true;
                OnHold(p);
            }
        }

        private void OnHold(Pointer p)
        {
            var monster = p.touchedMonster;
            if (monster == null) return;
            if (monster.TryHit(2, p.startWorld))
                monster.FreezePosition(2f);
            else
                SliceShoot.Core.ScoreManager.Instance?.RegisterMiss();
        }

        private void OnCharacterFire(Pointer p)
        {
            if (CharacterClass.Instance == null) return;
            bool enhanced = (Time.time - p.startTime) >= HOLD_MIN_TIME;
            Vector3 charPos = CharacterClass.Instance.transform.position;
            Vector3 fingerEnd = ToWorld(p.currentScreen);
            Vector3 dir = (charPos - fingerEnd).normalized;
            if (dir.sqrMagnitude < 0.001f) return;
            CharacterClass.Instance.FireProjectile(dir, enhanced);
        }

        private void OnTap(Pointer p)
        {
            var col = Physics2D.OverlapPoint(new Vector2(p.startWorld.x, p.startWorld.y));
            var monster = col?.GetComponent<MonsterClass>();
            if (monster != null && !monster.TryHit(0, p.startWorld))
                SliceShoot.Core.ScoreManager.Instance?.RegisterMiss();
        }

        private void OnSlide(Pointer p)
        {
            var hit = new HashSet<MonsterClass>();
            bool hitAny = false;
            for (int i = 0; i < p.path.Count - 1; i++)
            {
                foreach (var h in Physics2D.LinecastAll(p.path[i], p.path[i + 1]))
                {
                    var monster = h.collider?.GetComponent<MonsterClass>();
                    if (monster != null && hit.Add(monster))
                    {
                        var hp = new Vector3(h.point.x, h.point.y, 0f);
                        if (monster.TryHit(1, hp)) hitAny = true;
                    }
                }
            }
            if (!hitAny && hit.Count == 0)
                SliceShoot.Core.ScoreManager.Instance?.RegisterMiss();

            TrySlideAreaAttack(p);
        }

        private void TrySlideAreaAttack(Pointer p)
        {
            if (SliceShoot.Monster.MonsterSpawner.Instance == null) return;
            if (CharacterClass.Instance == null) return;

            float zone1Outer = SliceShoot.Monster.MonsterSpawner.Instance.GetSpawnZone1Outer();
            Vector3 charPos = CharacterClass.Instance.transform.position;

            if (Vector3.Distance(p.startWorld, charPos) >= zone1Outer) return;
            if (Vector2.Distance(p.startScreen, p.currentScreen) < SLIDE_AREA_MIN_PX) return;

            Vector2 startFromChar = new Vector2(p.startWorld.x - charPos.x, p.startWorld.y - charPos.y);
            Vector3 slideEnd = ToWorld(p.currentScreen);
            Vector2 slideDir = new Vector2(slideEnd.x - p.startWorld.x, slideEnd.y - p.startWorld.y);

            float cross = startFromChar.x * slideDir.y - startFromChar.y * slideDir.x;
            bool isCW = cross < 0f;

            float touchAngle = Mathf.Atan2(startFromChar.y, startFromChar.x) * Mathf.Rad2Deg;
            float sectorCenter = isCW ? touchAngle - 35f : touchAngle + 35f;

            float knockbackWorld = 0.3f;
            if (_cam != null)
                knockbackWorld = 30f * (2f * _cam.orthographicSize) / Screen.height;

            var monsters = SliceShoot.Monster.MonsterSpawner.Instance.GetActiveMonsters();
            for (int i = 0; i < monsters.Count; i++)
            {
                var m = monsters[i];
                if (m == null) continue;

                Vector3 toMonster = m.transform.position - charPos;
                float dist = toMonster.magnitude;
                if (dist > zone1Outer) continue;

                float monsterAngle = Mathf.Atan2(toMonster.y, toMonster.x) * Mathf.Rad2Deg;
                if (Mathf.Abs(Mathf.DeltaAngle(monsterAngle, sectorCenter)) > 45f) continue;

                Vector3 knockDir = dist > 0.001f ? toMonster / dist : Vector3.up;
                if (m.TryHit(4, m.transform.position))
                    m.Knockback(knockDir, knockbackWorld, 0.5f);
            }
        }

        private Vector3 ToWorld(Vector2 screen)
        {
            if (_cam == null) _cam = Camera.main;
            var w = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_cam.transform.position.z));
            return new Vector3(w.x, w.y, 0f);
        }
    }
}
