using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StopNavmesh", story: "Stop [Navmesh]", category: "Action", id: "25e13c1f34ebb524a1787555669223bd")]
public partial class StopNavmeshAction : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Navmesh;

    protected override Status OnStart()
    {
        if (Navmesh.Value != null)
        {
            // Dừng việc tính toán đường đi
            Navmesh.Value.isStopped = true;
            // Triệt tiêu vận tốc ngay lập tức để tránh trượt (Sliding)
            Navmesh.Value.velocity = Vector3.zero;
            return Status.Success;
        }
        return Status.Failure;
    }
}

