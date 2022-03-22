Shader "VolumeRendering/DirectVolumeRenderingShader"
{
    Properties
    {
        _DataTex ("Data Texture (Generated)", 3D) = "" {}
        _GradientTex("Gradient Texture (Generated)", 3D) = "" {}
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
        ZTest LEqual
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma multi_compile MODE_DVR MODE_MIP MODE_SURF
            #pragma multi_compile __ TF2D_ON
            #pragma multi_compile __ CUTOUT_PLANE CUTOUT_BOX_INCL CUTOUT_BOX_EXCL
            #pragma multi_compile __ LIGHTING_ON
            #pragma multi_compile DEPTHWRITE_ON DEPTHWRITE_OFF
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define CUTOUT_ON CUTOUT_PLANE || CUTOUT_BOX_INCL || CUTOUT_BOX_EXCL

            struct vert_in
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexLocal : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct frag_out
            {
                float4 colour : SV_TARGET;
#if DEPTHWRITE_ON
                float depth : SV_DEPTH;
#endif
            };

            sampler3D _DataTex;
            sampler3D _GradientTex;
            sampler2D _NoiseTex;
            sampler2D _TFTex;

            float _MinVal;
            float _MaxVal;

#if CUTOUT_ON
            float4x4 _CrossSectionMatrix;
#endif

            float3 getViewRayDir(frag_in fragIn)
            {
                if(unity_OrthoParams.w == 0)
                {
                    // Perspective
                    return normalize(ObjSpaceViewDir(float4(fragIn.vertexLocal, 0.0f)));
                }
                else
                {
                    // Orthographic
                    float3 camfwd = mul((float3x3)unity_CameraToWorld, float3(0,0,-1));
                    float4 camfwdobjspace = mul(unity_WorldToObject, camfwd);
                    return normalize(camfwdobjspace);
                }
            }

            float2 intersectAABB(float3 rayOrigin, float3 rayDir, float3 boxMin, float3 boxMax)
            {
                float3 tMin = (boxMin - rayOrigin) / rayDir;
                float3 tMax = (boxMax - rayOrigin) / rayDir;
                float3 t1 = min(tMin, tMax);
                float3 t2 = max(tMin, tMax);
                float tNear = max(max(t1.x, t1.y), t1.z);
                float tFar = min(min(t2.x, t2.y), t2.z);
                return float2(tNear, tFar);
            };

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
                return tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0f));
            }

            // Gets the gradient at the specified position
            float3 getGradient(float3 pos)
            {
                return tex3Dlod(_GradientTex, float4(pos.x, pos.y, pos.z, 0.0f)).rgb;
            }

            // Performs lighting calculations, and returns a modified colour.
            float3 calculateLighting(float3 col, float3 normal, float3 lightDir, float3 eyeDir, float specularIntensity)
            {
                float ndotl = max(lerp(0.0f, 1.5f, dot(normal, lightDir)), 0.5f); // modified, to avoid volume becoming too dark
                float3 diffuse = ndotl * col;
                float3 v = eyeDir;
                float3 r = normalize(reflect(-lightDir, normal));
                float rdotv = max( dot( r, v ), 0.0 );
                float3 specular = pow(rdotv, 32.0f) * float3(1.0f, 1.0f, 1.0f) * specularIntensity;
                return diffuse + specular;
            }

            // Converts local position to depth value
            float localToDepth(float3 localPos)
            {
                float4 clipPos = UnityObjectToClipPos(float4(localPos, 1.0f));

#if defined(SHADER_API_GLCORE) || defined(SHADER_API_OPENGL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
                return (clipPos.z / clipPos.w) * 0.5 + 0.5;
#else
                return clipPos.z / clipPos.w;
#endif
            }

            bool IsCutout(float3 currPos)
            {
#if CUTOUT_ON
                // Move the reference in the middle of the mesh, like the pivot
                float3 pos = currPos - float3(0.5f, 0.5f, 0.5f);

                // Convert from model space to plane's vector space
                float3 planeSpacePos = mul(_CrossSectionMatrix, float4(pos, 1.0f));
                
    #if CUTOUT_PLANE
                return planeSpacePos.z > 0.0f;
    #elif CUTOUT_BOX_INCL
                return !(planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f);
    #elif CUTOUT_BOX_EXCL
                return planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f;
    #endif
#else
                return false;
#endif
            }

            frag_in vert_main (vert_in v)
            {
                frag_in o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            // Direct Volume Rendering
            frag_out frag_dvr (frag_in i)
            {
                #define NUM_STEPS 512

                float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));
                float3 rayDir = getViewRayDir(i);

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                // Create a small random offset in order to remove artifacts
                rayStartPos += (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;

                float2 aabbInters = intersectAABB(rayStartPos, rayDir, float3(0.0, 0.0, 0.0), float3(1.0f, 1.0f, 1.0));
                float3 rayEndPos = rayStartPos + rayDir * aabbInters.y;
                float stepSize = 1.0 / NUM_STEPS;

                float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float tDepth = 0.0;
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = lerp(rayStartPos, rayEndPos, t);

                    // Perform slice culling (cross section plane)
#ifdef CUTOUT_ON
                    if(IsCutout(currPos))
                    	continue;
#endif

                    // Get the dansity/sample value of the current position
                    const float density = getDensity(currPos);

                    // Calculate gradient (needed for lighting and 2D transfer functions)
#if defined(TF2D_ON) || defined(LIGHTING_ON)
                    float3 gradient = getGradient(currPos);
#endif

                    // Apply transfer function
#if TF2D_ON
                    float mag = length(gradient) / 1.75f;
                    float4 src = getTF2DColour(density, mag);
#else
                    float4 src = getTF1DColour(density);
#endif

                    // Apply lighting
#ifdef LIGHTING_ON
                    src.rgb = calculateLighting(src.rgb, normalize(gradient), lightDir, rayDir, 0.3f);
#endif

                    if (density < _MinVal || density > _MaxVal)
                        src.a = 0.0f;

                    col.rgb = src.a * src.rgb + (1.0f - src.a)*col.rgb;
                    col.a = src.a + (1.0f - src.a)*col.a;

                    if (src.a > 0.15f)
                        tDepth = t;

                    if (col.a > 1.0f)
                        break;
                }

                // Write fragment output
                frag_out output;
                output.colour = col;
