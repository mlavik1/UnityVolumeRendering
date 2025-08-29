using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace UnityVolumeRendering
{
    public class ShaderFactory
    {
        public static Shader GetVolumeRenderingShader()
        {
            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                shader = Shader.Find("VolumeRendering/URP/VolumeRendering");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                shader = Shader.Find("VolumeRendering/HDRP/VolumeRendering");
            }
            else
            {
                shader = Shader.Find("VolumeRendering/Builtin/VolumeRendering");
            }
            Debug.Assert(shader != null, "Could not get volume rendering shader");
            return shader;
        }

        public static Shader GetCrossSectionPlaneShader()
        {
            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                shader = Shader.Find("VolumeRendering/URP/CrossSectionPlane");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                shader = Shader.Find("VolumeRendering/HDRP/CrossSectionPlane");
            }
            else
            {
                shader = Shader.Find("VolumeRendering/Builtin/CrossSectionPlane");
            }
            Debug.Assert(shader != null, "Could not get volume rendering shader");
            return shader;
        }

        public static Shader GetSliceRenderingShader()
        {
            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                shader = Shader.Find("VolumeRendering/URP/SliceRenderingShader");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                shader = Shader.Find("VolumeRendering/HDRP/SliceRenderingShader");
            }
            else
            {
                shader = Shader.Find("VolumeRendering/Builtin/SliceRenderingShader");
            }
            Debug.Assert(shader != null, "Could not get volume rendering shader");
            return shader;
        }
    }
}
