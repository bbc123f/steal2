using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Reflection;
using UnityEngine;
using Player = GorillaLocomotion.Player;

namespace Steal.Patchers.Misc
{
    [HarmonyPatch(typeof(GameObject), "CreatePrimitive", MethodType.Normal)]
    internal class GameObjectPatch
    {
        private static void Postfix(GameObject __result)
        {
            __result.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            __result.GetComponent<Renderer>().material.color = Color.black;
        }
    }

    [HarmonyPatch(typeof(GorillaNetworkPublicTestsJoin))]
    [HarmonyPatch("GracePeriod", MethodType.Enumerator)]
    class NoGracePeriod
    {
        public static bool Prefix()
        {
            return false;
        }
    }
    [HarmonyPatch(typeof(GorillaNetworkPublicTestsJoin))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    class NoGracePeriod4
    {
        public static bool Prefix()
        {
            return false;
        }
    }
    [HarmonyPatch(typeof(GorillaNetworkPublicTestJoin2))]
    [HarmonyPatch("GracePeriod", MethodType.Enumerator)]
    class NoGracePeriod3
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaNetworkPublicTestsJoin))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    class NoGracePeriod4
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaNetworkPublicTestJoin2))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    class NoGracePeriod2
    {
        public static bool Prefix()
        {
            return false;
        }
    }
<<<<<<< HEAD

    [HarmonyPatch(typeof(GorillaNetworkPublicTestJoin2))]
    [HarmonyPatch("GracePeriod", MethodType.Enumerator)]
    class NoGracePeriod3
    {
        public static bool Prefix()
        {
            return false;
        }
    }
=======
    
    // [HarmonyPatch(typeof(PhotonNetworkController), "ProcessConnectedAndWaitingState", MethodType.Normal)]
    // public class DontReconnect
    // {
    //     public static bool status = true;
    //     public static bool Prefix()
    //     {
    //         return status;
    //     }
    // }

>>>>>>> ccf540160b4ff51fd6b9d4e75d230d9c1792c6c0
}
