namespace HexaEngine.Animations
{
    public class AnimatorTransition
    {
        public AnimatorState State { get; set; } = null!;

        public AnimatorStateTransition Transition { get; set; } = null!;

        public AnimatorCondition Condition { get; set; }
    }
}