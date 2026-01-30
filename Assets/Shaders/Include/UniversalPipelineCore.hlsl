#ifndef UNIVERSAL_PIPELINE_CORE_INCLUDED
#define UNIVERSAL_PIPELINE_CORE_INCLUDED

// This file provides URP-compatible functions without requiring the URP package
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

// Transform functions compatible with URP
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

// Stereo/VR support macros
#ifndef UNITY_VERTEX_INPUT_INSTANCE_ID
    #define UNITY_VERTEX_INPUT_INSTANCE_ID
#endif

#ifndef UNITY_VERTEX_OUTPUT_STEREO
    #define UNITY_VERTEX_OUTPUT_STEREO
#endif

#endif // UNIVERSAL_PIPELINE_CORE_INCLUDED
