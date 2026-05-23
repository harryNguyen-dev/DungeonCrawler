using DG.Tweening;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private GameObject Door;
    
    public void Start()
    {
        Door.SetActive(false);
        Door.transform.localPosition = new Vector3(0.7f,-5,0);
    }
    public void SetClose()
    {
        Door.SetActive(true);
        MoveDoorTween(false);
    }
    public void SetOpen()
    {
        Door.SetActive(false);
        MoveDoorTween(true);
    }

    private void MoveDoorTween(bool isOpen)
    {
        if(isOpen)
        {
            Door.transform.DOLocalMoveY(-5,0.5f,true).SetEase(Ease.Flash);
        } else
        {
            Door.transform.DOLocalMoveY(0,0.5f,true).SetEase(Ease.Flash);
        }
    }
}
