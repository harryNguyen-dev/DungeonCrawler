using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Projectile
{
    public class ProjectileMove : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 2f;

        [Header("VFX")]
        [SerializeField] private GameObject muzzlePrefab;
        [SerializeField] private bool rotate;
        [SerializeField] private float rotateAmount = 45f;
        [SerializeField] private List<GameObject> trails = new();

        private bool canMove = false;
        private bool isBoomerang = false;
        private bool isReturning = false;
        private Transform playerTransform;
        private Action onReturnStart;
        private Action onDespawnRequested;
        private Action<Vector3, Vector3> onEnvironmentHit;
        private CancellationTokenSource disableCTS;

        public void SetSpeed(float speed)
        {
            this.speed = speed;
        }

        public void SetDespawnCallback(Action callback) => onDespawnRequested = callback;

        public void SetEnvironmentHitCallback(Action<Vector3, Vector3> callback) => onEnvironmentHit = callback;

        public void ActiveSelf(bool isBoomerang, Action onReturnStart)
        {
            CancelInvoke();
            this.isBoomerang = isBoomerang;
            this.onReturnStart = onReturnStart;
            this.isReturning = false;
            this.canMove = true;

            PlayMuzzleVfx();
            RestartBodyVfx();

            if (isBoomerang)
            {
                var stats = Global.GlobalEntities.Instance?.PlayerStats;
                playerTransform = stats != null ? stats.transform : null;

                if (playerTransform != null)
                    Invoke(nameof(StartReturnState), lifeTime * 0.5f);
            }
            else
            {
                if (disableCTS == null)
                    disableCTS = new CancellationTokenSource();

                ReturnAfterTime(disableCTS.Token).Forget();
            }
        }

        private async UniTask ReturnAfterTime(CancellationToken cancellationToken)
        {
            bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(lifeTime), cancellationToken: cancellationToken).SuppressCancellationThrow();

            if (isCanceled) return;

            if (gameObject.activeSelf)
                RequestDespawn();
        }

        public void StartReturnState()
        {
            if (isReturning || !isBoomerang) return;

            CancelInvoke(nameof(StartReturnState));
            isReturning = true;
            onReturnStart?.Invoke();
        }

        private void Update()
        {
            if (!canMove) return;

            if (rotate)
                transform.Rotate(0f, 0f, rotateAmount, Space.Self);

            if (isReturning && playerTransform != null)
            {
                Vector3 direction = (playerTransform.position - transform.position).normalized;

                if (direction != Vector3.zero)
                    transform.forward = direction;

                if (!TryMoveForward(speed * 1.5f * Time.deltaTime))
                    return;

                if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
                    RequestDespawn();
            }
            else if (!TryMoveForward(speed * Time.deltaTime))
            {
                return;
            }
        }

        private bool TryMoveForward(float distance)
        {
            if (ProjectileEnvironmentCollision.TryGetBlockHit(transform.position, transform.forward, distance, out var hit))
            {
                canMove = false;
                onEnvironmentHit?.Invoke(hit.point, hit.normal);
                return false;
            }

            transform.Translate(Vector3.forward * distance, Space.Self);
            return true;
        }

        private void PlayMuzzleVfx() =>
            ProjectileVfxHelper.PlayMuzzle(muzzlePrefab, transform.position, transform.forward);

        private void RestartBodyVfx() => ProjectileVfxHelper.RestartProjectileVisuals(gameObject, trails);

        private void StopTrailVfx() => ProjectileVfxHelper.ResetTrails(trails);

        private void RequestDespawn()
        {
            if (onDespawnRequested != null)
                onDespawnRequested.Invoke();
            else
                PoolReturn.SafeReturn(gameObject);
        }

        public void OnSpawnedFromPool()
        {
            disableCTS = new CancellationTokenSource();
        }

        public void OnReturnedToPool()
        {
            CancelInvoke();
            canMove = false;
            isReturning = false;
            playerTransform = null;
            onReturnStart = null;
            onEnvironmentHit = null;
            StopTrailVfx();

            if (disableCTS != null)
            {
                disableCTS.Cancel();
                disableCTS.Dispose();
                disableCTS = null;
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();
            if (disableCTS != null)
            {
                disableCTS.Cancel();
                disableCTS.Dispose();
                disableCTS = null;
            }
        }
    }
}
