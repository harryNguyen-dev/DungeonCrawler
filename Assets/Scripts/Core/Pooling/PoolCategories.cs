namespace Core
{
    public static class PoolCategories
    {
        public static bool IsEnemy(PoolId id) =>
            id == PoolId.BatBaby ||
            id == PoolId.ImpMisChiefBaby ||
            id == PoolId.ImpMisChiefRangerBaby ||
            id == PoolId.EvergreenBaby ||
            id == PoolId.WormBaby ||
            id == PoolId.WormJunior ||
            id == PoolId.ImpMisChiefRangeJunior;

        public static bool IsProjectile(PoolId id) =>
            id == PoolId.BaitProjectile ||
            id == PoolId.ImpMisChiefRangerProjectile ||
            id == PoolId.WormJuniorProjectile ||
            id == PoolId.FinnAttack ||
            id == PoolId.OthaAttack ||
            id == PoolId.LunaAttack ||
            id == PoolId.ImpMisChiefJuniorProjectile_1 ||
            id == PoolId.ImpMisChiefJuniorProjectile_2;
    }
}
