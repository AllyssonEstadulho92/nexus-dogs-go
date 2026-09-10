using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class Map3DMarkerEffects : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float bobHeight = 0.12f;
        [SerializeField, Min(0.1f)] private float bobSpeed = 2.2f;
        [SerializeField, Min(0f)] private float spinSpeed = 28f;
        [SerializeField, Min(0f)] private float pulseAmount = 0.08f;
        [SerializeField, Min(0.1f)] private float pulseSpeed = 2.8f;

        private Vector3 _baseLocalPosition;
        private Vector3 _baseLocalScale;
        private float _phase;

        private void Awake()
        {
            _baseLocalPosition = transform.localPosition;
            _baseLocalScale = transform.localScale;
            _phase = Mathf.Abs(GetInstanceID() % 1000) * 0.013f;
        }

        private void OnEnable()
        {
            _baseLocalPosition = transform.localPosition;
            _baseLocalScale = transform.localScale;
        }

        private void Update()
        {
            var t = Time.unscaledTime + _phase;
            transform.localPosition = _baseLocalPosition + Vector3.up * (Mathf.Sin(t * bobSpeed) * bobHeight);
            transform.Rotate(Vector3.up, spinSpeed * Time.unscaledDeltaTime, Space.Self);
            var pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
            transform.localScale = _baseLocalScale * pulse;
        }
    }

    public sealed class Map3DBillboard : MonoBehaviour
    {
        private Camera _camera;

        private void LateUpdate()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;
            var direction = transform.position - _camera.transform.position;
            if (direction.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
