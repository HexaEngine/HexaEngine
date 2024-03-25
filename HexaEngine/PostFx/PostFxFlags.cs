namespace HexaEngine.PostFx
{
    [Flags]
    public enum PostFxFlags
    {
        /// <summary>
        /// The Effect is a normal effect.
        /// </summary>
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
        Inline = 4,

        /// <summary>
        /// The Effect has a prepass.
        /// </summary>
        PrePass = 8,

        /// <summary>
        /// The Effect is optional, meaning it will be automatically enabled and disabled
        /// based on whether it is needed by any other shader in the pipeline.
        /// </summary>
        Optional = 16,

        /// <summary>
        /// The Effect has conditional render code.
        /// Will be rendered in immediate mode.
        /// </summary>
        Dynamic = 32,

        /// <summary>
        /// This Effect will be composed later.
        /// </summary>
        Compose = 64,

        /// <summary>
        /// Do not use or else the effect will be treated as composition target.
        /// </summary>
        ComposeTarget = 128,

        /// <summary>
        /// The effect is always enabled and cannot be disabled.
        /// </summary>
        AlwaysEnabled = 256,
    }
}