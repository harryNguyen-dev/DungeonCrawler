using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace CombatFeel
{
    public sealed class KnockbackAgent
    {
        private readonly Rigidbody rb;
        private readonly NavMeshAgent agent;
        private readonly Transform ownerTransform;
        private int sequenceId;

        public bool IsActive { get; private set; }

        public KnockbackAgent(Rigidbody rb, NavMeshAgent agent, Transform ownerTransform)
        {
            this.rb = rb;
            this.agent = agent;
            this.ownerTransform = ownerTransform;
        }

        public async UniTaskVoid Play(Vector3 direction, float force, float duration, Transform chaseTarget = null)
        {
            if (rb == null || agent == null || ownerTransform == null)
            {
                return;
            }

            Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            if (flatDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            float appliedDuration = Mathf.Max(0.01f, duration);
            int currentSequence = ++sequenceId;
            bool shouldRestoreAgent = false;

            try
            {
                IsActive = true;
                shouldRestoreAgent = true;

                agent.enabled = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(flatDirection * force, ForceMode.Impulse);

                float startTime = Time.time;
                while (Time.time - startTime < appliedDuration)
                {
                    if (ownerTransform == null || currentSequence != sequenceId)
                    {
                        return;
                    }

                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                }
            }
            finally
            {
                if (shouldRestoreAgent && currentSequence == sequenceId)
                {
                    RestoreAgent(chaseTarget);
                    IsActive = false;
                }
            }
        }

        private void RestoreAgent(Transform chaseTarget)
        {
            if (rb == null || agent == null || ownerTransform == null)
            {
                return;
            }

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 targetPosition = ownerTransform.position;
            if (NavMesh.SamplePosition(ownerTransform.position, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
            }

            ownerTransform.position = targetPosition;
            agent.enabled = true;

            if (!agent.isOnNavMesh)
            {
                return;
            }

            agent.Warp(targetPosition);
            agent.nextPosition = targetPosition;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
            agent.isStopped = false;

            if (chaseTarget != null)
            {
                agent.SetDestination(chaseTarget.position);
            }
        }
    }
}
