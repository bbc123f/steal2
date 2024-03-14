using System;
using HarmonyLib;
using UnityEngine;

namespace Steal.Patchers.VRRigPatchers
{
    [HarmonyPatch(typeof(VRRig), "IncrementRPC", new Type[] { typeof(PhotonMessageInfoWrapped), typeof(string) })]
    public class NoIncrementRPC : MonoBehaviour
    {
        private static bool Prefix(PhotonMessageInfoWrapped info, string sourceCall)
        {
            return false;
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

    [HarmonyPatch(typeof(GorillaNetworkPublicTestJoin2), "GracePeriod", MethodType.Enumerator)]
    public class GracePeriod : MonoBehaviour
    {
        public static bool Prefix()
        {
            return false ;
        }
    }

    [HarmonyPatch(typeof(GorillaNetworkPublicTestsJoin), "GracePeriod", MethodType.Enumerator)]
    public class GracePeriod2 : MonoBehaviour
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
