using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    public static class DensityHelper
    {
        public static DensitySource IdentifyDensitySource(Color[] voxels)
        {
            DensitySource source = DensitySource.Unknown;

            for (int i = 0; i < voxels.Length - 1; i++)
            {
                if (!Mathf.Approximately(voxels[i].a, voxels[i + 1].a))
                {
                    source = DensitySource.Alpha;
                    break;
                }
                else if (!Mathf.Approximately(voxels[i].r, voxels[i + 1].r))
                {
                    source = DensitySource.Grey;
                    break;
                }
                else if (!Mathf.Approximately(voxels[i].g, voxels[i + 1].g))
                {
                    source = DensitySource.Grey;
                    break;
                }
                else if (!Mathf.Approximately(voxels[i].b, voxels[i + 1].b))
                {
                    source = DensitySource.Grey;
                    break;
                }
            }

            return source;
        }

        public static int[] ConvertColorsToDensities (Color[] colors)
        {
            DensitySource source = IdentifyDensitySource(colors);
            return ConvertColorsToDensities(colors, source);
        }

        public static int[] ConvertColorsToDensities (Color[] colors, DensitySource source)
        {
            int[] densities = new int[colors.Length];
            for (int i = 0; i < densities.Length; i++)
                densities[i] = ConvertColorToDensity(colors[i], source);
            return densities;
        }

        public static int ConvertColorToDensity (Color color, DensitySource source)
        {
            switch (source)
            {
                case DensitySource.Alpha:
                    return Mathf.RoundToInt(color.a * 255f);
                case DensitySource.Grey:
                    return Mathf.RoundToInt(color.r * 255f);
                default:
                    throw new ArgumentOutOfRangeException(source.ToString());
            }
        }

        public static Color ConvertDensityToColor (int density, DensitySource source)
        {
            float grey = source == DensitySource.Grey ? density / 255f : 0f;
            float alpha = source == DensitySource.Alpha ? density / 255f : 0f;

            return new Color()
            {
                r = grey,
                g = grey,
                b = grey,
                a = alpha
            };
        }
    }
}