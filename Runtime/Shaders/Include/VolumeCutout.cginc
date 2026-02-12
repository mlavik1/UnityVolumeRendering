#define CROSS_SECTION_TYPE_PLANE 1 
#define CROSS_SECTION_TYPE_BOX_INCL 2 
#define CROSS_SECTION_TYPE_BOX_EXCL 3
#define CROSS_SECTION_TYPE_SPHERE_INCL 4
#define CROSS_SECTION_TYPE_SPHERE_EXCL 5

float4x4 _CrossSectionMatrices[8];
float _CrossSectionTypes[8];
int _NumCrossSections;

bool IsCutout(float3 currPos)
{
    // Move the reference in the middle of the mesh, like the pivot
    float4 pivotPos = float4(currPos - float3(0.5f, 0.5f, 0.5f), 1.0f);

    bool clipped = false;
    for (int i = 0; i < _NumCrossSections && !clipped; ++i)
    {
        const int type = (int)_CrossSectionTypes[i];
        const float4x4 mat = _CrossSectionMatrices[i];

        // Convert from model space to plane's vector space
        float3 planeSpacePos = mul(mat, pivotPos).xyz;
        if (type == CROSS_SECTION_TYPE_PLANE)
            clipped = planeSpacePos.z > 0.0f;
        else if (type == CROSS_SECTION_TYPE_BOX_INCL)
            clipped = !(planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f);
        else if (type == CROSS_SECTION_TYPE_BOX_EXCL)
            clipped = planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f;
        else if (type == CROSS_SECTION_TYPE_SPHERE_INCL)
            clipped = length(planeSpacePos) > 0.5;
        else if (type == CROSS_SECTION_TYPE_SPHERE_EXCL)
            clipped = length(planeSpacePos) < 0.5;
    }
    return clipped;
}
