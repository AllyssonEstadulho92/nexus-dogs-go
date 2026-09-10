using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class CompanionFollower : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 followOffset = new Vector3(-1.2f, 0f, -1.6f);
        [SerializeField, Min(0.1f)] private float moveSpeed = 4.5f;
        [SerializeField, Min(0.1f)] private float rotationSpeed = 8f;
        [SerializeField, Min(0f)] private float stoppingDistance = 0.35f;
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParameter = "Speed";

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void Update()
        {
            if (target == null) return;

            var desired = target.TransformPoint(followOffset);
            desired.y = transform.position.y;
            var delta = desired - transform.position;
            var distance = delta.magnitude;

            var normalizedSpeed = 0f;
            if (distance > stoppingDistance)
            {
                var step = Mathf.Min(moveSpeed * Time.deltaTime, distance);
                transform.position += delta.normalized * step;
                normalizedSpeed = Mathf.Clamp01(distance / 2f);

                var look = target.position - transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.001f)
                {
                    var rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
                }
            }

            if (animator != null && !string.IsNullOrWhiteSpace(speedParameter))
                animator.SetFloat(speedParameter, normalizedSpeed, 0.10f, Time.deltaTime);
        }
    }
}
