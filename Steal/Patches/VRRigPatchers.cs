using System;
using HarmonyLib;
using UnityEngine;
using Steal.Background;

namespace Steal.Patchers.VRRigPatchers
{
    [HarmonyPatch(typeof(VRRig), "OnEnable", MethodType.Normal)]
    public class OnEnable : MonoBehaviour
    {
        public static bool nameTags = true;
        static void Postfix(VRRig __instance)
        {
            if (!nameTags && __instance.GetComponent<NameTags>())
            {
                Destroy(__instance.GetComponent<NameTags>());
            }
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
