using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Progress handler, for tracking the progress of long (async) actions, such as import.
    /// How to use:
    /// - Create instace with the "using" statement, to ensure that failure callback is called on unhandled exceptions. 
    /// - Call Start() when starting
    /// - Call ReportProgress() to update progress
    /// - (optionally) call StartStage() and EndStage() to create a sub-stage to track the progress of.
    /// - Call Finish() or Fail() when done.
    /// </summary>
    public class ProgressHandler : IProgressHandler, IDisposable
    {
        private class ProgressStage
        {
            public float start;
            public float end;
        }

        private string description = "";
        private float currentStageProgress = 0.0f;
        private float totalProgress = 0.0f;
        private Stack<ProgressStage> stageStack = new Stack<ProgressStage>(3);
        private IProgressView progressView;

        public ProgressHandler(IProgressView progressView)
        {
            this.progressView = progressView;
            stageStack.Push(new ProgressStage{ start = 0.0f, end = 1.0f });
        }

        /// <summary>
        /// Start the processing.
        /// </summary>
        public void Start(string title, string description)
        {
            this.progressView.StartProgress(title, description);
            this.description = description;
            currentStageProgress = 0.0f;
            totalProgress = 0.0f;
            stageStack.Clear();
            stageStack.Push(new ProgressStage{ start = 0.0f, end = 1.0f });
        }

        /// <summary>
        /// Finish the processing.
        /// <param name="status">Completion status (succeeded or failed)</param>
        /// </summary>
        public void Finish(ProgressStatus status = ProgressStatus.Succeeded)
        {
            this.progressView.FinishProgress(status);
            stageStack.Clear();
        }

        /// <summary>
        /// Bramch a new sub-stage to track progress for.
        /// Example:
        ///   progress.StartStage(0.6f, "Do A"); // Will take up 60% of the progress.
        ///   // Do work for A, and report progress with progress.ReportProgress(...)
        ///   progress.EndStage();
        ///   progress.StartStage(0.4f, "Do B"); // Will take up 40% of the progress.
        ///   // Do work for B, and report progress with progress.ReportProgress(...)
        ///   progress.EndStage();
        /// <param name="status">Completion status (succeeded or failed)</param>
        /// </summary>
        public void StartStage(float weight, string description = "")
        {
            if (description != "")
                this.description = description;
            
            ProgressStage stage = stageStack.Peek();
            stageStack.Push(new ProgressStage{ start = totalProgress, end = totalProgress + (stage.end - stage.start) * weight });
            UpdateProgressView();
        }

        /// <summary>
        /// End a previously started stage.
        /// </summary>
        public void EndStage()
        {
            ReportProgress(1.0f);
            ProgressStage childStage = stageStack.Pop();
            totalProgress = childStage.end;
        }

        /// <summary>
        /// Report current progress.
        /// <param name="progress">Current progress. Value between 0.0 and 1.0 (0-100%)</param>
        /// <param name="description">Description of the work being done</param>
        /// </summary>
        public void ReportProgress(float progress, string description = "")
        {
            if (description != "")
                this.description = description;
            currentStageProgress = progress;
            totalProgress = GetAbsoluteProgress(progress);

            UpdateProgressView();
        }

        /// <summary>
        /// Report current progress, by step.
        /// <param name="currentStep">Index of current step (must be less than or equal to totalSteps)</param>
        /// <param name="totalSteps">Total number of steps</param>
        /// <param name="description">Description of the work being done</param>
        /// </summary>
        public void ReportProgress(int currentStep, int totalSteps, string description = "")
        {
            if (description != "")
                this.description = description;
            currentStageProgress = currentStep / (float)totalSteps;
            totalProgress = GetAbsoluteProgress(currentStageProgress);

            UpdateProgressView();
        }

        public void Dispose()
        {
            Finish(ProgressStatus.Failed);
        }

        private float GetAbsoluteProgress(float progress)
        {
            ProgressStage stage = stageStack.Peek();
            return Mathf.Lerp(stage.start, stage.end, progress);
        }

        private void UpdateProgressView()
        {
            this.progressView.UpdateProgress(totalProgress, currentStageProgress, description);
        }
    }
}
