using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class DistanceMeasureTest : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        void Start()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                VolumeRaycaster raycaster = new VolumeRaycaster();
                RaycastHit hit;
                if (raycaster.RaycastScene(ray, out hit))
                {
                    //Debug.DrawLine(ray.origin, hit.point, Color.red, 10.0f, true);
                    lineRenderer.SetPosition(0, lineRenderer.GetPosition(1));
                    lineRenderer.SetPosition(1, hit.point);
                    float distance = Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1));
                    Debug.Log($"Distance: {distance}");
                }
            }
        }
    }
}
