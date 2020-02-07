// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "VolumeRendering/SliceRenderingPostPass"
{
    Properties
    {
        _TFTex("Transfer Function Texture", 2D) = "white" {}
        _InputTex("Input texture from previous pass", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _TFTex;
            sampler2D _InputTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 dataVal = tex2D(_InputTex, i.uv);
                float4 col = tex2D(_TFTex, float2(dataVal.a, 0.0f));
                col.a = 1.0f;
                return col;
            }
            ENDCG
        }
    }
}
