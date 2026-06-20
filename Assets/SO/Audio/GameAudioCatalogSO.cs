using UnityEngine;

namespace SO
{
    /// <summary>
    /// Central registry of all game audio clips. Assign WAV files from
    /// Assets/ThirdPackage/Audio/ — see Assets/Tasks/Audio-Requirements.md.
    /// </summary>
    [CreateAssetMenu(fileName = "GameAudioCatalog", menuName = "Audio/Game Audio Catalog")]
    public class GameAudioCatalogSO : ScriptableObject
    {
        [Header("UI (tag: ui)")]
        public AudioClip uiClickConfirm;
        public AudioClip uiClickBack;
        public AudioClip uiClickTab;
        public AudioClip uiError;
        public AudioClip uiPurchase;
        public AudioClip sfxLoading;

        [Header("Music & Stingers")]
        public AudioClip musicLobby;
        public AudioClip musicBattle;
        public AudioClip musicBoss;
        public AudioClip stingerWin;
        public AudioClip stingerLose;
        public AudioClip ambientDungeon;

        [Header("Player (tag: vfx)")]
        public AudioClip playerAttack;
        public AudioClip playerSkill;
        public AudioClip playerDash;
        public AudioClip playerHit;
        public AudioClip playerDeath;
        public AudioClip playerLevelUp;
        public AudioClip playerHeal;

        [Header("Enemy & Combat (tag: vfx)")]
        public AudioClip enemyHit;
        public AudioClip enemyDeath;
        public AudioClip enemyMeleeSwing;
        public AudioClip enemyRangedShoot;
        public AudioClip projectileHit;
        public AudioClip explosion;

        [Header("Dungeon & Meta (tag: vfx)")]
        public AudioClip roomEnter;
        public AudioClip roomClear;
        public AudioClip doorOpen;
        public AudioClip goldPickup;
        public AudioClip cardReveal;
        public AudioClip cardSelect;

        [Header("Boss (tag: vfx) — P1")]
        public AudioClip bossIntro;
        public AudioClip bossPhase;
        public AudioClip bossSpecial;

        [Header("Pause UI — P1")]
        public AudioClip uiPauseOpen;
        public AudioClip uiPauseClose;
    }
}
