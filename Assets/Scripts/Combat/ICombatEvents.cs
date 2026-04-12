namespace Combat
{
    public interface ICombatEvents
    {
        public void AE_AttackEnd();
        public void AE_HitboxOn();
        public void AE_HitboxOff();
        public void AE_ParryEnd();
        public void AE_HitReactionEnd();
        public void AE_ParryStart();
        public void AE_ParryStop();
    }
}