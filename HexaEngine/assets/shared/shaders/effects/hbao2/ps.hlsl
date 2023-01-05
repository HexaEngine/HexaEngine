// THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
// OF SUCH DAMAGE.

struct FragmentIn
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORD;
	float4 FrustumVector : FRUSTUM_VECTOR;
};

Texture2D<float> depthTexture;
Texture2D<float4> normalsTexture;
Texture2D<float3> ditherTexture;	// dither texture contains: x = cos(angle), y = -sin(angle), z = jitter

SamplerState sourceSampler
{
	Filter = MIN_MAG_MIP_POINT;
};

SamplerState ditherSampler
{
	Filter = MIN_MAG_MIP_POINT;
};

cbuffer cameraData
{
	float4x4 projectionMatrix;	  	// the local projection matrix
	float4 viewFrustumVectors[4]; 	// same as in vertex shader
	float2 renderTargetResolution;	// the size of the render target
};

cbuffer shaderData
{
	float2 sampleDirections[32];
	float strengthPerRay = 0.1875;	// strength / numRays
	uint numRays = 8;
	uint maxStepsPerRay = 5;
	float halfSampleRadius = .25;	// sampleRadius / 2
	float fallOff = 2.0;			// the maximum distance to count samples
	float ditherScale;				// the ratio between the render target size and the dither texture size. Normally: renderTargetResolution / 4
	float bias = .03;				// minimum factor to start counting occluders
};

// Unproject a value from the depth buffer to the Z value in view space.
// Multiply the result with an interpolated frustum vector to get the actual view-space coordinates
float DepthToViewZ(float depthValue)
{
	return projectionMatrix[3][2] / (depthValue - projectionMatrix[2][2]);
}

// snaps a uv coord to the nearest texel centre
float2 SnapToTexel(float2 uv, float2 maxScreenCoords)
{
	return round(uv * maxScreenCoords) * rcp(maxScreenCoords);
}

// rotates a sample direction according to the row-vectors of the rotation matrix
float2 Rotate(float2 vec, float2 rotationX, float2 rotationY)
{
	float2 rotated;
	// just so we can use dot product
	float3 expanded = float3(vec, 0.0);
	rotated.x = dot(expanded.xyz, rotationX.xyy);
	rotated.y = dot(expanded.xyz, rotationY.xyy);
	return rotated;
}

// Gets the view position for a uv coord
float3 GetViewPosition(float2 uv, float2 frustumDiff)
{
	float depth = depthTexture.SampleLevel(sourceSampler, uv, 0);
	float3 frustumVector = float3(viewFrustumVectors[3].xy + uv * frustumDiff, 1.0);
	return frustumVector * DepthToViewZ(depth);
}

// Retrieves the occlusion factor for a particular sample
// uv: the centre coordinate of the kernel
// frustumVector: The frustum vector of the sample point
// centerViewPos: The view space position of the centre point
// centerNormal: The normal of the centre point
// tangent: The tangent vector in the sampling direction at the centre point
// topOcclusion: The maximum cos(angle) found so far, will be updated when a new occlusion segment has been found
float GetSampleOcclusion(float2 uv, float3 frustumVector, float3 centerViewPos, float3 centerNormal, float3 tangent, inout float topOcclusion)
{
	// reconstruct sample's view space position based on depth buffer and interpolated frustum vector
	float sampleDepth = depthTexture.SampleLevel(sourceSampler, uv, 0);
	float3 sampleViewPos = frustumVector * DepthToViewZ(sampleDepth);

	// get occlusion factor based on candidate horizon elevation
	float3 horizonVector = sampleViewPos - centerViewPos;
	float horizonVectorLength = length(horizonVector);

	float occlusion;

	// If the horizon vector points away from the tangent, make an estimate
	if (dot(tangent, horizonVector) < 0.0)
		return 0.5;
	else
		occlusion = dot(centerNormal, horizonVector) / horizonVectorLength;

	// this adds occlusion only if angle of the horizon vector is higher than the previous highest one without branching
	float diff = max(occlusion - topOcclusion, 0.0);
	topOcclusion = max(occlusion, topOcclusion);

	// attenuate occlusion contribution using distance function 1 - (d/f)^2
	float distanceFactor = saturate(horizonVectorLength / fallOff);
	distanceFactor = 1.0 - distanceFactor * distanceFactor;
	return diff * distanceFactor;
}

