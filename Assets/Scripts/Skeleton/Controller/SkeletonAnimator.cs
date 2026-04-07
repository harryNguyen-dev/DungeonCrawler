using UnityEngine;
namespace Enemy
{
    public class SkeletonAnimator : MonoBehaviour
    {
        Animator _anim;
        static readonly int TakeDamage = Animator.StringToHash("TakeDamage");

        void Awake() => _anim = GetComponentInChildren<Animator>();

        public void TriggerTakeDamage() => _anim.SetTrigger(TakeDamage);

    }

}