#ifndef BACKWARDS_COMPATIBILITY_HLSL
#define BACKWARDS_COMPATIBILITY_HLSL

// These functions are available in UnityCG.cginc (Built-in) but not in URP/HDRP
// Only define them if not already available

#ifndef UNITY_CG_INCLUDED
// ObjSpaceViewDir - returns object space direction from vertex to camera
float3 ObjSpaceViewDir(float3 v)
{
    return TransformWorldToObject(GetCameraPositionWS()) - v;
}

// UnityObjectToClipPos - transforms object space position to clip space
float4 UnityObjectToClipPos(float3 pos)
{
    return TransformObjectToHClip(pos);
}
#endif

#endif // BACKWARDS_COMPATIBILITY_HLSL
