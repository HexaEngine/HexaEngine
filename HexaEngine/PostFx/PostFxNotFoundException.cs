namespace HexaEngine.PostFx
{
    public class PostFxNotFoundException : Exception
    {
        public PostFxNotFoundException(string postFxName) : base($"The effect {postFxName} was not found.")
        {
        }
    }
}