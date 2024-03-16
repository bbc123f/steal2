using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using GorillaLocomotion;
using UnityEngine;

namespace Steal.Patchers
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    internal class TeleportationLib : MonoBehaviour
    {
        internal static bool Prefix(Player __instance, ref Vector3 ___lastPosition, ref Vector3[] ___velocityHistory,
            ref Vector3 ___lastHeadPosition, ref Vector3 ___lastLeftHandPosition, ref Vector3 ___lastRightHandPosition,
            ref Vector3 ___currentVelocity, ref Vector3 ___denormalizedVelocityAverage)
        {
            if (TeleportationLib.teleporting)
            {
                Vector3 vector = TeleportationLib.destination - __instance.bodyCollider.transform.position +
                                 __instance.transform.position;
                try
                {
                    __instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                    __instance.bodyCollider.attachedRigidbody.isKinematic = true;
                    ___velocityHistory = new Vector3[__instance.velocityHistorySize];
                    ___currentVelocity = Vector3.zero;
                    ___denormalizedVelocityAverage = Vector3.zero;
                    ___lastRightHandPosition = vector;
                    ___lastLeftHandPosition = vector;
                    ___lastHeadPosition = vector;
                    __instance.transform.position = vector;
                    ___lastPosition = vector;
                    __instance.bodyCollider.attachedRigidbody.isKinematic = false;
                }
                catch
                {
                }

                TeleportationLib.teleporting = false;
            }

            return true;
        }

        internal static void Teleport(Vector3 TeleportDestination)
        {
            TeleportationLib.teleporting = true;
            TeleportationLib.destination = TeleportDestination;
        }

        internal static void TeleportOnce(Vector3 TeleportDestination, bool stateDepender)
        {
            if (stateDepender)
            {
                if (!TeleportationLib.teleportOnce)
                {
                    TeleportationLib.teleporting = true;
                    TeleportationLib.destination = TeleportDestination;
                }

                TeleportationLib.teleportOnce = true;
            }
            else
            {
                TeleportationLib.teleportOnce = false;
            }
        }

        private static bool teleporting;

        private static Vector3 destination;

        private static bool teleportOnce;
    }

        }