// Retrieves the occlusion for a given ray
// origin: The uv coordinates of the ray origin (the AO kernel centre)
// direction: The direction of the ray
// jitter: The random jitter factor by which to offset the start position
// maxScreenCoords: The maximum screen position (the texel that corresponds with uv = 1)
// projectedRadii: The sample radius in uv space
// numStepsPerRay: The amount of samples to take along the ray
// centerViewPos: The view space position of the centre point
// centerNormal: The normal of the centre point
// frustumDiff: The difference between frustum vectors horizontally and vertically, used for frustum vector interpolation
float GetRayOcclusion(float2 origin, float2 direction, float jitter, float2 maxScreenCoords, float2 projectedRadii, uint numStepsPerRay, float3 centerViewPos, float3 centerNormal, float2 frustumDiff)
{
	// calculate the nearest neighbour sample along the direction vector
	float2 texelSizedStep = direction * rcp(renderTargetResolution);
	direction *= projectedRadii;

	// gets the tangent for the current ray, this will be used to handle opposing horizon vectors
	// Tangent is corrected with respect to per-pixel normal by projecting it onto the tangent plane defined by the normal
	float3 tangent = GetViewPosition(origin + texelSizedStep, frustumDiff) - centerViewPos;
	tangent -= dot(centerNormal, tangent) * centerNormal;

	// calculate uv increments per marching step, snapped to texel centres to avoid depth discontinuity artefacts
	float2 stepUV = SnapToTexel(direction.xy / (numStepsPerRay - 1), maxScreenCoords);

	// jitter the starting position for ray marching between the nearest neighbour and the sample step size
	float2 jitteredOffset = lerp(texelSizedStep, stepUV, jitter);
	float2 uv = SnapToTexel(origin + jitteredOffset, maxScreenCoords);

	// initial frustum vector matching the starting position and its per-step increments
	float3 frustumVector = float3(viewFrustumVectors[3].xy + uv * frustumDiff, 1.0);
	float2 frustumVectorStep = stepUV * frustumDiff;

	// top occlusion keeps track of the occlusion contribution of the last found occluder.
	// set to bias value to avoid near-occluders
	float topOcclusion = bias;
	float occlusion = 0.0;

	// march!
	for (uint step = 0; step < numStepsPerRay; ++step) {
		occlusion += GetSampleOcclusion(uv, frustumVector, centerViewPos, centerNormal, tangent, topOcclusion);

		uv += stepUV;
		frustumVector.xy += frustumVectorStep.xy;
	}

	return occlusion;
}

float main(FragmentIn pin) : SV_TARGET
{
	// The maximum screen position (the texel that corresponds with uv = 1), used to snap to texels
	// (normally, this would be passed in as a constant)
	float2 maxScreenCoords = renderTargetResolution - 1.0;

	// reconstruct view-space position from depth buffer
	float centerDepth = depthTexture.SampleLevel(sourceSampler, pin.UV, 0);
	float3 centerViewPos = pin.FrustumVector.xyz * DepthToViewZ(centerDepth);

	// unpack normal
	float3 centerNormal = normalize(normalsTexture.SampleLevel(sourceSampler, pin.UV, 0).xyz);

	// Get the random factors and construct the row vectors for the 2D matrix from cos(a) and -sin(a) to rotate the sample directions
	float3 randomFactors = ditherTexture.SampleLevel(ditherSampler, pin.UV * ditherScale, 0);
	float2 rotationX = normalize(randomFactors.xy - .5);
	float2 rotationY = rotationX.yx * float2(-1.0f, 1.0f);

	// normally, you'd pass this in as a constant, but placing it here makes things easier to understand.
	// basically, this is just so we can use UV coords to interpolate frustum vectors
	float2 frustumDiff = float2(viewFrustumVectors[2].x - viewFrustumVectors[3].x, viewFrustumVectors[0].y - viewFrustumVectors[3].y);

	// scale the sample radius perspectively according to the given view depth (becomes ellipse)
	float w = centerViewPos.z * projectionMatrix[2][3] + projectionMatrix[3][3];
	float2 projectedRadii = halfSampleRadius * float2(projectionMatrix[1][1], projectionMatrix[2][2]) / w;	// half radius because projection ([-1, 1]) -> uv ([0, 1])
	float screenRadius = projectedRadii.x * renderTargetResolution.x;

	// bail out if there's nothing to march
	if (screenRadius < 1.0)
		return 1.0;

	// do not take more steps than there are pixels
	uint numStepsPerRay = min(maxStepsPerRay, screenRadius);

	float totalOcclusion = 0.0;

	for (uint i = 0; i < numRays; ++i) {
		float2 sampleDir = Rotate(sampleDirections[i].xy, rotationX, rotationY);
		totalOcclusion += GetRayOcclusion(pin.UV, sampleDir, randomFactors.z, maxScreenCoords, projectedRadii, numStepsPerRay, centerViewPos, centerNormal, frustumDiff);
	}

	return 1.0 - saturate(strengthPerRay * totalOcclusion);
}