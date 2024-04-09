using Celeste.Mod.SRT_MoreSaveSlots.Utils;

namespace Celeste.Mod.SRT_MoreSaveSlots.Module;

internal static class Loader {
    public static void Load() {
        Reloading = GFX.Loaded;
        AttributeUtils.Invoke<LoadAttribute>();
    }

    public static void Unload() {
        AttributeUtils.Invoke<UnloadAttribute>();
        HookHelper.Unload();
    }

    public static void Initialize() {
        HookHelper.InitializeAtFirst();
        ModUtils.InitializeAtFirst();
        AttributeUtils.Invoke<InitializeAttribute>();
        SRT_MoreSaveSlotsModule.Instance.SaveSettings();
        if (Reloading) {
            OnReload();
            Reloading = false;
        }
    }

    public static void LoadContent() {
        AttributeUtils.Invoke<LoadContentAttribute>();
    }

    public static void OnReload() {
    }

    public static bool Reloading;
}