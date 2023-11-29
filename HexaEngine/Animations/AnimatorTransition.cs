namespace HexaEngine.Animations
{
    public class AnimatorTransition
    {
        public AnimatorState State { get; set; }

        public AnimatorStateTransition Transition { get; set; }

        public AnimatorCondition Condition { get; set; }
    }
}