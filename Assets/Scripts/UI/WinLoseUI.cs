using Core;
using Global;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class WinLoseUI : MonoBehaviour
    {
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [SerializeField] private Button exitWinButton;
        [SerializeField] private Button exitLoseButton;

        [SerializeField] private Button replayButton;
        [SerializeField] private Button ReviveButton; // revive need watch ads

        private bool lastWasWin;

        private void OnEnable()
        {
            GlobalEvents.OnRequestEndGameUI += ShowPanel;
            GlobalEvents.OnMatchReset += HidePanels;
            WireButtons();
            HidePanels();
        }

        private void OnDisable()
        {
            GlobalEvents.OnRequestEndGameUI -= ShowPanel;
            GlobalEvents.OnMatchReset -= HidePanels;
            UnwireButtons();
        }

        private void WireButtons()
        {
            if (exitWinButton != null)
                exitWinButton.onClick.AddListener(ExitToHub);

            if (exitLoseButton != null)
                exitLoseButton.onClick.AddListener(ExitToHub);

            if (replayButton != null)
                replayButton.onClick.AddListener(RestartRun);

            if (ReviveButton != null)
                ReviveButton.onClick.AddListener(OnRevivePlaceholder);
        }

        private void UnwireButtons()
        {
            if (exitWinButton != null)
                exitWinButton.onClick.RemoveListener(ExitToHub);

            if (exitLoseButton != null)
                exitLoseButton.onClick.RemoveListener(ExitToHub);

            if (replayButton != null)
                replayButton.onClick.RemoveListener(RestartRun);

            if (ReviveButton != null)
                ReviveButton.onClick.RemoveListener(OnRevivePlaceholder);
        }

        private void ShowPanel(RunSummary summary)
        {
            if (summary == null)
                return;

            lastWasWin = summary.IsWin;

            if (winPanel != null)
                winPanel.SetActive(summary.IsWin);

            if (losePanel != null)
                losePanel.SetActive(!summary.IsWin);
        }

        private void HidePanels()
        {
            if (winPanel != null)
                winPanel.SetActive(false);

            if (losePanel != null)
                losePanel.SetActive(false);
        }

        private void RestartRun()
        {
            Time.timeScale = 1f;
            HidePanels();
            SceneManagerCustom.ReloadDungeon();
        }

        private void ExitToHub()
        {
            Time.timeScale = 1f;
            HidePanels();

            if (lastWasWin)
                GlobalVariable.OpenLevelSelectOnLobbyReturn = true;

            SceneManagerCustom.LoadLobby();
        }

        private void OnRevivePlaceholder()
        {
            // TODO: rewarded ad -> revive player and resume run
            Debug.Log("[WinLoseUI] Revive (watch ads) — not implemented yet.");
        }
    }
}
 