Shader "VolumeRendering/URP/TransferFunctionShader"
{
    Properties
    {
        _HistTex ("Histogram Texture", 2D) = "white" {}
        _TFTex("Transfer Function Texture", 2D) = "white" {}
    }
    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal":"[10.0,10.5.3]"
        }

        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            Texture2D _HistTex;             SamplerState sampler_HistTex;
            Texture2D _TFTex;               SamplerState sampler_TFTex;

            float4 _TFTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.vertex = TransformWorldToHClip(worldPos);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float density = i.uv.x;
                float histY = _HistTex.Sample(sampler_HistTex, float2(density, 0.0f)).r;
                fixed4 tfCol = _TFTex.Sample(sampler_TFTex, float2(density, 0.0f));
                float4 histCol = histY > i.uv.y ? float4(1.0f, 1.0f, 1.0f, 1.0f) : float4(0.0f, 0.0f, 0.0f, 0.0f);
                
                float alpha = tfCol.a;
                if (i.uv.y > alpha)
                    tfCol.a = 0.0f;

                float4 col = histCol * 0.5f + tfCol * 0.7f;
                
                return col;
            }
            ENDCG
        }
    }
}
