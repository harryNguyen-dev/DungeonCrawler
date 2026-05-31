using Core;
using CustomUI.SciFi;
using Global;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.Battle
{
    [RequireComponent(typeof(UIDocument))]
    public class BattlePauseMenuController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement pauseRoot;
        private Button continueBtn;
        private Button exitBtn;

        private bool isVisible;
        private bool loadingVisible;
        private bool cardPickVisible;
        private bool gameOver;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null)
                uiDocument.sortingOrder = 125;
        }

        private void OnEnable()
        {
            GlobalEvents.OnDungeonSceneLoaded += HandleLoadingStarted;
            GlobalEvents.OnDungeonGenerationStarted += HandleLoadingStarted;
            GlobalEvents.OnDungeonGenerated += HandleLoadingFinished;
            GlobalEvents.OnMatchReset += HandleMatchReset;
            GlobalEvents.OnRequestBattleCardUI += HandleCardPickShown;
            GlobalEvents.OnBattleCardUIDismissed += HandleCardPickHidden;
            GlobalEvents.OnGameOver += HandleGameOver;

            CacheElements();
            SciFiUiHelper.StyleSciFiDocument(uiDocument?.rootVisualElement);
            WireButtons();
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

            UnwireButtons();
        }

        private void Update()
        {
            if (InputManager.Instance == null || !InputManager.Instance.WasPausePressed())
                return;

            if (isVisible)
                Resume();
            else
                TryShow();
        }

        private void CacheElements()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null) return;

            pauseRoot = root.Q<VisualElement>("pause-root");
            continueBtn = root.Q<Button>("continue-btn");
            exitBtn = root.Q<Button>("exit-btn");
        }

        private void WireButtons()
        {
            if (continueBtn != null)
                continueBtn.clicked += Resume;

            if (exitBtn != null)
                exitBtn.clicked += ExitToHub;
        }

        private void UnwireButtons()
        {
            if (continueBtn != null)
                continueBtn.clicked -= Resume;

            if (exitBtn != null)
                exitBtn.clicked -= ExitToHub;
        }

        private void TryShow()
        {
            if (!CanPause())
                return;

            ShowPanel();
        }

        private bool CanPause()
        {
            return GlobalVariable.CurrentGameMode == GameMode.InMatch
                   && !loadingVisible
                   && !cardPickVisible
                   && !gameOver;
        }

        private void ShowPanel()
        {
            if (pauseRoot == null)
                CacheElements();

            if (pauseRoot == null)
                return;

            isVisible = true;
            pauseRoot.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f;
        }

        private void Resume()
        {
            HidePanel();
            if (!gameOver && !cardPickVisible)
                Time.timeScale = 1f;
        }

        private void ExitToHub()
        {
            Time.timeScale = 1f;
            HidePanel();
            SceneManagerCustom.LoadLobby();
        }

        private void HidePanel()
        {
            if (pauseRoot == null)
                CacheElements();

            isVisible = false;

            if (pauseRoot != null)
                pauseRoot.style.display = DisplayStyle.None;
        }

        private void HandleLoadingStarted() => loadingVisible = true;

        private void HandleLoadingFinished(int _) => loadingVisible = false;

        private void HandleMatchReset()
        {
            loadingVisible = false;
            cardPickVisible = false;
            gameOver = false;
            HidePanel();
            Time.timeScale = 1f;
        }

        private void HandleCardPickShown()
        {
            cardPickVisible = true;
            HidePanel();
        }

        private void HandleCardPickHidden() => cardPickVisible = false;

        private void HandleGameOver()
        {
            gameOver = true;
            HidePanel();
        }
    }
}
