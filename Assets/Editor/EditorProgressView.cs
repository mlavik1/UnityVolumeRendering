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

        public void FinishProgress(bool failed = false)
        {
            if (progressId != -1)
            {
                Progress.Finish(progressId, failed ? Progress.Status.Failed : Progress.Status.Succeeded);
                progressId = -1;
            }
            EditorApplication.update -= EditorUpdate;
        }

        public void UpdateProgress(float progress, string description)
        {
            this.cachedProgress = progress;
            this.cachedDescription = description;
            ShowProgress(progress, description);
        }

        private void EditorUpdate()
        {
            ShowProgress(this.cachedProgress, this.cachedDescription);
        }

        private void ShowProgress(float progress, string description)
        {
            if (progressId != -1)
            {
                Progress.Report(progressId, progress, description);
            }
        }
    }
}
