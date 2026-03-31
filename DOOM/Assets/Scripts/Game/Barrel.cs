using UnityEngine;
using TMPro;
using DOOM.Core;

namespace DOOM.Game
{
    public enum UpgradeType { Weapon, Defense, SquadSize }

    /// <summary>
    /// DOOM-4.7 / DOOM-4.8 — Бочка с улучшением. Отображает порог попаданий.
    /// При достижении порога мгновенно применяет бонус.
    /// </summary>
    public class Barrel : MonoBehaviour, IPoolable
    {
        [SerializeField] private TextMeshPro hitsLabel;
        [SerializeField] private int hitThreshold = 10;
        [SerializeField] private UpgradeType upgradeType;
        [SerializeField] private float upgradeValue = 1.25f;   // множитель или количество
        [SerializeField] private float moveSpeed = 1.5f;       // движется вниз с врагами

        private int _hits;
        private bool _active;

        public void Init(int threshold, UpgradeType type, float value)
        {
            hitThreshold = threshold;
            upgradeType = type;
            upgradeValue = value;
            _hits = 0;
            UpdateLabel();
        }

        public void OnSpawn() { _active = true; _hits = 0; UpdateLabel(); }
        public void OnDespawn() { _active = false; }

        private void Update()
        {
            if (!_active || !GameStateManager.Instance.IsPlaying) return;
            transform.Translate(0, -moveSpeed * Time.deltaTime, 0);

            // Бочка вышла за нижний край — засчитать как пропущенное улучшение
            if (transform.position.y < -7f)
            {
                UpgradeSystem.Instance?.OnBarrelMissed();
                ObjectPoolManager.Instance?.Despawn("barrel", gameObject);
            }
        }

        public void RegisterHit()
        {
            _hits++;
            UpdateLabel();
            if (_hits >= hitThreshold)
                Activate();
        }

        private void Activate()
        {
            UpgradeSystem.Instance?.ApplyUpgrade(upgradeType, upgradeValue);
            ObjectPoolManager.Instance?.Despawn("barrel", gameObject);
        }

        private void UpdateLabel()
        {
            if (hitsLabel != null)
                hitsLabel.text = (hitThreshold - _hits).ToString();
        }
    }
}
