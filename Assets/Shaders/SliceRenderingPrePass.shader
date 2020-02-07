// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "VolumeRendering/SliceRenderingPrePass"
{
    Properties
    {
        _DataTex("Data Texture (Generated)", 3D) = "" {}
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
            
            uniform float4x4 _parentInverseMat;
            uniform float4x4 _planeMat;

            v2f vert (appdata v)
            {
                v2f o;
                float3 vert = float3(-v.uv.x, v.uv.y, 0.0f) + float3(0.5f, -0.5f, 0.0f);
                vert = mul(_planeMat, float4(vert, 1.0f));
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.relVert = mul(_parentInverseMat, float4(vert, 1.0f));
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 pos = i.relVert + float3(0.5f, 0.5f, 0.5f);
                float4 output = tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0f));
                return output;
            }
            ENDCG
        }
    }
}
