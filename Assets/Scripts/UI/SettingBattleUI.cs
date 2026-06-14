using Core;
using Global;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class SettingBattleUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button continueButton;

        private bool pausedByMenu;
        private bool blockPause;
        private bool isGameOver;

        public bool IsVisible => gameObject.activeSelf;

        private void OnEnable() => WireButtons();

        private void OnDisable()
        {
            UnwireButtons();
            ResumeTimeIfNeeded();
        }

        public void Show()
        {
            if (blockPause || isGameOver || gameObject.activeSelf)
                return;

            pausedByMenu = Time.timeScale > 0f;
            Time.timeScale = 0f;
            InputManager.Instance?.SetUiAttackHeld(false);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }

        public void NotifyLoadingStarted()
        {
            blockPause = true;
            Hide();
        }

        public void NotifyLoadingFinished() => blockPause = false;

        public void NotifyMatchReset()
        {
            blockPause = false;
            isGameOver = false;
            pausedByMenu = false;
            Hide();
        }

        public void NotifyCardPickShown()
        {
            if (gameObject.activeSelf)
                Hide();
        }

        public void NotifyGameOver()
        {
            isGameOver = true;
            Hide();
        }

        private void WireButtons()
        {
            if (backButton != null)
                backButton.onClick.AddListener(ExitToLobby);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void UnwireButtons()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(ExitToLobby);

            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        private void OnContinueClicked() => Hide();

        private void ExitToLobby()
        {
            Time.timeScale = 1f;
            pausedByMenu = false;
            gameObject.SetActive(false);
            SceneManagerCustom.LoadLobby();
        }

        private void ResumeTimeIfNeeded()
        {
            if (!pausedByMenu)
                return;

            Time.timeScale = 1f;
            pausedByMenu = false;
        }
    }
}
