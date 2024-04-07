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
        static Shader uberShader = null;
        private static void Postfix(GameObject __result)
        {
            if (uberShader == null)
                uberShader = Shader.Find("GorillaTag/UberShader");

            __result.GetComponent<Renderer>().material.shader = uberShader;
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

    [HarmonyPatch(typeof(GorillaNetworkPublicTestJoin2))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    class NoGracePeriod2
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("AntiTeleportTechnology", MethodType.Normal)]
    class AntiTeleport
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
