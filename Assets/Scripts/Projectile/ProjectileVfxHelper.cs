using System.Collections.Generic;
using UnityEngine;

namespace Projectile
{
    public static class ProjectileVfxHelper
    {
        public static void DestroyAfterParticle(GameObject go)
        {
            if (go == null) return;

            var ps = go.GetComponent<ParticleSystem>();
            if (ps == null && go.transform.childCount > 0)
                ps = go.transform.GetChild(0).GetComponent<ParticleSystem>();

            if (ps != null)
                Object.Destroy(go, ps.main.duration + ps.main.startLifetime.constantMax);
            else
                Object.Destroy(go, 2f);
        }

        public static void PlayMuzzle(GameObject muzzlePrefab, Vector3 position, Vector3 forward)
        {
            if (muzzlePrefab == null) return;

            var muzzle = Object.Instantiate(muzzlePrefab, position, Quaternion.LookRotation(forward));
            muzzle.transform.forward = forward;
            DestroyAfterParticle(muzzle);
        }

        public static void PlayTrails(IReadOnlyList<GameObject> trails)
        {
            if (trails == null) return;

            foreach (var trailObject in trails)
            {
                if (trailObject == null) continue;

                var trail = trailObject.GetComponent<TrailRenderer>();
                if (trail == null) continue;

                trail.Clear();
                trail.emitting = true;
                trail.enabled = true;
            }
        }

        public static void StopTrails(IReadOnlyList<GameObject> trails)
        {
            if (trails == null) return;

            foreach (var trailObject in trails)
            {
                if (trailObject == null) continue;

                var trail = trailObject.GetComponent<TrailRenderer>();
                if (trail == null) continue;

                trail.emitting = false;
            }
        }

        public static void ResetTrails(IReadOnlyList<GameObject> trails)
        {
            if (trails == null) return;

            foreach (var trailObject in trails)
            {
                if (trailObject == null) continue;

                var trail = trailObject.GetComponent<TrailRenderer>();
                if (trail == null) continue;

                trail.emitting = false;
                trail.Clear();
            }
        }

        public static void SpawnHit(GameObject hitPrefab, Vector3 position, Vector3 normal)
        {
            if (hitPrefab == null) return;

            var rot = Quaternion.FromToRotation(Vector3.up, normal);
            var hit = Object.Instantiate(hitPrefab, position, rot);
            DestroyAfterParticle(hit);
        }
    }
}
