namespace Combat
{
    public interface ICombatable
    {
        public int GetDamageThisAttack();

        public bool IsParry();
    }
}