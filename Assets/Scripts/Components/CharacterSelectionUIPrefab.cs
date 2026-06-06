using System;
using Core.Save;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class CharacterSelectionUIPrefab : MonoBehaviour
    {
        [SerializeField] private Button chooseButton;
        [SerializeField] private Image characterIcon;
        [SerializeField] private TMP_Text characterName;
        [SerializeField] private TMP_Text characterLevel;

        private HeroSO boundHero;
        private Action<HeroSO> onChoose;

        private void Awake()
        {
            chooseButton?.onClick.AddListener(OnChooseClicked);
        }

        private void OnDestroy()
        {
            if (chooseButton != null)
                chooseButton.onClick.RemoveListener(OnChooseClicked);
        }

        public void Bind(HeroSO hero, Action<HeroSO> onChooseCallback)
        {
            boundHero = hero;
            onChoose = onChooseCallback;

            if (hero == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (characterName != null)
                characterName.text = hero.displayName;

            if (characterIcon != null)
            {
                characterIcon.sprite = hero.icon;
                characterIcon.enabled = hero.icon != null;
            }

            if (characterLevel != null)
            {
                var metaLevel = GetMetaLevel(hero.heroId);
                var equipped = HeroProgressService.GetEquippedHeroId() == hero.heroId;
                characterLevel.text = metaLevel.ToString();
            }
        }

        private static int GetMetaLevel(string heroId) =>
            HeroProgressService.GetUpgradeTier(heroId);

        private void OnChooseClicked()
        {
            if (boundHero != null)
                onChoose?.Invoke(boundHero);
        }
    }
}
