using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTallyCore.Utils.TootTallyModules
{
    public interface ITootTallyModule
    {
        string Name { get; set; }
        bool IsConfigInitialized { get; set; }
        ConfigEntry<bool> ModuleConfigEnabled { get; set; }
        void LoadModule();
        void UnloadModule();
    }
}
