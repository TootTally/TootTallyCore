using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTallyCore.Graphics.ProgressCounter
{
    public interface IProgressCountable
    {
        void OnProgressCounterFinish();
        void OnProgressCounterUpdate();
    }
}
