namespace HexaEngine.Animations
{
    public class AnimatorStateMachine
    {
        public AnimatorState DefaultState { get; set; }

        public List<AnimatorTransition> Transitions { get; } = [];

        public AnimatorState CurrentState { get; protected set; }

        public event Action<AnimatorTransition>? Transition;

        public event Action<AnimatorState>? StateChanged;

        public void UpdateState(IList<AnimatorParameter> parameters)
        {
            static AnimatorParameter? GetParameter(IList<AnimatorParameter> parameters, string name)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.Name == name)
                        return parameter;
                }
                return null;
            }

            for (int i = 0; i < Transitions.Count; i++)
            {
                var transition = Transitions[i];
                var condition = transition.Condition;
                var parameter = GetParameter(parameters, condition.ParameterName);

                // if the parameter is null just skip.
                if (parameter == null)
                {
                    continue;
                }

                var value = parameter.ConvertToFloat();

                bool conditionMet;
                switch (condition.Mode)
                {
                    case AnimatorConditionMode.Equals:
                        conditionMet = condition.Threshold == value;
                        break;

                    case AnimatorConditionMode.NotEquals:
                        conditionMet = condition.Threshold != value;
                        break;

                    case AnimatorConditionMode.Less:
                        conditionMet = condition.Threshold < value;
                        break;

                    case AnimatorConditionMode.Greater:
                        conditionMet = condition.Threshold > value;
                        break;

                    case AnimatorConditionMode.LessEquals:
                        conditionMet = condition.Threshold <= value;
                        break;

                    case AnimatorConditionMode.GreaterEquals:
                        conditionMet = condition.Threshold >= value;
                        break;

                    default:
                        // skip if the condition mode is unknown.
                        continue;
                }

                if (conditionMet)
                {
                    OnTransition(transition);
                }
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (parameter.Type == AnimatorParameterType.Trigger)
                {
                    parameter.Value = false;
                }
            }
        }

        protected virtual void OnTransition(AnimatorTransition transition)
        {
            Transition?.Invoke(transition);
            OnStateChanged(transition.State);
        }

        protected virtual void OnStateChanged(AnimatorState state)
        {
            if (CurrentState == state)
            {
                return;
            }
            CurrentState = state;
            StateChanged?.Invoke(state);
        }
    }
}