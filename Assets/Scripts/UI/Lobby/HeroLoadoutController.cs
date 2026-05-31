using System.Globalization;
using Core.Save;
using CustomUI.SciFi;
using Global;
using SO;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.Lobby
{
    [RequireComponent(typeof(UIDocument))]
    public class HeroLoadoutController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement loadoutRoot;
        private ScrollView weaponScroll;
        private VisualElement detailPanel;
        private VisualElement previewPanel;
        private Image previewImage;
        private Label metaGoldLabel;
        private Label detailName;
        private Label detailDescription;
        private Label detailStatus;
        private Button closeButton;
        private Button detailCloseButton;
        private Button primaryActionButton;
        private Button equipButton;
        private Button upgradeDamageButton;
        private Button upgradeFireRateButton;

        private WeaponSO selectedWeapon;
        private VisualElement selectedRow;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null)
                uiDocument.sortingOrder = 50;
        }

        private void OnEnable()
        {
            GlobalEvents.OnRequestHeroLoadoutUI += ShowPanel;
            GlobalEvents.OnMetaGoldChanged += RefreshMetaGold;
            CacheElements();
            SciFiUiHelper.StyleSciFiDocument(uiDocument?.rootVisualElement);
            HidePanel();
        }

        private void OnDisable()
        {
            GlobalEvents.OnRequestHeroLoadoutUI -= ShowPanel;
            GlobalEvents.OnMetaGoldChanged -= RefreshMetaGold;
            HeroPreviewController.Instance?.UnregisterPreviewElement(previewPanel);
            HeroPreviewController.Instance?.HidePreview();
        }

        private void CacheElements()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null) return;

            loadoutRoot = root.Q<VisualElement>("hero-loadout-root");
            weaponScroll = root.Q<ScrollView>("weapon-scroll");
            detailPanel = root.Q<VisualElement>("detail-panel");
            previewPanel = root.Q<VisualElement>("preview-panel");
            previewImage = root.Q<Image>("preview-image");
            metaGoldLabel = root.Q<Label>("meta-gold-label");
            detailName = root.Q<Label>("detail-name");
            detailDescription = root.Q<Label>("detail-description");
            detailStatus = root.Q<Label>("detail-status");
            closeButton = root.Q<Button>("close-button");
            detailCloseButton = root.Q<Button>("detail-close-button");
            primaryActionButton = root.Q<Button>("primary-action-button");
            equipButton = root.Q<Button>("equip-button");
            upgradeDamageButton = root.Q<Button>("upgrade-damage-button");
            upgradeFireRateButton = root.Q<Button>("upgrade-fire-rate-button");

            closeButton?.RegisterCallback<ClickEvent>(_ => HidePanel());
            detailCloseButton?.RegisterCallback<ClickEvent>(_ => HideDetailPanel());
            primaryActionButton?.RegisterCallback<ClickEvent>(_ => OnPrimaryAction());
            equipButton?.RegisterCallback<ClickEvent>(_ => OnEquip());
            upgradeDamageButton?.RegisterCallback<ClickEvent>(_ => OnUpgradeDamage());
            upgradeFireRateButton?.RegisterCallback<ClickEvent>(_ => OnUpgradeFireRate());
        }

        private void ShowPanel()
        {
            if (loadoutRoot == null)
                CacheElements();

            var catalog = GlobalEntities.Instance?.WeaponCatalog;
            if (catalog == null || weaponScroll == null)
            {
                Debug.LogWarning("[HeroLoadout] WeaponCatalog chưa gán.");
                return;
            }

            selectedWeapon = null;
            selectedRow = null;
            HideDetailPanel();

            BuildWeaponList(catalog);
            RefreshMetaGold();

            if (loadoutRoot != null)
                loadoutRoot.style.display = DisplayStyle.Flex;

            var preview = HeroPreviewController.Instance;
            if (preview != null)
            {
                preview.ShowPreview();
                preview.RegisterPreviewElement(previewPanel, previewImage);
            }
        }

        private void HidePanel()
        {
            HideDetailPanel();

            if (loadoutRoot != null)
                loadoutRoot.style.display = DisplayStyle.None;

            HeroPreviewController.Instance?.UnregisterPreviewElement(previewPanel);
            HeroPreviewController.Instance?.HidePreview();
        }

        private void ShowDetailPanel()
        {
            if (detailPanel != null)
                detailPanel.style.display = DisplayStyle.Flex;
        }

        private void HideDetailPanel()
        {
            if (detailPanel != null)
                detailPanel.style.display = DisplayStyle.None;
        }

        private void BuildWeaponList(WeaponCatalogSO catalog)
        {
            weaponScroll.Clear();

            for (var i = 0; i < catalog.WeaponCount; i++)
            {
                var weapon = catalog.GetWeapon(i);
                if (weapon == null) continue;
                weaponScroll.Add(BuildWeaponRow(weapon));
            }
        }

        private VisualElement BuildWeaponRow(WeaponSO weapon)
        {
            var row = new VisualElement();
            row.AddToClassList("weapon-row");
            row.pickingMode = PickingMode.Position;

            if (!WeaponProgressService.IsUnlocked(weapon.weaponId))
                row.AddToClassList("weapon-row--locked");

            var name = new Label(weapon.displayName);
            name.AddToClassList("weapon-row-name");
            name.pickingMode = PickingMode.Ignore;

            var equipped = WeaponProgressService.GetEquippedWeaponId() == weapon.weaponId;
            var statusText = !WeaponProgressService.IsUnlocked(weapon.weaponId)
                ? "LOCKED"
                : equipped ? "EQUIPPED" : "OWNED";
            var status = new Label(statusText);
            status.AddToClassList("weapon-row-status");
            status.pickingMode = PickingMode.Ignore;

            row.Add(name);
            row.Add(status);

            row.RegisterCallback<ClickEvent>(evt =>
            {
                SelectWeapon(weapon, row);
                evt.StopPropagation();
            });

            return row;
        }

        private void SelectWeapon(WeaponSO weapon, VisualElement row)
        {
            selectedWeapon = weapon;

            if (selectedRow != null)
                selectedRow.RemoveFromClassList("weapon-row--selected");

            selectedRow = row;
            selectedRow.AddToClassList("weapon-row--selected");

            ShowDetailPanel();
            RefreshDetailPanel();
        }

        private void RefreshDetailPanel()
        {
            if (selectedWeapon == null || detailPanel == null) return;
            if (detailPanel.style.display == DisplayStyle.None) return;

            var unlocked = WeaponProgressService.IsUnlocked(selectedWeapon.weaponId);
            var equipped = WeaponProgressService.GetEquippedWeaponId() == selectedWeapon.weaponId;
            var gold = LevelProgressService.GetMetaGold();

            detailName.text = selectedWeapon.displayName;
            detailDescription.text = selectedWeapon.description;

            if (!unlocked)
            {
                detailStatus.text = $"Cost: {selectedWeapon.unlockCost:N0} gold";
                primaryActionButton.style.display = DisplayStyle.Flex;
                primaryActionButton.text = "MUA";
                primaryActionButton.SetEnabled(gold >= selectedWeapon.unlockCost);
                equipButton.style.display = DisplayStyle.None;
                upgradeDamageButton.style.display = DisplayStyle.None;
                upgradeFireRateButton.style.display = DisplayStyle.None;
                return;
            }

            var dmgTier = WeaponProgressService.GetDamageTier(selectedWeapon.weaponId);
            var rofTier = WeaponProgressService.GetFireRateTier(selectedWeapon.weaponId);
            var dmgCost = dmgTier < selectedWeapon.maxDamageTier
                ? selectedWeapon.GetDamageUpgradeCost(dmgTier)
                : 0;
            var rofCost = rofTier < selectedWeapon.maxFireRateTier
                ? selectedWeapon.GetFireRateUpgradeCost(rofTier)
                : 0;

            detailStatus.text = equipped
                ? "Currently equipped"
                : "Owned — tap EQUIP";

            primaryActionButton.style.display = DisplayStyle.None;
            equipButton.style.display = DisplayStyle.Flex;
            equipButton.SetEnabled(!equipped);

            upgradeDamageButton.style.display = DisplayStyle.Flex;
            upgradeFireRateButton.style.display = DisplayStyle.Flex;

            if (dmgTier >= selectedWeapon.maxDamageTier)
            {
                upgradeDamageButton.text = $"DMG MAX ({dmgTier}/{selectedWeapon.maxDamageTier})";
                upgradeDamageButton.SetEnabled(false);
            }
            else
            {
                upgradeDamageButton.text = $"DMG {dmgTier}/{selectedWeapon.maxDamageTier} · {dmgCost:N0}g";
                upgradeDamageButton.SetEnabled(gold >= dmgCost);
            }

            if (rofTier >= selectedWeapon.maxFireRateTier)
            {
                upgradeFireRateButton.text = $"ROF MAX ({rofTier}/{selectedWeapon.maxFireRateTier})";
                upgradeFireRateButton.SetEnabled(false);
            }
            else
            {
                upgradeFireRateButton.text = $"ROF {rofTier}/{selectedWeapon.maxFireRateTier} · {rofCost:N0}g";
                upgradeFireRateButton.SetEnabled(gold >= rofCost);
            }
        }

        private void RefreshMetaGold()
        {
            if (metaGoldLabel != null)
                metaGoldLabel.text = LevelProgressService.GetMetaGold().ToString("N0", CultureInfo.InvariantCulture);

            RefreshDetailPanel();
            RefreshWeaponRowBadges();
        }

        private void RefreshWeaponRowBadges()
        {
            if (weaponScroll == null) return;

            var index = 0;
            var catalog = GlobalEntities.Instance?.WeaponCatalog;
            if (catalog == null) return;

            foreach (var child in weaponScroll.Children())
            {
                var weapon = catalog.GetWeapon(index++);
                if (weapon == null) continue;

                var status = child.Q<Label>(className: "weapon-row-status");
                if (status == null) continue;

                if (!WeaponProgressService.IsUnlocked(weapon.weaponId))
                    status.text = "LOCKED";
                else if (WeaponProgressService.GetEquippedWeaponId() == weapon.weaponId)
                    status.text = "EQUIPPED";
                else
                    status.text = "OWNED";

                child.EnableInClassList("weapon-row--locked", !WeaponProgressService.IsUnlocked(weapon.weaponId));
            }
        }

        private void OnPrimaryAction()
        {
            if (selectedWeapon == null) return;

            if (WeaponProgressService.TryUnlock(selectedWeapon))
            {
                RefreshWeaponRowBadges();
                RefreshDetailPanel();
            }
        }

        private void OnEquip()
        {
            if (selectedWeapon == null) return;

            if (WeaponProgressService.TryEquip(selectedWeapon.weaponId))
            {
                RefreshWeaponRowBadges();
                RefreshDetailPanel();
            }
        }

        private void OnUpgradeDamage()
        {
            if (selectedWeapon == null) return;

            if (WeaponProgressService.TryUpgradeDamage(selectedWeapon))
                RefreshDetailPanel();
        }

        private void OnUpgradeFireRate()
        {
            if (selectedWeapon == null) return;

            if (WeaponProgressService.TryUpgradeFireRate(selectedWeapon))
                RefreshDetailPanel();
        }
    }
}
