namespace PlayerController.Skill
{
    public interface ISkillDelivery
    {
        void Execute(in SkillExecutionContext context);
    }
}
