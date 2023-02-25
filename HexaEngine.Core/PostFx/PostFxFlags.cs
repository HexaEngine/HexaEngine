namespace HexaEngine.Core.PostFx
{
    public enum PostFxFlags
    {
        None = 0,

        /// <summary>
        /// The Effect has no output or outputs to an buffer for binding it later in the pipeline.
        /// </summary>
        NoOutput,

        /// <summary>
        /// The Effect has no input.
        /// </summary>
        NoInput,
    }
}