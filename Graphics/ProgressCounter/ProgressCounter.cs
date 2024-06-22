using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTallyCore.Graphics.ProgressCounter
{
    public abstract class ProgressCounter
    {
        public Action<float> OnProgressCounterUpdate;
        public Action OnProgressCounterFinish;
        private float _progressPercent;
        public bool IsCompleted { get; private set; }

        public void Update(float progressPercent)
        {
            _progressPercent = progressPercent;
            if (IsCompleted)
            {
                OnProgressCounterFinish?.Invoke();
                return;
            }

            OnProgressCounterUpdate?.Invoke(_progressPercent);

        }
    }
}
