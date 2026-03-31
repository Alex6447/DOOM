using UnityEngine;

namespace DOOM.Game
{
    public enum SpecialAbilityType { None, SpawnMinions, Shield, Charge, MultiPhase }

    /// <summary>
    /// DOOM-5.1 / DOOM-5.2 — ScriptableObject конфигурации босса.
    /// bossLevel: 1 (Мелкий чиновник) … 6 (Президент).
    /// </summary>
    [CreateAssetMenu(menuName = "DOOM/BossConfig", fileName = "BossConfig_New")]
    public class BossConfig : ScriptableObject
    {
        [Header("Identity")]
        public int bossLevel;           // 1–6
        public string displayName;

        [Header("Stats")]
        public float health;
        public float damage;
        public float speed;

        [Header("Special")]
        public SpecialAbilityType specialAbility;
        public float abilityInterval = 5f;  // интервал применения способности
        public int minionCount = 3;         // для SpawnMinions
        public float shieldDuration = 2f;   // для Shield

        [Header("Art")]
        public Sprite sprite;
        public RuntimeAnimatorController animatorController;
    }
}
