using Global;
using UnityEngine;

namespace CustomUI
{
    /// <summary>uGUI pause menu placeholder — wire panel and buttons when UI is ready.</summary>
    public class BattlePauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;

        private void OnEnable()
        {
            GlobalEvents.OnDungeonSceneLoaded += HandleLoadingStarted;
            GlobalEvents.OnDungeonGenerationStarted += HandleLoadingStarted;
            GlobalEvents.OnDungeonGenerated += HandleLoadingFinished;
            GlobalEvents.OnMatchReset += HandleMatchReset;
            GlobalEvents.OnRequestBattleCardUI += HandleCardPickShown;
            GlobalEvents.OnBattleCardUIDismissed += HandleCardPickHidden;
            GlobalEvents.OnGameOver += HandleGameOver;

            HidePanel();
        }

        private void OnDisable()
        {
            GlobalEvents.OnDungeonSceneLoaded -= HandleLoadingStarted;
            GlobalEvents.OnDungeonGenerationStarted -= HandleLoadingStarted;
            GlobalEvents.OnDungeonGenerated -= HandleLoadingFinished;
            GlobalEvents.OnMatchReset -= HandleMatchReset;
            GlobalEvents.OnRequestBattleCardUI -= HandleCardPickShown;
            GlobalEvents.OnBattleCardUIDismissed -= HandleCardPickHidden;
            GlobalEvents.OnGameOver -= HandleGameOver;
        }

        private void Update()
        {
            if (InputManager.Instance == null || !InputManager.Instance.WasPausePressed())
                return;

            Debug.Log("[BattlePauseMenuUI] Pause UI not wired yet.");
        }

        private void HandleLoadingStarted() { }

        private void HandleLoadingFinished(int _) { }

        private void HandleMatchReset() => HidePanel();

        private void HandleCardPickShown() => HidePanel();

        private void HandleCardPickHidden() { }

        private void HandleGameOver() => HidePanel();

        private void HidePanel()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }
    }
}
