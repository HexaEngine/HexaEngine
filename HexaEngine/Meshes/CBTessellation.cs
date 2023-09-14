namespace HexaEngine.Meshes
{
    public struct CBTessellation
    {
        public float MinFactor;
        public float MaxFactor;
        public float MinDistance;
        public float MaxDistance;

        public CBTessellation()
        {
            MinFactor = 1.0f;
            MaxFactor = 2.0f;
            MinDistance = 4.0f;
            MaxDistance = 50.0f;
        }

        public CBTessellation(float minFactor, float maxFactor, float minDistance, float maxDistance)
        {
            MinFactor = minFactor;
            MaxFactor = maxFactor;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }
    }
}