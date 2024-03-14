using HarmonyLib;
using UnityEngine;

namespace Steal.Patchers.VRRigPatchers
{
    [HarmonyPatch(typeof(VRRig), "IncrementRPC", MethodType.Normal)]
    public class NoIncrementRPC : MonoBehaviour
    {
        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(VRRig), "OnDisable", MethodType.Normal)]
    public class OnDisable : MonoBehaviour
    {
        public static bool Prefix(VRRig __instance)
        {
            if (__instance == GorillaTagger.Instance.offlineVRRig)
            {
                Traverse.Create(__instance).Field("initialized").SetValue(false);
                __instance.muted = false;
                Traverse.Create(__instance).Field("voiceAudio").SetValue(null);
                Traverse.Create(__instance).Field("tempRig").SetValue(null);
                Traverse.Create(__instance).Field("timeSpawned").SetValue(0f);
                __instance.initializedCosmetics = false;
                Traverse.Create(__instance).Field("tempMatIndex").SetValue(0);
                __instance.setMatIndex = 0;
                Traverse.Create(__instance).Field("creator").SetValue(null);
                return false;
            }
            return true;
        }
    }
}
