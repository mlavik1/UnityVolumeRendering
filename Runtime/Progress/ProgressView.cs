namespace UnityVolumeRendering
{
    /// <summary>
    /// Interface for the view of a progress.
    /// Can be attached to a <see cref="ProgressHandler"/>..
    /// Implement this interface if you wish you update your own progress bar GUI, or execute some code based on current progress.
    /// </summary>
    public interface IProgressView
    {
        /// <summary>
        /// This function is called when the work starts.
        /// </summary>
        void StartProgress(string title, string description);

        /// <summary>
        /// This function is called the work has been finished (when total progress is 1.0).
        /// <param name="status">Status, indicating whether the work has failed or succeeded.</param>
        /// </summary>
        void FinishProgress(ProgressStatus status = ProgressStatus.Succeeded);

        /// <summary>
        /// This function is called whenever the progress is updated.
        /// </summary>
        /// <param name="totalProgress">Total progress (between 0.0 and 1.0).</param>
        /// <param name="currentStageProgress">Progress of current stage (between 0.0 and 1.0).</param>
        /// <param name="description">Description of work currently being done.</param>
        void UpdateProgress(float totalProgress, float currentStageProgress, string description);
    }
}
