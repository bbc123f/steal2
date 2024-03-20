using System;
using HarmonyLib;
using UnityEngine;
using Steal.Background;
using Steal.Components;

namespace Steal.Patchers.VRRigPatchers
{
    [HarmonyPatch(typeof(VRMapMiddle), "MapMyFinger", MethodType.Normal)]
    [HarmonyPatch(typeof(VRMapIndex), "MapMyFinger", MethodType.Normal)]
    [HarmonyPatch(typeof(VRMapThumb), "MapMyFinger", MethodType.Normal)]
    public class NoFingerPatch : MonoBehaviour
    {
        public static bool Prefix()
        {
            return (!ModHandler.FindButton("Disable Fingers").Enabled);
        }
    }
    
    [HarmonyPatch(typeof(VRRig), "OnEnable", MethodType.Normal)]
    public class OnEnable : MonoBehaviour
    {
        public static bool nameTags = false;
        static void Postfix(VRRig __instance)
        {
            var nametag = __instance.gameObject.AddComponent<NameTags>();
        }
    }
    
    [HarmonyPatch(typeof(Debug), "Log", MethodType.Normal)]
    public class DebugLog : MonoBehaviour
    {
        public static bool Prefix()
        {
            return (!ModHandler.FindButton("Crash Gun").Enabled);
        }
    }
    
    [HarmonyPatch(typeof(VRRig), "OnDisable", MethodType.Normal)]
    public class OnDisable : MonoBehaviour
    {
        public static bool Prefix(VRRig __instance)
        {
            return !(__instance == GorillaTagger.Instance.offlineVRRig);
        }
    }
    
    [HarmonyPatch(typeof(GorillaMouthFlap), "CheckMouthflapChange")]
    [HarmonyPatch(typeof(GorillaMouthFlap), "UpdateMouthFlapFlipbook")]
    [HarmonyPatch(typeof(GorillaMouthFlap), "Update")]
    public class AntiFlap : MonoBehaviour
    {
        public static bool Prefix()
        {
            return (!ModHandler.FindButton("Anti MouthFlap").Enabled);
        }
    }
}
