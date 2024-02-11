#ifndef MATERIAL_H_INCLUDED
#define MATERIAL_H_INCLUDED

#define MATERIAL_SHADING_MODEL_UNLIT 0
#define MATERIAL_SHADING_MODEL_BLIN_PHONG 1
#define MATERIAL_SHADING_MODEL_COOK_TORRANCE 2
#define MATERIAL_SHADING_MODEL_COOK_TORRANCE_ANSIOTROPIC 3
#define MATERIAL_SHADING_MODEL_COOK_TORRANCE_CLOTH 4

struct Material
{
    uint ShadingModel;
    float EmissiveStrength;
};

#endif