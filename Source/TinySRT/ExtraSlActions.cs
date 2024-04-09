using Celeste.Mod.SpeedrunTool.SaveLoad;
using Celeste.Mod.SRT_MoreSaveSlots.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Reflection;
using SRT = Celeste.Mod.SpeedrunTool.SaveLoad.SaveLoadAction;
using TH = Celeste.Mod.SRT_MoreSaveSlots.TinySRT.TH_SaveLoadAction;

namespace Celeste.Mod.SRT_MoreSaveSlots.TinySRT;

#pragma warning disable CS8625
public static class ExtraSlActions {

    // if there's mod which add SlAction by itself, instead of by SRT, then we also add it to our SaveLoadActions

    public static readonly List<TH> TH_Actions = new();

    public static List<SRT> SRT_Actions = new();

    // we want to ensure these actions are added lastly

    [Initialize]
    public static void LoadSRT() {
        // game crash if you saved before, hot reload, and load state, (mostly crash for reason like some mod type does not exist in the tracker) so we need to clear state when game reload
        StateManager.Instance.ClearState();

        foreach (SRT action in SRT_Actions) {
#pragma warning disable CS0618
            SRT.Add(action);
#pragma warning restore CS0618
        }
    }

    [Initialize]
    private static void Initialize() {

    }

    internal static void LoadTH() {
        // this is initialized when savestate is first invoked, so that's quite late
        foreach (TH action in TH_Actions) {
            TH.Add(action);
        }
    }

    [Unload]
    public static void Unload() {
        foreach (TH action in TH_Actions) {
            TH.Remove(action);
        }
        TH_Actions.Clear();

        foreach (SRT action in SRT_Actions) {
            SRT.Remove(action);
        }
        SRT_Actions.Clear();
    }
}