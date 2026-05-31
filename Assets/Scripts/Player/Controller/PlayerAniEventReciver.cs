using UnityEngine;
using Cysharp.Threading.Tasks;

namespace PlayerController
{
    public class PlayerAniEventReciver : MonoBehaviour
    {
        private Attack attack;
        private Rotate playerRotate;

        private void Start()
        {
            attack = GetComponentInParent<Attack>();
            playerRotate = GetComponentInParent<Rotate>();
        }
        public void AE_OnAttack()
        {
            playerRotate?.SnapFaceAimDirection();
            attack?.SpawnProjectile().Forget();
        }
    }
}