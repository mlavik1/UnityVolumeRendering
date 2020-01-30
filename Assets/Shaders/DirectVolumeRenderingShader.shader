Shader "VolumeRendering/DirectVolumeRenderingShader"
{
    Properties
    {
        _DataTex ("Data Texture (Generated)", 3D) = "" {}
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _TFTex("Transfer Function Texture (Generated)", 2D) = "" {}
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
            #pragma multi_compile MODE_DVR MODE_MIP MODE_SURF
            #pragma multi_compile __ TF2D_ON
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexLocal : TEXCOORD1;
                float3 normal : NORMAL;
            };

            sampler3D _DataTex;
            sampler2D _NoiseTex;
            sampler2D _TFTex;

            float _MinVal;
            float _MaxVal;

			float3 _PlanePos;
			float3 _PlaneNormal;

            // Gets the colour from a 1D Transfer Function (x = density)
            float4 getTF1DColour(float density)
            {
                return tex2Dlod(_TFTex, float4(density, 0.0f, 0.0f, 0.0f));
            }

            // Gets the colour from a 2D Transfer Function (x = density, y = gradient magnitude)
            float4 getTF2DColour(float density, float gradientMagnitude)
            {
                return tex2Dlod(_TFTex, float4(density, gradientMagnitude, 0.0f, 0.0f));
            }

            // Gets the density at the specified position
            float getDensity(float3 pos)
            {
                return tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0f)).a;
            }

            // Gets the gradient at the specified position
            float3 getGradient(float3 pos)
            {
                return tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0f)).rgb;
            }

            v2f vert_main (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            // Direct Volume Rendering
            fixed4 frag_dvr (v2f i)
            {
                #define NUM_STEPS 512

                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                // Create a small random offset in order to remove artifacts
                rayStartPos = rayStartPos + (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;

                float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = rayStartPos + rayDir * t;
                    if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        break;

                    const float density = getDensity(currPos);

#if TF2D_ON
                    float3 gradient = getGradient(currPos);
                    float mag = length(gradient) / 1.75f;
                    float4 src = getTF2DColour(density, mag);
#else
                    float4 src = getTF1DColour(density);
#endif
                    if (density < _MinVal || density > _MaxVal)
                        src.a = 0.0f;

					// Move the reference in the middle of the mesh, like the pivot
					float3 pivotPos = currPos - float3(0.5f, 0.5f, 0.5f);

					// Convert to world position
					float3 pos = mul(unity_ObjectToWorld, -pivotPos);

					// If the dot product is < 0, the current position is "below" the plane, if it's > 0 it's "above"
					// Then cull if the current position is below
					float cull = dot(_PlaneNormal, pos - _PlanePos);
					if (cull < 0)
						src.a = 0;

                    col.rgb = src.a * src.rgb + (1.0f - src.a)*col.rgb;
                    col.a = src.a + (1.0f - src.a)*col.a;

                    if (col.a > 1.0f)
                        break;
                }

                return col;
            }

            // Maximum Intensity Projection mode
            fixed4 frag_mip(v2f i)
            {
                #define NUM_STEPS 512
                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                float maxDensity = 0.0f;
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = rayStartPos + rayDir * t;
                    // Stop when we are outside the box
                    if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        break;

                    const float density = getDensity(currPos);
                    if (density > _MinVal && density < _MaxVal)
                        maxDensity = max(density, maxDensity);
                }

                // Maximum intensity projection
                float4 col = float4(1.0f, 1.0f, 1.0f, maxDensity);

                return col;
            }

            // Surface rendering mode
            // Draws the first point (closest to camera) with a density within the user-defined thresholds.
            fixed4 frag_surf(v2f i)
            {
#define NUM_STEPS 1024
                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = normalize(ObjSpaceViewDir(float4(i.vertexLocal, 0.0f)));
                // Start from the end, tand trace towards the vertex
                rayStartPos += rayDir * stepSize * NUM_STEPS;
                rayDir = -rayDir;

                float3 lightDir = -rayDir;

                // Create a small random offset in order to remove artifacts
                rayStartPos = rayStartPos + (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;

                float4 col = float4(0,0,0,0);
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = rayStartPos + rayDir * t;
                    // Make sure we are inside the box
                    if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        continue;

                    const float density = getDensity(currPos);
                    if (density > _MinVal && density < _MaxVal)
                    {
                        float3 normal = normalize(getGradient(currPos));
                        float lightReflection = dot(normal, lightDir);
                        lightReflection =  max(lerp(0.0f, 1.5f, lightReflection), 0.5f);
                        col = lightReflection * getTF1DColour(density);
                        col.a = 1.0f;
                        break;
                    }
                }

                return col;
            }

            v2f vert(appdata v)
            {
                return vert_main(v);
            }

            fixed4 frag(v2f i) : SV_Target
            {
#if MODE_DVR
                return frag_dvr(i);
#elif MODE_MIP
                return frag_mip(i);
#elif MODE_SURF
                return frag_surf(i);
#endif
            }

            ENDCG
        }
    }
}
