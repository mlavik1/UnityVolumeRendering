using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Distance measure tool, for measuring distance between two points inside a dataset.
    /// Click on two points inside a dataset to measure the distance between them.
    /// The distance will be show in the upper right of the screen.
    /// </summary>
    public class DistanceMeasureTool : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        void Start()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.SetColor("_Color", Color.red);
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.startWidth = 0.003f;
            lineRenderer.endWidth = 0.003f;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }

        void Update()
        {
            if (Camera.main != null && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                VolumeRaycaster raycaster = new VolumeRaycaster();
                if (raycaster.RaycastScene(ray, out RaycastHit hit))
                {
                    //Debug.DrawLine(ray.origin, hit.point, Color.red, 10.0f, true);
                    lineRenderer.SetPosition(0, lineRenderer.GetPosition(1));
                    lineRenderer.SetPosition(1, hit.point);
                }
            }
        }

        private void OnGUI()
        {
            // Display distance
            float distance = Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1));
            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width - 150.0f);
            GUILayout.Label($"Distance: {distance}");
            GUILayout.EndHorizontal();
        }
    }
}
