// THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
// OF SUCH DAMAGE.

struct VertexIn
{
	float2 Position : POSITION;
	float2 UV : TEXCOORD;
};

struct VertexOut
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORD;
	float4 FrustumVector : FRUSTUM_VECTOR;
};

cbuffer cameraData
{
	// contains the view-space vector pointing to the frustum corner in the same order corresponding to the vertices of the quad being rendered,
	// scaled so that z == 1 (used to reconstruct view space positions in the fragment shader)
	// ie: viewFrustumVectors[0] = nearCorner[0] / nearCorner[0].z
	float4 viewFrustumVectors[4];
};

VertexOut main(VertexIn vin, uint vertexID : SV_VertexID)
{
	VertexOut vertex;
	vertex.Position = float4(vin.Position, 0.0f, 1.0f);
	vertex.UV = vin.UV;
	vertex.FrustumVector = viewFrustumVectors[vertexID];
	return vertex;
}