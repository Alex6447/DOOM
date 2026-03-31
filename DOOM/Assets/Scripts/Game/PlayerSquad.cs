using System.Collections.Generic;
using UnityEngine;
using DOOM.Core;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-3.4 / DOOM-3.5 — Управление отрядом бойцов.
    /// Горизонтальный свайп смещает весь отряд. Отряд автоматически движется вперёд.
    /// </summary>
    public class PlayerSquad : MonoBehaviour
    {
        [Header("Formation")]
        [SerializeField] private float unitSpacing = 0.6f;
        [SerializeField] private float advanceSpeed = 2f;

        [Header("Boundaries")]
        [SerializeField] private float leftBound = -1.5f;
        [SerializeField] private float rightBound = 1.5f;

        [Header("Touch")]
        [SerializeField] private float swipeSensitivity = 0.015f;

        [Header("Default")]
        [SerializeField] private int defaultSquadSize = 5;

        private List<PlayerUnit> _units = new();
        private int _squadSize;
        private Vector2 _touchStartPos;
        private bool _isDragging;
        private bool _initialized;

        public int SquadSize => _units.Count;

        private System.Collections.IEnumerator Start()
        {
            // Ждём один кадр чтобы ObjectPoolManager.Awake() успел прогреть пулы
            yield return null;
            if (!_initialized)
                Init(defaultSquadSize);
        }

        public void Init(int size)
        {
            _initialized = true;
            _squadSize = size;
            SpawnUnits(size);
        }

        private void SpawnUnits(int count)
        {
            foreach (var u in _units)
                ObjectPoolManager.Instance?.Despawn("player_unit", u.gameObject);
            _units.Clear();

            for (int i = 0; i < count; i++)
            {
                float xOffset = (i - (count - 1) / 2f) * unitSpacing;
                var go = ObjectPoolManager.Instance?.Spawn("player_unit",
                    transform.position + new Vector3(xOffset, 0, 0),
                    Quaternion.identity);
                if (go != null)
                    _units.Add(go.GetComponent<PlayerUnit>());
            }
        }

        private void Update()
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;

            HandleTouch();
            // Отряд стоит внизу, фон скроллится — иллюзия движения
        }

        private void HandleTouch()
        {
#if UNITY_EDITOR
            // Клавиатура — стрелки и A/D
            float keyDelta = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.A)) keyDelta = -80f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) keyDelta =  80f;
            if (keyDelta != 0f) MoveSquad(keyDelta * Time.deltaTime);

            // Мышь — дельта за кадр
            if (Input.GetMouseButtonDown(0)) { _touchStartPos = Input.mousePosition; _isDragging = true; }
            if (Input.GetMouseButton(0) && _isDragging)
            {
                Vector2 current = Input.mousePosition;
                float delta = current.x - _touchStartPos.x;
                if (Mathf.Abs(delta) > 0.5f) MoveSquad(delta);
                _touchStartPos = current;
            }
            if (Input.GetMouseButtonUp(0)) { _isDragging = false; }
#else
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began) { _touchStartPos = t.position; _isDragging = true; }
                if (t.phase == TouchPhase.Moved && _isDragging) MoveSquad(t.deltaPosition.x);
                if (t.phase == TouchPhase.Ended) _isDragging = false;
            }
#endif
        }

        private void MoveSquad(float deltaX)
        {
            float newX = transform.position.x + deltaX * swipeSensitivity;
            newX = Mathf.Clamp(newX, leftBound, rightBound);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        public void OnUnitDied(PlayerUnit unit)
        {
            _units.Remove(unit);
            RearrangeFormation();

            if (_units.Count == 0)
                GameStateManager.Instance?.SetState(Game.GameState.GameOver);
        }

        private void RearrangeFormation()
        {
            int count = _units.Count;
            for (int i = 0; i < count; i++)
            {
                float xOffset = (i - (count - 1) / 2f) * unitSpacing;
                _units[i].transform.localPosition = new Vector3(xOffset, 0, 0);
            }
        }

        public void AddUnits(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float xOffset = (_units.Count - _squadSize / 2f) * unitSpacing;
                var go = ObjectPoolManager.Instance?.Spawn("player_unit",
                    transform.position + new Vector3(xOffset, 0, 0),
                    Quaternion.identity);
                if (go != null) _units.Add(go.GetComponent<PlayerUnit>());
            }
            _squadSize = _units.Count;
            RearrangeFormation();
        }

        public List<PlayerUnit> GetUnits() => _units;
    }
}
