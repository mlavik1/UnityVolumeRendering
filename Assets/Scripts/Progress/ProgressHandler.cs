using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class ProgressHandler : IProgressHandler, IDisposable
    {
        private class ProgressStage
        {
            public float start;
            public float end;
        }

        private string description = "";
        private float currentProgress = 0.0f;
        private Stack<ProgressStage> stageStack = new Stack<ProgressStage>(3);
        private IProgressView progressView;

        public ProgressHandler(IProgressView progressView)
        {
            this.progressView = progressView;
        }

        public void Start(string title, string description)
        {
            this.progressView.StartProgress(title, description);
            this.description = description;
            currentProgress = 0.0f;
            stageStack.Clear();
            stageStack.Push(new ProgressStage{ start = 0.0f, end = 1.0f });
        }

        public void Finish()
        {
            this.progressView.FinishProgress();
            stageStack.Clear();
        }

        public void Fail()
        {
            this.progressView.FinishProgress(true);
            stageStack.Clear();
        }

        public void StartStage(float weight, string description = "")
        {
            if (description != "")
                this.description = description;

            ProgressStage stage = stageStack.Peek();
            stageStack.Push(new ProgressStage{ start = currentProgress, end = currentProgress + (stage.end - stage.start) * weight });
            UpdateProgressView();
        }

        public void EndStage()
        {
            ProgressStage childStage = stageStack.Pop();
            currentProgress = childStage.end;
        }

        public void ReportProgress(float progress, string description = "")
        {
            if (description != "")
                this.description = description;
            currentProgress = GetAbsoluteProgress(progress);

            UpdateProgressView();
        }

        public void ReportProgress(int currentStep, int totalSteps, string description = "")
        {
            if (description != "")
                this.description = description;
            currentProgress = GetAbsoluteProgress(currentStep / (float)totalSteps);

            UpdateProgressView();
        }

        public void Dispose()
        {
            Fail();
        }

        private float GetAbsoluteProgress(float progress)
        {
            ProgressStage stage = stageStack.Peek();
            return Mathf.Lerp(stage.start, stage.end, progress);
        }

        private void UpdateProgressView()
        {
            this.progressView.UpdateProgress(currentProgress, description);
        }
    }
}
