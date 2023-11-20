using System;
using System.Collections.Generic;
using TootTallyCore.Utils.TootTallyNotifs;
using UnityEngine;

namespace TootTallyCore.Utils.TootTallyModules
{
    public static class TootTallyModuleManager
    {
        public static List<ITootTallyModule> TootTallyModules { get; private set; }

        internal static void Initialize()
        {
            TootTallyModules = new List<ITootTallyModule>();
        }

        public static void AddModule(ITootTallyModule module)
        {
            if (TootTallyModules == null) Plugin.LogInfo("tootTallyModules IS NULL");
            TootTallyModules.Add(module);
            if (!module.IsConfigInitialized)
            {
                module.ModuleConfigEnabled.SettingChanged += delegate { ModuleConfigEnabled_SettingChanged(module); };
                //_tootTallyModulePage.AddToggle(module.Name.Split('.')[1], module.ModuleConfigEnabled); // Holy shit this sucks why did I do this LMFAO
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
