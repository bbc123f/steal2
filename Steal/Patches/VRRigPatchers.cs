using System;
using GorillaLocomotion;
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
            return (!MenuPatch.FindButton("Disable Fingers").Enabled);
        }
    }
    
    [HarmonyPatch(typeof(Player), "GetSlidePercentage", MethodType.Normal)]
    public class SlidePatch
    {
        static void Postfix(Player __instance, ref float __result)
        {
            try
            {
                if (MenuPatch.FindButton("NoSlip").Enabled)
                    __result = 0;
            }
            catch {}
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
            return (!MenuPatch.FindButton("Anti MouthFlap").Enabled);
        }
    }

}
