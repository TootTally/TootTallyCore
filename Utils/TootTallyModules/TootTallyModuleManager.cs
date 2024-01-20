using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using TootTallyCore.Utils.TootTallyNotifs;
using UnityEngine;
using UnityEngine.Analytics;

namespace TootTallyCore.Utils.TootTallyModules
{
    public static class TootTallyModuleManager
    {
        private static List<ITootTallyModule> _tootTallyModuleList;
        public static void AddModule(ITootTallyModule module)
        {
            _tootTallyModuleList ??= new List<ITootTallyModule>();
            _tootTallyModuleList.Add(module);
            if (!module.IsConfigInitialized)
            {
                module.ModuleConfigEnabled.SettingChanged += delegate { ModuleConfigEnabled_SettingChanged(module); };
                module.IsConfigInitialized = true;

            }
        }

        public static void LoadModules()
        {
            _tootTallyModuleList?.ForEach(m =>
            {
                try
                {
                    if (m.ModuleConfigEnabled.Value)
                        m.LoadModule();
                }
                catch (Exception e)
                {
                    Plugin.LogError($"Module {m.Name} couldn't be loaded.");
                    Plugin.LogException(e);
                }
            });

        }

        private static void ModuleConfigEnabled_SettingChanged(ITootTallyModule module)
        {
            try
            {
                if (module.ModuleConfigEnabled.Value)
                {
                    module.LoadModule();
                    TootTallyNotifManager.DisplayNotif($"Module {module.Name} Enabled.");
                }
                else
                {
                    module.UnloadModule();
                    TootTallyNotifManager.DisplayNotif($"Module {module.Name} Disabled.");
                }
            }
            catch (Exception e)
            {
                Plugin.LogError($"Module {module.Name} couldn't be un/loaded.");
                Plugin.LogException(e);
            }
        }
    }
}
