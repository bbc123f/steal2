using GorillaLocomotion;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Steal.Background;
using UnityEngine;
using UnityEngine.XR;
using BepInEx;

namespace Steal.Patchers
{

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public class GunLibraries : MonoBehaviour
    {
        private static void Prefix(Player __instance)
        {
            RaycastPositionStart = __instance.headCollider.transform.position + (PositionWithOffset(__instance.rightControllerTransform, __instance.rightHandOffset) - __instance.headCollider.transform.position).normalized * __instance.maxArmLength * __instance.scale;
        }

        private static Vector3 PositionWithOffset(Transform transformToModify, Vector3 offsetVector)
        {
            return transformToModify.position + transformToModify.rotation * offsetVector * Player.Instance.scale;
        }

        public static void GunCleanUp()
        {
            if (GunRenderer == null) return;
            lockedOnPlayer = null;
            UnityEngine.Object.Destroy(GunRenderer);
            GunRenderer = null;
            Destroy(GunPointer);
            Mods.Mods.ResetTexure();
            GunPointer = null;
        }

        public static VRRig lockedOnPlayer = null;
        public static object Shoot(bool stateDepender, bool stateTrigger)
        {
            object result = null;
            RaycastHit raycastHit;
            if (stateDepender || UnityInput.Current.GetMouseButton(1))
            {
                Player instance = Player.Instance;
                if (!XRSettings.isDeviceActive)
                {
                    Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit);
                }
                else
                {
                    Physics.Raycast(RaycastPositionStart, instance.rightControllerTransform.forward - instance.rightControllerTransform.up, out raycastHit);
                }
                if (GunRenderer == null)
                {
                    GunRenderer = new GameObject("line");
                    GunRenderer.AddComponent<LineRenderer>();
                    GunRenderer.GetComponent<LineRenderer>().endWidth = 0.015f;
                    GunRenderer.GetComponent<LineRenderer>().startWidth = 0.015f;
                    GunRenderer.GetComponent<LineRenderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                if (GunPointer == null)
                {
                    GunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(GunPointer.GetComponent<Rigidbody>());
                    Destroy(GunPointer.GetComponent<SphereCollider>());
                    GunPointer.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    GunPointer.GetComponent<Renderer>().material.color = new Color32(146, 99, 255, 90);
                    GunPointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                GunPointer.transform.position = raycastHit.point;
                GunRenderer.GetComponent<LineRenderer>().SetPosition(0, Player.Instance.rightControllerTransform.position);
                GunRenderer.GetComponent<LineRenderer>().SetPosition(1, raycastHit.point);
                GunRenderer.GetComponent<LineRenderer>().material.color = stateTrigger ? new Color32(146, 99, 255, 90) : new Color32(255, 255, 255, 90);
                result = raycastHit;
            }
            else
            {
                GunCleanUp();
                result = null;
            }
            if (result != null)
            {
                return (RaycastHit)result;
            }
            else
            {
                return null;
            }
        }

        public static bool isCasted = false;

        public static void ShootLock(bool stateDepender, bool stateTrigger)
        {
            try
            {
                RaycastHit raycastHit2 = default;
                if (stateDepender || UnityInput.Current.GetMouseButton(1))
                {
                    if (lockedOnPlayer == null)
                    {
                        Player instance = Player.Instance;
                        RaycastHit raycastHit;
                        if (!XRSettings.isDeviceActive)
                        {
                            Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit);
                        }
                        else
                        {
                            Physics.Raycast(RaycastPositionStart, instance.rightControllerTransform.forward - instance.rightControllerTransform.up, out raycastHit);
                        }
                        if (GunRenderer == null)
                        {
                            GunRenderer = new GameObject("line");
                            GunRenderer.AddComponent<LineRenderer>();
                            GunRenderer.GetComponent<LineRenderer>().endWidth = 0.02f;
                            GunRenderer.GetComponent<LineRenderer>().startWidth = 0.02f;
                            GunRenderer.GetComponent<LineRenderer>().material.shader = Shader.Find("GUI/Text Shader");
                        }

                        if (GunPointer == null)
                        {
                            GunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Destroy(GunPointer.GetComponent<Rigidbody>());
                            Destroy(GunPointer.GetComponent<SphereCollider>());
                            GunPointer.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                            GunPointer.GetComponent<Renderer>().material.color = new Color32(146, 99, 255, 90);
                            GunPointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                        }

                        GunPointer.transform.position = raycastHit.point;
                        GunRenderer.GetComponent<LineRenderer>()
                            .SetPosition(0, Player.Instance.rightControllerTransform.position);
                        GunRenderer.GetComponent<LineRenderer>().SetPosition(1, raycastHit.point);
                        GunRenderer.GetComponent<LineRenderer>().material.color = (stateTrigger ? Color.red : Color.white);
                        raycastHit2 = raycastHit;

                        if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
                        {
                            if (raycastHit.collider.GetComponentInParent<VRRig>())
                            {
                                GunPointer.transform.position = raycastHit2.point;
                                lockedOnPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                                if (lockedOnPlayer != null)
                                    GunRenderer.GetComponent<LineRenderer>().SetPosition(1, lockedOnPlayer.transform.position);
                            }
                        }
                        else
                        {
                            lockedOnPlayer = null;
                        }
                    }
                    else
                    {
                        if (lockedOnPlayer != null)
                            GunPointer.transform.position = lockedOnPlayer.transform.position;
                        GunRenderer.GetComponent<LineRenderer>().SetPosition(0, Player.Instance.rightControllerTransform.position);
                        GunRenderer.GetComponent<LineRenderer>().SetPosition(1, lockedOnPlayer.transform.position);
                    }
                }
                else
                {
                    GunCleanUp();
                    raycastHit2 = default(RaycastHit);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static Vector3 RaycastPositionStart;

        private static GameObject? GunRenderer;

        static GameObject GunPointer = null;
    }

}
