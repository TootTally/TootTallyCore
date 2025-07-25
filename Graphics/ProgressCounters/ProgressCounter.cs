﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TootTallyCore.Graphics.ProgressCounters
{
    public class ProgressCounter
    {
        private float _progressPercent;
        private Stopwatch _timer;
        public bool IsCompleted { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }
        public double GetCurrentElapsedTime => _timer != null ? _timer.Elapsed.TotalMilliseconds : 0;
        public double GetFinishedElapsedTime { get; private set; }

        public List<IProgressCountable> _progressCounterList { get; private set; }
        public List<IProgressCountable> _progressCounterToAdd { get; private set; }
        public List<IProgressCountable> _progressCounterToRemove { get; private set; }
        public ProgressCounter(params IProgressCountable[] counters)
        {
            _progressCounterList = new List<IProgressCountable>();
            _progressCounterList.AddRange(counters);
            _progressCounterToAdd = new List<IProgressCountable>();
            _progressCounterToRemove = new List<IProgressCountable>();

            GetFinishedElapsedTime = 0d;
            _progressPercent = 0;
            IsCompleted = IsRunning = IsPaused = false;
        }

        public void Update(float progressPercent)
        {
            if (IsCompleted) return;

            if (!IsRunning && !IsCompleted)
                Start();
            _progressPercent = Mathf.Clamp(progressPercent, 0, 1);
            _progressCounterList.ForEach(c => c.OnProgressCounterUpdate(this, _progressPercent));
            UpdateCounterList();
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
            IsPaused = false;
            _timer.Reset();
            _progressPercent = 0;
        }

        public void Pause()
        {
            IsPaused = true;
            IsRunning = false;
            _timer.Stop();
        }

        public void Finish()
        {
            IsCompleted = true;
            IsRunning = false;
            _timer.Stop();
            GetFinishedElapsedTime = _timer.Elapsed.TotalMilliseconds;
            _progressCounterList.ForEach(c => c.OnProgressCounterFinish(this, GetFinishedElapsedTime));
            UpdateCounterList();
        }

        private void UpdateCounterList()
        {
            if (_progressCounterToRemove.Count > 0)
                foreach (var c in _progressCounterToRemove) _progressCounterList.Remove(c);
            if (_progressCounterToAdd.Count > 0)
                _progressCounterList.AddRange(_progressCounterToAdd);
            _progressCounterToRemove.Clear();
            _progressCounterToAdd.Clear();

        }


        public void ClearCounters()
        {
            _progressCounterList.Clear();
        }

        public void AddCounter(IProgressCountable counter) => _progressCounterList.Add(counter);
        public void RemoveCounter(IProgressCountable counter)
        {
            if (_progressCounterList.Contains(counter))
                _progressCounterToRemove.Remove(counter);
        }
    }
}
