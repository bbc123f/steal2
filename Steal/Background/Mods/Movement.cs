using BepInEx;
using BepInEx.Configuration;
using ExitGames.Client.Photon;
using GorillaLocomotion.Gameplay;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Steal.Patchers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;
using static Steal.Background.InputHandler;
using Object = UnityEngine.Object;

namespace Steal.Background.Mods
{
    internal class Movement : Mod
    {
        private static GameObject LeftPlat;
        private static GameObject RightPlat;
        static GameObject pointer;
        private static GameObject[] LeftPlat_Networked = new GameObject[9999];
        private static GameObject[] RightPlat_Networked = new GameObject[9999];

        public static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);
        private static bool gravityToggled;
        private static bool flying;
        static string oldRoom;
        static bool canTP = false;

        static float RG;
        static float LG;
        static bool RGrabbing;
        static Vector3 CurHandPos;
        static bool LGrabbing;
        static bool AirMode;
        static int layers = int.MaxValue;

        public static float Spring;
        public static float Damper;
        public static float MassScale;
        public static ConfigEntry<float> sp;
        public static ConfigEntry<float> dp;
        public static Color grapplecolor;
        public static ConfigEntry<float> ms;
        public static ConfigEntry<Color> rc;
        public static bool wackstart = false;
        public static bool canleftgrapple = true;

        static bool canBHop = false;
        static bool isBHop = false;
        static bool isnoclipped = false;
        static float LongArmsOffset = 0;
        static LineRenderer lineRenderer;
        static bool disablegrapple = false;
        static RaycastHit hit;
        public static float speedBoostMultiplier = 1.15f;
        public static float flightMultiplier = 1.15f;
        public static float WallWalkMultiplier = 1.15f;
        private static bool clipped = false;

        public static List<Vector3> Macro = new List<Vector3>();

        public static float MacroHelp = 0f;

        private static Vector3 head_direction;

        private static Vector3 roll_direction;

        private static Vector2 left_joystick;

        private static bool Start = false;

        private static float multiplier = 1f;

        private static float speed = 0f;

        private static float maxs = 10f;

        private static float acceleration = 5f;

        private static float distance = 0.35f;

        public static float WallWalkMultiplierManager(float currentSpeed)
        {
            float speed = currentSpeed;

            if (speed == 3f)
                speed = 4f;
            else if (speed == 4f)
                speed = 6f;
            else if (speed == 6f)
                speed = 7f;
            else if (speed == 7f)
                speed = 8f;
            else if (speed == 8f)
                speed = 9f;
            else if (speed == 9f)
                speed = 3f;

            return speed;
        }

        public static float multiplierManager(float currentSpeed)
        {
            float speed = currentSpeed;

            if (speed == 1.15f)
                speed = 1.3f;
            else if (speed == 1.3f)
                speed = 1.5f;
            else if (speed == 1.5f)
                speed = 1.7f;
            else if (speed == 1.7f)
                speed = 2f;
            else if (speed == 2f)
                speed = 3f;
            else if (speed == 3f)
                speed = 1.15f;

            return speed;
        }

        public static List<Vector3> positions = new List<Vector3>();

        public static float RewindHelp = 0f;

        public static void Reverse()
        {
            if (LeftGrip)
            {
                for (int i = positions.Count - 1; i >= 0; i--)
                {
                    if (RewindHelp == 0f)
                    {
                        GorillaLocomotion.Player.Instance.transform.position = positions[i];
                        positions.RemoveAt(i);
                        RewindHelp = Time.deltaTime + 1f;
                    }
                }
            }
            else if (RightGrip)
            {
                positions.Add(GorillaLocomotion.Player.Instance.transform.position);
            }
        }

        internal static Vector3 previousMousePosition;

        public static float getSlideMultiplier()
        {
            return slideControlMultiplier;
        }
        public static float slideControlMultiplier = 1.15f;
        public static void slideControl(bool enable, float control)
        {
            if (enable)
            {
                GorillaLocomotion.Player.Instance.slideControl = control;
            }
            else
            {
                GorillaLocomotion.Player.Instance.slideControl = 0.00425f;
            }
        }
        public static void SwitchSlide()
        {
            slideControlMultiplier = multiplierManager(slideControlMultiplier);
        }

