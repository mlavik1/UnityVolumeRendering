Shader "VolumeRendering/Builtin/CrossSectionPlane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RimWidth ("Rim Width", Range(0.001, 0.2)) = 0.03
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        CULL Off

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
                float4 vertex : SV_POSITION;
            };

            Texture2D _MainTex;             SamplerState sampler_MainTex;
            float4 _MainTex_ST;
            float _RimWidth;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 edgeDist = min(i.uv, 1.0 - i.uv);
                float dist = min(edgeDist.x, edgeDist.y);
                float rimAlpha = 1.0 - smoothstep(0.0, _RimWidth, dist);
                return fixed4(0, 1, 0, rimAlpha);
            }
            ENDCG
        }
    }
}
