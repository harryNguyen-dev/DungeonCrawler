using Global;
using SO;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Subscribes to <see cref="GlobalEvents"/> and plays clips from <see cref="GameAudioCatalogSO"/>.
    /// Skips any clip that is not assigned in the catalog.
    /// </summary>
    public sealed class GameAudioController : MonoBehaviour
    {
        [SerializeField] private GameAudioCatalogSO catalog;

        private bool _hasPreviousRoom;
        private RoomType _previousRoomType;

        private void Awake()
        {
            if (catalog != null)
                GameAudio.SetCatalog(catalog);

            EnsureUiVolumeTag();
        }

        private void OnEnable()
        {
            GlobalEvents.OnLobbyReady += HandleLobbyReady;
            GlobalEvents.OnGameStart += HandleGameStart;
            GlobalEvents.OnRoomEntered += HandleRoomEntered;
            GlobalEvents.OnRoomCleared += HandleRoomCleared;
            GlobalEvents.OnLevelUp += HandleLevelUp;
            GlobalEvents.OnRequestBattleCardUI += HandleCardReveal;
            GlobalEvents.OnBossDefeated += HandleBossDefeated;
            GlobalEvents.OnPlayerEliminated += HandlePlayerEliminated;
            GlobalEvents.OnMatchReset += HandleMatchReset;
        }

        private void OnDisable()
        {
            GlobalEvents.OnLobbyReady -= HandleLobbyReady;
            GlobalEvents.OnGameStart -= HandleGameStart;
            GlobalEvents.OnRoomEntered -= HandleRoomEntered;
            GlobalEvents.OnRoomCleared -= HandleRoomCleared;
            GlobalEvents.OnLevelUp -= HandleLevelUp;
            GlobalEvents.OnRequestBattleCardUI -= HandleCardReveal;
            GlobalEvents.OnBossDefeated -= HandleBossDefeated;
            GlobalEvents.OnPlayerEliminated -= HandlePlayerEliminated;
            GlobalEvents.OnMatchReset -= HandleMatchReset;
        }

        private void EnsureUiVolumeTag()
        {
            var audio = AudioManager.Singleton;
            if (audio == null) return;

            foreach (var entry in audio.customTagVolumes)
            {
                if (entry.tag == GameAudio.TagUi)
                    return;
            }

            audio.SetCustomTagVolume(GameAudio.TagUi, 1f);
        }

        private void HandleLobbyReady() => GameAudio.PlayLobbyMusic();

        private void HandleGameStart()
        {
            ResetRoomTransitionState();
            GameAudio.PlayAmbientDungeon();
        }

        private void HandleRoomEntered(RoomEnteredInfo info)
        {
            var isHallwayToHallway = _hasPreviousRoom
                && _previousRoomType == RoomType.Hallway
                && info.RoomType == RoomType.Hallway;

            if (!isHallwayToHallway)
                GameAudio.PlayRoomEnter();

            _hasPreviousRoom = true;
            _previousRoomType = info.RoomType;

            if (info.IsBossRoom)
            {
                GameAudio.PlayBossMusic();
                GameAudio.PlayBossIntro();
                return;
            }

            switch (info.RoomType)
            {
                case RoomType.Hallway:
                    GameAudio.PlayAmbientDungeon();
                    break;
                case RoomType.Combat:
                    GameAudio.PlayBattleMusic();
                    break;
            }
        }

        private void HandleRoomCleared(Vector2Int _)
        {
            GameAudio.PlayRoomClear();
            GameAudio.PlayAmbientDungeon();
        }

        private void HandleLevelUp(int _) => GameAudio.PlayPlayerLevelUp();

        private void HandleCardReveal() => GameAudio.PlayCardReveal();

        private void HandleBossDefeated()
        {
            AudioManager.Singleton?.StopAmbientAudio();
            GameAudio.PlayStingerWin();
        }

        private void HandlePlayerEliminated()
        {
            AudioManager.Singleton?.StopAmbientAudio();
            GameAudio.PlayStingerLose();
        }

        private void HandleMatchReset()
        {
            AudioManager.Singleton?.StopLoadingAudioPlay();
            ResetRoomTransitionState();
        }

        private void ResetRoomTransitionState()
        {
            _hasPreviousRoom = false;
        }
    }
}

