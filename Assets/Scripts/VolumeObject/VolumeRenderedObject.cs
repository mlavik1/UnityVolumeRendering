using UnityEngine;

public class VolumeRenderedObject : MonoBehaviour
{
    [HideInInspector]
    public TransferFunction transferFunction;

    [HideInInspector]
    public TransferFunction2D transferFunction2D;

    [HideInInspector]
    public VolumeDataset dataset;

    private RenderMode remderMode;

    public SlicingPlane CreateSlicingPlane()
    {
        GameObject sliceRenderingPlane = GameObject.Instantiate(Resources.Load<GameObject>("SlicingPlane"));
        sliceRenderingPlane.transform.parent = transform;
        sliceRenderingPlane.transform.localPosition = Vector3.zero;
        sliceRenderingPlane.transform.localRotation = Quaternion.identity;
        MeshRenderer sliceMeshRend = sliceRenderingPlane.GetComponent<MeshRenderer>();
        sliceMeshRend.material = new Material(sliceMeshRend.sharedMaterial);
        Material sliceMat = sliceRenderingPlane.GetComponent<MeshRenderer>().sharedMaterial;
        sliceMat.SetTexture("_DataTex", dataset.GetTexture());
        sliceMat.SetTexture("_TFTex", transferFunction.GetTexture());
        sliceMat.SetMatrix("_parentInverseMat", transform.worldToLocalMatrix);
        sliceMat.SetMatrix("_planeMat", Matrix4x4.TRS(sliceRenderingPlane.transform.position, sliceRenderingPlane.transform.rotation, Vector3.one)); // TODO: allow changing scale

        return sliceRenderingPlane.GetComponent<SlicingPlane>();
    }

    public void SetRenderMode(RenderMode mode)
    {
        remderMode = mode;

        switch (mode)
        {
            case RenderMode.DirectVolumeRendering:
                {
                    GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_DVR");
                    GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
                    GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
                    break;
                }
            case RenderMode.MaximumIntensityProjectipon:
                {
                    GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_DVR");
                    GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_MIP");
                    GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
                    break;
                }
            case RenderMode.IsosurfaceRendering:
                {
                    GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_DVR");
                    GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
                    GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_SURF");
                    break;
                }
        }
    }

    public RenderMode GetRemderMode()
    {
        return remderMode;
    }
}
