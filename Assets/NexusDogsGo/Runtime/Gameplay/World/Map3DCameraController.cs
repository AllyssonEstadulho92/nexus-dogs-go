using UnityEngine;
using UnityEngine.EventSystems;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class Map3DCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Range(25f, 75f)] private float pitch = 54f;
        [SerializeField] private float yaw = 18f;
        [SerializeField, Min(2f)] private float distance = 17f;
        [SerializeField, Min(2f)] private float minimumDistance = 7f;
        [SerializeField, Min(4f)] private float maximumDistance = 38f;
        [SerializeField, Min(0.1f)] private float followSmoothness = 11f;
        [SerializeField, Min(0.01f)] private float rotationSensitivity = 0.16f;
        [SerializeField, Min(0.01f)] private float zoomSensitivity = 0.018f;
        [SerializeField, Min(0f)] private float lookAhead = 1.25f;

        private Vector3 _velocity;
        private Vector2 _lastPointer;
        private bool _dragging;
        private float _previousPinchDistance;
        private Vector3 _lastTargetPosition;
        private Vector3 _targetMotion;

        public void SetTarget(Transform value)
        {
            target = value;
            if (target != null) _lastTargetPosition = target.position;
        }

        public void ResetView()
        {
            yaw = 18f;
            pitch = 54f;
            distance = 17f;
        }

        public void ZoomIn()
        {
            distance = Mathf.Clamp(distance - 3f, minimumDistance, maximumDistance);
        }

        public void ZoomOut()
        {
            distance = Mathf.Clamp(distance + 3f, minimumDistance, maximumDistance);
        }

        private void LateUpdate()
        {
            TrackTargetMotion();
            HandleInput();
            UpdateCamera();
        }

        private void TrackTargetMotion()
        {
            if (target == null) return;
            var delta = target.position - _lastTargetPosition;
            delta.y = 0f;
            _targetMotion = Vector3.Lerp(_targetMotion, delta, Time.unscaledDeltaTime * 5f);
            _lastTargetPosition = target.position;
        }

        private void HandleInput()
        {
            if (Input.touchCount >= 2)
            {
                var first = Input.GetTouch(0);
                var second = Input.GetTouch(1);
                var pinchDistance = Vector2.Distance(first.position, second.position);
                if (_previousPinchDistance > 0f)
                {
                    var delta = pinchDistance - _previousPinchDistance;
                    distance = Mathf.Clamp(distance - delta * zoomSensitivity, minimumDistance, maximumDistance);
                }
                _previousPinchDistance = pinchDistance;
                _dragging = false;
                return;
            }

            _previousPinchDistance = 0f;

            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
                    _lastPointer = touch.position;
                    _dragging = true;
                }
                else if (_dragging && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
                {
                    RotateFromPointer(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    _dragging = false;
                }
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f)
                distance = Mathf.Clamp(distance - scroll * 1.8f, minimumDistance, maximumDistance);

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                _lastPointer = Input.mousePosition;
                _dragging = true;
            }
            else if (Input.GetMouseButton(0) && _dragging)
            {
                RotateFromPointer(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _dragging = false;
            }
#endif
        }

        private void RotateFromPointer(Vector2 current)
        {
            var delta = current - _lastPointer;
            _lastPointer = current;
            yaw += delta.x * rotationSensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * rotationSensitivity, 32f, 70f);
        }

        private void UpdateCamera()
        {
            var baseFocus = target != null ? target.position : Vector3.zero;
            var motionDirection = _targetMotion.sqrMagnitude > 0.00001f ? _targetMotion.normalized : Vector3.zero;
            var focus = baseFocus + motionDirection * lookAhead;

            var normalizedZoom = Mathf.InverseLerp(minimumDistance, maximumDistance, distance);
            var dynamicPitch = Mathf.Lerp(pitch - 5f, pitch + 4f, normalizedZoom);
            var rotation = Quaternion.Euler(dynamicPitch, yaw, 0f);
            var desired = focus + rotation * new Vector3(0f, 0f, -distance);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, 1f / followSmoothness);

            var look = focus - transform.position;
            if (look.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
        }
    }
}
