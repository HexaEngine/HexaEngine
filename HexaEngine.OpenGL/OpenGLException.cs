namespace HexaEngine.OpenGL
{
    using Silk.NET.OpenGL;
    using System;

    public class OpenGLException : Exception
    {
        public OpenGLException(ErrorCode code, string message) : base(message)
        {
            Code = code;
        }

        public ErrorCode Code { get; }
    }
}