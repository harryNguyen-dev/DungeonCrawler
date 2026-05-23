using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class EndGameUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text Title;
        [SerializeField] private GameObject endGamePanel;
        [SerializeField] private Button restartBtn;
        [SerializeField] private Button homeBtn;

        private void OnEnable()
        {
            Global.GlobalEvents.OnAllRoomsCleared += ShowWinPanel;
            Global.GlobalEvents.OnPlayerEliminated += ShowLosePanel;
        }

        private void OnDisable()
        {
            Global.GlobalEvents.OnAllRoomsCleared -= ShowWinPanel;
            Global.GlobalEvents.OnPlayerEliminated -= ShowLosePanel;
        }

        private void Start()
        {
            endGamePanel.SetActive(false);
            restartBtn.onClick.AddListener(RestartGame);
            homeBtn.onClick.AddListener(ReturnHome);
        }

        private void OnDestroy()
        {
            restartBtn.onClick.RemoveListener(RestartGame);
            homeBtn.onClick.RemoveListener(ReturnHome);
        }

        private void ShowWinPanel()
        {
            endGamePanel.SetActive(true);
            Title.text = "YOU WIN";
        }

        private void ShowLosePanel()
        {
            endGamePanel.SetActive(true);
            Title.text = "YOU LOSE";
        }

        private void RestartGame()
        {
            Debug.Log("[EndGameUI] Restart Game");
            endGamePanel.SetActive(false);
            Core.SceneManagerCustom.ReloadDungeon();
        }

        private void ReturnHome()
        {
            Debug.Log("[EndGameUI] Return Home");
            endGamePanel.SetActive(false);
            Core.SceneManagerCustom.LoadLobby();
        }
    }
}
