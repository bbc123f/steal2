using System;
using HarmonyLib;
using UnityEngine;
using Steal.Background;

namespace Steal.Patchers.VRRigPatchers
{
    [HarmonyPatch(typeof(VRRig), "OnDisable", MethodType.Normal)]
    public class OnDisable : MonoBehaviour
    {
        public static bool Prefix(VRRig __instance)
        {
            return !(__instance == GorillaTagger.Instance.offlineVRRig);
        }
    }
}
