using UnityEngine;

namespace CustomUI.Battle
{
    /// <summary>
    /// Deprecated: battle loading now uses the persistent <see cref="Core.LoadingManager"/> via
    /// <see cref="Core.BattleLoadingBridge"/>. Keep this component disabled in scenes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DungeonLoadingController : MonoBehaviour
    {
    }
}
