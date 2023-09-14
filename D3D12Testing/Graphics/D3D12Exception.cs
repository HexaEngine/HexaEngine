﻿namespace HexaEngine.D3D12
{
    using System;
    using System.Runtime.InteropServices;

    public class D3D12Exception : Exception
    {
        public D3D12Exception(ResultCode code) : base((Marshal.GetExceptionForHR((int)code) ?? new Exception("Unable to get exception from HRESULT")).Message)
        {
        }
    }
}