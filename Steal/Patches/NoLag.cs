using GorillaExtensions;
using GorillaNetworking.Store;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.Patches
{
    public static class NoLag
    {
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

        [HarmonyPatch(typeof(GorillaParent), "LateUpdate", MethodType.Normal)]
        public class GorillaParentPatch
        {
            static float cooldown;
            public static bool Prefix(GorillaParent __instance)
            {
                if (Time.time > cooldown)
                {
                    cooldown = Time.time + 15;
                    if (PhotonNetwork.CurrentRoom != null && GorillaTagger.Instance.myVRRig.IsNull())
                    {
                        Traverse.Create(typeof(PhotonPrefabPool)).Field("networkPrefabs").GetValue<Dictionary<string, GameObject>>().TryGetValue("Player Network Controller", out GameObject go);
                        if (go == null)
                        {
                            return false;
                        }
                        NetworkSystem.Instance.NetInstantiate(go, GorillaTagger.Instance.offlineVRRig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.rotation, false);
                    }
                }
                return false;
            }
        }
    }
}