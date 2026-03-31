using UnityEngine;

namespace DOOM.Core
{
    /// <summary>
    /// DOOM-6.7 — Адаптация HUD к Safe Area (notch, вырезы экрана).
    /// Attach к корневому RectTransform Canvas'а (или отдельного HUD-слоя).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdapter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            // Пересчитать при изменении (поворот, складной экран)
            if (_lastSafeArea != Screen.safeArea) Apply();
        }

        private void Apply()
        {
            _lastSafeArea = Screen.safeArea;

            Vector2 anchorMin = _lastSafeArea.position;
            Vector2 anchorMax = _lastSafeArea.position + _lastSafeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
        }
    }
}
