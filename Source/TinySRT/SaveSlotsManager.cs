using Monocle;

namespace Celeste.Mod.SRT_MoreSaveSlots.TinySRT;

public static class SaveSlotsManager {
    public static IEnumerable<SaveSlot> SaveSlots => Dictionary.Values;

    public static Dictionary<string, SaveSlot> Dictionary = new Dictionary<string, SaveSlot>();

    public static SaveSlot Slot;

    public static string SlotName;

    public static bool IsSaved() {
        return Slot.StateManager.IsSaved;
    }

    public static void SwitchSlot(int index) {
        SwitchSlot(GetSlotName(index));
    }
    public static void SwitchSlot(string name) {
        SlotName = name;
        if (Dictionary.TryGetValue(name, out SaveSlot slot)) {
            Slot = slot;
        }
        else {
            Slot = new SaveSlot(name);
            Dictionary.Add(name, Slot);
        }
    }
    public static string GetSlotName(int index) {
        return index == 1 ? "Default Slot" : $"SaveSlot@{index}";
    }
    public static bool SaveState() {
        if (Engine.Scene is not Level) {
            return false;
        }

        if (!IsAllFree()) {
            return false;
        }

        return TH_StateManager.SaveState();
    }

    public static bool LoadState() {
        if (Engine.Scene is not Level) {
            return false;
        }

        if (!IsAllFree()) {
            return false;
        }

        return TH_StateManager.LoadState();
    }

    public static void ClearState() {
        TH_StateManager.ClearState();
    }

    public static void ClearStateAndShowMessage() {
        if (Slot is null) {
            SpeedrunTool.SaveLoad.StateManager.Instance.ClearStateAndShowMessage();
            return;
        }
        TH_StateManager.ClearStateAndShowMessage();
    }

    public static bool IsFree(this TH_StateManager manager) {
        return manager.State == State.None;
    }

    public static bool IsAllFree() {
        foreach (SaveSlot slot in SaveSlots) {
            if (!IsFree(slot.StateManager)) {
                return false;
            }
        }
        return true;
    }
}

public class SaveSlot {
    public string Name;

    public TH_StateManager StateManager;

    public bool SaveLoadActionInitialized = false;

    public readonly List<TH_SaveLoadAction> All = new();

    public SaveSlot(string name) {
        Name = name;
        SaveLoadActionInitialized = false;
        All = new();
        StateManager = new();
    }
}