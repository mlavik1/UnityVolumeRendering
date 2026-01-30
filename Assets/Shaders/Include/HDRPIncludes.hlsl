#ifndef HDRP_INCLUDES_HLSL
#define HDRP_INCLUDES_HLSL

// Conditionally include HDRP package or fallback to compatibility layer
#ifdef UVR_HDRP
    // HDRP package is installed, use the real headers
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#else
    // HDRP package not installed, use compatibility layer
    #include "HDRPCompatibility.hlsl"
#endif

#endif // HDRP_INCLUDES_HLSL
