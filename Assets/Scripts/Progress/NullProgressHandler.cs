namespace UnityVolumeRendering
{
    public class NullProgressHandler : IProgressHandler
    {
        public static readonly IProgressHandler instance = new NullProgressHandler();

        public void StartStage(float weight, string description = "")
        {
        }

        public void EndStage()
        {
        }

        public void ReportProgress(float progress, string description = "")
        {
        }

        public void ReportProgress(int currentStep, int totalSteps, string description = "")
        {
        }
    }
}
