using System.Collections.Generic;
using UnityEngine;
using DOOM.Core;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-5.4 — Спавн боссов по номеру волны и масштабу страны.
    /// Маленькие страны: 3–4 уровня, крупные: 5–6.
    /// </summary>
    public class BossSpawner : MonoBehaviour
    {
        public static BossSpawner Instance { get; private set; }

        [SerializeField] private List<BossConfig> bossConfigs;  // индекс = bossLevel-1

        [Header("Population Thresholds")]
        [SerializeField] private long smallCountryMax = 50_000_000;
        [SerializeField] private long largeCountryMin = 200_000_000;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SpawnBoss(int waveIndex)
        {
            int bossLevel = DetermineBossLevel(waveIndex);
            if (bossLevel < 1 || bossLevel > bossConfigs.Count) return;

            var config = bossConfigs[bossLevel - 1];
            string poolKey = $"boss_{bossLevel}";
            var go = ObjectPoolManager.Instance?.Spawn(poolKey,
                new Vector3(0, 8f, 0), Quaternion.identity);

            var squad = FindFirstObjectByType<PlayerSquad>();
            go?.GetComponent<BossController>()?.Init(config, squad);
        }

        private int DetermineBossLevel(int waveIndex)
        {
            var country = CountryDatabase.Instance?.GetById(
                GameManager.Instance?.CurrentSession.selectedCountryId);

            int maxLevel = 4;
            if (country != null)
            {
                if (country.population >= largeCountryMin)  maxLevel = 6;
                else if (country.population >= smallCountryMax) maxLevel = 5;
            }

            // Последняя волна — всегда финальный босс
            int totalWaves = FindFirstObjectByType<WaveController>() != null
                ? maxLevel : maxLevel;

            if (waveIndex >= maxLevel - 1) return maxLevel;

            // Равномерное распределение уровней
            return Mathf.Clamp(waveIndex + 1, 1, maxLevel);
        }
    }
}
