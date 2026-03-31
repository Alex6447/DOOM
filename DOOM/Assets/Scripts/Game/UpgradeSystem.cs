using System.Collections.Generic;
using UnityEngine;
using DOOM.Core;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-4.9 / DOOM-4.10 / DOOM-4.11 — Система улучшений (бочки).
    /// Считает пропущенные улучшения. При 3 пропусках — перезапуск волны.
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        public static UpgradeSystem Instance { get; private set; }

        [Header("Barrel Spawn")]
        [SerializeField] private float corridorHalfWidth = 1.2f;
        [SerializeField] private float spawnY = 5f;
        [SerializeField] private int hitThresholdMin = 8;
        [SerializeField] private int hitThresholdMax = 15;

        [Header("Upgrade Values")]
        [SerializeField] private float weaponMultiplier = 1.3f;
        [SerializeField] private float defenseMultiplier = 1.25f;
        [SerializeField] private int squadSizeBonus = 3;

        private int _missedUpgrades;
        private PlayerSquad _squad;
        private WeaponSystem _weapon;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _squad = FindFirstObjectByType<PlayerSquad>();
            _weapon = FindFirstObjectByType<WeaponSystem>();
        }

        public void SpawnBarrels(int count)
        {
            var types = new[] { UpgradeType.Weapon, UpgradeType.Defense, UpgradeType.SquadSize };
            for (int i = 0; i < count; i++)
            {
                float x = Random.Range(-corridorHalfWidth, corridorHalfWidth);
                var go = ObjectPoolManager.Instance?.Spawn("barrel",
                    new Vector3(x, spawnY, 0), Quaternion.identity);
                if (go == null) continue;

                var barrel = go.GetComponent<Barrel>();
                var type = types[Random.Range(0, types.Length)];
                float val = type switch
                {
                    UpgradeType.Weapon    => weaponMultiplier,
                    UpgradeType.Defense   => defenseMultiplier,
                    UpgradeType.SquadSize => squadSizeBonus,
                    _                    => 1f
                };
                barrel.Init(Random.Range(hitThresholdMin, hitThresholdMax + 1), type, val);
            }
        }

        public void ApplyUpgrade(UpgradeType type, float value)
        {
            _missedUpgrades = 0;
            GameManager.Instance.CurrentSession.missedUpgrades = 0;

            switch (type)
            {
                case UpgradeType.Weapon:
                    _weapon?.ApplyDamageUpgrade(value);
                    GameManager.Instance.CurrentSession.squadDamageMultiplier *= value;
                    break;
                case UpgradeType.Defense:
                    foreach (var u in _squad.GetUnits()) u.ApplyDefenseUpgrade(value);
                    GameManager.Instance.CurrentSession.squadDefenseMultiplier *= value;
                    break;
                case UpgradeType.SquadSize:
                    _squad.AddUnits((int)value);
                    GameManager.Instance.CurrentSession.squadSize = _squad.SquadSize;
                    break;
            }

            // Визуальный эффект
            Debug.Log($"[Upgrade] Применено: {type} x{value}");
        }

        public void OnBarrelMissed()
        {
            _missedUpgrades++;
            GameManager.Instance.CurrentSession.missedUpgrades = _missedUpgrades;

            if (_missedUpgrades >= 3)
            {
                _missedUpgrades = 0;
                WaveController.Instance?.RestartCurrentWave();
            }
        }
    }
}
