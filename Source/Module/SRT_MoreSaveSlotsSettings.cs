using Microsoft.Xna.Framework.Input;
using TAS.EverestInterop;

namespace Celeste.Mod.SRT_MoreSaveSlots.Module;

[SettingName("SRT_MSS_MODTITLE")]
public class SRT_MoreSaveSlotsSettings : EverestModuleSettings {

    public static SRT_MoreSaveSlotsSettings Instance { get; private set; }

    public SRT_MoreSaveSlotsSettings() {
        Instance = this;
    }

    internal void OnLoadSettings() {
    }

    public bool Enabled = true;

    [SettingName("SRT_MoreSaveSlots_Slot1")]
    [DefaultButtonBinding2(0, Keys.LeftControl, Keys.D1)]
    public ButtonBinding Slot1 { get; set; } = new((Buttons)0, Keys.LeftControl, Keys.D1);

    [SettingName("SRT_MoreSaveSlots_Slot2")]
    [DefaultButtonBinding2(0, Keys.LeftControl, Keys.D2)]
    public ButtonBinding Slot2 { get; set; } = new((Buttons)0, Keys.LeftControl, Keys.D2);

    [SettingName("SRT_MoreSaveSlots_Slot3")]
    [DefaultButtonBinding2(0, Keys.LeftControl, Keys.D3)]
    public ButtonBinding Slot3 { get; set; } = new((Buttons)0, Keys.LeftControl, Keys.D3);

    [SettingName("SRT_MoreSaveSlots_Slot4")]
    [DefaultButtonBinding2(0, Keys.LeftControl, Keys.D4)]
    public ButtonBinding Slot4 { get; set; } = new((Buttons)0, Keys.LeftControl, Keys.D4);

    [SettingName("SRT_MoreSaveSlots_Slot5")]
    [DefaultButtonBinding2(0, Keys.LeftControl, Keys.D5)]
    public ButtonBinding Slot5 { get; set; } = new((Buttons)0, Keys.LeftControl, Keys.D5);

    [SettingName("SRT_MoreSaveSlots_Slot6")]
    [DefaultButtonBinding2(0, Keys.LeftControl, Keys.D6)]
    public ButtonBinding Slot6 { get; set; } = new((Buttons)0, Keys.LeftControl, Keys.D6);

    [SettingName("SRT_MoreSaveSlots_Slot7")]
    [DefaultButtonBinding2(0, Keys.LeftControl, Keys.D7)]
    public ButtonBinding Slot7 { get; set; } = new((Buttons)0, Keys.LeftControl, Keys.D7);

    [SettingName("SRT_MoreSaveSlots_Slot8")]
    [DefaultButtonBinding2(0, Keys.LeftControl, Keys.D8)]
    public ButtonBinding Slot8 { get; set; } = new((Buttons)0, Keys.LeftControl, Keys.D8);

    [SettingName("SRT_MoreSaveSlots_Slot9")]
    [DefaultButtonBinding2(0, Keys.LeftControl, Keys.D9)]
    public ButtonBinding Slot9 { get; set; } = new((Buttons)0, Keys.LeftControl, Keys.D9);
}