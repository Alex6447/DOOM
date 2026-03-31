using System.Collections;
using UnityEngine;

namespace DOOM.Core
{
    /// <summary>
    /// DOOM-6.1 — Мониторинг FPS и адаптивное качество графики.
    /// При FPS &lt; 55 снижает качество, при FPS &gt; 58 восстанавливает.
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        [Header("FPS Thresholds")]
        [SerializeField] private float lowFpsThreshold  = 55f;
        [SerializeField] private float highFpsThreshold = 58f;
        [SerializeField] private float sampleWindow     = 1f;   // секунд

        private float _fpsAccum;
        private int   _frames;
        private float _currentFps;
        private int   _qualityLevel = 2;  // 0=low, 1=mid, 2=high

        private void Start() => StartCoroutine(MeasureFps());

        private IEnumerator MeasureFps()
        {
            while (true)
            {
                _fpsAccum = 0;
                _frames   = 0;

                float elapsed = 0;
                while (elapsed < sampleWindow)
                {
                    elapsed    += Time.unscaledDeltaTime;
                    _fpsAccum  += 1f / Time.unscaledDeltaTime;
                    _frames++;
                    yield return null;
                }

                _currentFps = _fpsAccum / _frames;
                AdjustQuality();
            }
        }

        private void AdjustQuality()
        {
            if (_currentFps < lowFpsThreshold && _qualityLevel > 0)
            {
                _qualityLevel--;
                ApplyQuality(_qualityLevel);
            }
            else if (_currentFps > highFpsThreshold && _qualityLevel < 2)
            {
                _qualityLevel++;
                ApplyQuality(_qualityLevel);
            }
        }

        private void ApplyQuality(int level)
        {
            switch (level)
            {
                case 0: // Low
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.particleRaycastBudget = 16;
                    Application.targetFrameRate = 30;
                    break;
                case 1: // Mid
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.particleRaycastBudget = 64;
                    Application.targetFrameRate = 60;
                    break;
                case 2: // High
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.particleRaycastBudget = 256;
                    Application.targetFrameRate = 60;
                    break;
            }
            Debug.Log($"[Performance] Уровень качества: {level}, FPS: {_currentFps:0.#}");
        }
    }
}
