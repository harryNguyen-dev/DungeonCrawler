using Core.Save;

namespace Core
{
    /// <summary>
    /// Loads local persistent data during the boot scene before entering Lobby.
    /// </summary>
    public static class GameBootstrap
    {
        public static void LoadPersistentData()
        {
            LevelProgressService.GetSaveData();
            HeroProgressService.SyncEquippedHeroCache();
        }
    }
}
