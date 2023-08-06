struct VertexData
{
    float3 position;
    float3 tex;
    float3 normal;
    float3 tangent;
    float3 bitangent;
};

Buffer<uint> indexBuffer : register(t0);
RWStructuredBuffer<VertexData> vertexBuffer : register(u0);

#define TerrainWidth 32
#define TerrainHeight 32

[numthreads(1, 1, 1)]
void main(uint3 dispatchThreadId : SV_DispatchThreadID)
{
    uint index = dispatchThreadId.y * TerrainWidth + dispatchThreadId.x;

    float3 normal = float3(0, 0, 0);
    if (dispatchThreadId.x < TerrainWidth - 1 && dispatchThreadId.y < TerrainHeight - 1)
    {
        uint i0 = indexBuffer[index * 3];
        uint i1 = indexBuffer[index * 3 + 1];
        uint i2 = indexBuffer[index * 3 + 2];

        // Calculate face normals of two adjacent triangles
        float3 p0 = vertexBuffer[i0].position;
        float3 p1 = vertexBuffer[i1].position;
        float3 p2 = vertexBuffer[i2].position;

        float3 u = p1 - p0;
        float3 v = p2 - p0;

        float3 faceNormal = normalize(cross(u, v));

        if (isnan(faceNormal.x))
        {
            return;
        }

        float3 a = normalize(u);
        float3 b = normalize(v);
        float w0 = dot(a, b);
        w0 = clamp(w0, -1, 1);
        w0 = acos(w0);

        float3 c = normalize(p2 - p1);
        float3 d = normalize(p0 - p1);
        float w1 = dot(c, d);
        w1 = clamp(w1, -1, 1);
        w1 = acos(w1);

        float3 e = normalize(p0 - p2);
        float3 f = normalize(p1 - p2);
        float w2 = dot(e, f);
        w2 = clamp(w2, -1, 1);
        w2 = acos(w2);

        vertexBuffer[i0].normal = faceNormal * w0 + vertexBuffer[i0].normal;
        vertexBuffer[i1].normal = faceNormal * w1 + vertexBuffer[i1].normal;
        vertexBuffer[i2].normal = faceNormal * w2 + vertexBuffer[i2].normal;
    }

    AllMemoryBarrierWithGroupSync();
}