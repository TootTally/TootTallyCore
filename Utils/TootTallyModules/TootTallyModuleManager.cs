﻿using System;
using System.Collections.Generic;
using TootTallyCore.Utils.TootTallyNotifs;
using UnityEngine;

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
                if (m.ModuleConfigEnabled.Value)
                {
                    try
                    {
                        m.LoadModule();
                    }
                    catch (Exception e)
                    {
                        Plugin.LogError($"Module {m.Name} couldn't be loaded.");
                        Plugin.LogException(e);
                    }
                }
            });

        }

        private static void ModuleConfigEnabled_SettingChanged(ITootTallyModule module)
        {
            if (module.ModuleConfigEnabled.Value)
            {
                try
                {
                    module.LoadModule();
                    TootTallyNotifManager.DisplayNotif($"Module {module.Name} Enabled.", Color.white);
                }
                catch (Exception e)
                {
                    Plugin.LogError($"Module {module.Name} couldn't be loaded.");
                    Plugin.LogException(e);
                }
            }
            else
            {
                module.UnloadModule();
                TootTallyNotifManager.DisplayNotif($"Module {module.Name} Disabled.", Color.white);
            }
        }
    }
}
