﻿

using Celeste.Mod.SRT_MoreSaveSlots.Utils;
using FMOD;
using FMOD.Studio;
using Force.DeepCloner;
using Force.DeepCloner.Helpers;
using Monocle;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Celeste.Mod.SpeedrunTool.Extensions.CommonExtensions;
using TH = Celeste.Mod.SRT_MoreSaveSlots.TinySRT.TH_SaveLoadAction;

namespace Celeste.Mod.SRT_MoreSaveSlots.TinySRT;

// we want to use SRT as fewer as possible (to avoid wrong reference)
// so we copy almost everything to here
#pragma warning disable CS8625
#pragma warning disable CS8603
#pragma warning disable CS8600
internal static class TH_MuteAudioUtils {
    public static readonly HashSet<string> RequireMuteAudioPaths = new HashSet<string> { "event:/game/general/strawberry_get", "event:/game/general/strawberry_laugh", "event:/game/general/strawberry_flyaway", "event:/game/general/seed_complete_main", "event:/game/general/key_get", "event:/game/general/cassette_get", "event:/game/05_mirror_temple/eyewall_destroy", "event:/char/badeline/boss_hug", "event:/char/badeline/boss_laser_fire" };

    public static readonly List<FMOD.Studio.EventInstance> RequireMuteAudios = new List<FMOD.Studio.EventInstance>();

    [Load]
    public static void Load() {
        On.FMOD.Studio.EventDescription.createInstance += EventDescriptionOnCreateInstance;
    }

    [Unload]
    public static void Unload() {
        On.FMOD.Studio.EventDescription.createInstance -= EventDescriptionOnCreateInstance;
    }

    public static RESULT EventDescriptionOnCreateInstance(On.FMOD.Studio.EventDescription.orig_createInstance orig, FMOD.Studio.EventDescription self, out FMOD.Studio.EventInstance instance) {
        RESULT result = orig(self, out instance);
        if ((TH_StateManager.Instance?.IsSaved ?? false) && instance != null && self.getPath(out var path) == RESULT.OK && path != null && RequireMuteAudioPaths.Contains(path)) {
            RequireMuteAudios.Add(instance);
        }

        return result;
    }

    public static void AddAction() {
        TH.SafeAdd(null, delegate (Dictionary<Type, Dictionary<string, object>> _, Level level) {
            level.Entities.FindAll<SoundEmitter>().ForEach(delegate (SoundEmitter emitter) {
                emitter.Source.instance?.setVolume(0f);
            });
            foreach (FMOD.Studio.EventInstance requireMuteAudio in RequireMuteAudios) {
                requireMuteAudio.setVolume(0f);
            }

            RequireMuteAudios.Clear();
        }, delegate {
            RequireMuteAudios.Clear();
        });
    }
}

internal static class TH_FrostHelperUtils {
    public static readonly Lazy<Type> AttachedDataHelperType = new Lazy<Type>(() => ModUtils.GetType("FrostHelper", "FrostHelper.Helpers.AttachedDataHelper"));

    public static readonly Lazy<Func<object, object[]>> GetAllData = new Lazy<Func<object, object[]>>(() => (Func<object, object[]>)(AttachedDataHelperType.Value?.GetMethodInfo("GetAllData")?.CreateDelegate(typeof(Func<object, object[]>))));

    public static readonly Lazy<Action<object, object[]>> SetAllData = new Lazy<Action<object, object[]>>(() => (Action<object, object[]>)(AttachedDataHelperType.Value?.GetMethodInfo("SetAllData")?.CreateDelegate(typeof(Action<object, object[]>))));

    public static void TH_CloneDataStore(object sourceObj, object clonedObj, DeepCloneState deepCloneState) {
        if (GetAllData.Value != null && SetAllData.Value != null) {
            object[] array = GetAllData.Value(sourceObj);
            if (array != null) {
                SetAllData.Value(clonedObj, array.DeepClone(deepCloneState));
            }
        }
    }

    public static void SupportFrostHelper() {
        if (AttachedDataHelperType.Value != null && GetAllData.Value == null
            && AttachedDataHelperType.Value.GetMethodInfo("SetAttached") is { } setAttached
            && ModUtils.GetType("FrostHelper", "FrostHelper.Entities.Boosters.GenericCustomBooster") is { } genericCustomBoosterType
            && genericCustomBoosterType.GetMethodInfo("GetBoosterThatIsBoostingPlayer") is { } getBoosterThatIsBoostingPlayer
           ) {
            setAttached = setAttached.MakeGenericMethod(genericCustomBoosterType);
            TH.SafeAdd(
                saveState: (values, level) => {
                    Dictionary<string, object> dict = new();
                    List<Entity> players = level.Tracker.GetEntities<Player>();
                    List<object> boosters = players.Select(player => getBoosterThatIsBoostingPlayer.Invoke(null, new object[] { player })).ToList();
                    dict["players"] = players;
                    dict["boosters"] = boosters;
                    values[genericCustomBoosterType] = dict.TH_DeepCloneShared();
                },
                loadState: (values, level) => {
                    Dictionary<string, object> dict = values[genericCustomBoosterType].TH_DeepCloneShared();
                    if (dict.TryGetValue("players", out object players) && dict.TryGetValue("boosters", out object boosters)) {
                        if (players is List<Entity> playerList && boosters is List<object> boosterList) {
                            for (int i = 0; i < playerList.Count; i++) {
                                setAttached.Invoke(null, new[] { playerList[i], boosterList[i] });
                            }
                        }
                    }
                });
        }

        if (ModUtils.GetType("FrostHelper", "FrostHelper.ChangeDashSpeedOnce") is { } changeDashSpeedOnceType) {
            TH.SafeAdd(
                (savedValues, _) => TH.SaveStaticMemberValues(savedValues, changeDashSpeedOnceType, "NextDashSpeed", "NextSuperJumpSpeed"),
                (savedValues, _) => TH.LoadStaticMemberValues(savedValues));
        }

        if (ModUtils.GetType("FrostHelper", "FrostHelper.TimeBasedClimbBlocker") is { } timeBasedClimbBlockerType) {
            TH.SafeAdd(
                (savedValues, _) => TH.SaveStaticMemberValues(savedValues, timeBasedClimbBlockerType, "_NoClimbTimer"),
                (savedValues, _) => TH.LoadStaticMemberValues(savedValues));
        }
    }
}


