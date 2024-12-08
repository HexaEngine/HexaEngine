struct DrawIndexedInstancedIndirectArgs
{
	uint IndexCountPerInstance;
	uint InstanceCount;
	uint StartIndexLocation;
	int BaseVertexLocation;
	uint StartInstanceLocation;
};

StructuredBuffer<DrawIndexedInstancedIndirectArgs> input;
RWStructuredBuffer<DrawIndexedInstancedIndirectArgs> output;

[numthreads(1, 1, 1)]
void main(uint threadId : SV_DispatchThreadID)
{
	output[threadId] = input[threadId];
}