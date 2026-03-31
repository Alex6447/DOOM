using UnityEngine;

namespace DOOM.Game
{
    /// <summary>
    /// DOOM-3.2 — Бесконечный параллакс-скроллинг фона.
    /// Attach к каждому слою фона. Указать scrollSpeed и размер спрайта (tileSize).
    /// </summary>
    public class BackgroundScroller : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 2f;
        [SerializeField] private float tileSize = 10f;   // высота тайла в world-units

        private float _startY;

        private void Start() => _startY = transform.position.y;

        private void Update()
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;

            transform.Translate(0, -scrollSpeed * Time.deltaTime, 0);

            // Зацикливание: если прошли на tileSize вниз — сдвинуть назад
            if (transform.position.y <= _startY - tileSize)
            {
                Vector3 pos = transform.position;
                pos.y += tileSize;
                transform.position = pos;
            }
        }

        public void SetSpeed(float speed) => scrollSpeed = speed;
    }
}
