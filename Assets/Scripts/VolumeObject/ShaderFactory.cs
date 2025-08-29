using UnityEngine;
using UnityEngine.Rendering;

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
            Debug.Assert(shader != null, "Could not get shader");
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
            Debug.Assert(shader != null, "Could not get shader");
            return shader;
        }

        public static Shader GetCrossSectionSphereShader()
        {
            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                shader = Shader.Find("VolumeRendering/URP/CrossSectionSphere");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                shader = Shader.Find("VolumeRendering/HDRP/CrossSectionSphere");
            }
            else
            {
                shader = Shader.Find("VolumeRendering/Builtin/CrossSectionSphere");
            }
            Debug.Assert(shader != null, "Could not get shader");
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
            Debug.Assert(shader != null, "Could not get shader");
            return shader;
        }

        public static Shader GetTransferFunctionShader()
        {
            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                shader = Shader.Find("VolumeRendering/URP/TransferFunctionShader");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                shader = Shader.Find("VolumeRendering/HDRP/TransferFunctionShader");
            }
            else
            {
                shader = Shader.Find("VolumeRendering/Builtin/TransferFunctionShader");
            }
            Debug.Assert(shader != null, "Could not get shader");
            return shader;
        }

        public static Shader GetTransferFunction2DShader()
        {
            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                shader = Shader.Find("VolumeRendering/URP/TransferFunction2DShader");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                shader = Shader.Find("VolumeRendering/HDRP/TransferFunction2DShader");
            }
            else
            {
                shader = Shader.Find("VolumeRendering/Builtin/TransferFunction2DShader");
            }
            Debug.Assert(shader != null, "Could not get shader");
            return shader;
        }

        public static Shader GetTransferFunctionPaletteShader()
        {
            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("Universal"))
            {
                shader = Shader.Find("VolumeRendering/URP/TransferFunctionPaletteShader");
            }
            else if (GraphicsSettings.currentRenderPipeline && GraphicsSettings.currentRenderPipeline.name.Contains("HD"))
            {
                shader = Shader.Find("VolumeRendering/HDRP/TransferFunctionPaletteShader");
            }
            else
            {
                shader = Shader.Find("VolumeRendering/Builtin/TransferFunctionPaletteShader");
            }
            Debug.Assert(shader != null, "Could not get shader");
            return shader;
        }
    }
}
