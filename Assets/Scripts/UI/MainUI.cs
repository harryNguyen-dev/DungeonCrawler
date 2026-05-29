using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using WFC;
using Global;
public class MainUI : MonoBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private Button restartBtn;
    [SerializeField] private WFCGeneration _wfcGeneration;
    private void Awake()
    {
        startBtn.onClick.AddListener(OnStartBtnClick);
        restartBtn.onClick.AddListener(OnResetBtnClick);
    }

    private void OnStartBtnClick()
    {
        GlobalEvents.RaiseGameStart();
        _wfcGeneration.GenerateWithRetry(5).Forget();
        startBtn.gameObject.SetActive(false);
    }
    private void OnResetBtnClick()
    {
        _wfcGeneration.ResetAndGenerate().Forget();
    }
}
