using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DOOM.Core;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-4.4 / DOOM-4.6 / DOOM-4.11 — Управление волнами врагов.
    /// Количество врагов масштабируется от населения страны.
    /// </summary>
    public class WaveController : MonoBehaviour
    {
        public static WaveController Instance { get; private set; }

        [Header("Wave Settings")]
        [SerializeField] private List<WaveConfig> waveConfigs;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private float timeBetweenWaves = 4f;

        [Header("Population Scaling")]
        [SerializeField] private int baseEnemyCount = 10;
        [SerializeField] private long populationFactor = 10_000_000;

        private int _currentWaveIndex;
        private int _aliveEnemyCount;
        private bool _bossPhase;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Авто-поиск EnemySpawner если не привязан в Inspector
            if (enemySpawner == null)
                enemySpawner = FindFirstObjectByType<EnemySpawner>();

            // Не запускаем волны если нет конфигов (настроить в Inspector)
            if (waveConfigs == null || waveConfigs.Count == 0)
            {
                Debug.LogWarning("[WaveController] WaveConfigs не заданы — волны не запущены. Добавь WaveConfig через Inspector.");
                return;
            }

            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            var country = Core.CountryDatabase.Instance?.GetById(
                Core.GameManager.Instance?.CurrentSession?.selectedCountryId);

            int totalEnemies = baseEnemyCount;
            if (country != null)
                totalEnemies += (int)(country.population / populationFactor);

            for (_currentWaveIndex = 0; _currentWaveIndex < waveConfigs.Count; _currentWaveIndex++)
            {
                if (!GameStateManager.Instance.IsPlaying)
                    yield return new WaitUntil(() => GameStateManager.Instance.IsPlaying);

                Core.GameManager.Instance.CurrentSession.currentWave = _currentWaveIndex + 1;
                var config = waveConfigs[_currentWaveIndex];

                yield return StartCoroutine(SpawnWave(config, totalEnemies));

                // Ждём уничтожения всех рядовых врагов
                yield return new WaitUntil(() => _aliveEnemyCount <= 0);

                // Спавн босса
                yield return StartCoroutine(SpawnBoss(_currentWaveIndex));

                yield return new WaitUntil(() => !_bossPhase);

                GameStateManager.Instance.SetState(GameState.WaveComplete);
                yield return new WaitForSeconds(timeBetweenWaves);
                GameStateManager.Instance.SetState(GameState.Playing);
            }

            GameStateManager.Instance.SetState(GameState.Victory);
        }

        private IEnumerator SpawnWave(WaveConfig config, int populationScaledCount)
        {
            // Перемешиваем и масштабируем количество врагов
            var allEntries = new List<EnemySpawnEntry>(config.enemies);
            int totalFromConfig = 0;
            foreach (var e in allEntries) totalFromConfig += e.count;

            float scaleFactor = (float)populationScaledCount / Mathf.Max(totalFromConfig, 1);

            foreach (var entry in allEntries)
            {
                int count = Mathf.Max(1, Mathf.RoundToInt(entry.count * scaleFactor));
                _aliveEnemyCount += count;
                for (int i = 0; i < count; i++)
                {
                    enemySpawner.Spawn(entry.enemyConfig);
                    yield return new WaitForSeconds(config.spawnInterval);
                }
            }

            // Спавним бочки с улучшениями
            UpgradeSystem.Instance?.SpawnBarrels(config.barrelCount);
        }

        private IEnumerator SpawnBoss(int waveIndex)
        {
            _bossPhase = true;
            BossSpawner.Instance?.SpawnBoss(waveIndex);
            yield return null;
        }

        public void OnEnemyKilled()
        {
            _aliveEnemyCount = Mathf.Max(0, _aliveEnemyCount - 1);
        }

        public void OnBossKilled() => _bossPhase = false;

        public void RestartCurrentWave()
        {
            StopAllCoroutines();
            _aliveEnemyCount = 0;
            _bossPhase = false;
            // Очистить врагов
            foreach (var e in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                ObjectPoolManager.Instance?.Despawn(e.name, e.gameObject);

            GameStateManager.Instance.SetState(GameState.WaveRestart);
            StartCoroutine(DelayedRestartWave());
        }

        private IEnumerator DelayedRestartWave()
        {
            yield return new WaitForSeconds(1.5f);
            GameStateManager.Instance.SetState(GameState.Playing);
            _currentWaveIndex = Mathf.Max(0, _currentWaveIndex - 1);
            StartCoroutine(RunWaves());
        }
    }
}
