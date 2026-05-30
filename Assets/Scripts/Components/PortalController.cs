using UnityEngine;

namespace Components
{
    public class PortalController : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Global.GlobalEvents.RaiseRequestLevelSelectUI();
            }
        }
    }
}
