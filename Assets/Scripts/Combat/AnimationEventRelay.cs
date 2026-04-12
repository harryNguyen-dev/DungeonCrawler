using UnityEngine;
namespace Combat
{
    public class AnimationEventRelay : MonoBehaviour
    {
        ICombatEvents _combat;

        void Awake()
        {
            // Leo lên parent để tìm CombatController
            _combat = GetComponentInParent<ICombatEvents>();
        }

        // Animation Event gọi thẳng vào đây
        public void AE_AttackEnd() 
        {
            _combat.AE_AttackEnd();
        } 

        // Thêm các event khác khi cần
        public void AE_HitboxOn() => _combat.AE_HitboxOn();
        public void AE_HitboxOff() => _combat.AE_HitboxOff();
        public void AE_ParryEnd() => _combat.AE_ParryEnd();

        public void AE_HitReactionEnd() => _combat.AE_HitReactionEnd();
        public void AE_ParryStart() => _combat.AE_ParryStart();
        public void AE_ParryStop() => _combat.AE_ParryStop();
    }

}