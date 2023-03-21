namespace UnityVolumeRendering
{
    public interface IProgressHandler
    {
        void StartStage(float weight, string description = "");
        void EndStage();
        void ReportProgress(float progress, string description = "");
        void ReportProgress(int currentStep, int totalSteps, string description = "");
    }
}
