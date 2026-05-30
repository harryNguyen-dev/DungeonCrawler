using Core;
using Global;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.Battle
{
    [RequireComponent(typeof(UIDocument))]
    public class RunEndGameController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement runEndRoot;
        private Label resultTitle;
        private Label levelLabel;
        private Label statLevel;
        private Label statHp;
        private Label statAtk;
        private Label statAs;
        private Label statMs;
        private Label statArmor;
        private Label statKills;
        private Label statRooms;
        private Label statRunGold;
        private Label metaGainLabel;
        private Label metaTotalLabel;
        private Button retryBtn;
        private Button hubBtn;

        private bool lastSummaryWasWin;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null)
                uiDocument.sortingOrder = 150;
        }

        private void OnEnable()
        {
            GlobalEvents.OnRequestEndGameUI += ShowSummary;
            GlobalEvents.OnMatchReset += HidePanel;
            CacheElements();
            WireButtons();
            HidePanel();
        }

        private void OnDisable()
        {
            GlobalEvents.OnRequestEndGameUI -= ShowSummary;
            GlobalEvents.OnMatchReset -= HidePanel;
            UnwireButtons();
        }

        private void CacheElements()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null) return;

            runEndRoot = root.Q<VisualElement>("run-end-root");
            resultTitle = root.Q<Label>("result-title");
            levelLabel = root.Q<Label>("level-label");
            statLevel = root.Q<Label>("stat-level");
            statHp = root.Q<Label>("stat-hp");
            statAtk = root.Q<Label>("stat-atk");
            statAs = root.Q<Label>("stat-as");
            statMs = root.Q<Label>("stat-ms");
            statArmor = root.Q<Label>("stat-armor");
            statKills = root.Q<Label>("stat-kills");
            statRooms = root.Q<Label>("stat-rooms");
            statRunGold = root.Q<Label>("stat-run-gold");
            metaGainLabel = root.Q<Label>("meta-gain-label");
            metaTotalLabel = root.Q<Label>("meta-total-label");
            retryBtn = root.Q<Button>("retry-btn");
            hubBtn = root.Q<Button>("hub-btn");
        }

        private void WireButtons()
        {
            if (retryBtn != null)
                retryBtn.clicked += RestartGame;

            if (hubBtn != null)
                hubBtn.clicked += ReturnHome;
        }

        private void UnwireButtons()
        {
            if (retryBtn != null)
                retryBtn.clicked -= RestartGame;

            if (hubBtn != null)
                hubBtn.clicked -= ReturnHome;
        }

        private void ShowSummary(RunSummary summary)
        {
            if (runEndRoot == null)
                CacheElements();

            if (summary == null || runEndRoot == null)
                return;

            lastSummaryWasWin = summary.IsWin;

            if (resultTitle != null)
            {
                resultTitle.text = summary.IsWin ? "VICTORY" : "DEFEAT";
                resultTitle.RemoveFromClassList("win-title");
                resultTitle.RemoveFromClassList("lose-title");
                resultTitle.AddToClassList(summary.IsWin ? "win-title" : "lose-title");
            }

            if (levelLabel != null)
                levelLabel.text = summary.LevelLabel;

            SetLabel(statLevel, $"Lv.{summary.PlayerLevel}");
            SetLabel(statHp, $"HP: {summary.CurrentHealth}/{summary.MaxHealth}");
            SetLabel(statAtk, $"ATK: {summary.AttackDamage}");
            SetLabel(statAs, $"AS: {summary.AttackCooldown:F2}");
            SetLabel(statMs, $"MS: {summary.MoveSpeed}");
            SetLabel(statArmor, $"ARM: {summary.Armor}");
            SetLabel(statKills, $"Kills: {summary.EnemiesKilled}");
            SetLabel(statRooms, $"Rooms: {summary.RoomsCleared}/{summary.TotalRooms}");
            SetLabel(statRunGold, $"Run Gold: {summary.RunGold}");
            SetLabel(metaGainLabel, $"+{summary.MetaGoldGained} meta gold");
            SetLabel(metaTotalLabel, $"Total meta: {summary.TotalMetaGold}");

            runEndRoot.style.display = DisplayStyle.Flex;
        }

        private static void SetLabel(Label label, string text)
        {
            if (label != null)
                label.text = text;
        }

        private void HidePanel()
        {
            if (runEndRoot == null)
                CacheElements();

            if (runEndRoot != null)
                runEndRoot.style.display = DisplayStyle.None;
        }

        private void RestartGame()
        {
            Time.timeScale = 1f;
            HidePanel();
            SceneManagerCustom.ReloadDungeon();
        }

        private void ReturnHome()
        {
            Time.timeScale = 1f;
            HidePanel();
            if (lastSummaryWasWin)
                GlobalVariable.OpenLevelSelectOnLobbyReturn = true;
            SceneManagerCustom.LoadLobby();
        }
    }
}
