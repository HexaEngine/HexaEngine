﻿namespace HexaEngine.Core.Assets.Importer
{
    [Flags]
    public enum TexPostProcessSteps
    {
        None = 0,
        Scale = 1,
        Convert = 2,
        GenerateMips = 4,
    }
}