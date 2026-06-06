using SO;
using UnityEngine;

namespace PlayerController
{
    /// <summary>Spawns hero visual prefab under VisualContainer and rebinds animator / fire point.</summary>
    public class HeroVisualController : MonoBehaviour
    {
        public const string FirePointName = "FirePoint";

        [SerializeField] private Transform visualContainer;
        [SerializeField] private Transform fallbackFirePoint;

        private GameObject activeVisual;
        private PlayerAnimation playerAnimation;
        private Attack attack;

        private void Awake()
        {
            playerAnimation = GetComponent<PlayerAnimation>();
            attack = GetComponent<Attack>();
        }

        public void ApplyHeroVisual(HeroSO hero)
        {
            ClearVisual();

            if (ShouldSpawnVisualPrefab(hero))
            {
                activeVisual = Instantiate(hero.visualPrefab, visualContainer);
                activeVisual.transform.localPosition = hero.visualLocalPosition;
                activeVisual.transform.localEulerAngles = hero.visualLocalEulerAngles;
                activeVisual.transform.localScale = hero.visualLocalScale;
            }

            RebindFromHierarchy();
        }

        public void ClearVisual()
        {
            if (activeVisual != null)
                Destroy(activeVisual);

            activeVisual = null;
        }

        private bool ShouldSpawnVisualPrefab(HeroSO hero)
        {
            if (hero == null || hero.visualPrefab == null || visualContainer == null)
                return false;

            if (hero.visualPrefab.GetComponent<PlayerStats>() != null)
                return false;

            return true;
        }

        private void EnsurePlayerRefs()
        {
            if (playerAnimation == null)
                playerAnimation = GetComponent<PlayerAnimation>();
            if (attack == null)
                attack = GetComponent<Attack>();
        }

        private void RebindFromHierarchy()
        {
            EnsurePlayerRefs();

            Transform searchRoot = activeVisual != null ? activeVisual.transform : transform;

            var animator = searchRoot.GetComponentInChildren<Animator>();
            playerAnimation?.RebindAnimator(animator);

            var firePoint = FindFirePoint(searchRoot);
            if (firePoint == null && visualContainer != null)
                firePoint = FindFirePoint(visualContainer);

            BindFirePoint(firePoint != null ? firePoint : fallbackFirePoint);
        }

        private static Transform FindFirePoint(Transform root)
        {
            if (root == null)
                return null;

            var direct = root.Find(FirePointName);
            if (direct != null)
                return direct;

            var legacy = root.Find("Fire point");
            if (legacy != null)
                return legacy;

            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == FirePointName || t.name == "Fire point")
                    return t;
            }

            return null;
        }

        private void BindFirePoint(Transform firePoint)
        {
            EnsurePlayerRefs();
            attack?.SetFirePoint(firePoint);
        }
    }
}
