using System;
using Combat;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "BeAttack", story: "[Agent] be attacked by [Player]", category: "Conditions", id: "c754c2b0945b8ca75486aef25574ad02")]
public partial class BeAttackCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    public override bool IsTrue()
    {
        GameObject agent = Agent != null ? Agent.Value : null;
        GameObject player = Player != null ? Player.Value : null;
        if (agent == null || player == null) return false;
        if (!agent.TryGetComponent<EnemyHitSignal>(out var signal)) return false;
        return signal.IsPendingHitFrom(player);
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
