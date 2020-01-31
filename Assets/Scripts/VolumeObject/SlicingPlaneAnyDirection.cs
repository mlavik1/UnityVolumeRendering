using UnityEngine;

[ExecuteInEditMode]
public class SlicingPlaneAnyDirection : MonoBehaviour
{
    public Material mat;
    public Transform volumeTransform;

    void Update()
    {
        mat.SetVector("_PlanePos", volumeTransform.position - transform.position);
        mat.SetVector("_PlaneNormal", transform.forward);
    }
}
