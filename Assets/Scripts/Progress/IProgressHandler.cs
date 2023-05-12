namespace UnityVolumeRendering
{
    /// <summary>
    /// Interface for progress handlers.
    /// Used for tracking the progress of long-lasting async operations, such as model import.
    /// Normally you will want to use <see cref="ProgressHandler"/>.
    /// </summary>
    public interface IProgressHandler
    {
        void StartStage(float weight, string description = "");
        void EndStage();
        void ReportProgress(float progress, string description = "");
        void ReportProgress(int currentStep, int totalSteps, string description = "");
        void Fail();
    }
}
