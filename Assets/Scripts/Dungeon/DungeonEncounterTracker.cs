using UnityEngine;

public static class DungeonEncounterTracker
{
    public static int TotalCombatRooms { get; private set; }
    public static int EnteredCombatRooms { get; private set; }

    public static void Reset(int totalCombatRooms)
    {
        TotalCombatRooms = Mathf.Max(0, totalCombatRooms);
        EnteredCombatRooms = 0;
    }

    public static bool RegisterCombatRoomEntered()
    {
        EnteredCombatRooms++;
        if (TotalCombatRooms <= 0)
            return false;

        return EnteredCombatRooms >= TotalCombatRooms;
    }
}
