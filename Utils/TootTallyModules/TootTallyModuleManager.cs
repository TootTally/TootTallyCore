using System;
using System.Collections.Generic;
using TootTallyCore.Utils.TootTallyNotifs;
using UnityEngine;

namespace TootTallyCore.Utils.TootTallyModules
{
    public static class TootTallyModuleManager
    {
        public static List<ITootTallyModule> TootTallyModules { get; private set; }

        public static void AddModule(ITootTallyModule module)
        {
            TootTallyModules ??= new List<ITootTallyModule>();
            TootTallyModules.Add(module);
            if (!module.IsConfigInitialized)
            {
                module.ModuleConfigEnabled.SettingChanged += delegate { ModuleConfigEnabled_SettingChanged(module); };
                module.IsConfigInitialized = true;

            }
            if (module.ModuleConfigEnabled.Value)
            {
                try
                {
                    module.LoadModule();
                }
                catch (Exception e)
                {
                    Plugin.LogError($"Module {module.Name} couldn't be loaded.");
                    Plugin.LogError(e.Message);
                    Plugin.LogError(e.StackTrace);
                }
            }
        }

        private static void ModuleConfigEnabled_SettingChanged(ITootTallyModule module)
        {
            if (module.ModuleConfigEnabled.Value)
            {
                module.LoadModule();
                TootTallyNotifManager.DisplayNotif($"Module {module.Name} Enabled.", Color.white);
            }
            else
            {
                module.UnloadModule();
                TootTallyNotifManager.DisplayNotif($"Module {module.Name} Disabled.", Color.white);
            }
        }
    }
}
