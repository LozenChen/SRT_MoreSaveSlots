
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System.Reflection;
using Celeste.Mod.SRT_MoreSaveSlots.Utils;
using System.Reflection;
using TAS.EverestInterop;
using CMCore = Celeste.Mod.Core;
using TAS_Hotkey = TAS.EverestInterop.Hotkeys.Hotkey; // use this just coz i'm lazy
using Celeste.Mod.SpeedrunTool.Other;
using Celeste.Mod.SRT_MoreSaveSlots.TinySRT;
using Celeste.Mod.SpeedrunTool.Message;
using Celeste.Mod.SpeedrunTool;

namespace Celeste.Mod.SRT_MoreSaveSlots.Module;

public static class TH_Hotkeys {

    public static TAS_Hotkey Slot1;

    public static TAS_Hotkey Slot2;

    public static TAS_Hotkey Slot3;

    public static TAS_Hotkey Slot4;

    public static TAS_Hotkey Slot5;

    public static TAS_Hotkey Slot6;

    public static TAS_Hotkey Slot7;

    public static TAS_Hotkey Slot8;

    public static TAS_Hotkey Slot9;

    public static List<TAS_Hotkey> Hotkeys = new ();

    [Load]
    public static void Load() {
        On.Celeste.Level.Render += HotkeysPressed;
        IL.Celeste.Mod.ModuleSettingsKeyboardConfigUI.Reset += ModReload;
    }

    [Unload]
    public static void Unload() {
        On.Celeste.Level.Render -= HotkeysPressed;
        IL.Celeste.Mod.ModuleSettingsKeyboardConfigUI.Reset -= ModReload;
    }


    [Initialize]
    public static void HotkeyInitialize() {
        Slot1 = BindingToHotkey(modSettings.Slot1);
        Slot2 = BindingToHotkey(modSettings.Slot2);
        Slot3 = BindingToHotkey(modSettings.Slot3);
        Slot4 = BindingToHotkey(modSettings.Slot4);
        Slot5 = BindingToHotkey(modSettings.Slot5);
        Slot6 = BindingToHotkey(modSettings.Slot6);
        Slot7 = BindingToHotkey(modSettings.Slot7);
        Slot8 = BindingToHotkey(modSettings.Slot8);
        Slot9 = BindingToHotkey(modSettings.Slot9);

        Hotkeys = new List<TAS_Hotkey> { Slot1, Slot2, Slot3, Slot4, Slot5, Slot6, Slot7, Slot8, Slot9 };


        // we are after SRT, so this overrides srt's original action
        // and we should not use original SRT savestate, there will be some issue with VirtualAssets clone i guess
        HotkeyReRegister();
    }

    private static void HotkeyReRegister() {
        Hotkey.SaveState.RegisterPressedAction(scene => {
            if (scene is Level) {
                SaveSlotsManager.SaveState();
            }
        });
        Hotkey.LoadState.RegisterPressedAction(scene => {
            if (scene is Level { Paused: false } && SaveSlotsManager.IsAllFree()) {
                if (SaveSlotsManager.IsSaved()) {
                    SaveSlotsManager.LoadState();
                }
                else {
                    PopupMessageUtils.Show(DialogIds.NotSavedStateTooltip.DialogClean(), DialogIds.NotSavedStateYetDialog);
                }
            }
        });
        Hotkey.ClearState.RegisterPressedAction(scene => {
            if (scene is Level { Paused: false } && SaveSlotsManager.IsAllFree()) {
                SaveSlotsManager.ClearStateAndShowMessage();
            }
        });
    }

