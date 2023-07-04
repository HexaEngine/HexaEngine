namespace HexaEngine.Core.Animations
{
    using HexaEngine.Scenes.Components;

    public interface IStateMachineBehaviour
    {
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }

        // OnStateMachineEnter is called when entering a statemachine via its Entry Node
        public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
        }

        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
        }

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
        }

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
        }

        // OnStateMachineEnter is called when entering a statemachine via its Entry Node
        public void OnStateMachineEnter(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller)
        {
        }

        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        public void OnStateMachineExit(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller)
        {
        }
    }
}