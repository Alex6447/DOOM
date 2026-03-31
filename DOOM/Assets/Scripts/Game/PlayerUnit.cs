using UnityEngine;
using DOOM.Core;

namespace DOOM.Game
{
    /// <summary>
    /// Один боец отряда. Имеет HP, обрабатывает урон, умирает и возвращается в пул.
    /// </summary>
    public class PlayerUnit : MonoBehaviour, IPoolable
    {
        [SerializeField] private float maxHp = 500f;
        private float _hp;

        public bool IsAlive => _hp > 0;

        public void OnSpawn()
        {
            _hp = maxHp;
            gameObject.SetActive(true);
        }

        public void OnDespawn()
        {
            gameObject.SetActive(false);
        }

        public void TakeDamage(float amount)
        {
            _hp -= amount;
            if (_hp <= 0) Die();
        }

        private void Die()
        {
            // Сообщить отряду о гибели
            GetComponentInParent<PlayerSquad>()?.OnUnitDied(this);
            ObjectPoolManager.Instance?.Despawn("player_unit", gameObject);
        }

        public void ApplyDefenseUpgrade(float multiplier)
        {
            maxHp *= multiplier;
            _hp = Mathf.Min(_hp * multiplier, maxHp);
        }
    }
}
