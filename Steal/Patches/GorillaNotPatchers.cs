using System;
using HarmonyLib;
using Photon.Pun;
using Steal.Background;
using System.IO;
using System.Threading.Tasks;
using GorillaTag.GuidedRefs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Steal.Patchers.GorillaNotPatchers
{
    
    [HarmonyPatch(typeof(GorillaNot), "LogErrorCount", MethodType.Normal)]
    public class NoLogErrorCount : MonoBehaviour
    {
        static bool Prefix(string logString, string stackTrace, LogType type)
        {
            return false;
        }
    }
    
    [HarmonyPatch(typeof(GorillaNot), "SendReport", MethodType.Normal)]
    public class NoSendReport : MonoBehaviour
    {
        static bool Prefix(string susReason, string susId, string susNick)
        {
            ShowConsole.Log(susNick + " was reported! Reason: " + susReason + " ID: " + susId);
            return false;
        }
    }
    
    
    [HarmonyPatch(typeof(GorillaNot), "CloseInvalidRoom", MethodType.Normal)]
    public class NoCloseInvalidRoom : MonoBehaviour
    {
        static bool Prefix()
        {
            return false;
        }
    }
    
    [HarmonyPatch(typeof(GorillaNot), "ShouldDisconnectFromRoom")]
    internal class NoShouldDisconnectFromRoom : MonoBehaviour
    {
        private static bool Prefix()
        {
            return false;
        }
    }
    
    [HarmonyPatch(typeof(GorillaNot), "CheckReports", MethodType.Enumerator)]
    public class NoCheckReports : MonoBehaviour
    {
        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaNot), "QuitDelay", MethodType.Enumerator)]
    public class NoQuitDelay : MonoBehaviour
    {
        static bool Prefix()
        {
            return false;
        }
    }
    
    // [HarmonyPatch(typeof(GorillaNot), "IncrementRPCTracker", MethodType.Normal)]
    // public class NoIncrementRPCTracker : MonoBehaviour
    // {
    //     static bool Prefix()
    //     {
    //         return false;
    //     }
    // }
    
    [HarmonyPatch(typeof(GorillaNot), "IncrementRPCCallLocal", MethodType.Normal)]
    public class NoIncrementRPCCallLocal : MonoBehaviour
    {
        private static bool Prefix(PhotonMessageInfoWrapped infoWrapped, string rpcFunction)
        {
            //ShowConsole.Log(info.Sender.NickName + " sent rpc: " + rpcFunction);
            return false;
        }
    }




    
    [HarmonyPatch(typeof(GorillaNot), "IncrementRPCCall", new Type[] { typeof(PhotonMessageInfo), typeof(string) })]
    public class NoIncrementRPCCall : MonoBehaviour
    {
        static bool Prefix(PhotonMessageInfo info, string callingMethod = "")
        {
            //ShowConsole.Log($"Increment RPC: Info {info.ToString()}, callingMethod {callingMethod}");
    
            return false;
        }
    }
}
