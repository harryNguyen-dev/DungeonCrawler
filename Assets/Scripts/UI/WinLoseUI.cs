using Core;
using Global;
using TMPro;
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
        [SerializeField] private Button ReviveButton;

        private const float ReviveButtonDisabledAlpha = 0.35f;

        private CanvasGroup reviveButtonCanvasGroup;

        [Header("Stars")]
        [SerializeField] private Image starWin1;
        [SerializeField] private Image starWin2;
        [SerializeField] private Image starWin3;
        [SerializeField] private TMP_Text starsWinLabel;

        [SerializeField] private Image starLose1;
        [SerializeField] private Image starLose2;
        [SerializeField] private Image starLose3;
        [SerializeField] private TMP_Text starsLoseLabel;

        private bool lastWasWin;
        private bool lastUnlockedNewLevel;

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
                ReviveButton.onClick.AddListener(OnRevive);
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
                ReviveButton.onClick.RemoveListener(OnRevive);
        }

        private void ShowPanel(RunSummary summary)
        {
            if (summary == null)
                return;

            lastWasWin = summary.IsWin;
            lastUnlockedNewLevel = summary.UnlockedNewLevel;
            ApplyStars(summary.StarsEarned);

            if (winPanel != null)
                winPanel.SetActive(summary.IsWin);

            if (losePanel != null)
                losePanel.SetActive(!summary.IsWin);

            RefreshReviveButton(summary.IsWin);
        }

        private void RefreshReviveButton(bool isWin)
        {
            if (ReviveButton == null)
                return;

            EnsureReviveButtonCanvasGroup();

            var canRevive = !isWin && GameManager.Instance != null && GameManager.Instance.CanRevive;
            ReviveButton.interactable = canRevive;
            reviveButtonCanvasGroup.alpha = canRevive ? 1f : ReviveButtonDisabledAlpha;
        }

        private void EnsureReviveButtonCanvasGroup()
        {
            if (reviveButtonCanvasGroup != null)
                return;

            reviveButtonCanvasGroup = ReviveButton.GetComponent<CanvasGroup>();
            if (reviveButtonCanvasGroup == null)
                reviveButtonCanvasGroup = ReviveButton.gameObject.AddComponent<CanvasGroup>();
        }

        private void HidePanels()
        {
            if (winPanel != null)
                winPanel.SetActive(false);

            if (losePanel != null)
                losePanel.SetActive(false);

            ApplyStars(0);
        }

        private void ApplyStars(int earnedStars)
        {
            StarDisplayHelper.Apply(starWin1, starWin2, starWin3, starsWinLabel, earnedStars);
            StarDisplayHelper.Apply(starLose1, starLose2, starLose3, starsLoseLabel, earnedStars);
        }

        private void RestartRun()
        {
            GameAudio.PlayUiConfirm();
            Time.timeScale = 1f;
            HidePanels();
            SceneManagerCustom.ReloadDungeon();
        }

        private void ExitToHub()
        {
            GameAudio.PlayUiBack();
            Time.timeScale = 1f;
            HidePanels();

            if (lastWasWin || lastUnlockedNewLevel)
                GlobalVariable.OpenLevelSelectOnLobbyReturn = true;

            SceneManagerCustom.LoadLobby();
        }

        private void OnRevive()
        {
            if (GameManager.Instance == null || !GameManager.Instance.TryRevivePlayer())
                return;

            GameAudio.PlayUiConfirm();
            HidePanels();
        }
    }
}
