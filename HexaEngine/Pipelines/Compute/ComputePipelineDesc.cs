namespace HexaEngine.Pipelines.Compute
{
    public struct ComputePipelineDesc
    {
        public string? Path;
        public string Entry;

        public ComputePipelineDesc()
        {
            Path = null;
            Entry = "main";
        }
    }
}