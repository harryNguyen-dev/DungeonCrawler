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
            id == PoolId.WormJunior;

        public static bool IsProjectile(PoolId id) =>
            id == PoolId.BaitProjectile ||
            id == PoolId.ImpMisChiefRangerProjectile ||
            id == PoolId.WormJuniorProjectile ||
            id == PoolId.FinnAttack ||
            id == PoolId.OthaAttack;
    }
}
