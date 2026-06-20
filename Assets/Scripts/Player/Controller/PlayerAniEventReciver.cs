using UnityEngine;
using Cysharp.Threading.Tasks;

namespace PlayerController
{
    public class PlayerAniEventReciver : MonoBehaviour
    {
        private Attack attack;
        private Rotate playerRotate;

        private void Awake()
        {
            attack = GetComponentInParent<Attack>();
            playerRotate = GetComponentInParent<Rotate>();
        }

        public void AE_OnAttack()
        {
            if (attack == null)
                attack = GetComponentInParent<Attack>();
            if (playerRotate == null)
                playerRotate = GetComponentInParent<Rotate>();

            playerRotate?.SnapFaceAimDirection();
            Core.GameAudio.PlayPlayerAttack(transform.position);
            attack?.SpawnProjectile().Forget();
        }
    }
}