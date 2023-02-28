﻿namespace HexaEngine.Core.Culling
{
    using System;

    [Flags]
    public enum CullingFlags
    {
        None = 0,
        Frustum = 1,
        Occlusion = 2,
    }
}