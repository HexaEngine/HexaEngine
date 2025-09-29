namespace HexaEngine.ShadingLang
{
    internal static class Helper
    {
        public static HXSLFunctionFlags ToFunctionFlags(this HXSLModifierFlags modifiers, bool strict = true)
        {
            if (strict)
            {
                const HXSLModifierFlags validForFunction = HXSLModifierFlags.Inline | HXSLModifierFlags.Public | HXSLModifierFlags.Private;

                var invalid = modifiers & ~validForFunction;
                if (invalid != HXSLModifierFlags.None)
                {
                    throw new Exception($"Invalid modifiers for function: {invalid}");
                }
            }

            HXSLFunctionFlags result = HXSLFunctionFlags.None;

            if ((modifiers & HXSLModifierFlags.Inline) != 0)
                result |= HXSLFunctionFlags.Inline;
            if ((modifiers & HXSLModifierFlags.Public) != 0)
                result |= HXSLFunctionFlags.Public;
            if ((modifiers & HXSLModifierFlags.Private) != 0)
                result |= HXSLFunctionFlags.Private;

            return result;
        }

        public static HXSLFieldFlags ToFieldFlags(this HXSLModifierFlags modifiers, bool strict = true)
        {
            if (strict)
            {
                const HXSLModifierFlags validForFunction = HXSLModifierFlags.Static | HXSLModifierFlags.Nointerpolation | HXSLModifierFlags.Shared | HXSLModifierFlags.GroupShared | HXSLModifierFlags.Uniform | HXSLModifierFlags.Volatile | HXSLModifierFlags.Public | HXSLModifierFlags.Private;

                var invalid = modifiers & ~validForFunction;
                if (invalid != HXSLModifierFlags.None)
                {
                    throw new Exception($"Invalid modifiers for function: {invalid}");
                }
            }

            HXSLFieldFlags result = HXSLFieldFlags.None;

            if ((modifiers & HXSLModifierFlags.Static) != 0)
                result |= HXSLFieldFlags.Static;
            if ((modifiers & HXSLModifierFlags.Nointerpolation) != 0)
                result |= HXSLFieldFlags.Nointerpolation;
            if ((modifiers & HXSLModifierFlags.Shared) != 0)
                result |= HXSLFieldFlags.Shared;
            if ((modifiers & HXSLModifierFlags.GroupShared) != 0)
                result |= HXSLFieldFlags.GroupShared;
            if ((modifiers & HXSLModifierFlags.Uniform) != 0)
                result |= HXSLFieldFlags.Uniform;
            if ((modifiers & HXSLModifierFlags.Volatile) != 0)
                result |= HXSLFieldFlags.Volatile;
            if ((modifiers & HXSLModifierFlags.Public) != 0)
                result |= HXSLFieldFlags.Public;
            if ((modifiers & HXSLModifierFlags.Private) != 0)
                result |= HXSLFieldFlags.Private;

            return result;
        }
    }
}