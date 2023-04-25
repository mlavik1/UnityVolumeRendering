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
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_VERTEX_OUTPUT_STEREO
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 relVert : TEXCOORD1;
            };

            sampler3D _DataTex;
            sampler2D _TFTex;
            // Parent's inverse transform (used to convert from world space to volume space)
            uniform float4x4 _parentInverseMat;
            // Plane transform
            uniform float4x4 _planeMat;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Modified UV (because slicing plane has coodinates between 0 and 100, while the volue mas 0-1)
                // TODO: Create a different slice mesh!
                float2 uvMod = float2((0.5f - v.uv.x) * 10.0f, (0.5f - v.uv.y) * 10.0f);
                // Calculate plane vertex world position.
                float3 vert = mul(_planeMat, float4(uvMod.x, 0.0f, uvMod.y, 1.0f));
                // Convert from world space to volume space.
                o.relVert = mul(_parentInverseMat, float4(vert, 1.0f));
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 dataCoord = i.relVert + float3(0.5f, 0.5f, 0.5f);
                // If the current fragment is outside the volume, simply colour it black.
                // Note: Unity does not seem to have support for clamping texture coordinates to a border value, so we have to do this manually
                if (dataCoord.x > 1.0f || dataCoord.y > 1.0f || dataCoord.z > 1.0f || dataCoord.x < 0.0f || dataCoord.y < 0.0f || dataCoord.z < 0.0f)
                {
                   return float4(0.0f, 0.0f, 0.0f, 1.0f);
                }
                else
                {
                   // Sample the volume texture.
                   float dataVal = tex3D(_DataTex, dataCoord);
                   float4 col = tex2D(_TFTex, float2(dataVal, 0.0f));
                   col.a = 1.0f;
                   return col;
                }
            }
            ENDCG
        }
    }
}
