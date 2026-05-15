using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using WFC;
using Global;
public class MainUI : MonoBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private WFCGeneration _wfcGeneration;
    private void Awake()
    {
        startBtn.onClick.AddListener(OnStartBtnClick);
    }

    private void OnStartBtnClick()
    {
        GlobalEvents.RaiseGameStart();
        _wfcGeneration.GenerateWithRetry(5).Forget();
        startBtn.gameObject.SetActive(false);
    }
}
