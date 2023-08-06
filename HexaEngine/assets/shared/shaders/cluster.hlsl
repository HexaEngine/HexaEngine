#define CLUSTERS_X 16
#define CLUSTERS_Y 9
#define CLUSTERS_Z 24

#define CLUSTERS_X_THREADS 16
#define CLUSTERS_Y_THREADS 9
#define CLUSTERS_Z_THREADS 4

#define MAX_LIGHTS_PER_CLUSTER 128

struct Cluster
{
    float4 minPoint;
    float4 maxPoint;
};

struct LightGrid
{
    uint lightOffset;
    uint lightCount;
    uint decalOffset;
    uint decalCount;
    uint probeOffset;
    uint probeCount;
};

uint GetClusterIndex(float depth, float camNear, float camFar, float2 screenDim, float4 pos)
{
    // linear depth
    float z_b = depth;
    float z_n = 2.0 * z_b - 1.0;
    float linearDepth = 2.0 * camNear * camFar / (camFar + camNear - z_n * (camFar - camNear));

    // could be pre computed on cpu.
    float scale = CLUSTERS_Z / log2(camFar / camNear);
    float bias = -(CLUSTERS_Z * log2(camNear) / log2(camFar / camNear));

    uint zTile = uint(max(log2(linearDepth) * scale + bias, 0.0));
    uint clusterDim = (uint) ceil(screenDim.x / (float) CLUSTERS_X);
    uint3 tiles = uint3(uint2(pos.xy / clusterDim), zTile);

    uint tileIndex = tiles.x + CLUSTERS_X * tiles.y + (CLUSTERS_X * CLUSTERS_Y) * tiles.z;
    return tileIndex;
}