global using Celeste.Mod.SRT_MoreSaveSlots.Utils.Attributes;
global using static Celeste.Mod.SRT_MoreSaveSlots.GlobalVariables;
global using static Celeste.Mod.SRT_MoreSaveSlots.GlobalMethod;
using Celeste.Mod.SRT_MoreSaveSlots.Module;
using Monocle;

namespace Celeste.Mod.SRT_MoreSaveSlots;

internal static class GlobalVariables {

    public static SRT_MoreSaveSlotsSettings modSettings => SRT_MoreSaveSlotsSettings.Instance;
    public static Player? player => Engine.Scene.Tracker.GetEntity<Player>();

    public static readonly object[] parameterless = { };
}


internal static class GlobalMethod {
    public static T Apply<T>(this T obj, Action<T> action) {
        action(obj);
        return obj;
    }

}