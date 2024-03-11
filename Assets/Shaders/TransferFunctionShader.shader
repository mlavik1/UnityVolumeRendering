Shader "VolumeRendering/TransferFunctionShader"
{
    Properties
    {
        _HistTex ("Histogram Texture", 2D) = "white" {}
        _TFTex("Transfer Function Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _HistTex;
            sampler2D _TFTex;

            float4 _TFTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _TFTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float density = i.uv.x;
                float histY = tex2D(_HistTex, float2(density, 0.0f)).r;
                float4 tfCol = tex2D(_TFTex, float2(density, 0.0f));
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
