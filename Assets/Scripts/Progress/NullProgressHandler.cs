namespace UnityVolumeRendering
{
    /// <summary>
    /// Default progress handler, used when a progress handler is needed but none was provider by the user.
    /// </summary>
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

        public void Fail()
        {
        }
    }
}
