namespace HexaEngine.Core.Animations
{
    using System.Numerics;

    public class AnimatorStateMachine
    {
        public ChildAnimatorState[] States { get; set; }

        public ChildAnimatorStateMachine[] StateMachines { get; set; }

        public AnimatorState DefaultState { get; set; }

        public Vector3 AnyStatePosition { get; set; }

        public Vector3 EntryPosition { get; set; }

        public Vector3 ExitPosition { get; set; }

        public Vector3 ParentStateMachinePosition { get; set; }

        public AnimatorStateTransition[] AnyStateTransitions { get; set; }

        public AnimatorTransition[] EntryTransitions { get; set; }

        public AnimatorTransition[] Transitions { get; set; }
    }
}