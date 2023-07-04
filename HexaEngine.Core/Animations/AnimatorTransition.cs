namespace HexaEngine.Core.Animations
{
    public class AnimatorTransition
    {
        public bool Solo { get; set; }

        public bool Mute { get; set; }

        public bool IsExit { get; set; }

        public AnimatorStateMachine DestinationStateMachine { get; set; }

        public AnimatorState DestinationState { get; set; }

        public AnimatorCondition[] Conditions { get; set; }

        public void AddCondition(AnimatorCondition condition)
        {
            AnimatorCondition[] conditions = Conditions;
            ArrayUtils.Add(ref conditions, condition);
            Conditions = conditions;
        }

        public void RemoveCondition(AnimatorCondition condition)
        {
            AnimatorCondition[] conditions = Conditions;
            ArrayUtils.Remove(ref conditions, condition);
            Conditions = conditions;
        }
    }
}