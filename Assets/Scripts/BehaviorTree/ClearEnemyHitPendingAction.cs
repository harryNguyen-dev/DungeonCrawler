using System;
using Combat;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ClearEnemyHitPending", story: "Clear hit pending on [Agent]", category: "Action", id: "a8c3e1d2b4f5478990ab12cd34ef5678")]
public partial class ClearEnemyHitPendingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected override Status OnStart()
    {
        if (Agent?.Value != null && Agent.Value.TryGetComponent<EnemyHitSignal>(out var sig))
            sig.ClearPending();
        return Status.Success;
    }
}
