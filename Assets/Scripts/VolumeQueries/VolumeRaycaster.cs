using UnityEngine;
using System.Collections.Generic;

namespace UnityVolumeRendering
{
    public struct RaycastHit
    {
        public VolumeRenderedObject volumeObject;
        public Vector3 point;
        public float distance;
    }

    public class VolumeRaycaster
    {
        public bool RaycastScene(Ray ray, out RaycastHit hit)
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            List<RaycastHit> hits = new List<RaycastHit>();
            foreach (VolumeRenderedObject obj in objects)
            {
                RaycastHit currentHit;
                if (RaycastObject(ray, obj, out currentHit))
                {
                    hits.Add(currentHit);
                }
            }
            if (hits.Count > 0)
            {
                hits.Sort((a, b) => a.distance.CompareTo(b.distance));
                hit = hits[0];
                return true;
            }
            else
            {
                hit = new RaycastHit();
                return false;
            }
        }

        private bool RaycastObject(Ray worldSpaceRay, VolumeRenderedObject volumeObject, out RaycastHit hit)
        {
            hit = new RaycastHit();
            VolumeDataset dataset = volumeObject.dataset;
            Vector2 visibilityWindow = volumeObject.GetVisibilityWindow();
            float minValue = Mathf.Lerp(dataset.GetMinDataValue(), dataset.GetMaxDataValue(), visibilityWindow.x);
            float maxValue = Mathf.Lerp(dataset.GetMinDataValue(), dataset.GetMaxDataValue(), visibilityWindow.y);
            Ray localRay = worldSpaceRay;
            localRay.origin = volumeObject.transform.InverseTransformPoint(worldSpaceRay.origin);
            localRay.direction = volumeObject.transform.InverseTransformVector(worldSpaceRay.direction);
            Bounds localBounds = new Bounds(Vector3.zero, Vector3.one);
            float tStart = 0.0f;
            if (localBounds.IntersectRay(localRay, out tStart))
            {
                Vector3 start = localRay.origin + localRay.direction * tStart;
                Vector3 direction = localRay.direction.normalized;
                for (float t = 0.0f;; t+= 0.01f)
                {
                    Vector3 position = start + direction * t;
                    if (!localBounds.Contains(position))
                        break;
                    Vector3 uvw = position + Vector3.one * 0.5f;
                    Vector3Int index = new Vector3Int((int)(uvw.x * dataset.dimX), (int)(uvw.y * dataset.dimY), (int)(uvw.z * dataset.dimZ));
                    float value = dataset.GetData(index.x, index.y, index.z);
                    float normalisedValue = Mathf.InverseLerp(dataset.GetMinDataValue(), dataset.GetMaxDataValue(), value);
                    if (value >= minValue && value <= maxValue && volumeObject.transferFunction.GetColour(normalisedValue).a > 0.0f)
                    {
                        hit.point = volumeObject.transform.TransformPoint(position);
                        hit.distance = (worldSpaceRay.origin - volumeObject.transform.TransformPoint(position)).magnitude;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
