namespace HexaEngine.Scenes
{
    using System;

    public class InvalidSceneException : Exception
    {
        public InvalidSceneException(string? message) : base(message)
        {
        }
    }
}