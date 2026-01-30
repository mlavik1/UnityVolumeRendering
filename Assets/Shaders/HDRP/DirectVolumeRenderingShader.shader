Shader "VolumeRendering/HDRP/VolumeRendering"
{
    Properties
    {
        _DataTex ("Data Texture (Generated)", 3D) = "" {}
        _GradientTex("Gradient Texture (Generated)", 3D) = "" {}
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _TFTex("Transfer Function Texture (Generated)", 2D) = "" {}
        _ShadowVolume("Shadow volume Texture (Generated)", 3D) = "" {}
        _SamplingRateMultiplier("Sampling rate multiplier", Range(0.2, 2.0)) = 1.0
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
        _MinGradient("Gradient visibility threshold", Range(0.0, 1.0)) = 0.0
        _LightingGradientThresholdStart("Gradient threshold for lighting (end)", Range(0.0, 1.0)) = 0.0
        _LightingGradientThresholdEnd("Gradient threshold for lighting (start)", Range(0.0, 1.0)) = 0.0
        _SecondaryDataTex ("Secondary Data Texture (Generated)", 3D) = "" {}
        _SecondaryTFTex("Transfer Function Texture for secondary volume", 2D) = "" {}
        [HideInInspector] _ShadowVolumeTextureSize("Shadow volume dimensions", Vector) = (1, 1, 1)
        [HideInInspector] _TextureSize("Dataset dimensions", Vector) = (1, 1, 1)
}
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "HDRenderPipeline" }
        LOD 100
        Cull Front
        ZTest LEqual
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile MODE_DVR MODE_MIP MODE_SURF
            #pragma multi_compile __ TF2D_ON
            #pragma multi_compile __ CROSS_SECTION_ON
            #pragma multi_compile __ LIGHTING_ON
            #pragma multi_compile __ SHADOWS_ON
            #pragma multi_compile DEPTHWRITE_ON DEPTHWRITE_OFF
            #pragma multi_compile __ RAY_TERMINATE_ON
            #pragma multi_compile __ USE_MAIN_LIGHT
            #pragma multi_compile __ CUBIC_INTERPOLATION_ON
            #pragma multi_compile __ SECONDARY_VOLUME_ON
            #pragma multi_compile MULTIVOLUME_NONE MULTIVOLUME_OVERLAY MULTIVOLUME_ISOLATE
            #pragma vertex vert
            #pragma fragment frag

            #include "../Include/HDRPIncludes.hlsl"
            #include "../Include/VolumeRendering.hlsl"

            #define AMBIENT_LIGHTING_FACTOR 0.2
            #define JITTER_FACTOR 5.0

            struct vert_in
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexLocal : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct frag_out
            {
                float4 colour : SV_TARGET;
#if DEPTHWRITE_ON
                float depth : SV_DEPTH;
#endif
            };

            frag_in vert (vert_in v)
            {
                frag_in o;
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.vertex = TransformWorldToHClip(worldPos);
                o.uv = v.uv;
                o.vertexLocal = v.vertex.xyz;
                o.normal = TransformObjectToWorldNormal(v.normal);
                return o;
            }

            frag_out frag(frag_in i)
            {
                volrend_result result = volrend(i.vertexLocal, i.uv);
                frag_out o;
                o.colour = result.colour;
#if DEPTHWRITE_ON
                o.depth = result.depth;
#endif
                return o;
            }

            ENDHLSL
        }
    }
}
