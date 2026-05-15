using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
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
            Global.GlobalEvents.OnPlayerEliminated += ShowPanel;
        }
        private void OnDisable()
        {
            Global.GlobalEvents.OnPlayerEliminated -= ShowPanel;    
        }
        private void Start()
        {
            endGamePanel.SetActive(false);
            restartBtn.onClick.AddListener(() => RestartGame());
            homeBtn.onClick.AddListener(() => ReturnHome());
        }
        private void ShowPanel()
        {
            endGamePanel.SetActive(true);
            Title.text = "YOU LOSE";
        }
        private void RestartGame()
        {
            Debug.Log("[EndGameUI] Restart Game");
            Time.timeScale = 1f;
            endGamePanel.SetActive(false);
            Global.GlobalEntities.Instance.ClearPlayer();
            Global.GlobalEntities.Instance.SpawnPlayer();

            Global.GlobalEntities.Instance.ClearAllEnemies();
            Global.GlobalEntities.Instance.PlayerStats.RestartGame();
            Global.GlobalEntities.Instance.SpawnEnemyManager.ResetGame();
        }
        private void ReturnHome()
        {
            Debug.Log("[EndGameUI] Restart Game");
            Time.timeScale = 1f;
            endGamePanel.SetActive(false);
            Global.GlobalEntities.Instance.ClearPlayer();
            Global.GlobalEntities.Instance.SpawnPlayer();
            Global.GlobalEntities.Instance.ClearAllEnemies();
            Global.GlobalEntities.Instance.PlayerStats.RestartGame();

            // SAVE DATA
        }
    }
}