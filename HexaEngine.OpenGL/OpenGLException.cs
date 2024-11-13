namespace HexaEngine.OpenGL
{
    using Hexa.NET.OpenGL;
    using System;

    public class OpenGLException : Exception
    {
        public OpenGLException(GLErrorCode code, string message) : base(message)
        {
            Code = code;
        }

        public GLErrorCode Code { get; }
    }
}