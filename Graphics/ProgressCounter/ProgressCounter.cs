﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TootTallyCore.Graphics.ProgressCounter
{
    public class ProgressCounter
    {
        public Action<ProgressCounter, float> OnProgressCounterUpdate;
        public Action<ProgressCounter> OnProgressCounterFinish;
        private float _progressPercent;
        private Stopwatch _timer;
        public bool IsCompleted { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsForcedStopped { get; private set; }
        public double GetCurrentElapsedTime => _timer != null ? _timer.Elapsed.TotalMilliseconds : 0;
        public double GetFinishedElapsedTime { get; private set; }

        public ProgressCounter()
        {
            GetFinishedElapsedTime = 0d;
            _progressPercent = 0;
            IsCompleted = IsRunning = IsForcedStopped = false;
        }

        public void Update(float progressPercent)
        {
            if (IsForcedStopped || IsCompleted) return;

            if (!IsRunning && !IsCompleted)
                Start();
            _progressPercent = Mathf.Clamp(progressPercent, 0, 1);

            OnProgressCounterUpdate?.Invoke(this, _progressPercent);
        }

        public void Start()
        {
            IsRunning = true;
            _timer = Stopwatch.StartNew();
        }

        public void Reset()
        {
            IsRunning = false;
            IsCompleted = false;
            IsForcedStopped = false;
            _progressPercent = 0;
        }

        public void ForceStop()
        {
            IsForcedStopped = true;
            IsRunning = false;
        }

        public void Finish()
        {
            IsCompleted = true;
            IsRunning = false;
            _timer.Stop();
            GetFinishedElapsedTime = _timer.Elapsed.TotalMilliseconds;
            OnProgressCounterFinish?.Invoke(this);
        }

        public void ClearEvents()
        {
            OnProgressCounterUpdate = null;
            OnProgressCounterFinish = null;
        }
    }
}
