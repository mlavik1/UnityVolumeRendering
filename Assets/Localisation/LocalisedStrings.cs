#define UVR_LOCALE_CHINESE

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class LocalisedStrings
    {
#if UVR_LOCALE_CHINESE
        public const string MENUITEM_VOLREND = "体绘制";
        public const string MENUITEM_LOADDATASET = MENUITEM_VOLREND + "/导入数据集";
        public const string MENUITEM_LOAD_DICOM = MENUITEM_LOADDATASET + "/导入DICOM数据";
        public const string MENUITEM_LOAD_RAW = MENUITEM_LOADDATASET + "/导入RAW数据";
        public const string MENUITEM_LOAD_NRRD = MENUITEM_LOADDATASET + "/导入NRRD数据";
        public const string MENUITEM_LOAD_NIFTI = MENUITEM_LOADDATASET + "/导入NIFTI数据";
        public const string MENUITEM_LOAD_PARCHG = MENUITEM_LOADDATASET + "/导入PARCHG数据";
        public const string MENUITEM_LOAD_IMGSEQ = MENUITEM_LOADDATASET + "/导入图像序列格式";
        public const string MENUITEM_LOAD_IMG = MENUITEM_LOADDATASET + "/导入图像数据";
#else
        public const string MENUITEM_VOLREND = "Volume Rendering";
        public const string MENUITEM_LOADDATASET = MENUITEM_VOLREND + "/Load dataset";
        public const string MENUITEM_LOAD_DICOM = MENUITEM_LOADDATASET + "/Load DICOM";
        public const string MENUITEM_LOAD_RAW = MENUITEM_LOADDATASET + "/Load raw dataset";
        public const string MENUITEM_LOAD_NRRD = MENUITEM_LOADDATASET + "/Load NRRD dataset";
        public const string MENUITEM_LOAD_NIFTI = MENUITEM_LOADDATASET + "/Load NIFTI dataset";
        public const string MENUITEM_LOAD_PARCHG = MENUITEM_LOADDATASET + "/Load PARCGH dataset";
        public const string MENUITEM_LOAD_IMGSEQ = MENUITEM_LOADDATASET + "/Load image sequence dataset";
        public const string MENUITEM_LOAD_IMG = MENUITEM_LOADDATASET + "/Load image dataset";
#endif
    }
}
