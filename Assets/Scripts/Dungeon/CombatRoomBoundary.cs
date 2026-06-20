using Core.Minimap;
using UnityEngine;

public static class CombatRoomBoundary
{
    public static bool IsSealed { get; private set; }
    public static MinimapZoneBounds Bounds { get; private set; }

    public static void Seal(MinimapZoneBounds bounds)
    {
        IsSealed = bounds.IsValid;
        Bounds = bounds;
    }

    public static void Unseal()
    {
        IsSealed = false;
        Bounds = default;
    }

    public static bool TryGetBounds(out MinimapZoneBounds bounds)
    {
        bounds = Bounds;
        return IsSealed && bounds.IsValid;
    }
}
