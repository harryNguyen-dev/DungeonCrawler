using System.Collections.Generic;

using UnityEngine;



namespace SO

{

    /// <summary>Normal-attack behavior only — base combat stats live on HeroSO.</summary>

    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapon/Weapon")]

    public class WeaponSO : ScriptableObject

    {

        [Header("Identity")]

        public string weaponId;

        public string displayName;

        [TextArea(2, 4)]

        public string description;

        public Sprite icon;



        [Header("Attack behavior")]

        public GameObject projectilePrefab;

        public List<WeaponEffectModifier> intrinsicEffects = new();

    }

}


