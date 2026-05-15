using UnityEngine;
namespace Core
{

    public class GameManager : MonoBehaviour
    {
        private bool isGameOver = false;
        private void OnEnable() 
        {
            Global.GlobalEvents.OnPlayerEliminated += HandleLose;
        }

        private void OnDisable() 
        {
            Global.GlobalEvents.OnPlayerEliminated -= HandleLose;
        }

        public void HandleWin()
        {
            if (isGameOver) return;
            isGameOver = true;
            
            Debug.Log("LEVEL CLEAR!");
            // Hiện UI Thắng, cộng tiền thưởng x2
            ShowEndScreen(true);
        }

        private void HandleLose()
        {
            if (isGameOver) return;
            isGameOver = true;

            Debug.Log("GAME OVER!");
            // Hiện UI Thua, cộng tiền dựa trên số quái đã giết
            ShowEndScreen(false);
        }

        private void ShowEndScreen(bool isWin)
        {
            // Dừng game
            Time.timeScale = 0f;
            
            // Gọi Event để UI hiển thị (Bạn sẽ tạo UI EndGame lắng nghe event này)
            // Global.GlobalEvents.RaiseGameDone(isWin);
        }
    }

}