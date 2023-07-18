// These variables should be in a struct but some GPU drivers ignore the
// precision qualifier on individual struct members
float3x3 shading_tangentToWorld; // TBN matrix
float3 shading_position; // position of the fragment in world space
float3 shading_view; // normalized vector from the fragment to the eye
float3 shading_normal; // normalized transformed normal, in world space
float3 shading_geometricNormal; // normalized geometric normal, in world space
float3 shading_reflected; // reflection of view about normal
float shading_NoV; // dot(normal, view), always strictly >= MIN_N_DOT_V

#if defined(MATERIAL_HAS_BENT_NORMAL)
float3 shading_bentNormal;       // normalized transformed normal, in world space
#endif

#if defined(MATERIAL_HAS_CLEAR_COAT)
float3 shading_clearCoatNormal;  // normalized clear coat layer normal, in world space
#endif

float2 shading_normalizedViewportCoord;