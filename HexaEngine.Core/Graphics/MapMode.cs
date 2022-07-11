namespace HexaEngine.Core.Graphics
{
    public enum MapMode : int
    {
        Read = unchecked((int)1),
        Write = unchecked((int)2),
        ReadWrite = unchecked((int)3),
        WriteDiscard = unchecked((int)4),
        WriteNoOverwrite = unchecked((int)5)
    }
}