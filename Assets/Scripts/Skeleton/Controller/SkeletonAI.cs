using Unity.Behavior;
using UnityEngine;

namespace Enemy
{
    public class SkeletonAI : MonoBehaviour
    {
        [SerializeField] private BehaviorGraphAgent _agent; // component Unity Behavior
        public void SetTarget(Transform player)
        {
            // Set blackboard variable mà Behavior Graph đang dùng
            _agent.BlackboardReference.SetVariableValue("Player", player.gameObject);
        }
    }
}
