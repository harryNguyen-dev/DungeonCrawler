using Core;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Combat
{
    public class HitboxController : MonoBehaviour
    {
        [SerializeField] Collider _collider;
        [SerializeField] CameraShakeController _cameraShake;
        public int damage = 30;
        public float knockback = 5f;
        public float verticalKnockback = 1.5f;

        HashSet<IDamageReceiver> _hitThisSwing = new();

        void Awake() {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            SetActive(false);
        }

        public void SetActive(bool value) {
            _collider.enabled = value;
            if (!value) _hitThisSwing.Clear();
        }

        void OnTriggerEnter(Collider other) {
            if (!other.TryGetComponent<IDamageReceiver>(out var receiver)) return;
            if (_hitThisSwing.Contains(receiver)) return;

            _hitThisSwing.Add(receiver);
            
            Vector3 dir = (other.transform.position - transform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                dir.Normalize();

            var info = new DamageHitInfo(damage, dir, knockback, verticalKnockback);
            CombatUtils.HitStop(0.07f).Forget();
            receiver.ReceiveHit(in info);
            _cameraShake?.PlayHitShake();

            SpawnHitVfx(other, dir);
        }

        void SpawnHitVfx(Collider other, Vector3 knockDirHorizontal)
        {
            var mgr = ObjectPoolingManager.Instance;
            if (mgr == null) return;

            Vector3 pos = other.ClosestPoint(transform.position);
            Quaternion rot = knockDirHorizontal.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(knockDirHorizontal)
                : transform.rotation;

            var vfx = mgr.Get(PoolId.Vfx_Generic, pos, rot);
            if (vfx == null) return;
            ReturnVfxWhenComplete(vfx).Forget();
        }

        static async UniTaskVoid ReturnVfxWhenComplete(GameObject vfx)
        {
            var systems = vfx.GetComponentsInChildren<ParticleSystem>(true);
            for (var i = 0; i < systems.Length; i++)
            {
                if (!systems[i].isPlaying)
                    systems[i].Play(true);
            }

            if (systems.Length > 0)
            {
                await UniTask.WaitUntil(() =>
                {
                    for (var i = 0; i < systems.Length; i++)
                    {
                        if (systems[i] != null && systems[i].IsAlive(true))
                            return false;
                    }
                    return true;
                });
            }
            else
                await UniTask.WaitForSeconds(1f);

            if (vfx != null && ObjectPoolingManager.Instance != null)
                ObjectPoolingManager.Instance.Return(vfx);
        }
    }

    public static class CombatUtils 
    {
        static bool isWaiting = false;
        public static async UniTask HitStop(float duration) 
        {
            if (isWaiting) return;
            isWaiting = true;
            Time.timeScale = 0f;
            await UniTask.WaitForSeconds(duration, ignoreTimeScale: true);
            Time.timeScale = 1f;
            isWaiting = false;
        }
    }
}