internal static class TH_EventInstanceUtils {
    [Load]
    private static void Load() {
        On.FMOD.Studio.EventInstance.setParameterValue += EventInstanceOnsetParameterValue;
    }

    [Unload]
    private static void OnUnhook() {
        On.FMOD.Studio.EventInstance.setParameterValue -= EventInstanceOnsetParameterValue;
    }

    private static RESULT EventInstanceOnsetParameterValue(On.FMOD.Studio.EventInstance.orig_setParameterValue orig,
        EventInstance self, string name, float value) {
        RESULT result = orig(self, name, value);
        if (result == RESULT.OK) {
            self.SaveParameters(name, value);
        }

        return result;
    }
}


internal static class TH_EventInstanceExtensions {
    public static readonly ConditionalWeakTable<EventInstance, ConcurrentDictionary<string, float>> CachedParameters = new ConditionalWeakTable<EventInstance, ConcurrentDictionary<string, float>>();

    public static readonly ConditionalWeakTable<EventInstance, object> NeedManualClonedEventInstances = new ConditionalWeakTable<EventInstance, object>();

    public static readonly ConditionalWeakTable<EventInstance, object> CachedTimelinePositions = new ConditionalWeakTable<EventInstance, object>();

    public static EventInstance NeedManualClone(this EventInstance eventInstance) {
#pragma warning disable CS8620
        NeedManualClonedEventInstances.Set(eventInstance, null);
#pragma warning disable CS8620
        return eventInstance;
    }

    public static bool IsNeedManualClone(this EventInstance eventInstance) {
        return NeedManualClonedEventInstances.ContainsKey(eventInstance);
    }

    public static ConcurrentDictionary<string, float> GetSavedParameterValues(this EventInstance eventInstance) {
        if (!(eventInstance == null)) {
            return CachedParameters.GetOrCreateValue(eventInstance);
        }

        return null;
    }

    public static void SaveParameters(this EventInstance eventInstance, string param, float value) {
        if (param != null) {
            eventInstance.GetSavedParameterValues()[param] = value;
        }
    }

    public static int LoadTimelinePosition(this EventInstance eventInstance) {
        int num = 0;
        if (CachedTimelinePositions.TryGetValue(eventInstance, out var value)) {
            num = (int)value;
        }

        if (num > 0) {
            return num;
        }

        eventInstance.getTimelinePosition(out var position);
        return position;
    }

    public static void SaveTimelinePosition(this EventInstance eventInstance, int timelinePosition) {
        CachedTimelinePositions.Set(eventInstance, timelinePosition);
    }

    public static void CopyTimelinePosition(this EventInstance eventInstance, EventInstance otherEventInstance) {
        int num = otherEventInstance.LoadTimelinePosition();
        if (num > 0) {
            eventInstance.setTimelinePosition(num);
            eventInstance.SaveTimelinePosition(otherEventInstance.LoadTimelinePosition());
        }
    }

    public static EventInstance Clone(this EventInstance eventInstance) {
        string eventName = Audio.GetEventName(eventInstance);
        if (eventName.IsNullOrEmpty()) {
            return null;
        }

        EventInstance eventInstance2 = Audio.CreateInstance(eventName);
        if (eventInstance2 == null) {
            return null;
        }

        if (eventInstance.IsNeedManualClone()) {
            eventInstance2.NeedManualClone();
        }

        ConcurrentDictionary<string, float> savedParameterValues = eventInstance.GetSavedParameterValues();
        if (savedParameterValues != null) {
            foreach (KeyValuePair<string, float> item in savedParameterValues) {
                eventInstance2.setParameterValue(item.Key, item.Value);
            }
        }

        eventInstance2.CopyTimelinePosition(eventInstance);
        return eventInstance2;
    }

    public static void CopyParametersFrom(this EventInstance eventInstance, ConcurrentDictionary<string, float> parameters) {
        if (eventInstance == null || parameters == null) {
            return;
        }

        ConcurrentDictionary<string, float> concurrentDictionary = new ConcurrentDictionary<string, float>(eventInstance.GetSavedParameterValues());
        foreach (KeyValuePair<string, float> parameter2 in parameters) {
            eventInstance.setParameterValue(parameter2.Key, parameter2.Value);
        }

        foreach (KeyValuePair<string, float> item in concurrentDictionary) {
            if (!parameters.ContainsKey(item.Key) && eventInstance.getDescription(out var description) == RESULT.OK && description.getParameter(item.Key, out var parameter) == RESULT.OK) {
                eventInstance.setParameterValue(item.Key, parameter.defaultvalue);
            }
        }
    }
}