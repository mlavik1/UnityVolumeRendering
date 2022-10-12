using UnityEngine;
using System.Collections.Generic;

namespace UnityVolumeRendering
{
    public enum CrossSectionType
    {
        Plane = 1,
        BoxInclusive = 2,
        BoxExclusive = 3
    }

    /// <summary>
    /// Manager for all cross section objects (planes and boxes).
    /// </summary>
    [ExecuteInEditMode]
    public class CrossSectionManager : MonoBehaviour
    {
        /// <summary>
        /// Volume dataset to cross section.
        /// </summary>
        private VolumeRenderedObject targetObject;
        private List<CrossSectionObject> crossSectionObjects = new List<CrossSectionObject>();
        private List<Matrix4x4> crossSectionMatrices = new List<Matrix4x4>();
        private List<float> crossSectionTypes = new List<float>();

        public void AddCrossSectionObject(CrossSectionObject crossSectionObject)
        {
            crossSectionObjects.Add(crossSectionObject);
            ClearDataArrays();
        }

        public void RemoveCrossSectionObject(CrossSectionObject crossSectionObject)
        {
            crossSectionObjects.Remove(crossSectionObject);
            ClearDataArrays();
        }

        private void ClearDataArrays()
        {
            crossSectionMatrices.Clear();
            crossSectionTypes.Clear();
            foreach (CrossSectionObject crossSectionObject in crossSectionObjects)
            {
                crossSectionMatrices.Add(crossSectionObject.GetMatrix());
                crossSectionTypes.Add((int)crossSectionObject.GetCrossSectionType());
            }
        }

        private void Awake()
        {
            targetObject = GetComponent<VolumeRenderedObject>();
        }

        private void Update()
        {
            if (targetObject == null)
                return;

            Material mat = targetObject.meshRenderer.sharedMaterial;

            if (crossSectionObjects.Count > 0)
            {
                int numCrossSections = System.Math.Min(crossSectionObjects.Count, 8);

                for (int i = 0; i < numCrossSections; i++)
                {
                    CrossSectionObject crossSectionObject = crossSectionObjects[i];
                    crossSectionMatrices[i] = crossSectionObject.GetMatrix();
                    crossSectionTypes[i] = (int)crossSectionObject.GetCrossSectionType();
                }

                mat.EnableKeyword("CROSS_SECTION_ON");
                mat.SetMatrixArray("_CrossSectionMatrices", crossSectionMatrices);
                mat.SetFloatArray("_CrossSectionTypes", crossSectionTypes);
                mat.SetInt("_NumCrossSections", numCrossSections);
            }
            else
            {
                mat.DisableKeyword("CROSS_SECTION_ON");
            }
        }
    }
}
