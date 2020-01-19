// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "VolumeRendering/SliceRenderingShader"
{
    Properties
    {
        _DataTex("Data Texture (Generated)", 3D) = "" {}
        _TFTex("Transfer Function Texture", 2D) = "white" {}
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
                float4 relVert : TEXCOORD1;
            };

            sampler3D _DataTex;
            sampler2D _TFTex;
            uniform float4x4 _parentInverseMat;
            uniform float4x4 _planeMat;

            v2f vert (appdata v)
            {
                v2f o;
                float3 vert = float3(-v.uv.x, 0.0f, -v.uv.y) + float3(0.5f, 0.0f, 0.5f);
                vert = mul(_planeMat, float4(vert, 1.0f));
                //o.vertex = mul(UNITY_MATRIX_VP, float4(vert, 1.0f));
                o.vertex = UnityObjectToClipPos(v.vertex);;
                o.relVert = mul(_parentInverseMat, float4(vert, 1.0f));
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 dataCoord = i.relVert +float3(0.5f, 0.5f, 0.5f);
                float dataVal = tex3D(_DataTex, dataCoord).a;
                float4 col = tex2D(_TFTex, float2(dataVal, 0.0f));
                col.a = 1.0f;

                return col;
            }
            ENDCG
        }
    }
}
