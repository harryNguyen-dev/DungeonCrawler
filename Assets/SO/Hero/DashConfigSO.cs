using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "DashConfig", menuName = "Hero/Dash Config")]
    public class DashConfigSO : ScriptableObject
    {
        public float distance = 4f;
        public float duration = 0.2f;
        public float cooldown = 2f;
        public float iFrameDuration = 0.18f;
    }
}