#if DEPTHWRITE_ON
                if (tDepth > 0)
                    output.depth = localToDepth(lerp(rayStartPos, rayEndPos, tDepth) - float3(0.5f, 0.5f, 0.5f));
                else
                    output.depth = 0;
#endif
                return output;
            }

            // Maximum Intensity Projection mode
            frag_out frag_mip(frag_in i)
            {
                #define NUM_STEPS 512

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = getViewRayDir(i);
                float2 aabbInters = intersectAABB(rayStartPos, rayDir, float3(0.0, 0.0, 0.0), float3(1.0f, 1.0f, 1.0));
                float3 rayEndPos = rayStartPos + rayDir * aabbInters.y;
                float stepSize = 1.0 / NUM_STEPS;

                float maxDensity = 0.0f;
                float3 maxDensityPos = rayStartPos;
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = lerp(rayStartPos, rayEndPos, t);
                    
#ifdef CUTOUT_ON
                    if (IsCutout(currPos))
                        continue;
#endif

                    const float density = getDensity(currPos);
                    if (density > maxDensity && density > _MinVal && density < _MaxVal)
                    {
                        maxDensity = density;
                        maxDensityPos = currPos;
                    }
                }

                // Write fragment output
                frag_out output;
                output.colour = float4(1.0f, 1.0f, 1.0f, maxDensity); // maximum intensity
#if DEPTHWRITE_ON
                output.depth = localToDepth(maxDensityPos - float3(0.5f, 0.5f, 0.5f));
#endif
                return output;
            }

            // Surface rendering mode
            // Draws the first point (closest to camera) with a density within the user-defined thresholds.
            frag_out frag_surf(frag_in i)
            {
                #define NUM_STEPS 1024
                float3 vertPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 viewRayDir = getViewRayDir(i);

                // Find intersections with axis aligned boundinng box (the volume)
                float2 aabbInters = intersectAABB(vertPos, viewRayDir, float3(0.0, 0.0, 0.0), float3(1.0f, 1.0f, 1.0));
                // Front-to-back tracing
                float3 rayDir = -viewRayDir;
                float3 rayStartPos = vertPos + viewRayDir * aabbInters.y;
                float3 rayEndPos = vertPos;
                // Create a small random offset in order to remove artifacts
                rayStartPos = rayStartPos + (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;
                float stepSize = 1.0 / NUM_STEPS;

                float4 col = float4(0,0,0,0);
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = lerp(rayStartPos, rayEndPos, t);
                    
#ifdef CUTOUT_ON
                    if (IsCutout(currPos))
                        continue;
#endif

                    const float density = getDensity(currPos);
                    if (density > _MinVal && density < _MaxVal)
                    {
                        float3 normal = normalize(getGradient(currPos));
                        col = getTF1DColour(density);
                        col.rgb = calculateLighting(col.rgb, normal, -rayDir, -rayDir, 0.15);
                        col.a = 1.0f;
                        break;
                    }
                }

                // Write fragment output
                frag_out output;
                output.colour = col;
#if DEPTHWRITE_ON
                output.depth = localToDepth(lerp(rayStartPos, rayEndPos, (iStep * stepSize)) - float3(0.5f, 0.5f, 0.5f));
#endif
                return output;
            }

            frag_in vert(vert_in v)
            {
                return vert_main(v);
            }

            frag_out frag(frag_in i)
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
