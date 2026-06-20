using SO;
using UnityEngine;

namespace Core
{
    public static class GameAudio
    {
        public const string TagUi = "ui";
        public const string TagVfx = "vfx";
        public const string TagMaster = "master";
        public const string TagAmbient = "ambient";

        static GameAudioCatalogSO _catalog;

        public static void SetCatalog(GameAudioCatalogSO catalog) => _catalog = catalog;

        public static AudioClip LoadingClip => _catalog != null ? _catalog.sfxLoading : null;

        public static void PlayUiConfirm() => Play2D(_catalog?.uiClickConfirm, TagUi);
        public static void PlayUiBack() => Play2D(_catalog?.uiClickBack, TagUi);
        public static void PlayUiTab() => Play2D(_catalog?.uiClickTab, TagUi);
        public static void PlayUiError() => Play2D(_catalog?.uiError, TagUi);
        public static void PlayUiPurchase() => Play2D(_catalog?.uiPurchase, TagUi);
        public static void PlayUiPauseOpen() => Play2D(_catalog?.uiPauseOpen, TagUi);
        public static void PlayUiPauseClose() => Play2D(_catalog?.uiPauseClose, TagUi);

        public static void PlayAmbientDungeon() => PlayAmbient(_catalog?.ambientDungeon);
        public static void PlayLobbyMusic() => PlayAmbient(_catalog?.musicLobby);
        public static void PlayBattleMusic() => PlayAmbient(_catalog?.musicBattle);
        public static void PlayBossMusic() => PlayAmbient(_catalog?.musicBoss);

        public static void PlayStingerWin() => Play2D(_catalog?.stingerWin, TagMaster);
        public static void PlayStingerLose() => Play2D(_catalog?.stingerLose, TagMaster);

        public static void PlayPlayerAttack(Vector3 position) => PlayAt(_catalog?.playerAttack, TagVfx, position);
        public static void PlayPlayerSkill(Vector3 position) => PlayAt(_catalog?.playerSkill, TagVfx, position);
        public static void PlayPlayerDash(Vector3 position) => PlayAt(_catalog?.playerDash, TagVfx, position);
        public static void PlayPlayerHit(Vector3 position) => PlayAt(_catalog?.playerHit, TagVfx, position);
        public static void PlayPlayerDeath() => Play2D(_catalog?.playerDeath, TagVfx);
        public static void PlayPlayerLevelUp() => Play2D(_catalog?.playerLevelUp, TagVfx);
        public static void PlayPlayerHeal() => Play2D(_catalog?.playerHeal, TagVfx);

        public static void PlayEnemyHit(Vector3 position) => PlayAt(_catalog?.enemyHit, TagVfx, position);
        public static void PlayEnemyDeath(Vector3 position) => PlayAt(_catalog?.enemyDeath, TagVfx, position);
        public static void PlayGoldPickup() => Play2D(_catalog?.goldPickup, TagVfx);
        public static void PlayCardReveal() => Play2D(_catalog?.cardReveal, TagVfx);
        public static void PlayCardSelect() => Play2D(_catalog?.cardSelect, TagVfx);
        public static void PlayRoomEnter() => Play2D(_catalog?.roomEnter, TagVfx);
        public static void PlayRoomClear() => Play2D(_catalog?.roomClear, TagVfx);
        public static void PlayDoorOpen() => Play2D(_catalog?.doorOpen, TagVfx);
        public static void PlayBossIntro() => Play2D(_catalog?.bossIntro, TagVfx);

        static void Play2D(AudioClip clip, string tag)
        {
            if (clip == null) return;
            AudioManager.Singleton?.PlayAudio(clip, tag);
        }

        static void PlayAt(AudioClip clip, string tag, Vector3 position)
        {
            if (clip == null) return;
            AudioManager.Singleton?.PlayAudio(clip, tag, position);
        }

        static void PlayAmbient(AudioClip clip)
        {
            if (clip == null) return;
            AudioManager.Singleton?.PlayAmbientAudio(clip, TagAmbient);
        }
    }
}