        public static void AdvancedWASD(float speed)
        {
            GorillaTagger.Instance.rigidbody.useGravity = false;
            GorillaTagger.Instance.rigidbody.velocity = new Vector3(0, 0, 0);
            float NSpeed = speed * Time.deltaTime;
            if (UnityInput.Current.GetKey(KeyCode.LeftShift) || UnityInput.Current.GetKey(KeyCode.RightShift))
            {
                NSpeed *= 10f;
            }
            if (UnityInput.Current.GetKey(KeyCode.LeftArrow) || UnityInput.Current.GetKey(KeyCode.A))
            {
                Camera.main.transform.position += Camera.main.transform.right * -1f * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.RightArrow) || UnityInput.Current.GetKey(KeyCode.D))
            {
                Camera.main.transform.position += Camera.main.transform.right * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.UpArrow) || UnityInput.Current.GetKey(KeyCode.W))
            {
                Camera.main.gameObject.transform.position += Camera.main.transform.forward * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.DownArrow) || UnityInput.Current.GetKey(KeyCode.S))
            {
                Camera.main.transform.position += Camera.main.transform.forward * -1f * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.Space) || UnityInput.Current.GetKey(KeyCode.PageUp))
            {
                Camera.main.transform.position += Camera.main.transform.up * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.LeftControl) || UnityInput.Current.GetKey(KeyCode.PageDown))
            {
                Camera.main.transform.position += Camera.main.transform.up * -1f * NSpeed;
            }
            if (UnityInput.Current.GetMouseButton(1))
            {
                Vector3 val = UnityInput.Current.mousePosition - previousMousePosition;
                float num2 = Camera.main.transform.localEulerAngles.y + val.x * 0.3f;
                float num3 = Camera.main.transform.localEulerAngles.x - val.y * 0.3f;
                Camera.main.transform.localEulerAngles = new Vector3(num3, num2, 0f);
            }
            previousMousePosition = UnityInput.Current.mousePosition;
        }

        #region Tp Stuff

        public static void ProcessCheckPoint(bool on)
        {
            if (on)
            {
                if (InputHandler.RightGrip)
                {
                    if (pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    }

                    pointer.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.position;
                }
                else if (!RightGrip && !RightTrigger)
                {
                    GameObject.Destroy(pointer);
                    pointer = null;
                }

                if (!InputHandler.RightGrip && InputHandler.RightTrigger)
                {
                    pointer.GetComponent<Renderer>().material.color = Color.green;
                    TeleportationLib.Teleport(pointer.transform.position);
                }

                if (!InputHandler.RightTrigger)
                {
                    pointer.GetComponent<Renderer>().material.color = Color.red;
                }
            }
            else
            {
                GameObject.Destroy(pointer);
                pointer = null;
            }
        }


        public static void TeleportGun()
        {
            var data = GunLib.Shoot();
            if (data != null)
            {
                if (data.isShooting && data.isTriggered)
                {
                    if (canTP)
                    {
                        canTP = false;
                        TeleportationLib.Teleport(data.hitPosition);
                    }
                }
                else
                {
                    canTP = true;
                }
            }
        }


        #endregion

        #region platforms

        public static string[] platformTypes =
        {
            "Normal",
            "Invisible",
            "Sticky"
        };

        public static void ChangePlatforms()
        {
            if (currentPlatform < 2)
                currentPlatform++;
            else
                currentPlatform = 0;
        }

        public static void Platforms()
        {
            RaiseEventOptions safs = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others
            };
            if (RightGrip)
            {
                if (RightPlat == null)
                {
                    RightPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    if (currentPlatform != 2)
                        RightPlat.transform.position = new Vector3(0f, -0.0175f, 0f) +
                                                       GorillaLocomotion.Player.Instance.rightControllerTransform
                                                           .position;
                    else
                        RightPlat.transform.position = new Vector3(0f, 0.025f, 0f) +
                                                       GorillaLocomotion.Player.Instance.rightControllerTransform
                                                           .position;

                    RightPlat.transform.rotation = GorillaLocomotion.Player.Instance.rightControllerTransform.rotation;
                    RightPlat.transform.localScale = scale;

                    if (currentPlatform == 1)
                    {
                        if (RightPlat.GetComponent<MeshRenderer>() != null)
                        {
                            Destroy(RightPlat.GetComponent<MeshRenderer>());
                        }
                    }
                    else
                    {
                        if (RightPlat.GetComponent<MeshRenderer>() == null)
                        {
                            RightPlat.AddComponent<MeshRenderer>();
                        }

                        RightPlat.GetComponent<Renderer>().material.color = Color.black;
                        PhotonNetwork.RaiseEvent(110,
                            new object[]
                            {
                                RightPlat.transform.position, RightPlat.transform.rotation, scale,
                                RightPlat.GetComponent<Renderer>().material.color
                            }, safs, SendOptions.SendReliable);
                    }
                }
            }
            else
            {
                if (RightPlat != null)
                {
                    PhotonNetwork.RaiseEvent(111, null, safs, SendOptions.SendReliable);
                    Object.Destroy(RightPlat);
                    RightPlat = null;
                }
            }

            if (LeftGrip)
            {
                if (LeftPlat == null)
                {
                    LeftPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    LeftPlat.transform.localScale = scale;
                    if (currentPlatform != 2)
                        LeftPlat.transform.position = new Vector3(0f, -0.0175f, 0f) +
                                                      GorillaLocomotion.Player.Instance.leftControllerTransform
                                                          .position;
                    else
                        LeftPlat.transform.position = new Vector3(0f, 0.025f, 0f) +
                                                      GorillaLocomotion.Player.Instance.leftControllerTransform
                                                          .position;

                    LeftPlat.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;

                    if (currentPlatform == 1)
                    {
                        if (LeftPlat.GetComponent<MeshRenderer>() != null)
                        {
                            Destroy(LeftPlat.GetComponent<MeshRenderer>());
                        }
                    }
                    else
                    {
                        if (LeftPlat.GetComponent<MeshRenderer>() == null)
                        {
                            LeftPlat.AddComponent<MeshRenderer>();
                        }

                        LeftPlat.GetComponent<Renderer>().material.color = Color.black;
                        PhotonNetwork.RaiseEvent(110,
                            new object[]
                            {
                                LeftPlat.transform.position, LeftPlat.transform.rotation, scale,
                                LeftPlat.GetComponent<Renderer>().material.color
                            }, safs, SendOptions.SendReliable);
                    }
                }
            }
            else
            {
                if (LeftPlat != null)
                {
                    PhotonNetwork.RaiseEvent(121, null, safs, SendOptions.SendReliable);
                    Object.Destroy(LeftPlat);
                    LeftPlat = null;
                }
            }
        }

        public static void PlatformNetwork(EventData data)
        {
            if (data.Code == 110)
            {
                object[] customshit = (object[])data.CustomData;
                RightPlat_Networked[data.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightPlat_Networked[data.Sender].transform.position = (Vector3)customshit[0];
                RightPlat_Networked[data.Sender].transform.rotation = (Quaternion)customshit[1];
                RightPlat_Networked[data.Sender].transform.localScale = (Vector3)customshit[2];
                RightPlat_Networked[data.Sender].GetComponent<Renderer>().material.color = (Color)customshit[3];
                RightPlat_Networked[data.Sender].GetComponent<BoxCollider>().enabled = false;
            }

            if (data.Code == 120)
            {
                object[] customshit = (object[])data.CustomData;
                LeftPlat_Networked[data.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftPlat_Networked[data.Sender].transform.position = (Vector3)customshit[0];
                LeftPlat_Networked[data.Sender].transform.rotation = (Quaternion)customshit[1];
                LeftPlat_Networked[data.Sender].transform.localScale = (Vector3)customshit[2];
                LeftPlat_Networked[data.Sender].GetComponent<Renderer>().material.color = (Color)customshit[3];
                LeftPlat_Networked[data.Sender].GetComponent<BoxCollider>().enabled = false;
            }

            if (data.Code == 110)
            {
                Destroy(RightPlat_Networked[data.Sender]);
                RightPlat_Networked[data.Sender] = null;
            }

            if (data.Code == 121)
            {
                Destroy(LeftPlat_Networked[data.Sender]);
                LeftPlat_Networked[data.Sender] = null;
            }
        }

        #endregion

        #region Default Stuff
        public static void CarMonke()
        {
            if (!Start)
            {
                multiplier = 3f;
                Start = true;
            }

            left_joystick = LeftJoystick;

            RaycastHit raycastHit;
            Physics.Raycast(global::GorillaLocomotion.Player.Instance.bodyCollider.transform.position, Vector3.down, out raycastHit, 100f, layers);
            head_direction = global::GorillaLocomotion.Player.Instance.headCollider.transform.forward;
            roll_direction = Vector3.ProjectOnPlane(head_direction, raycastHit.normal);
            if (left_joystick.y != 0f)
            {
                if (left_joystick.y < 0f)
                {
                    if (speed > -maxs)
                    {
                        speed -= acceleration * Math.Abs(left_joystick.y) *
                                           Time.deltaTime;
                    }
                }
                else if (speed < maxs)
                {
                    speed += acceleration * Math.Abs(left_joystick.y) * Time.deltaTime;
                }
            }
            else if (speed < 0f)
            {
                speed += acceleration * Time.deltaTime * 0.5f;
            }
            else if (speed > 0f)
            {
                speed -= acceleration * Time.deltaTime * 0.5f;
            }

            if (speed > maxs)
            {
                speed = maxs;
            }

            if (speed < -maxs)
            {
                speed = -maxs;
            }

            if (speed != 0f && raycastHit.distance < distance)
            {
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity =
                    roll_direction.normalized * speed * multiplier;
            }

            if (GorillaLocomotion.Player.Instance.IsHandTouching(true) ||
                GorillaLocomotion.Player.Instance.IsHandTouching(false))
            {
                speed *= 0.75f;
            }
        }

        public static void Rewind()
        {
            MeshCollider[] array = Resources.FindObjectsOfTypeAll<MeshCollider>();
            if (LeftGrip)
            {
                Macro.Add(global::GorillaLocomotion.Player.Instance.transform.position);
            }

            if (RightGrip)
            {
                global::GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = false;
                if (!clipped)
                {
                    foreach (MeshCollider meshCollider in array)
                    {
                        meshCollider.transform.localScale = meshCollider.transform.localScale / 10000f;
                    }
                }
                clipped = true;
                for (int k = 0; k < Macro.Count; k++)
                {
                    if (MacroHelp == 0f)
                    {
                        global::GorillaLocomotion.Player.Instance.transform.position = Macro[k];
                        Macro.RemoveAt(k);
                        MacroHelp = Time.deltaTime + 1f;
                    }
                }
            }
            else
            {
                global::GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = true;
            }
            if (clipped && !RightGrip)
            {
                foreach (MeshCollider meshCollider2 in array)
                {
                    meshCollider2.transform.localScale = meshCollider2.transform.localScale * 10000f;
                }
                clipped = false;
            }
        }

        public static void DisableReverseTime()
        {
            positions.Clear();
            Macro.Clear();
        }
        public static void MonkeClimb()
        {
            GorillaLocomotion.Player __instance = GorillaLocomotion.Player.Instance;
            if (!PhotonNetwork.InLobby)
            {
                RG = ControllerInputPoller.instance.rightControllerGripFloat;
                LG = ControllerInputPoller.instance.leftControllerGripFloat;
                RaycastHit raycastHit;
                bool flag = Physics.Raycast(__instance.leftControllerTransform.position,
                    __instance.leftControllerTransform.right, out raycastHit, 0.2f, layers);
                if ((Physics.Raycast(__instance.rightControllerTransform.position,
                        -__instance.rightControllerTransform.right, out raycastHit, 0.2f, layers) || AirMode) &&
                    RG > 0.5f)
                {
                    if (!RGrabbing)
                    {
                        CurHandPos = __instance.rightControllerTransform.position;
                        RGrabbing = true;
                        LGrabbing = false;
                    }

                    ApplyVelocity(__instance.rightControllerTransform.position, CurHandPos, __instance);
                    return;
                }

                if ((flag || AirMode) && LG > 0.5f)
                {
                    if (!LGrabbing)
                    {
                        CurHandPos = __instance.leftControllerTransform.position;
                        LGrabbing = true;
                        RGrabbing = false;
                    }

                    ApplyVelocity(__instance.leftControllerTransform.position, CurHandPos, __instance);
                    return;
                }

                if (LGrabbing || RGrabbing)
                {
                    Physics.gravity = new Vector3(0, -9.81f, 0);
                    LGrabbing = false;
                    RGrabbing = false;
                }
            }
        }

        public static void FasterSwimming()
        {
            if (GorillaLocomotion.Player.Instance.InWater)
            {
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity =
                    GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity * 1.013f;
            }
        }

        public static void BHop()
        {
            if (RightSecondary)
            {
                if (canBHop)
                {
                    isBHop = !isBHop;
                    canBHop = false;
                }
            }
            else
            {
                canBHop = true;
            }

            if (isBHop)
            {
                if (GorillaLocomotion.Player.Instance.IsHandTouching(false))
                {
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>()
                        .AddForce(Vector3.up * 270f, ForceMode.Impulse);
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(
                        GorillaTagger.Instance.offlineVRRig.rightHandPlayer.transform.right * 220f, ForceMode.Impulse);
                }

                if (GorillaLocomotion.Player.Instance.IsHandTouching(true))
                {
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>()
                        .AddForce(Vector3.up * 270f, ForceMode.Impulse);
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(
                        -GorillaTagger.Instance.offlineVRRig.leftHandPlayer.transform.right * 220f, ForceMode.Impulse);
                }
            }
        }
        public static void ZeroGravity()
        {
            Physics.gravity = new Vector3(0, 0, 0);
        }

        public static void WallWalk()
        {
            Vector3 gravityMultiplier = default;
            if ((GorillaLocomotion.Player.Instance.wasLeftHandTouching ||
                 GorillaLocomotion.Player.Instance.wasRightHandTouching) && (LeftGrip || RightGrip))
            {
                FieldInfo fieldInfo = typeof(GorillaLocomotion.Player).GetField("lastHitInfoHand",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                RaycastHit ray = (RaycastHit)fieldInfo.GetValue(GorillaLocomotion.Player.Instance);
                gravityMultiplier = ray.normal;
            }

            if (LeftGrip || RightGrip)
            {
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(
                    (gravityMultiplier * -5) * WallWalkMultiplier, ForceMode.Acceleration);
                Physics.gravity = new Vector3(0, 0, 0);
            }
            else
            {
                Physics.gravity = new Vector3(0, -9.81f, 0);
            }
        }

        public static void ResetGravity()
        {
            Physics.gravity = new Vector3(0, -9.81f, 0);
        }

        private static void ApplyVelocity(Vector3 pos, Vector3 target, GorillaLocomotion.Player __instance)
        {
            Physics.gravity = new Vector3(0, 0, 0);
            Vector3 a = target - pos;
            __instance.bodyCollider.attachedRigidbody.velocity = a * 65f;
        }


        public static void LeftDrawRope(GorillaLocomotion.Player __instance)
        {
            bool flag = !leftjoint;
            if (!flag)
            {
                leftlr.SetPosition(0, __instance.leftControllerTransform.position);
                leftlr.SetPosition(1, leftgrapplePoint);
            }
        }


        public static void LeftStopGrapple()
        {
            leftlr.positionCount = 0;
            Object.Destroy(leftjoint);
            canleftgrapple = true;
        }

        public static bool cangrapple = true;

        public static void SpiderMonke()
        {
            ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "spiderpatch.cfg"), true);
            sp = configFile.Bind<float>("Configuration", "Spring", 10f, "spring");
            dp = configFile.Bind<float>("Configuration", "Damper", 30f, "damper");
            ms = configFile.Bind<float>("Configuration", "MassScale", 12f, "massscale");
            rc = configFile.Bind<Color>("Configuration", "webColor", Color.white, "webcolor hex code");
            bool flag3 = !wackstart;
            if (flag3)
            {
                GameObject gameObject = new GameObject();
                Spring = sp.Value;
                Damper = dp.Value;
                MassScale = ms.Value;
                grapplecolor = rc.Value;
                lr = GorillaLocomotion.Player.Instance.gameObject.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = grapplecolor;
                lr.endColor = grapplecolor;
                lr.startWidth = 0.02f;
                lr.endWidth = 0.02f;
                leftlr = gameObject.AddComponent<LineRenderer>();
                leftlr.material = new Material(Shader.Find("Sprites/Default"));
                leftlr.startColor = grapplecolor;
                leftlr.endColor = grapplecolor;
                leftlr.startWidth = 0.02f;
                leftlr.endWidth = 0.02f;
                wackstart = true;
            }

            DrawRope(GorillaLocomotion.Player.Instance);
            LeftDrawRope(GorillaLocomotion.Player.Instance);
            if (InputHandler.RightTrigger)
            {
                if (cangrapple)
                {
                    Spring = sp.Value;
                    StartGrapple(GorillaLocomotion.Player.Instance);
                    cangrapple = false;
                }
            }
            else
            {
                StopGrapple(GorillaLocomotion.Player.Instance);
            }

            if (InputHandler.LeftTrigger)
            {
                Spring /= 2f;
            }
            else
            {
                Spring = sp.Value;
            }

            if (InputHandler.LeftTrigger)
            {
                if (canleftgrapple)
                {
                    Spring = sp.Value;
                    LeftStartGrapple(GorillaLocomotion.Player.Instance);
                    canleftgrapple = false;
                }
            }
            else
            {
                LeftStopGrapple();
            }
        }

        public static void LeftStartGrapple(GorillaLocomotion.Player __instance)
        {
            RaycastHit raycastHit;
            bool flag = Physics.Raycast(__instance.leftControllerTransform.position,
                __instance.leftControllerTransform.forward,
                out raycastHit, maxDistance);
            if (flag)
            {
                leftgrapplePoint = raycastHit.point;
                leftjoint = __instance.gameObject.AddComponent<SpringJoint>();
                leftjoint.autoConfigureConnectedAnchor = false;
                leftjoint.connectedAnchor = leftgrapplePoint;
                float num = Vector3.Distance(__instance.bodyCollider.attachedRigidbody.position, leftgrapplePoint);
                leftjoint.maxDistance = num * 0.8f;
                leftjoint.minDistance = num * 0.25f;
                leftjoint.spring = Spring;
                leftjoint.damper = Damper;
                leftjoint.massScale = MassScale;
                leftlr.positionCount = 2;
            }
        }


        public static float maxDistance = 100f;
        private static float myVarY1;
        static LineRenderer lr;
        public static Vector3 grapplePoint;
        public static Vector3 leftgrapplePoint;
        private static float myVarY2;
        private static float gainSpeed;
        public static SpringJoint joint;
        public static Vector3? leftHandOffsetInitial = null;
        public static Vector3? rightHandOffsetInitial = null;
        public static SpringJoint leftjoint;
        public static LineRenderer leftlr;
        public static float? maxArmLengthInitial = null;
        private static int currentPlatform;
        private static float ropetimeout;

        public static void StartGrapple(GorillaLocomotion.Player __instance)
        {
            RaycastHit raycastHit;
            bool flag = Physics.Raycast(__instance.rightControllerTransform.position,
                __instance.rightControllerTransform.forward,
                out raycastHit, maxDistance);
            if (flag)
            {
                grapplePoint = raycastHit.point;
                joint = __instance.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;
                float num = Vector3.Distance(__instance.bodyCollider.attachedRigidbody.position, grapplePoint);
                joint.maxDistance = num * 0.8f;
                joint.minDistance = num * 0.25f;
                joint.spring = Spring;
                joint.damper = Damper;
                joint.massScale = MassScale;
                lr.positionCount = 2;
            }
        }


        public static void DrawRope(GorillaLocomotion.Player __instance)
        {
            bool flag = !joint;
            if (!flag)
            {
                lr.SetPosition(0, __instance.rightControllerTransform.position);
                lr.SetPosition(1, grapplePoint);
            }
        }


        public static void StopGrapple(GorillaLocomotion.Player __instance)
        {
            lr.positionCount = 0;
            Object.Destroy(joint);
            cangrapple = true;
        }


        public static void ProcessIronMonke()
        {
            Rigidbody RB = GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody;
            if (RightGrip)
            {
                RB.AddForce(8f * GorillaLocomotion.Player.Instance.rightControllerTransform.right,
                    ForceMode.Acceleration);
                GorillaTagger.Instance.StartVibration(false,
                    GorillaTagger.Instance.tapHapticStrength / 50f * RB.velocity.magnitude,
                    GorillaTagger.Instance.tapHapticDuration);
            }

            if (LeftGrip)
            {
                RB.AddForce(-8f * GorillaLocomotion.Player.Instance.leftControllerTransform.right,
                    ForceMode.Acceleration);
                GorillaTagger.Instance.StartVibration(true,
                    GorillaTagger.Instance.tapHapticStrength / 50f * RB.velocity.magnitude,
                    GorillaTagger.Instance.tapHapticDuration);
            }

            if (LeftGrip | RightGrip) RB.velocity = Vector3.ClampMagnitude(RB.velocity, 50f);
        }

        public static void GrappleHook()
        {
            Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position,
    -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out var hit);
            if (InputHandler.RightGrip && !InputHandler.RightTrigger)
            {
                disablegrapple = false;

                if (lineRenderer == null)
                {
                    GameObject ob = new GameObject("line");
                    lineRenderer = ob.AddComponent<LineRenderer>();
                    lineRenderer.startColor = new Color(0, 0, 0, 1f);
                    lineRenderer.endColor = new Color(0, 0, 0, 1f);
                    lineRenderer.startWidth = 0.01f;
                    lineRenderer.endWidth = 0.01f;
                    lineRenderer.positionCount = 2;
                    lineRenderer.useWorldSpace = true;
                    lineRenderer.material.shader = Shader.Find("GUI/Text Shader");
                }
            }

            if (InputHandler.RightGrip && InputHandler.RightTrigger && !disablegrapple)
            {
                if (Vector3.Distance(GorillaLocomotion.Player.Instance.bodyCollider.transform.position, hit.point) < 4)
                {
                    disablegrapple = true;
                    Object.Destroy(lineRenderer);
                    lineRenderer = null;
                    return;
                }

                Vector3 dir2 = (hit.point - GorillaLocomotion.Player.Instance.bodyCollider.transform.position)
                    .normalized * 30;
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(dir2, ForceMode.Acceleration);
            }

            if (!InputHandler.RightGrip && !InputHandler.RightTrigger)
            {
                if (lineRenderer != null)
                {
                    Object.Destroy(lineRenderer);
                }
            }

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, hit.point);
                lineRenderer.SetPosition(1, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
            }
        }

        public static void AirstrikeGun()
        {
            var data = GunLib.Shoot();
            if (data != null)
            {
                if (data.isShooting && data.isTriggered)
                {
                    GorillaLocomotion.Player.Instance.transform.position = data.hitPosition;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = new Vector3(0f, 55f, 0f);
                }
            }
        }


        public static void SuperMonkey()
        {
            if (RightPrimary)
            {
                GorillaLocomotion.Player.Instance.transform.position +=
                    (GorillaLocomotion.Player.Instance.rightControllerTransform.forward * Time.deltaTime) *
                    ((12f) * flightMultiplier);
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                if (!flying)
                {
                    flying = true;
                }
            }
            else if (flying)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity =
                    (GorillaLocomotion.Player.Instance.headCollider.transform.forward * Time.deltaTime) * 12f;
                flying = false;
            }

            if (RightSecondary)
            {
                if (!gravityToggled &&
                    GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity == true)
                {
                    GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = false;
                    gravityToggled = true;
                }
                else if (!gravityToggled &&
                         GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity == false)
                {
                    GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = true;
                    gravityToggled = true;
                }
            }
            else
            {
                gravityToggled = false;
            }
        }

        public static void NoClip()
        {
            if (LeftTrigger || !XRSettings.isDeviceActive)
            {
                if (!isnoclipped)
                {
                    MeshCollider[] array = Resources.FindObjectsOfTypeAll<MeshCollider>();
                    if (array != null)
                    {
                        foreach (MeshCollider collider in array)
                        {
                            if (collider.enabled == true)
                            {
                                collider.enabled = false;
                            }
                        }
                    }

                    isnoclipped = true;
                }
            }
            else
            {
                if (isnoclipped)
                {
                    MeshCollider[] array = Resources.FindObjectsOfTypeAll<MeshCollider>();
                    if (array != null)
                    {
                        foreach (MeshCollider collider in array)
                        {
                            if (!collider.enabled)
                            {
                                collider.enabled = true;
                            }
                        }
                    }

                    isnoclipped = false;
                }
            }
        }

        public static void DisableNoClip()
        {
            MeshCollider[] array = Resources.FindObjectsOfTypeAll<MeshCollider>();
            if (array != null)
            {
                foreach (MeshCollider collider in array)
                {
                    if (!collider.enabled)
                    {
                        collider.enabled = true;
                    }
                }
            }
        }

        public static void LongArms()
        {
            if (LeftTrigger)
            {
                LongArmsOffset += 0.05f;
            }

            if (RightTrigger)
            {
                LongArmsOffset -= 0.05f;
            }

            if (LeftPrimary)
            {
                GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(-0.02f, 0f, -0.07f);
                GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0.02f, 0f, -0.07f);
                return;
            }

            GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0, LongArmsOffset, 0);
            GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(0, LongArmsOffset, 0);
        }

        public static void SwitchSpeed()
        {
            speedBoostMultiplier = multiplierManager(speedBoostMultiplier);
        }

        public static void SwitchFlight()
        {
            flightMultiplier = multiplierManager(flightMultiplier);
        }

        public static void SwitchWallWalk()
        {
            WallWalkMultiplier = multiplierManager(WallWalkMultiplier);
        }

        public static string getPlats()
        {
            string platforms = "";

            if (MenuPatch.currentPlatform == 0)
            {
                platforms = "Default";
            }
            else if (MenuPatch.currentPlatform == 1)
            {
                platforms = "Invisible";
            }
            else if (MenuPatch.currentPlatform == 2)
            {
                platforms = "Sticky";
            }

            return platforms;
        }

        public static Vector3[] lastLeft = new Vector3[]
{
            Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero,
            Vector3.zero, Vector3.zero, Vector3.zero
};

        public static Vector3[] lastRight = new Vector3[]
        {
            Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero,
            Vector3.zero, Vector3.zero, Vector3.zero
        };


        public static void PunchMod()
        {
            int num = -1;
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {

                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    num++;
                    Vector3 position = vrrig.rightHandTransform.position;
                    Vector3 position2 = GorillaTagger.Instance.offlineVRRig.head.rigTarget.position;
                    if ((double)Vector3.Distance(position, position2) < 0.25)
                    {
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity +=
                            Vector3.Normalize(vrrig.rightHandTransform.position - lastRight[num]) * 10f;
                    }

                    lastRight[num] = vrrig.rightHandTransform.position;
                    if ((double)Vector3.Distance(vrrig.leftHandTransform.position, position2) < 0.25)
                    {
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity +=
                            Vector3.Normalize(vrrig.rightHandTransform.position - lastLeft[num]) * 10f;
                    }

                    lastLeft[num] = vrrig.leftHandTransform.position;
                }
            }
        }


        public static void SpeedBoost(float speedMult, bool Enable)
        {
            if (!Enable)
            {
                GorillaLocomotion.Player.Instance.maxJumpSpeed = 6.5f;
                GorillaLocomotion.Player.Instance.jumpMultiplier = 1.1f;
                return;
            }

            GorillaLocomotion.Player.Instance.maxJumpSpeed = 6.5f * speedMult;
            GorillaLocomotion.Player.Instance.jumpMultiplier = 1.1f * speedMult;
        }

        #endregion

        #region Rope Mods
        public static void SendRopeRPC(Vector3 velocity)
        {
            if (Time.time > ropetimeout + 0.1f)
            {
                ropetimeout = Time.time;
                var ropes = Traverse.Create(RopeSwingManager.instance).Field("ropes").GetValue<Dictionary<int, GorillaRopeSwing>>();
                foreach (var rope in ropes)
                {
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.Key, 1, velocity, true, null);
                }
            }
        }

        public static void RopeFreeze()
        {
            if (Time.time > ropetimeout + 0.1f)
            {
                ropetimeout = Time.time;
                var ropes = Traverse.Create(RopeSwingManager.instance).Field("ropes").GetValue<Dictionary<int, GorillaRopeSwing>>();
                foreach (var rope in ropes)
                {
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.Key, 1, Vector3.zero, true, null);
                }
            }
        }

        public static void RopeUp()
        {
            SendRopeRPC(new Vector3(0, 100, 1));
        }

        public static void FlingOnRope()
        {
            RopeUp();
            RopeDown();
        }

        public static void RopeDown()
        {
            SendRopeRPC(new Vector3(0, -100, 1));
        }

        public static void RopeToSelf()
        {
            if (Time.time > ropetimeout + 0.1f)
            {
                ropetimeout = Time.time;
                var ropes = Traverse.Create(RopeSwingManager.instance).Field("ropes").GetValue<Dictionary<int, GorillaRopeSwing>>();
                foreach (var rope in ropes)
                {
                    Vector3 targetPosition = GorillaLocomotion.Player.Instance.transform.position;
                    Vector3 ropeToCursor = targetPosition - rope.Value.transform.position;
                    float distanceToCursor = ropeToCursor.magnitude;
                    float speed = 9999;
                    float t = Mathf.Clamp01(speed / distanceToCursor);

                    Vector3 newPosition = rope.Value.transform.position + ropeToCursor.normalized * distanceToCursor * t;
                    Vector3 velocity = (newPosition - rope.Value.transform.position).normalized * speed;
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.Key, 1, velocity, true, null);
                }
            }
        }
        #endregion

        public static float getSpeedBoostMultiplier()
        {
            return speedBoostMultiplier;
        }

        public static float getFlightMultiplier()
        {
            return flightMultiplier;
        }

        public static float getWallWalkMultiplier()
        {
            return WallWalkMultiplier;
        }
        public static void RopeGun()
        {
            var data = GunLib.Shoot();
            if (data != null)
            {
                if (data.isShooting && data.isTriggered)
                {
                    if (Time.time > ropetimeout + 0.1f)
                    {
                        ropetimeout = Time.time;
                        foreach (GorillaRopeSwing rope in Object.FindObjectsOfType<GorillaRopeSwing>())
                        {
                            Vector3 targetPosition = data.hitPosition;
                            Vector3 ropeToCursor = targetPosition - rope.transform.position;
                            float distanceToCursor = ropeToCursor.magnitude;
                            float speed = 9999;
                            float t = Mathf.Clamp01(speed / distanceToCursor);

                            Vector3 newPosition =
                                rope.transform.position + ropeToCursor.normalized * distanceToCursor * t;
                            Vector3 velocity = (newPosition - rope.transform.position).normalized * speed;
                            RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4,
                                velocity, true, null);
                        }
                    }
                }
            }
        }

    }
}
