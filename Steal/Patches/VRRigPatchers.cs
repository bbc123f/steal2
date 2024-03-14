using System;
using HarmonyLib;
using UnityEngine;
using WristMenu.Background;

namespace Steal.Patchers.VRRigPatchers
{
<<<<<<< HEAD
    [HarmonyPatch(typeof(VRRig), "OnEnable", MethodType.Normal)]
    public class OnEnable : MonoBehaviour
    {
        public static bool nameTags = true;
        static void Postfix(VRRig __instance)
=======
    [HarmonyPatch(typeof(VRRig), "IncrementRPC", new Type[] { typeof(PhotonMessageInfoWrapped), typeof(string) })]
    public class NoIncrementRPC : MonoBehaviour
    {
        private static bool Prefix(PhotonMessageInfoWrapped info, string sourceCall)
>>>>>>> ccf540160b4ff51fd6b9d4e75d230d9c1792c6c0
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
