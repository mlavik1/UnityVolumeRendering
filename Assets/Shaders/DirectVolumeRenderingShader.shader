Shader "VolumeRendering/DirectVolumeRenderingShader"
{
    Properties
    {
        _DataTex ("Data Texture (Generated)", 3D) = "" {}
        _GradientTex("Gradient Texture (Generated)", 3D) = "" {}
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _TFTex("Transfer Function Texture (Generated)", 2D) = "" {}
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Cull Front
        ZTest LEqual
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma multi_compile MODE_DVR MODE_MIP MODE_SURF
            #pragma multi_compile __ TF2D_ON
            #pragma multi_compile __ CROSS_SECTION_ON
            #pragma multi_compile __ LIGHTING_ON
            #pragma multi_compile DEPTHWRITE_ON DEPTHWRITE_OFF
            #pragma multi_compile __ DVR_BACKWARD_ON
            #pragma multi_compile __ RAY_TERMINATE_ON
            #pragma multi_compile __ USE_MAIN_LIGHT
            #pragma multi_compile __ CUBIC_INTERPOLATION_ON
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "TricubicSampling.cginc"
            #include "DirectVolumeRendering.cginc"

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

            frag_in vert(vert_in v)
            {
                frag_in o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            frag_out frag(frag_in i)
            {
#if MODE_DVR
                return frag_dvr(i.uv, i.vertexLocal);
#elif MODE_MIP
                return frag_mip(i.uv, i.vertexLocal);
#elif MODE_SURF
                return frag_surf(i.uv, i.vertexLocal);
#endif
            }

            ENDCG
        }

        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma multi_compile MODE_DVR MODE_MIP MODE_SURF
            #pragma multi_compile __ TF2D_ON
            #pragma multi_compile __ CROSS_SECTION_ON
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #define SHADOW_CASTER_PASS 1
            #include "DirectVolumeRendering.cginc"

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

            frag_in vert(vert_in v)
            {
                frag_in o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float4 frag(frag_in i) : SV_Target
            {
#if MODE_DVR
                frag_out output = frag_dvr(i.uv, i.vertexLocal);
#elif MODE_MIP
                frag_out output = frag_mip(i.uv, i.vertexLocal);
#elif MODE_SURF
                frag_out output = frag_surf(i.uv, i.vertexLocal);
#endif
                clip(output.colour.a - 0.01);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}
