StructuredBuffer<float3> positionsBuffer; // Input: Positionen des Meshes
StructuredBuffer<uint> indicesBuffer; // Input: Indizes des Meshes
RWStructuredBuffer<float3> normalsBuffer; // Output: Normalen des Meshes

cbuffer ComputeParams
{
    uint vertexCount;
    uint indexCount;
};

[numthreads(64, 1, 1)]
void ComputeNormals(uint3 dispatchThreadId : SV_DispatchThreadID)
{
    uint vertexIndex = dispatchThreadId.x + (dispatchThreadId.y * 64); // Berechnen des globalen Index der Vertex

    if (vertexIndex >= vertexCount)
        return;

    float3 normalSum = float3(0, 0, 0);

    // Berechne die Normalen für jede Fläche, die diese Vertex enthält
    for (uint face = 0; face < indexCount / 3; ++face)
    {
        uint i0 = indicesBuffer[face * 3];
        uint i1 = indicesBuffer[face * 3 + 1];
        uint i2 = indicesBuffer[face * 3 + 2];

        if (i0 == vertexIndex || i1 == vertexIndex || i2 == vertexIndex)
        {
            float3 p0 = positionsBuffer[i0];
            float3 p1 = positionsBuffer[i1];
            float3 p2 = positionsBuffer[i2];

            float3 u = p1 - p0;
            float3 v = p2 - p0;

            float3 faceNormal = cross(u, v);
            normalSum += faceNormal;
        }
    }

    // Normalisiere die Summe der Normalen
    normalSum = normalize(normalSum);

    // Speichere die berechnete Normalen in den Ausgabedaten
    normalsBuffer[vertexIndex] = normalSum;
}