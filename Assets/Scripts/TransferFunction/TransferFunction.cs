using UnityEngine;
using System.Collections.Generic;
using System;

namespace UnityVolumeRendering
{
    [Serializable]
    public class TransferFunction : ScriptableObject
    {
        [SerializeField]
        public List<TFColourControlPoint> colourControlPoints = new List<TFColourControlPoint>();
        [SerializeField]
        public List<TFAlphaControlPoint> alphaControlPoints = new List<TFAlphaControlPoint>();

        public bool relativeScale = false;

        public void AddControlPoint(TFColourControlPoint ctrlPoint)
        {
            colourControlPoints.Add(ctrlPoint);
        }

        public void AddControlPoint(TFAlphaControlPoint ctrlPoint)
        {
            alphaControlPoints.Add(ctrlPoint);
        }
    }
}
