using System.Threading;
using Cysharp.Threading.Tasks;
using Projectile;
using SO;
using UnityEngine;

namespace PlayerController
{
    /// <summary>Spawns hero visual prefab under VisualContainer and rebinds animator / fire point.</summary>
    public class HeroVisualController : MonoBehaviour
    {
        public const string FirePointName = "FirePoint";
        public const string SkillEffectLoopName = "SkillEffectLoop";
        public const string MedicalEffectName = "MedicalEffect";

        [SerializeField] private Transform visualContainer;
        [SerializeField] private Transform fallbackFirePoint;

        private GameObject activeVisual;
        private GameObject skillEffectLoop;
        private GameObject medicalEffect;
        private PlayerAnimation playerAnimation;
        private Attack attack;
        private PlayerEvents playerEvents;
        private CancellationTokenSource medicalEffectCts;

        private void Awake()
        {
            playerAnimation = GetComponent<PlayerAnimation>();
            attack = GetComponent<Attack>();
            playerEvents = GetComponent<PlayerEvents>();
            RebindMedicalEffect();
        }

        private void OnEnable()
        {
            if (playerEvents != null)
                playerEvents.OnHealHealth += HandleHealHealth;
        }

        private void OnDisable()
        {
            if (playerEvents != null)
                playerEvents.OnHealHealth -= HandleHealHealth;

            StopMedicalEffectPlayback();
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
            skillEffectLoop = null;
        }

        public void SetSkillEffectLoopActive(bool active)
        {
            if (skillEffectLoop == null)
                return;

            if (skillEffectLoop.activeSelf == active)
                return;

            skillEffectLoop.SetActive(active);
            if (active)
                ProjectileVfxHelper.RestartParticleSystems(skillEffectLoop);
        }

        public void PlayMedicalEffect()
        {
            if (medicalEffect == null)
                return;

            StopMedicalEffectPlayback();
            medicalEffect.SetActive(true);
            ProjectileVfxHelper.RestartParticleSystems(medicalEffect);

            medicalEffectCts = new CancellationTokenSource();
            HideMedicalEffectAfterDelay(medicalEffectCts.Token).Forget();
        }

        private void HandleHealHealth(int amount)
        {
            if (amount > 0)
                PlayMedicalEffect();
        }

        private void RebindMedicalEffect()
        {
            var medicalTransform = FindChildByName(transform, MedicalEffectName);
            if (medicalTransform == null)
            {
                medicalEffect = null;
                return;
            }

            medicalEffect = medicalTransform.gameObject;
            medicalEffect.SetActive(false);
        }

        private async UniTask HideMedicalEffectAfterDelay(CancellationToken token)
        {
            try
            {
                var duration = GetParticleEffectDuration(medicalEffect);
                await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: token);
                if (!token.IsCancellationRequested && medicalEffect != null)
                    medicalEffect.SetActive(false);
            }
            catch (System.OperationCanceledException)
            {
            }
        }

        private void StopMedicalEffectPlayback()
        {
            medicalEffectCts?.Cancel();
            medicalEffectCts?.Dispose();
            medicalEffectCts = null;
        }

        private static float GetParticleEffectDuration(GameObject root)
        {
            if (root == null)
                return 1.5f;

            var duration = 1.5f;
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                var particleDuration = main.duration + main.startLifetime.constantMax;
                if (particleDuration > duration)
                    duration = particleDuration;
            }

            return duration;
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
            RebindSkillEffectLoop(searchRoot);
        }

        private void RebindSkillEffectLoop(Transform searchRoot)
        {
            skillEffectLoop = null;
            if (searchRoot == null)
                return;

            var loopTransform = FindChildByName(searchRoot, SkillEffectLoopName);
            if (loopTransform == null)
                return;

            skillEffectLoop = loopTransform.gameObject;
            skillEffectLoop.SetActive(false);
        }

        private static Transform FindFirePoint(Transform root) =>
            FindChildByName(root, FirePointName) ?? FindChildByName(root, "Fire point");

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
                return null;

            var direct = root.Find(childName);
            if (direct != null)
                return direct;

            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == childName)
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
