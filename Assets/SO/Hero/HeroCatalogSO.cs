using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "HeroCatalog", menuName = "Hero/Hero Catalog")]
    public class HeroCatalogSO : ScriptableObject
    {
        public List<HeroSO> heroes = new();

        public HeroSO GetById(string id)
        {
            if (string.IsNullOrEmpty(id) || heroes == null)
                return null;

            foreach (var hero in heroes)
            {
                if (hero != null && hero.heroId == id)
                    return hero;
            }

            return null;
        }

        public HeroSO GetDefaultHero()
        {
            if (heroes == null || heroes.Count == 0)
                return null;

            foreach (var hero in heroes)
            {
                if (hero != null && hero.unlockedByDefault)
                    return hero;
            }

            return heroes[0];
        }
    }
}
