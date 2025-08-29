using UnityEngine;
using UnityEngine.Rendering;

namespace UnityVolumeRendering
{
    public class ShaderFactory
    {
        public static Shader GetVolumeRenderingShader()
        {
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                return Shader.Find("VolumeRendering/URP/VolumeRendering");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                return Shader.Find("VolumeRendering/HDRP/VolumeRendering");
            }
            else
            {
                return Shader.Find("VolumeRendering/Builtin/VolumeRendering");
            }
        }

        public static Shader GetCrossSectionPlaneShader()
        {
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                return Shader.Find("VolumeRendering/URP/CrossSectionPlane");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                return Shader.Find("VolumeRendering/HDRP/CrossSectionPlane");
            }
            else
            {
                return Shader.Find("VolumeRendering/Builtin/CrossSectionPlane");
            }
        }

        public static Shader GetSliceRenderingShader()
        {
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                return Shader.Find("VolumeRendering/URP/SliceRenderingShader");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                return Shader.Find("VolumeRendering/HDRP/SliceRenderingShader");
            }
            else
            {
                return Shader.Find("VolumeRendering/Builtin/SliceRenderingShader");
            }
        }
    }
}
