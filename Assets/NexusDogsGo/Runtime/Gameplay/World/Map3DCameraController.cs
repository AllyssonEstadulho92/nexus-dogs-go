using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class Map3DCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Range(25f, 75f)] private float pitch = 52f;
        [SerializeField] private float yaw = 18f;
        [SerializeField, Min(2f)] private float distance = 20f;
        [SerializeField, Min(2f)] private float minimumDistance = 8f;
        [SerializeField, Min(4f)] private float maximumDistance = 42f;
        [SerializeField, Min(0.1f)] private float followSmoothness = 10f;
        [SerializeField, Min(0.01f)] private float rotationSensitivity = 0.18f;
        [SerializeField, Min(0.01f)] private float zoomSensitivity = 0.018f;

        private Vector3 _velocity;
        private Vector2 _lastPointer;
        private bool _dragging;
        private float _previousPinchDistance;

        public void SetTarget(Transform value)
        {
            target = value;
        }

        private void LateUpdate()
        {
            HandleInput();
            UpdateCamera();
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
            pitch = Mathf.Clamp(pitch - delta.y * rotationSensitivity, 30f, 72f);
        }

        private void UpdateCamera()
        {
            var focus = target != null ? target.position : Vector3.zero;
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var desired = focus + rotation * new Vector3(0f, 0f, -distance);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, 1f / followSmoothness);
            transform.rotation = Quaternion.LookRotation((focus - transform.position).normalized, Vector3.up);
        }
    }
}
