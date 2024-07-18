struct SelectionData
{
    float2 Location;
    int FaceID;
    float Padd0;
};

cbuffer MouseBuffer
{
    float4x4 World;
	float3 Position;
    float Padd0;
	float3 Direction;
    float Padd1;
};

StructuredBuffer<float3> vertices : register(t0);
RWStructuredBuffer<SelectionData> output : register(u0);

bool intersect(in float3 v0, in float3 v1, in float3 v2, out float3 pointInTriangle)
{
    pointInTriangle = 0;
    const float EPSILON = 0.0000001f;

    float3 edge1, edge2, h, s, q;
    float a, f, u, v;
    edge1 = v1 - v0;
    edge2 = v2 - v0;
    h = cross(Direction, edge2);
    a = dot(edge1, h);

    if (a > -EPSILON && a < EPSILON)
    {
        return false; // This ray is parallel to this triangle.
    }

    f = 1.0f / a;
    s = Position - v0;
    u = f * dot(s, h);

    if (u < 0.0 || u > 1.0)
    {
        return false;
    }

    q = cross(s, edge1);
    v = f * dot(Direction, q);

    if (v < 0.0 || u + v > 1.0)
    {
        return false;
    }

            // At this stage we can compute t to find out where the intersection point is on the line.
    float t = f * dot(edge2, q);

    if (t > EPSILON) // ray intersection
    {
        pointInTriangle = Position + Direction * t;
        return true;
    }
    else // This means that there is a line intersection but not a ray intersection.
    {
        return false;
    }
}

[numthreads(32, 1, 1)]
void main(uint id : SV_DispatchThreadID)
{
    float3 pos0 = vertices[id * 3 + 0];
    float3 pos1 = vertices[id * 3 + 1];
    float3 pos2 = vertices[id * 3 + 2];
    
    pos0 = mul(float4(pos0, 1), World).xyz;
    pos1 = mul(float4(pos1, 1), World).xyz;
    pos2 = mul(float4(pos2, 1), World).xyz;
    
    float3 location;
    if (intersect(pos0, pos1, pos2, location))
    {
        SelectionData data;
        data.FaceID = id;
        data.Location = location;
        output[0] = data;
    }
}