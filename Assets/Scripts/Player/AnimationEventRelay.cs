using Combat;
using UnityEngine;
namespace Player
{

    public class AnimationEventRelay : MonoBehaviour
    {
        CombatController _combat;

        void Awake()
        {
            // Leo lên parent để tìm CombatController
            _combat = GetComponentInParent<CombatController>();
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
    }

}