#ifndef URP_INCLUDES_HLSL
#define URP_INCLUDES_HLSL

// Conditionally include URP package or fallback to compatibility layer
#ifdef UVR_URP
    // URP package is installed, use the real headers
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
    // URP package not installed, use compatibility layer
    #include "UniversalPipelineCore.hlsl"
#endif

#endif // URP_INCLUDES_HLSL
