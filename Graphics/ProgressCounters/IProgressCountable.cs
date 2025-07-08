using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTallyCore.Graphics.ProgressCounters
{
    public interface IProgressCountable
    {
        void OnProgressCounterFinish(ProgressCounter sender, double elapsed);
        void OnProgressCounterUpdate(ProgressCounter sender, float progress);
    }
}
