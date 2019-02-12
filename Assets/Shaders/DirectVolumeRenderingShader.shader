Shader "VolumeRendering/DirectVolumeRenderingShader"
{
	Properties
	{
		_DataTex ("Data Texture (Generated)", 3D) = "" {}
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _TFTex("Transfer Function Texture (Generated)", 2D) = "white" {}
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
    }
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100
        Cull Front
        ZWrite Off
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
				float4 vertex : POSITION;
                float4 normal : NORMAL;
			};

			struct v2f
			{
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
                float3 vertexLocal : TEXCOORD0;
                float3 normal : NORMAL;
                //float3 screenPos : TEXCOORD1;
			};

			sampler3D _DataTex;
            sampler2D _NoiseTex;
            sampler2D _TFTex;

            float _MinVal;
            float _MaxVal;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                //o.screenPos = ComputeScreenPos(o.vertex);
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                #define NUM_STEPS 1000//200
                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float4 col = float4(i.vertexLocal.x, i.vertexLocal.y, i.vertexLocal.z, 1.0f);

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                // Create a small random offset in order to remove artifacts
                //rayStartPos = rayStartPos + (rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(rayStartPos.x, rayStartPos.x * rayStartPos.y)).r;

                col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float maxDensity = 0.0f;
                //[unroll(NUM_STEPS)]
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize + stepSize * 0.5f;
                    const float3 currPos = rayStartPos + rayDir * t;
                    if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        break;
                    
                    //const float density = tex3D(_DataTex, currPos).r / 4095.0f;
                    const float density = tex3Dlod(_DataTex, float4(currPos.x, currPos.y, currPos.z, 0.0f)).r / 4095.0f;

                    float4 src = tex2Dlod(_TFTex, float4(density, 0.0f, 0.0f, 0.0f));// tex2D(_TFTex, float2(density, 0.0f));

                    if (density < _MinVal || density > _MaxVal)
                        src.a = 0.0f;

                    col.rgb = src.a * src.rgb + (1.0f - src.a)*col.rgb;
                    col.a = src.a + (1.0f - src.a)*col.a;
                    
                    if (col.a > 1.0f) break;
                }
                // Maximum intensity projection
                //col = float4(maxDensity, 0.0f, 0.0f, 1.0f);
                
                return col;
			}
			ENDCG
		}
	}
}
