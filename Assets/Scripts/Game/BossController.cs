using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DOOM.Core;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-5.3 / DOOM-5.5 — Контроллер босса.
    /// Многофазный финальный босс (level 6): 3 фазы по HP-порогам.
    /// </summary>
    public class BossController : MonoBehaviour, IPoolable
    {
        [Header("UI")]
        [SerializeField] private Slider hpBar;

        private BossConfig _config;
        private float _hp;
        private float _maxHp;
        private int _phase = 1;
        private bool _shielded;
        private float _abilityTimer;
        private PlayerSquad _target;
        private bool _active;

        public void Init(BossConfig config, PlayerSquad target)
        {
            _config = config;
            _maxHp = config.health;
            _hp = _maxHp;
            _target = target;
            _phase = 1;
            _shielded = false;
            _active = true;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && config.sprite != null) sr.sprite = config.sprite;

            var anim = GetComponent<Animator>();
            if (anim != null && config.animatorController != null)
                anim.runtimeAnimatorController = config.animatorController;

            if (hpBar != null) { hpBar.minValue = 0; hpBar.maxValue = _maxHp; hpBar.value = _hp; }

            StartCoroutine(EntranceAnimation());
        }

        private IEnumerator EntranceAnimation()
        {
            // Въезд сверху экрана
            var start = new Vector3(0, 8f, 0);
            var end   = new Vector3(0, 4f, 0);
            transform.position = start;
            float t = 0;
            while (t < 1f)
            {
                transform.position = Vector3.Lerp(start, end, t);
                t += Time.deltaTime * 2f;
                yield return null;
            }
            transform.position = end;
        }

        public void OnSpawn() { _active = true; }
        public void OnDespawn() { _active = false; }

        private void Update()
        {
            if (!_active || _target == null || !GameStateManager.Instance.IsPlaying) return;

            // Движение к отряду
            transform.position = Vector2.MoveTowards(transform.position,
                _target.transform.position, GetCurrentSpeed() * Time.deltaTime);

            // Атака
            if (Vector2.Distance(transform.position, _target.transform.position) < 0.8f)
                AttackSquad();

            // Способности
            _abilityTimer += Time.deltaTime;
            if (_abilityTimer >= _config.abilityInterval)
            {
                _abilityTimer = 0;
                UseSpecialAbility();
            }

            UpdatePhase();
        }

        private void UpdatePhase()
        {
            if (_config.bossLevel < 6) return;  // Многофазность только у финального

            float ratio = _hp / _maxHp;
            int newPhase = ratio > 0.5f ? 1 : (ratio > 0.25f ? 2 : 3);
            if (newPhase != _phase)
            {
                _phase = newPhase;
                OnPhaseChanged(newPhase);
            }
        }

        private void OnPhaseChanged(int phase)
        {
            Debug.Log($"[Boss] Финальный босс — Фаза {phase}!");
            // Визуальный эффект (частицы, вспышка)
        }

        private float GetCurrentSpeed()
        {
            if (_config.bossLevel < 6) return _config.speed;
            return _phase switch { 2 => _config.speed * 1.5f, 3 => _config.speed * 2.5f, _ => _config.speed };
        }

        private void AttackSquad()
        {
            float dmg = _config.damage * Time.deltaTime;
            if (_phase == 3) dmg *= 3f;  // Берсерк в 3-й фазе
            var units = _target.GetUnits();
            if (units.Count > 0) units[0].TakeDamage(dmg);
        }

        private void UseSpecialAbility()
        {
            switch (_config.specialAbility)
            {
                case SpecialAbilityType.SpawnMinions:
                    // WaveController просит EnemySpawner спавнить подкрепление
                    Debug.Log("[Boss] Призыв подкрепления!");
                    break;
                case SpecialAbilityType.Shield:
                    StartCoroutine(ActivateShield());
                    break;
                case SpecialAbilityType.Charge:
                    StartCoroutine(ChargeAttack());
                    break;
            }
        }

        private IEnumerator ActivateShield()
        {
            _shielded = true;
            Debug.Log("[Boss] Щит активирован!");
            yield return new WaitForSeconds(_config.shieldDuration);
            _shielded = false;
        }

        private IEnumerator ChargeAttack()
        {
            float origSpeed = _config.speed;
            // Визуальный сигнал — затем рывок
            yield return new WaitForSeconds(0.5f);
            transform.position = _target.transform.position;
            AttackSquad();
        }

        public void TakeDamage(float amount)
        {
            if (_shielded) return;

            // В фазе 2 финального босса — призывает миньонов каждый раз при уроне
            if (_config.bossLevel == 6 && _phase == 2 && Random.value < 0.1f)
                UseSpecialAbility();

            _hp -= amount;
            if (hpBar != null) hpBar.value = _hp;

            if (_hp <= 0) Die();
        }

        private void Die()
        {
            _active = false;
            WaveController.Instance?.OnBossKilled();
            Core.GameManager.Instance?.AddScore(1000 * _config.bossLevel);

            if (_config.bossLevel == 6)
                GameStateManager.Instance.SetState(GameState.Victory);

            ObjectPoolManager.Instance?.Despawn($"boss_{_config.bossLevel}", gameObject);
        }
    }
}
