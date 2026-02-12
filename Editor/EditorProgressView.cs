#if UNITY_2020_1_OR_NEWER
using UnityEditor;

namespace UnityVolumeRendering
{
    public class EditorProgressView : IProgressView
    {
        private int progressId = -1;
        private float cachedProgress;
        private string cachedDescription;

        public void StartProgress(string title, string description)
        {
            progressId = Progress.Start(title, description, Progress.Options.Sticky);
            Progress.ShowDetails();
            EditorApplication.update += EditorUpdate;
        }

        public void FinishProgress(ProgressStatus status = ProgressStatus.Succeeded)
        {
            if (progressId != -1)
            {
                Progress.Finish(progressId, status == ProgressStatus.Failed ? Progress.Status.Failed : Progress.Status.Succeeded);
                progressId = -1;
            }
            EditorApplication.update -= EditorUpdate;
        }

        public void UpdateProgress(float totalProgress, float currentStageProgress, string description)
        {
            this.cachedProgress = totalProgress;
            this.cachedDescription = description;
            ShowProgress(totalProgress, description);
        }

        private void EditorUpdate()
        {
            ShowProgress(this.cachedProgress, this.cachedDescription);
        }

        private void ShowProgress(float progress, string description)
        {
            if (progressId != -1)
            {
                Progress.Report(progressId, progress, progress == 1.0f ? "Done" : description);
            }
        }
    }
}
#else
namespace UnityVolumeRendering
{
    public class EditorProgressView : IProgressView
    {
        public void StartProgress(string title, string description)
        {
        }

        public void FinishProgress(ProgressStatus status = ProgressStatus.Succeeded)
        {
        }

        public void UpdateProgress(float totalProgress, float currentStageProgress, string description)
        {
        }
    }
}
#endif
