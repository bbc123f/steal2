using System;
using HarmonyLib;
using Photon.Pun;
using Steal.Background;
using System.IO;
using System.Threading.Tasks;
using GorillaTag.GuidedRefs;
using UnityEngine;
using Object = UnityEngine.Object;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Steal.Patchers.GorillaNotPatchers
{
   
    [HarmonyPatch(typeof(GorillaNot), "SendReport", MethodType.Normal)]
    public class NoSendReport : MonoBehaviour
    {
        static bool Prefix(string susReason, string susId, string susNick)
        {
            ShowConsole.Log(susNick + " was reported! Reason: " + susReason + " ID: " + susId);
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


}
