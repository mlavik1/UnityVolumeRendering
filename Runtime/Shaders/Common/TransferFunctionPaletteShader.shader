// Render pipeline agnostic shader, used by the transfer function editor GUI.
Shader "VolumeRendering/TransferFunctionPaletteShader"
{
    Properties
    {
        _TFTex("Transfer Function Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Built-in shader variables, set by the engine in all render pipelines.
            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Texture2D _TFTex;               SamplerState sampler_TFTex;

            float4 _TFTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = mul(unity_MatrixVP, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f)));
                o.uv = v.uv * _TFTex_ST.xy + _TFTex_ST.zw; // Applies zoom
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = _TFTex.Sample(sampler_TFTex, float2(i.uv.x, 0.0f));
                col.a = 1.0f;
#if !UNITY_COLORSPACE_GAMMA
#define INVERSA_GAMMA 0.4545454
                col.rgb = pow(col.rgb, float3(INVERSA_GAMMA, INVERSA_GAMMA, INVERSA_GAMMA));
#endif

                return col;
            }
            ENDHLSL
        }
    }
}
