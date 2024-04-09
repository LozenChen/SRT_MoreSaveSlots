using Celeste.Mod.SRT_MoreSaveSlots.TinySRT;

namespace Celeste.Mod.SRT_MoreSaveSlots.Module;

public class SRT_MoreSaveSlotsModule : EverestModule {

    public static SRT_MoreSaveSlotsModule Instance;

    public static SRT_MoreSaveSlotsSettings Settings => SRT_MoreSaveSlotsSettings.Instance;
    public SRT_MoreSaveSlotsModule() {
        Instance = this;
        AttributeUtils.CollectMethods<LoadAttribute>();
        AttributeUtils.CollectMethods<UnloadAttribute>();
        AttributeUtils.CollectMethods<LoadContentAttribute>();
        AttributeUtils.CollectMethods<InitializeAttribute>();
    }

    public override Type SettingsType => typeof(SRT_MoreSaveSlotsSettings);
    public override void Load() {
        SaveSlotsManager.SwitchSlot(1);
        Loader.Load();
    }

    public override void Unload() {
        Loader.Unload();
    }

    public override void Initialize() {
        Loader.Initialize();
    }

    public override void LoadContent(bool firstLoad) {
        if (firstLoad) {
            Loader.LoadContent();
        }
    }

    public override void LoadSettings() {
        base.LoadSettings();
        GlobalVariables.modSettings.OnLoadSettings();
    }

    public override void OnInputInitialize() {
        base.OnInputInitialize();
    }
}