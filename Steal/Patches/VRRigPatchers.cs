using System;
using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;
using Steal.Background;
using Steal.Components;

namespace Steal.Patchers.VRRigPatchers
{
    
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


}
