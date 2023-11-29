namespace HexaEngine.Animations
{
    public class AnimatorParameter
    {
        public string Name;
        public AnimatorParameterType Type;
        public object DefaultValue;
        public object Value;

        public AnimatorParameter(string name, AnimatorParameterType type, object defaultValue, object value)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Value = value;
        }

        public float ConvertToFloat()
        {
            switch (Type)
            {
                case AnimatorParameterType.Trigger:
                case AnimatorParameterType.Bool:
                    return ((bool)Value) ? 1 : 0;

                case AnimatorParameterType.Int:
                    return (int)Value;

                case AnimatorParameterType.Float:
                    return (float)Value;
            }
            throw new NotSupportedException($"Parameter type {Type} is not supported");
        }
    }
}