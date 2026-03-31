using UnityEngine;

namespace DOOM.Game
{
    public enum EnemyType { Zombie, Dog, Lizard, Monster, Guard }

    /// <summary>
    /// DOOM-4.1 — ScriptableObject конфигурации врага.
    /// Создаётся через Assets > Create > DOOM > EnemyConfig.
    /// </summary>
    [CreateAssetMenu(menuName = "DOOM/EnemyConfig", fileName = "EnemyConfig_New")]
    public class EnemyConfig : ScriptableObject
    {
        public EnemyType enemyType;
        public float health;
        public float damage;
        public float speed;
        public int reward;           // очки за убийство
        public Sprite sprite;
        public RuntimeAnimatorController animatorController;
    }
}
