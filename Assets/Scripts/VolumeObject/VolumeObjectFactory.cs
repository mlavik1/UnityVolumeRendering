using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class VolumeObjectFactory
    {
        public static VolumeRenderedObject CreateObject(VolumeDataset dataset)
        {
            GameObject outerObject = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            VolumeRenderedObject volObj = outerObject.AddComponent<VolumeRenderedObject>();

            GameObject meshContainer = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainer"));
            volObj.volumeContainerObject = meshContainer;
            MeshRenderer meshRenderer = meshContainer.GetComponent<MeshRenderer>();

            CreateObjectInternal(dataset, meshContainer, meshRenderer, volObj, outerObject);

            meshRenderer.sharedMaterial.SetTexture("_DataTex", dataset.GetDataTexture());

            return volObj;
        }
        public static async Task<VolumeRenderedObject> CreateObjectAsync(VolumeDataset dataset, IProgressHandler progressHandler = null)
        {
            GameObject outerObject = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            VolumeRenderedObject volObj = outerObject.AddComponent<VolumeRenderedObject>();

            GameObject meshContainer = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainer"));
            volObj.volumeContainerObject = meshContainer;
            MeshRenderer meshRenderer = meshContainer.GetComponent<MeshRenderer>();

            CreateObjectInternal(dataset,meshContainer, meshRenderer,volObj,outerObject) ;

            meshRenderer.sharedMaterial.SetTexture("_DataTex", await dataset.GetDataTextureAsync(progressHandler));

            return volObj;
        }

        private static void CreateObjectInternal(VolumeDataset dataset, GameObject meshContainer, MeshRenderer meshRenderer, VolumeRenderedObject volObj, GameObject outerObject, IProgressHandler progressHandler = null)
        {            
            meshContainer.transform.parent = outerObject.transform;
            meshContainer.transform.localScale = Vector3.one;
            meshContainer.transform.localPosition = Vector3.zero;
            meshContainer.transform.parent = outerObject.transform;
            outerObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
            volObj.meshRenderer = meshRenderer;
            volObj.dataset = dataset;

            TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
            TransferFunction2D tf2D = TransferFunctionDatabase.CreateTransferFunction2D();

            volObj.Initialise(dataset, tf);

            volObj.transferFunction2D = tf2D;

            meshContainer.transform.localScale = dataset.scale;
            meshContainer.transform.localRotation = dataset.rotation;

            // Normalise size (TODO: Add setting for diabling this?)
            float maxScale = Mathf.Max(dataset.scale.x, dataset.scale.y, dataset.scale.z);
            volObj.transform.localScale = Vector3.one / maxScale;
        }

        public static void SpawnCrossSectionPlane(VolumeRenderedObject volobj)
        {
            GameObject quad = GameObject.Instantiate((GameObject)Resources.Load("CrossSectionPlane"));
            quad.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CrossSectionPlane csplane = quad.gameObject.GetComponent<CrossSectionPlane>();
            csplane.SetTargetObject(volobj);
            quad.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { quad };
#endif
        }

        public static void SpawnCutoutBox(VolumeRenderedObject volobj)
        {
            GameObject obj = GameObject.Instantiate((GameObject)Resources.Load("CutoutBox"));
            obj.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CutoutBox cbox = obj.gameObject.GetComponent<CutoutBox>();
            cbox.SetTargetObject(volobj);
            obj.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { obj };
#endif
        }
        public static void SpawnCutoutSphere(VolumeRenderedObject volobj)
        {
            GameObject obj = GameObject.Instantiate((GameObject)Resources.Load("CutoutSphere"));
            obj.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CutoutSphere cSphere = obj.gameObject.GetComponent<CutoutSphere>();
            cSphere.SetTargetObject(volobj);
            obj.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { obj };
#endif
        }
    }
}
