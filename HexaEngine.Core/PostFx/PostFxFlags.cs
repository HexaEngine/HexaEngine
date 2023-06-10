﻿namespace HexaEngine.Core.PostFx
{
    [Flags]
    public enum PostFxFlags
    {
        None = 0,

        /// <summary>
        /// The Effect has no output or outputs to an buffer for binding it later in the pipeline.
        /// </summary>
        NoOutput = 1,

        /// <summary>
        /// The Effect has no input.
        /// </summary>
        NoInput = 2,

        /// <summary>
        /// The Effect uses the old RenderTarget of the last pipeline.
        /// </summary>
        Inline = 4
    }
}