Shader "VolumeRendering/HDRP/TransferFunctionPaletteShader"
{
    Properties
    {
        _TFTex("Transfer Function Texture", 2D) = "white" {}
    }
    SubShader
    {
        PackageRequirements { "com.unity.render-pipelines.high-definition" }
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "HDRenderPipeline" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_VERTEX_OUTPUT_STEREO
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Texture2D _TFTex;             SamplerState sampler_TFTex;

            float4 _TFTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.vertex = TransformWorldToHClip(worldPos);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = _TFTex.Sample(sampler_TFTex, float2(i.uv.x, 0.0f));
                col.a = 1.0f;
#if !UNITY_COLORSPACE_GAMMA
#define INVERSA_GAMMA 0.4545454
                col.rgb = pow(col.rgb, half3(INVERSA_GAMMA, INVERSA_GAMMA, INVERSA_GAMMA));
#endif

                return col;
            }
            ENDHLSL
        }
    }
}
