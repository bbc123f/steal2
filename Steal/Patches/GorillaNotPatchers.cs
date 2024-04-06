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
    /*
    
    [HarmonyPatch(typeof(GorillaNot), "LogErrorCount", MethodType.Normal)]
    public class NoLogErrorCount : MonoBehaviour
    {
        static bool Prefix(string logString, string stackTrace, LogType type)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaTagManager), "ReportTag", MethodType.Normal)]
    public class ReportTagPatch : MonoBehaviour
    {
        static bool Prefix(Photon.Realtime.Player taggedPlayer, Photon.Realtime.Player taggingPlayer)
        {
            GorillaTagManager tagger = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
            if (tagger.currentInfected.Contains(taggingPlayer) && !tagger.currentInfected.Contains(taggedPlayer) && (double)Time.time > tagger.lastTag + (double)tagger.tagCoolDown)
            {
                VRRig taggingRig = tagger.FindPlayerVRRig(taggingPlayer);
                VRRig taggedRig = tagger.FindPlayerVRRig(taggedPlayer);
                if (!taggingRig.CheckDistance(taggedRig.transform.position, 5f))
                {
                    return false;
                }
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(GorillaGameManager), "ForceStopGame_DisconnectAndDestroy", MethodType.Normal)]
    public class AntiQuit : MonoBehaviour
    {
        static bool Prefix()
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
    
     [HarmonyPatch(typeof(GorillaNot), "IncrementRPCTracker", MethodType.Normal)]
      public class NoIncrementRPCTracker : MonoBehaviour
      {
          static bool Prefix(in string userId, in string rpcFunction, in int callLimit)
          {
             return false;
          }
      }
    
    
    
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
    }*/
}
