using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTallyCore.Graphics.ProgressCounter
{
    public abstract class ProgressCounter
    {
        public Action<ProgressCounter, float> OnProgressCounterUpdate;
        public Action<ProgressCounter> OnProgressCounterFinish;
        private float _progressPercent;
        private Stopwatch _timer;
        public bool IsCompleted { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsForcedStopped { get; private set; }
        public double GetElapsedMillisecondsTime { get; private set; }

        public ProgressCounter()
        {
            GetElapsedMillisecondsTime = 0d;
            _progressPercent = 0;
            IsCompleted = IsRunning = IsForcedStopped = false;
        }

        public void Update(float progressPercent)
        {
            if (IsForcedStopped || IsCompleted) return;

            if (!IsRunning && !IsCompleted)
            {
                IsRunning = true;
                _timer = Stopwatch.StartNew();
            }
            _progressPercent = progressPercent;
            if (_progressPercent >= 1)
            {
                IsCompleted = true;
                IsRunning = false;
                _timer = null;
                GetElapsedMillisecondsTime = _timer.Elapsed.TotalMilliseconds;
                OnProgressCounterFinish?.Invoke(this);
                return;
            }

            OnProgressCounterUpdate?.Invoke(this, _progressPercent);
            GetElapsedMillisecondsTime = _timer.Elapsed.TotalMilliseconds;
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

        public void ClearEvents()
        {
            OnProgressCounterUpdate = null;
            OnProgressCounterFinish = null;
        }
    }
}
