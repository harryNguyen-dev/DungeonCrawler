using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "WeaponCatalog", menuName = "Weapon/Weapon Catalog")]
    public class WeaponCatalogSO : ScriptableObject
    {
        public List<WeaponSO> weapons = new();

        public int WeaponCount => weapons?.Count ?? 0;

        public WeaponSO GetWeapon(int index)
        {
            if (weapons == null || index < 0 || index >= weapons.Count)
                return null;
            return weapons[index];
        }

        public WeaponSO GetById(string id)
        {
            if (string.IsNullOrEmpty(id) || weapons == null)
                return null;

            foreach (var weapon in weapons)
            {
                if (weapon != null && weapon.weaponId == id)
                    return weapon;
            }

            return null;
        }

        public WeaponSO GetDefaultWeapon()
        {
            if (weapons == null)
                return null;

            foreach (var weapon in weapons)
            {
                if (weapon != null && weapon.unlockedByDefault)
                    return weapon;
            }

            return weapons.Count > 0 ? weapons[0] : null;
        }
    }
}
