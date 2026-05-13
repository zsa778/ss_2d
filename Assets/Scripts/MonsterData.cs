
using UnityEngine;

namespace SliceShoot.Monster
{
    [CreateAssetMenu(fileName = "NewMonster", menuName = "SliceShoot/MonsterData")]
    public class MonsterData : ScriptableObject
    {
        public string monsterName;
        public byte monsterNum;       // unique ID: 1+ for normal, 0 for boss
        public byte bossNum;          // 0 for normal, 1+ for boss (level appearance order)
        public float speed;           // world units per second
        public byte[] hitTypes;       // e.g. [0], [0,1], [1,2,0]
        public int[] spawnZones;      // which zones (1-7) this monster can spawn in
        public Monster prefab;        // per-type prefab (for unique colliders, etc.)
        public Vector3 spawnPosition; // world position for boss spawn cinematic
        public string spawnAnimTrigger; // Animator trigger name played during boss spawn
    }
}
