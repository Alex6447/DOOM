using System.Collections.Generic;
using UnityEngine;

namespace DOOM.Game
{
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public EnemyConfig enemyConfig;
        public int count;
    }

    /// <summary>
    /// DOOM-4.5 — ScriptableObject конфигурации волны.
    /// </summary>
    [CreateAssetMenu(menuName = "DOOM/WaveConfig", fileName = "WaveConfig_New")]
    public class WaveConfig : ScriptableObject
    {
        public int waveNumber;
        public List<EnemySpawnEntry> enemies = new();
        public float spawnInterval = 0.5f;
        public int barrelCount = 2;
    }
}
