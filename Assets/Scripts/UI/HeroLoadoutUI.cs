using Global;
using UnityEngine;

namespace CustomUI
{
    /// <summary>uGUI hero loadout placeholder — implement when loadout UI is ready.</summary>
    public class HeroLoadoutUI : MonoBehaviour
    {
        [SerializeField] private GameObject loadoutPanel;

        private void OnEnable()
        {
            GlobalEvents.OnRequestHeroLoadoutUI += Open;
            GlobalEvents.OnMetaGoldChanged += RefreshMetaGold;
            HidePanel();
        }

        private void OnDisable()
        {
            GlobalEvents.OnRequestHeroLoadoutUI -= Open;
            GlobalEvents.OnMetaGoldChanged -= RefreshMetaGold;
        }

        public void Open()
        {
            Debug.Log("[HeroLoadoutUI] Loadout UI not wired yet.");
            if (loadoutPanel != null)
                loadoutPanel.SetActive(false);
        }

        public void Close()
        {
            HidePanel();
        }

        private void RefreshMetaGold() { }

        private void HidePanel()
        {
            if (loadoutPanel != null)
                loadoutPanel.SetActive(false);
        }
    }
}
