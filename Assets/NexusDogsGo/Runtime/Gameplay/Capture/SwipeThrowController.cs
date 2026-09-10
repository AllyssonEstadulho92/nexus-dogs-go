using UnityEngine;
using UnityEngine.EventSystems;

namespace NexusDogsGo.Gameplay.Capture
{
    public sealed class SwipeThrowController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private CaptureEncounterController encounter;
        [SerializeField, Range(0.05f, 0.5f)] private float minimumSwipeScreenRatio = 0.10f;
        [SerializeField, Range(0.10f, 1f)] private float excellentSwipeScreenRatio = 0.42f;
        [SerializeField, Range(0f, 1f)] private float horizontalToleranceRatio = 0.40f;

        private Vector2 _start;
        private float _startTime;
        private bool _tracking;

        public void OnPointerDown(PointerEventData eventData)
        {
            _start = eventData.position;
            _startTime = Time.unscaledTime;
            _tracking = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_tracking) return;
            _tracking = false;

            var delta = eventData.position - _start;
            var duration = Mathf.Max(0.05f, Time.unscaledTime - _startTime);
            var quality = Evaluate(delta, duration, Mathf.Max(1f, Screen.height));
            if (encounter != null) encounter.Attempt(quality);
        }

        public ThrowQuality Evaluate(Vector2 delta, float durationSeconds, float screenHeight)
        {
            if (screenHeight <= 0f) return ThrowQuality.Miss;

            var upward = delta.y / screenHeight;
            var sideways = Mathf.Abs(delta.x) / screenHeight;
            if (upward < minimumSwipeScreenRatio || delta.y <= 0f) return ThrowQuality.Miss;
            if (sideways > upward * (1f + horizontalToleranceRatio)) return ThrowQuality.Miss;

            var speed = upward / Mathf.Max(0.05f, durationSeconds);
            var precision = Mathf.Clamp01(1f - (sideways / Mathf.Max(0.001f, upward)));
            var score = upward * 1.55f + Mathf.Clamp01(speed) * 0.25f + precision * 0.20f;

            if (upward >= excellentSwipeScreenRatio && score >= 0.95f) return ThrowQuality.Excellent;
            if (score >= 0.72f) return ThrowQuality.Great;
            if (score >= 0.50f) return ThrowQuality.Nice;
            return ThrowQuality.Normal;
        }
    }
}
