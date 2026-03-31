using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DOOM.Core;
using DOOM.Data;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-3.6 — Автоматическая стрельба по ближайшему врагу в радиусе поражения.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField] private float fireRate = 0.5f;   // выстрелов/сек
        [SerializeField] private float damage = 10f;
        [SerializeField] private float range = 5f;
        [SerializeField] private string bulletPoolKey = "bullet";

        private PlayerSquad _squad;
        private float _fireTimer;
        private float _damageMultiplier = 1f;

        private void Awake() => _squad = GetComponent<PlayerSquad>();

        private void Update()
        {
            if (!GameStateManager.Instance.IsPlaying) return;

            _fireTimer += Time.deltaTime;
            if (_fireTimer >= 1f / fireRate)
            {
                _fireTimer = 0;
                FireAtNearestEnemy();
            }
        }

        private void FireAtNearestEnemy()
        {
            var enemy = FindNearestEnemy();
            if (enemy == null) return;

            // Стреляем от каждого живого бойца
            foreach (var unit in _squad.GetUnits())
            {
                var bullet = ObjectPoolManager.Instance?.Spawn(bulletPoolKey,
                    unit.transform.position, Quaternion.identity);
                bullet?.GetComponent<Bullet>()?.Init(enemy.transform, damage * _damageMultiplier);
            }
        }

        private EnemyController FindNearestEnemy()
        {
            var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            EnemyController nearest = null;
            float minDist = range;

            foreach (var e in enemies)
            {
                float dist = Vector2.Distance(transform.position, e.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = e;
                }
            }
            return nearest;
        }

        public void ApplyDamageUpgrade(float multiplier) => _damageMultiplier *= multiplier;
    }
}
