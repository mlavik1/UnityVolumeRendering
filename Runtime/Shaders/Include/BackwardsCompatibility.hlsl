#ifndef BACKWARDS_COMPATIBILITY_HLSL
#define BACKWARDS_COMPATIBILITY_HLSL

// Main light direction - differs per render pipeline
// Note: UVR_RP_BUILTIN/UVR_RP_URP are expected to be defined by the outer shader
#if defined(UVR_RP_BUILTIN)
    #define MAIN_LIGHT_DIRECTION_WS _WorldSpaceLightPos0.xyz
#elif defined(UVR_RP_URP)
    #define MAIN_LIGHT_DIRECTION_WS _MainLightPosition.xyz
#else
    // HDRP or unknown: fall back to zero (will use view direction)
    #define MAIN_LIGHT_DIRECTION_WS float3(1, 0, 0)
#endif

// Matrix compatibility - HDRP errors on legacy names
#if defined(UVR_RP_HDRP)
    #define UVR_MATRIX_CAMERA_TO_WORLD UNITY_MATRIX_I_V
    #define UVR_MATRIX_WORLD_TO_OBJECT UNITY_MATRIX_I_M
    #define UVR_MATRIX_OBJECT_TO_WORLD UNITY_MATRIX_M
#else
    #define UVR_MATRIX_CAMERA_TO_WORLD unity_CameraToWorld
    #define UVR_MATRIX_WORLD_TO_OBJECT unity_WorldToObject
    #define UVR_MATRIX_OBJECT_TO_WORLD unity_ObjectToWorld
#endif

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
