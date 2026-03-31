using UnityEngine;
using DOOM.Core;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-4.2 — Контроллер врага: движение к отряду, атака, смерть.
    /// </summary>
    public class EnemyController : MonoBehaviour, IPoolable
    {
        private EnemyConfig _config;
        private float _hp;
        private PlayerSquad _target;
        private bool _active;

        public void Init(EnemyConfig config, PlayerSquad target)
        {
            _config = config;
            _hp = config.health;
            _target = target;
            _active = true;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && config.sprite != null) sr.sprite = config.sprite;

            var anim = GetComponent<Animator>();
            if (anim != null && config.animatorController != null)
                anim.runtimeAnimatorController = config.animatorController;
        }

        public void OnSpawn() { _active = true; }
        public void OnDespawn() { _active = false; }

        private void Update()
        {
            if (!_active || _target == null || !GameStateManager.Instance.IsPlaying) return;

            // Движение к отряду
            transform.position = Vector2.MoveTowards(transform.position,
                _target.transform.position, _config.speed * Time.deltaTime);

            // Атака при достижении отряда
            if (Vector2.Distance(transform.position, _target.transform.position) < 0.5f)
                Attack();
        }

        private void Attack()
        {
            var units = _target.GetUnits();
            if (units.Count > 0)
                units[0].TakeDamage(_config.damage * Time.deltaTime);
        }

        public void TakeDamage(float amount)
        {
            _hp -= amount;
            if (_hp <= 0) Die();
        }

        private void Die()
        {
            Core.GameManager.Instance?.AddScore(_config.reward);
            WaveController.Instance?.OnEnemyKilled();
            ObjectPoolManager.Instance?.Despawn(GetPoolKey(), gameObject);
        }

        private string GetPoolKey() => $"enemy_{_config.enemyType.ToString().ToLower()}";
    }
}
