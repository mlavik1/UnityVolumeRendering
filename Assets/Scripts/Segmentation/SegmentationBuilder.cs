using System.Collections.Generic;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class SegmentationBuilder
    {
        private static Dictionary<string, Color> segmentationColours = new Dictionary<string, Color>
        {
            { "liver", new Color(0.42f, 0.18f, 0.12f) },
            { "stomach", new Color(0.7f, 0.0f, 0.0f) },
            { "esophagus", new Color(0.85f, 0.44f, 0.58f) },
            { "colon", new Color(0.0f, 0.6f, 0.1f) },
            { "heart", new Color(0.9f, 0.05f, 0.1f) }
        };

        private static List<string> totalSegmentatorLabels = new List<string>
            { "spleen", "kidney_right", "kidney_left", "gallbladder", "liver", "stomach", "pancreas", "adrenal_gland_right", "adrenal_gland_left", "lung_upper_lobe_left",
            "lung_lower_lobe_left", "lung_upper_lobe_right", "lung_middle_lobe_right", "lung_lower_lobe_right", "esophagus", "trachea", "thyroid_gland", "small_bowel", "duodenum",
            "colon", "urinary_bladder", "prostate", "kidney_cyst_left", "kidney_cyst_right", "sacrum", "vertebrae_S1", "vertebrae_L5", "vertebrae_L4", "vertebrae_L3", "vertebrae_L2",
            "vertebrae_L1", "vertebrae_T12", "vertebrae_T11", "vertebrae_T10", "vertebrae_T9", "vertebrae_T8", "vertebrae_T7", "vertebrae_T6", "vertebrae_T5", "vertebrae_T4",
            "vertebrae_T3", "vertebrae_T2", "vertebrae_T1", "vertebrae_C7", "vertebrae_C6", "vertebrae_C5", "vertebrae_C4", "vertebrae_C3", "vertebrae_C2", "vertebrae_C1", "heart",
            "aorta", "pulmonary_vein", "brachiocephalic_trunk", "subclavian_artery_right", "subclavian_artery_left", "common_carotid_artery_right", "common_carotid_artery_left",
            "brachiocephalic_vein_left", "brachiocephalic_vein_right", "atrial_appendage_left", "superior_vena_cava", "inferior_vena_cava", "portal_vein_and_splenic_vein",
            "iliac_artery_left", "iliac_artery_right", "iliac_vena_left", "iliac_vena_right", "humerus_left", "humerus_right", "scapula_left", "scapula_right", "clavicula_left",
            "clavicula_right", "femur_left", "femur_right", "hip_left", "hip_right", "spinal_cord", "gluteus_maximus_left", "gluteus_maximus_right", "gluteus_medius_left",
            "gluteus_medius_right", "gluteus_minimus_left", "gluteus_minimus_right", "autochthon_left", "autochthon_right", "iliopsoas_left", "iliopsoas_right", "brain", "skull",
            "rib_left_1", "rib_left_2", "rib_left_3", "rib_left_4", "rib_left_5", "rib_left_6", "rib_left_7", "rib_left_8", "rib_left_9", "rib_left_10", "rib_left_11", "rib_left_12",
            "rib_right_1", "rib_right_2", "rib_right_3", "rib_right_4", "rib_right_5", "rib_right_6", "rib_right_7", "rib_right_8", "rib_right_9", "rib_right_10", "rib_right_11",
            "rib_right_12", "sternum", "costal_cartilages"
        };

        public static List<SegmentationLabel> BuildSegmentations(VolumeDataset dataset)
        {
            List<SegmentationLabel> result = new List<SegmentationLabel>();
            int minSegmentationId = int.MaxValue;
            int maxSegmentationId = int.MinValue;

            for (int i = 0; i < dataset.data.Length; i++)
            {
                int value = Mathf.RoundToInt(dataset.data[i]);
                if (value > 0)
                {
                    minSegmentationId = Mathf.Min(minSegmentationId, value);
                    maxSegmentationId = Mathf.Max(maxSegmentationId, value);
                }
            }

            bool multiLabel = maxSegmentationId - minSegmentationId > 1;

            for (int segmentationId = minSegmentationId; segmentationId <= maxSegmentationId; segmentationId++)
            {
                SegmentationLabel segmentationLabel = new SegmentationLabel();
                segmentationLabel.id = segmentationId;
                segmentationLabel.name = dataset.datasetName;
                segmentationLabel.colour = Random.ColorHSV();
                if (multiLabel && segmentationId < totalSegmentatorLabels.Count + 1)
                {
                    string labelName = totalSegmentatorLabels[segmentationId - 1];
                    if (segmentationColours.ContainsKey(labelName))
                    {
                        segmentationLabel.colour = segmentationColours[labelName];
                    }
                    segmentationLabel.name = labelName;
                }
                result.Add(segmentationLabel);
            }
            return result;
        }
    }
}
