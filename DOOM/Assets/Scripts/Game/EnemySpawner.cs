using UnityEngine;
using DOOM.Core;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-4.3 — Спавн врагов в верхней части экрана равномерными рядами.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float spawnY = 4.5f;
        [SerializeField] private float corridorHalfWidth = 1.5f;
        [SerializeField] private int enemiesPerRow = 5;

        private int _spawnedInRow;
        private float _rowX;

        private void Start() => ResetRow();

        private void ResetRow()
        {
            _spawnedInRow = 0;
            _rowX = -corridorHalfWidth;
        }

        public void Spawn(EnemyConfig config)
        {
            float spacing = (corridorHalfWidth * 2f) / (enemiesPerRow - 1);
            float x = -corridorHalfWidth + spacing * _spawnedInRow;

            string poolKey = $"enemy_{config.enemyType.ToString().ToLower()}";
            var go = ObjectPoolManager.Instance?.Spawn(poolKey,
                new Vector3(x, spawnY, 0), Quaternion.identity);

            var squad = FindFirstObjectByType<PlayerSquad>();
            go?.GetComponent<EnemyController>()?.Init(config, squad);

            _spawnedInRow++;
            if (_spawnedInRow >= enemiesPerRow) ResetRow();
        }
    }
}
