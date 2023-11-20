using BepInEx.Configuration;

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