    private static void HotkeysPressed(On.Celeste.Level.orig_Render orig, Level self) {
        orig(self);

        if (Engine.Scene is not Level) {
            return;
        }
        if (!SaveSlotsManager.IsAllFree()) {
            return;
        }

        TH_Hotkeys.Update(true, true);
        if (TH_Hotkeys.Slot1.Pressed) {
            SwitchTo(1);
        }
        else if (TH_Hotkeys.Slot2.Pressed) {
            SwitchTo(2);
        }
        else if (TH_Hotkeys.Slot3.Pressed) {
            SwitchTo(3);
        }
        else if (TH_Hotkeys.Slot4.Pressed) {
            SwitchTo(4);
        }
        else if (TH_Hotkeys.Slot5.Pressed) {
            SwitchTo(5);
        }
        else if (TH_Hotkeys.Slot6.Pressed) {
            SwitchTo(6);
        }
        else if (TH_Hotkeys.Slot7.Pressed) {
            SwitchTo(7);
        }
        else if (TH_Hotkeys.Slot8.Pressed) {
            SwitchTo(8);
        }
        else if (TH_Hotkeys.Slot9.Pressed) {
            SwitchTo(9);
        }
        else {
            return;
        }

        void SwitchTo(int index) {
            SaveSlotsManager.SwitchSlot(index);
            Refresh($"Switch to {SaveSlotsManager.GetSlotName(index)}");
        }
    }

    public static void Update(bool updateKey, bool updateButton) {
        foreach (TAS_Hotkey hotkey in Hotkeys) {
            hotkey.Update(updateKey, updateButton);
        }
    }

    internal static TAS_Hotkey BindingToHotkey(ButtonBinding binding, bool held = false) {
        return new(binding.Keys, binding.Buttons, true, held);
    }

    private static IEnumerable<PropertyInfo> bindingProperties;

    private static FieldInfo bindingFieldInfo;

    private static void ModReload(ILContext il) {
        bindingProperties = typeof(SRT_MoreSaveSlotsSettings)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(info => info.PropertyType == typeof(ButtonBinding) &&
                           info.GetCustomAttribute<DefaultButtonBinding2Attribute>() is { } extraDefaultKeyAttribute &&
                           extraDefaultKeyAttribute.ExtraKey != Keys.None);

        ILCursor ilCursor = new(il);
        if (ilCursor.TryGotoNext(
                MoveType.After,
                ins => ins.OpCode == OpCodes.Callvirt && ins.Operand.ToString().Contains("<Microsoft.Xna.Framework.Input.Keys>::Add(T)")
            )) {
            ilCursor.Emit(OpCodes.Ldloc_1).EmitDelegate(AddExtraDefaultKey);
        }
    }

    private static void AddExtraDefaultKey(object bindingEntry) {
        if (bindingFieldInfo == null) {
            bindingFieldInfo = bindingEntry.GetType().GetFieldInfo("Binding");
        }

        if (bindingFieldInfo?.GetValue(bindingEntry) is not ButtonBinding binding) {
            return;
        }

        if (bindingProperties.FirstOrDefault(info => info.GetValue(modSettings) == binding) is { } propertyInfo) {
            binding.Keys.Add(propertyInfo.GetCustomAttribute<DefaultButtonBinding2Attribute>().ExtraKey);
        }
    }

    private static MethodInfo method = null;

    private static void Refresh(string text) {
        method.Invoke(null, new object[] { text });
    }

    [Initialize]
    private static void Initialize() {
        method = ModUtils.GetType("TASHelper", "Celeste.Mod.TASHelper.Entities.HotkeyWatcher")?.GetMethodInfo("Refresh") ?? typeof(HotkeyWatcher).GetMethodInfo("Refresh");
    }
}


[Tracked(false)]
public class HotkeyWatcher : Message {

    public static HotkeyWatcher Instance;
    public static float lifetime = 3f;

    public float lifetimer = 0f;
    public HotkeyWatcher() : base("", new Vector2(10f, 1060f)) {
        this.Depth = -20000;
        base.Tag |= Tags.Global;
    }

    public static bool AddIfNecessary() {
        if (Engine.Scene is not Level level) {
            return false;
        }
        if (Instance is null || !level.Entities.Contains(Instance)) {
            Instance = new();
            level.Add(Instance);
        }
        return true;
    }


