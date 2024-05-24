﻿namespace HexaEngine.Network.Events
{
    using HexaEngine.Network.Protocol;

    public struct ProtocolErrorEventArgs
    {
        public ErrorCode ErrorCode;
        public ErrorSeverity Severity;

        public ProtocolErrorEventArgs(ProtocolError error)
        {
            ErrorCode = error.ErrorCode;
            Severity = error.Severity;
        }
    }
}