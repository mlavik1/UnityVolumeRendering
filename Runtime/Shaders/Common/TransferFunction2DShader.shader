// Render pipeline agnostic shader, used by the 2D transfer function editor GUI.
Shader "VolumeRendering/TransferFunction2DShader"
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = mul(unity_MatrixVP, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f)));
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return _TFTex.Sample(sampler_TFTex, i.uv);
            }
            ENDHLSL
        }
    }
}
