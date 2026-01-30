#ifndef HDRP_COMPATIBILITY_INCLUDED
#define HDRP_COMPATIBILITY_INCLUDED

// This file provides HDRP-compatible functions without requiring the HDRP package
// It uses Unity's built-in matrices which are available in all render pipelines

// Declare Unity built-in matrices and variables for HLSL
// These need to be declared when using HLSLPROGRAM instead of CGPROGRAM
float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_MatrixInvP;
float4x4 unity_MatrixInvVP;
float4x4 unity_CameraToWorld;
float4x4 unity_WorldToCamera;
float3 _WorldSpaceCameraPos;
float4 unity_OrthoParams; // x = width, y = height, z = unused, w = ortho (1.0) vs perspective (0.0)

// Define matrix accessors for compatibility
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_I_V unity_MatrixInvV
#define UNITY_MATRIX_P unity_MatrixInvP
#define UNITY_MATRIX_I_P unity_MatrixInvP

// Transform functions compatible with HDRP but work without it
float3 TransformObjectToWorld(float3 positionOS)
{
    return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
}

float4 TransformWorldToHClip(float3 positionWS)
{
    return mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
}

float4 TransformObjectToHClip(float3 positionOS)
{
    float3 positionWS = TransformObjectToWorld(positionOS);
    return TransformWorldToHClip(positionWS);
}

float3 TransformObjectToWorldNormal(float3 normalOS)
{
    return normalize(mul((float3x3)unity_ObjectToWorld, normalOS));
}

float3 TransformWorldToObject(float3 positionWS)
{
    return mul(unity_WorldToObject, float4(positionWS, 1.0)).xyz;
}

float3 GetCameraPositionWS()
{
    return _WorldSpaceCameraPos;
}

// Helper function for object-space view direction
float3 ObjSpaceViewDir(float3 v)
{
    return TransformWorldToObject(GetCameraPositionWS()) - v;
}

// Convenience function for Unity compatibility
float4 UnityObjectToClipPos(float3 pos)
{
    return TransformObjectToHClip(pos);
}

// Stereo/VR support macros - define as empty if not defined by pipeline
#ifndef UNITY_VERTEX_INPUT_INSTANCE_ID
    #define UNITY_VERTEX_INPUT_INSTANCE_ID
#endif

#ifndef UNITY_VERTEX_OUTPUT_STEREO
    #define UNITY_VERTEX_OUTPUT_STEREO
#endif

#endif // HDRP_COMPATIBILITY_INCLUDED
