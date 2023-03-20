namespace UnityVolumeRendering
{
    public interface IProgressView
    {
        void StartProgress(string title, string description);
        void FinishProgress(bool failed = false);
        void UpdateProgress(float progress, string description);
    }
}
