using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SO;
using UnityEngine;

namespace PlayerController.Skill
{
    public class PlayerTimedBuffTracker : MonoBehaviour
    {
        private sealed class ActiveBuff
        {
            public float EndTime;
            public List<AppliedStatDelta> Deltas = new();
            public CancellationTokenSource Cts;
        }

        private PlayerStats stats;
        private HeroVisualController heroVisual;
        private readonly Dictionary<string, ActiveBuff> activeBuffs = new();

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            heroVisual = GetComponent<HeroVisualController>();
        }

        public bool TryApplyBuff(string sourceId, BuffSkillConfig config)
        {
            if (stats == null || config.modifiers == null || config.duration <= 0f)
                return false;

            if (string.IsNullOrEmpty(sourceId))
                sourceId = "skill_buff";

            if (activeBuffs.TryGetValue(sourceId, out var existing))
            {
                if (!config.refreshOnReuse)
                    return false;

                existing.EndTime = Time.time + config.duration;
                existing.Cts?.Cancel();
                existing.Cts?.Dispose();
                existing.Cts = new CancellationTokenSource();
                UpdateSkillEffectLoop(sourceId, true);
                RunBuffTimer(sourceId, config.duration, existing.Cts.Token).Forget();
                return true;
            }

            var buff = new ActiveBuff();
            foreach (var modifier in config.modifiers)
                buff.Deltas.Add(StatModifierApplier.Apply(stats, modifier));

            buff.EndTime = Time.time + config.duration;
            buff.Cts = new CancellationTokenSource();
            activeBuffs[sourceId] = buff;
            UpdateSkillEffectLoop(sourceId, true);
            RunBuffTimer(sourceId, config.duration, buff.Cts.Token).Forget();
            return true;
        }

        private async UniTask RunBuffTimer(string sourceId, float duration, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: token);
                if (!token.IsCancellationRequested)
                    RemoveBuff(sourceId);
            }
            catch (System.OperationCanceledException)
            {
            }
        }

        private void RemoveBuff(string sourceId)
        {
            if (!activeBuffs.TryGetValue(sourceId, out var buff))
                return;

            foreach (var delta in buff.Deltas)
                StatModifierApplier.Revert(stats, delta);

            buff.Cts?.Cancel();
            buff.Cts?.Dispose();
            activeBuffs.Remove(sourceId);
            UpdateSkillEffectLoop(sourceId, false);
        }

        private void UpdateSkillEffectLoop(string sourceId, bool active)
        {
            var skill = stats?.ActiveSkill;
            if (skill == null || skill.skillId != sourceId)
                return;

            heroVisual?.SetSkillEffectLoopActive(active);
        }

        private void OnDestroy()
        {
            foreach (var key in new List<string>(activeBuffs.Keys))
                RemoveBuff(key);
        }
    }
}
