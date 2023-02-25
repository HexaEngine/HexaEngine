namespace HexaEngine.Core.Effects
{
    using HexaEngine.Core.Graphics;

    public class Effect
    {
#pragma warning disable CS8618 // Non-nullable field '_commandList' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS0169 // The field 'Effect._commandList' is never used
        private ICommandList _commandList;
#pragma warning restore CS0169 // The field 'Effect._commandList' is never used
#pragma warning restore CS8618 // Non-nullable field '_commandList' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS0169 // The field 'Effect.dirty' is never used
        private bool dirty;
#pragma warning restore CS0169 // The field 'Effect.dirty' is never used
    }
}