    private void RefreshImpl(string text) {
        RestoreAlpha(this.text.Equals(text));
        this.text = text;
        lifetimer = lifetime;
        Active = true;
        Visible = true;
    }

    public static void Refresh(string text) {
        if (AddIfNecessary()) {
            Instance.RefreshImpl(text);
        }
    }

    private void RestoreAlpha(bool sameText) {
        if (sameText) {
            FallAndRise = true;
        } else {
            alpha = 1f;
        }
    }

    private bool FallAndRise = false;
    public override void Update() {
        if (FallAndRise) {
            alpha -= 0.1f;
            if (alpha < 0f) {
                alpha = 1f;
                FallAndRise = false;
            }
        } else {
            if (lifetimer / lifetime < 0.1f) {
                alpha = 10 * lifetimer / lifetime;
            }
            lifetimer -= Engine.RawDeltaTime;
            if (lifetimer < 0f) {
                lifetimer = 0f;
                Active = Visible = false;
            }
        }

        base.Update();
    }

    public override void Render() {
        float scale = 0.6f;
        Vector2 Size = FontSize.Measure(text) * scale;
        Monocle.Draw.Rect(Position - 0.5f * Size.Y * Vector2.UnitY - 10f * Vector2.UnitX, Size.X + 20f, Size.Y + 10f, Color.Black * alpha * 0.5f);
        Font.Draw(BaseSize, text, Position, new Vector2(0f, 0.5f), Vector2.One * scale, Color.White * alpha, 0f, Color.Transparent, 1f, Color.Black);
    }

}

[Tracked(false)]
public class Message : Entity {
    internal static readonly Language english = Dialog.Languages["english"];

    internal static readonly PixelFont Font = Fonts.Get(english.FontFace);

    internal static readonly float BaseSize = english.FontFaceSize;

    public static readonly PixelFontSize FontSize = Font.Get(BaseSize);

    public string text;

    public float alpha;

    public Message(string text, Vector2 Position) : base(Position) {
        base.Tag = Tags.HUD;
        this.text = text;
        alpha = 1f;
    }
    public override void Update() {
        base.Update();
    }

    public override void Render() {
        RenderAtCenter(Position);
    }

    public void RenderAtTopLeft(Vector2 Position) {
        Font.Draw(BaseSize, text, Position, new Vector2(0f, 0f), Vector2.One * 0.6f, Color.White * alpha, 0f, Color.Transparent, 1f, Color.Black);
    }

    public void RenderAtCenter(Vector2 Position) {
        Font.Draw(BaseSize, text, Position, new Vector2(0.5f, 0.5f), Vector2.One * 0.5f, Color.White * alpha, 0f, Color.Transparent, 1f, Color.Black);
    }

    public static void RenderMessage(string str, Vector2 Position, Vector2 scale) {
        RenderMessage(str, Position, Vector2.One * 0.5f, scale);
    }

    public static void RenderMessage(string str, Vector2 Position, Vector2 justify, Vector2 scale) {
        Font.DrawOutline(BaseSize, str, Position, justify, scale, Color.White, 2f, Color.Black);
    }
    public static void RenderMessage(string str, Vector2 Position, Vector2 justify, Vector2 scale, float stroke) {
        Font.DrawOutline(BaseSize, str, Position, justify, scale, Color.White, stroke, Color.Black);
    }

    public static void RenderMessage(string str, Vector2 Position, Vector2 justify, Vector2 scale, float stroke, Color colorInside, Color colorOutline) {
        Font.DrawOutline(BaseSize, str, Position, justify, scale, colorInside, stroke, colorOutline);
    }

    public static void RenderMessageJetBrainsMono(string str, Vector2 Position, Vector2 justify, Vector2 scale, float stroke, Color colorInside, Color colorOutline) {
        TAS.EverestInterop.InfoHUD.JetBrainsMonoFont.DrawOutline(str, Position, justify, scale, colorInside, stroke, colorOutline);
    }
}