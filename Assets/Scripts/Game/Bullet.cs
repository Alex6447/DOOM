using UnityEngine;
using DOOM.Core;

namespace DOOM.Game
{
    /// <summary>
    /// Снаряд. Летит к цели, наносит урон при столкновении, возвращается в пул.
    /// </summary>
    public class Bullet : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 10f;

        private Transform _target;
        private float _damage;
        private bool _active;

        public void Init(Transform target, float damage)
        {
            _target = target;
            _damage = damage;
            _active = true;
        }

        public void OnSpawn() { _active = false; }
        public void OnDespawn() { _active = false; _target = null; }

        private void Update()
        {
            if (!_active || _target == null)
            {
                ReturnToPool();
                return;
            }

            transform.position = Vector2.MoveTowards(transform.position,
                _target.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, _target.position) < 0.1f)
            {
                _target.GetComponent<EnemyController>()?.TakeDamage(_damage);
                _target.GetComponent<BossController>()?.TakeDamage(_damage);
                ReturnToPool();
            }
        }

        private void ReturnToPool() =>
            ObjectPoolManager.Instance?.Despawn("bullet", gameObject);
    }
}
