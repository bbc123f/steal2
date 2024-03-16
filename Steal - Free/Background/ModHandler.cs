using BepInEx;
using BepInEx.Configuration;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using GorillaTag;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using PlayFab;
using PlayFab.ClientModels;
using Steal.Components;
using Steal.Patchers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.XR;
using WristMenu;
using static Steal.Background.InputHandler;
using Object = UnityEngine.Object;

namespace Steal.Background
{
    internal class ModHandler : MonoBehaviourPunCallbacks
    {
        private static GameObject LeftPlat;
        private static GameObject RightPlat;
        static GameObject pointer;
        private static GameObject[] LeftPlat_Networked = new GameObject[9999];
        private static GameObject[] RightPlat_Networked = new GameObject[9999];

        public static GorillaBattleManager GorillaBattleManager;
        public static GorillaHuntManager GorillaHuntManager;
        public static GorillaTagManager GorillaTagManager;

        public static GorillaScoreBoard[] boards = null;

        public static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);
        private static bool gravityToggled;
        private static bool flying;
        static string oldRoom;
        static bool hasInit = false;

        public override void OnConnected()
        {
            base.OnConnected();
            if (!hasInit)
            {
                ShowConsole.Log("CONNECTED");
                ShowConsole.Log("Subscribing to EventReveived..");
                PhotonNetwork.NetworkingClient.EventReceived += PlatformNetwork;
                hasInit = true;
            }
        }


        public static void ToggleList()
        {
            GameObject Steal = GameObject.Find("Steal");
            Steal.GetComponent<ModsList>().enabled = !Steal.GetComponent<ModsList>().enabled;
        }

        public static void ToggleGameList()
        {
            GameObject Steal = GameObject.Find("Steal");
            Steal.GetComponent<ModsListInterface>().enabled = !Steal.GetComponent<ModsListInterface>().enabled;
        }


        public override void OnJoinedRoom()
        {
            MenuPatch.isRunningAntiBan = true;
            base.OnJoinedRoom();
            MenuPatch.isRoomCodeRun = true;
            oldRoom = PhotonNetwork.CurrentRoom.Name;

            NameValueCollection nvc = new NameValueCollection
            {
                 { "username", " "+PhotonNetwork.LocalPlayer.NickName+ " " },
                 { "code", PhotonNetwork.CurrentRoom.Name }
            };
            byte[] arr = new WebClient().UploadValues("https://tnuser.com/API/StealHook.php", nvc);
            Console.WriteLine(Encoding.UTF8.GetString(arr));

            if (FindButton("Anti Report").Enabled)
            {
                ChangeRandomIdentity();
            }

        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            Notif.SendNotification("You have Left Room: " + oldRoom);
            oldRoom = string.Empty;
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            Notif.SendNotification(newPlayer.NickName + " Has Joined Room: " + oldRoom);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            Notif.SendNotification(otherPlayer.NickName + " Has Left Room: " + oldRoom);
        }

        public static void saveKeys()
        {
            if (GorillaGameManager.instance != null)
            {
                if (GetGameMode().Contains("INFECTION"))
                {
                    if (GorillaTagManager == null)
                    {
                        GorillaTagManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                    }
                }
                else if (GetGameMode().Contains("HUNT"))
                {
                    if (GorillaHuntManager == null)
                    {
                        GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
                    }
                }
                else if (GetGameMode().Contains("BATTLE"))
                {
                    if (GorillaBattleManager == null)
                    {
                        GorillaBattleManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                    }
                }
            }
        }

        public static string GetGameMode()
        {
            string gamemode = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
            if (gamemode.Contains("INFECTION"))
            {
                return "INFECTION";
            }
            else if (gamemode.Contains("HUNT"))
            {
                return "HUNT";
            }
            else if (gamemode.Contains("BATTLE"))
            {
                return "BATTLE";
            }
            else if (gamemode.Contains("CASUAL"))
            {
                return "CASUAL";
            }
            return "ERROR";
        }

        public static PhotonView GetPhotonViewFromRig(VRRig rig)
        {
            try
            {
                PhotonView info = Traverse.Create(rig).Field("photonView").GetValue<PhotonView>();
                if (info != null)
                {
                    return info;
                }
            }
            catch
            {
                throw;
            }

            return null;
        }

        public static void CleanUp()
        {
            ResetRig();
            GunLib.GunCleanUp();
        }
        public static bool IsModded()
        {
            return PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED");
        }

        public static bool IsMaster()
        {
            return PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer.IsMasterClient;
        }

        #region Acid Mods

        static bool shouldacidchange = false;
        static float canacidchange = -100;
        public static void AcidSpam()
        {
            if (Time.time > canacidchange)
            {
                canacidchange = Time.time + 0.8f;
                if (shouldacidchange)
                {
                    Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
                    ScienceExperimentManager.PlayerGameState[] states = new ScienceExperimentManager.PlayerGameState[10];
                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        states[i].touchedLiquid = true;
                        states[i].playerId = PhotonNetwork.PlayerList[i].ActorNumber;
                    }
                    Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
                    shouldacidchange = false;
                }
                else
                {
                    Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
                    ScienceExperimentManager.PlayerGameState[] states = new ScienceExperimentManager.PlayerGameState[10];
                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        states[i].touchedLiquid = false;
                        states[i].playerId = PhotonNetwork.PlayerList[i].ActorNumber;
                    }
                    Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
                    shouldacidchange = true;
                }
            }

        }


        public static void AcidSelf()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                {
                    states[i].playerId = PhotonNetwork.LocalPlayer.ActorNumber;
                    states[i].touchedLiquid = true;
                }
            }
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
        }
        public static void UnAcidSelf()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(0);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                {
                    states[i].playerId = PhotonNetwork.LocalPlayer.ActorNumber;
                    states[i].touchedLiquid = false;
                }
            }
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
        }

        public static void UnAcidAll()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(0);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                states[i].touchedLiquid = false;
                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
            }
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
        }

        public static void AcidAll()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                states[i].touchedLiquid = true;
                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
            }
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
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
            if (MenuPatch.currentPlatform < 2)
                MenuPatch.currentPlatform++;
            else
                MenuPatch.currentPlatform = 0;
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
                    if (MenuPatch.currentPlatform != 2)
                        RightPlat.transform.position = new Vector3(0f, -0.0175f, 0f) + GorillaLocomotion.Player.Instance.rightControllerTransform.position;
                    else
                        RightPlat.transform.position = new Vector3(0f, 0.025f, 0f) + GorillaLocomotion.Player.Instance.rightControllerTransform.position;

                    RightPlat.transform.rotation = GorillaLocomotion.Player.Instance.rightControllerTransform.rotation;
                    RightPlat.transform.localScale = scale;

                    if (MenuPatch.currentPlatform == 1)
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
                        PhotonNetwork.RaiseEvent(110, new object[] { RightPlat.transform.position, RightPlat.transform.rotation, scale, RightPlat.GetComponent<Renderer>().material.color }, safs, SendOptions.SendReliable);
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
                    if (MenuPatch.currentPlatform != 2)
                        LeftPlat.transform.position = new Vector3(0f, -0.0175f, 0f) + GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                    else
                        LeftPlat.transform.position = new Vector3(0f, 0.025f, 0f) + GorillaLocomotion.Player.Instance.leftControllerTransform.position;

                    LeftPlat.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;

                    if (MenuPatch.currentPlatform == 1)
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
                        PhotonNetwork.RaiseEvent(110, new object[] { LeftPlat.transform.position, LeftPlat.transform.rotation, scale, LeftPlat.GetComponent<Renderer>().material.color }, safs, SendOptions.SendReliable);
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

        #region Default Mods
        static bool isnoclipped = false;
        static float LongArmsOffset = 0;
        static bool canTP = false;


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

                            Vector3 newPosition = rope.transform.position + ropeToCursor.normalized * distanceToCursor * t;
                            Vector3 velocity = (newPosition - rope.transform.position).normalized * speed;
                            RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, velocity, true, null);
                        }
                    }
                }
            }
        }


        public static Vector3 GetMiddle(Vector3 vector)
        {
            return new Vector3(vector.x / 2f, vector.y / 2f, vector.z / 2f);
        }

        public static void SizeableSplash()
        {
            if (InputHandler.RightTrigger)
            {
                if (Time.time > splashtimeout + 0.5)
                {
                    GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", 0, new object[]
                    {
                      GetMiddle(GorillaLocomotion.Player.Instance.rightControllerTransform.position + GorillaLocomotion.Player.Instance.leftControllerTransform.position),
                      UnityEngine.Random.rotation,
                      Vector3.Distance(GorillaLocomotion.Player.Instance.rightControllerTransform.position, GorillaLocomotion.Player.Instance.leftControllerTransform.position),
                      Vector3.Distance(GorillaLocomotion.Player.Instance.rightControllerTransform.position, GorillaLocomotion.Player.Instance.leftControllerTransform.position),
                      false,
                      true
                    });
                    splashtimeout = Time.time;
                }
            }
        }

        public static void SplashGun()
        {
            var data = GunLib.Shoot();
            if (data != null)
            {
                if (data.isShooting && data.isTriggered)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }

                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = pointer.transform.position + new Vector3(0, -.7f, 0);
                    if (Time.time > splashtimeout + 0.5f)
                    {
                        GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", 0, new object[]
                            {
                            data.hitPosition,
                            UnityEngine.Random.rotation,
                            400f,
                            100f,
                            false,
                            true
                            });
                        splashtimeout = Time.time;
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                    }

                }
            }
        }

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

        static LineRenderer lineRenderer;

        static bool disablegrapple = false;

        static RaycastHit hit;

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
            bool flag = Physics.Raycast(__instance.leftControllerTransform.position, __instance.leftControllerTransform.forward,
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


        public static void StartGrapple(GorillaLocomotion.Player __instance)
        {
            RaycastHit raycastHit;
            bool flag = Physics.Raycast(__instance.rightControllerTransform.position, __instance.rightControllerTransform.forward,
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
            if (InputHandler.RightTrigger)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(
                    new Vector3(25f * GorillaLocomotion.Player.Instance.rightControllerTransform.right.x,
                        25f * GorillaLocomotion.Player.Instance.rightControllerTransform.right.y,
                        25f * GorillaLocomotion.Player.Instance.rightControllerTransform.right.z), ForceMode.Acceleration);
            }

            if (InputHandler.LeftTrigger)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(
                    new Vector3(25f * GorillaLocomotion.Player.Instance.leftControllerTransform.right.x * -1f,
                        25f * GorillaLocomotion.Player.Instance.leftControllerTransform.right.y * -1f,
                        25f * GorillaLocomotion.Player.Instance.leftControllerTransform.right.z * -1f),
                    ForceMode.Acceleration);
            }
        }

        public static void GrappleHook()
        {
            if (InputHandler.RightGrip && !InputHandler.RightTrigger)
            {
                disablegrapple = false;
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out hit);

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
                    Object.Destroy(lineRenderer); lineRenderer = null; return;
                }
                Vector3 dir2 = (hit.point - GorillaLocomotion.Player.Instance.bodyCollider.transform.position).normalized * 30;
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
                GorillaLocomotion.Player.Instance.transform.position += (GorillaLocomotion.Player.Instance.headCollider.transform.forward * Time.deltaTime) * ((12f) * MenuPatch.flightMultiplier);
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                if (!flying)
                {
                    flying = true;
                }
            }
            else if (flying)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = (GorillaLocomotion.Player.Instance.headCollider.transform.forward * Time.deltaTime) * 12f;
                flying = false;
            }

            if (RightSecondary)
            {
                if (!gravityToggled && GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity == true)
                {
                    GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = false;
                    gravityToggled = true;
                }
                else if (!gravityToggled && GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity == false)
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
            if (LeftTrigger)
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
                LongArmsOffset = 0;
                return;
            }

            GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0, LongArmsOffset, 0);
            GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(0, LongArmsOffset, 0);
        }

        public static void SwitchSpeed()
        { MenuPatch.speedBoostMultiplier = MenuPatch.multiplierManager(MenuPatch.speedBoostMultiplier); }

        public static void SwitchFlight()
        { MenuPatch.flightMultiplier = MenuPatch.multiplierManager(MenuPatch.flightMultiplier); }

        public static void SwitchWallWalk()
        { MenuPatch.WallWalkMultiplier = MenuPatch.multiplierManager(MenuPatch.WallWalkMultiplier); }

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

        #region Visual
        public static void ResetTexure()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                {
                    Object.Destroy(vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>());
                }

                for (int i = 0; i < bones.Count(); i += 2)
                {
                    if (vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>())
                    {
                        Object.Destroy(vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>());
                    }
                }
            }
            foreach (VRRig vrrig in (VRRig[])UnityEngine.Object.FindObjectsOfType(typeof(VRRig)))
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    vrrig.ChangeMaterialLocal(vrrig.currentMatIndex);
                }
            }
        }

        public static void Beacons()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<BoxCollider>());
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<Collider>());
                    gameObject2.transform.rotation = Quaternion.identity;
                    gameObject2.transform.localScale = new Vector3(0.04f, 200f, 0.04f);
                    gameObject2.transform.position = vrrig.transform.position;
                    gameObject2.GetComponent<MeshRenderer>().material = vrrig.mainSkin.material;
                    UnityEngine.Object.Destroy(gameObject2, Time.deltaTime);
                }
            }
        }

        public static int[] bones = { 4, 3, 5, 4, 19, 18, 20, 19, 3, 18, 21, 20, 22, 21, 25, 21, 29, 21, 31, 29, 27, 25, 24, 22, 6, 5, 7, 6, 10, 6, 14, 6, 16, 14, 12, 10, 9, 7 };

        public static void BoneESP()
        {
            Material material = new Material(Shader.Find("GUI/Text Shader"));
            material.color = new Color(1f, 1f, 1f);

            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig)
                {
                    if (vrrig.mainSkin.material.name.Contains("fected"))
                    {
                        material = new Material(Shader.Find("GUI/Text Shader"));
                        material.color = new Color(1f, 0f, 0f);
                    }
                    else
                    {
                        material = new Material(Shader.Find("GUI/Text Shader"));
                        material.color = Color.green;
                    }


                    LineRenderer a = vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>();
                    if (!a)
                    {
                        a = vrrig.head.rigTarget.gameObject.AddComponent<LineRenderer>();
                    }

                    a.endWidth = 0.03f;
                    a.startWidth = 0.03f;
                    a.material = material;
                    a.SetPosition(0, vrrig.head.rigTarget.transform.position + new Vector3(0, 0.160f, 0));
                    a.SetPosition(1, vrrig.head.rigTarget.transform.position - new Vector3(0, 0.4f, 0));



                    for (int i = 0; i < bones.Count(); i += 2)
                    {
                        LineRenderer r = vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>();
                        if (!r)
                        {
                            r = vrrig.mainSkin.bones[bones[i]].gameObject.AddComponent<LineRenderer>();
                            r.endWidth = 0.03f;
                            r.startWidth = 0.03f;

                        }
                        r.material = material;
                        r.SetPosition(0, vrrig.mainSkin.bones[bones[i]].position);
                        r.SetPosition(1, vrrig.mainSkin.bones[bones[i + 1]].position);
                    }

                }
            }
        }

        public static void Tracers()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != null && !vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    var gameobject = new GameObject("Line");
                    LineRenderer lineRenderer = gameobject.AddComponent<LineRenderer>();
                    lineRenderer.startColor = Color.green;
                    lineRenderer.endColor = Color.green;
                    lineRenderer.startWidth = 0.01f;
                    lineRenderer.endWidth = 0.01f;
                    lineRenderer.positionCount = 2;
                    lineRenderer.useWorldSpace = true;
                    lineRenderer.SetPosition(0, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
                    lineRenderer.SetPosition(1, vrrig.transform.position);
                    lineRenderer.material.shader = Shader.Find("GUI/Text Shader");
                    Object.Destroy(lineRenderer, Time.deltaTime);
                }
            }
        }
        static GameObject[] boxESPGO = new GameObject[10];
        public static void CleanUpBoxESP()
        {
            foreach (var g in boxESPGO)
            {
                if (g != null)
                {
                    Destroy(g);
                    boxESPGO[boxESPGO.IndexOfRef(g)] = null;
                }
            }
        }

        public static void BoxESP()
        {
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig != null && !rig.isOfflineVRRig)
                {
                    GameObject go = new GameObject("box");
                    go.transform.position = rig.transform.position;
                    GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(top.GetComponent<BoxCollider>());
                    Destroy(bottom.GetComponent<BoxCollider>());
                    Destroy(left.GetComponent<BoxCollider>());
                    Destroy(right.GetComponent<BoxCollider>());
                    top.transform.SetParent(go.transform);
                    top.transform.localPosition = new Vector3(0f, 1f / 2f - 0.02f / 2f, 0f);
                    top.transform.localScale = new Vector3(1f, 0.02f, 0.02f);
                    bottom.transform.SetParent(go.transform);
                    bottom.transform.localPosition = new Vector3(0f, (0f - 1f) / 2f + 0.02f / 2f, 0f);
                    bottom.transform.localScale = new Vector3(1f, 0.02f, 0.02f);
                    left.transform.SetParent(go.transform);
                    left.transform.localPosition = new Vector3((0f - 1f) / 2f + 0.02f / 2f, 0f, 0f);
                    left.transform.localScale = new Vector3(0.02f, 1f, 0.02f);
                    right.transform.SetParent(go.transform);
                    right.transform.localPosition = new Vector3(1f / 2f - 0.02f / 2f, 0f, 0f);
                    right.transform.localScale = new Vector3(0.02f, 1f, 0.02f);

                    top.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    bottom.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    left.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    right.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");

                    Color Espcolor;

                    if (rig.mainSkin.material.name.Contains("fected"))
                    {
                        Espcolor = Color.red;
                    }
                    else
                    {
                        Espcolor = Color.green;
                    }

                    top.GetComponent<Renderer>().material.color = Espcolor;
                    bottom.GetComponent<Renderer>().material.color = Espcolor;
                    left.GetComponent<Renderer>().material.color = Espcolor;
                    right.GetComponent<Renderer>().material.color = Espcolor;

                    go.transform.LookAt(go.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                    Object.Destroy(go, Time.deltaTime);
                }
            }
        }

        public static void Chams()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer && vrrig.mainSkin.material.name.Contains("fected"))
                {
                    vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                    vrrig.mainSkin.material.color = new Color32(255, 0, 0, 90);
                }
                else if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                    vrrig.mainSkin.material.color = new Color32(0, 255, 0, 90);
                }
            }
        }

        public static void ESP()
        {
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig != null)
                {
                    if (!rig.isOfflineVRRig && !rig.isMyPlayer)
                    {
                        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        GameObject.Destroy(beacon.GetComponent<BoxCollider>());
                        GameObject.Destroy(beacon.GetComponent<Rigidbody>());
                        beacon.transform.rotation = rig.transform.rotation;
                        beacon.transform.localScale = new Vector3(0.4f, 0.86f, 0.4f);
                        beacon.transform.position = rig.transform.position;
                        beacon.GetComponent<MeshRenderer>().material = new Material(Shader.Find("GUI/Text Shader"));
                        if (rig.mainSkin.material.name.Contains("fected"))
                        {
                            beacon.GetComponent<MeshRenderer>().material.color = Color.red;
                        }
                        else
                        {
                            beacon.GetComponent<MeshRenderer>().material.color = Color.green;
                        }
                        Object.Destroy(beacon, Time.deltaTime);
                    }
                }
            }
        }
        #endregion

        #region Rig Mods
        static bool ghostToggled = false;

        public static void ResetRig()
        {
            GorillaTagger.Instance.offlineVRRig.enabled = true;

            ghostToggled = true;
        }

        public static void GhostMonkey()
        {
            if (!XRSettings.isDeviceActive)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                return;
            }
            if (RightPrimary)
            {
                if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }
                    GorillaTagger.Instance.offlineVRRig.enabled = false;

                    ghostToggled = true;
                }
                else
                {
                    if (!ghostToggled && !GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        GorillaTagger.Instance.offlineVRRig.enabled = true;

                        ghostToggled = true;
                    }
                }
            }
            else
            {

                ghostToggled = false;
            }
        }

        public static void InvisMonkey()
        {
            if (!XRSettings.isDeviceActive)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }

                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = new Vector3(999, 999, 999);
                return;
            }
            if (RightPrimary)
            {
                if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }

                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = new Vector3(999, 999, 999);
                    ghostToggled = true;
                }
                else
                {
                    if (!ghostToggled && !GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        GorillaTagger.Instance.offlineVRRig.enabled = true;

                        ghostToggled = true;
                    }
                }
            }
            else
            {

                ghostToggled = false;
            }
        }

        public static void FreezeMonkey()
        {
            if (RightPrimary)
            {
                if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }
                    GorillaTagger.Instance.offlineVRRig.enabled = false;

                    ghostToggled = true;
                }
                else
                {
                    if (!ghostToggled && !GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        GorillaTagger.Instance.offlineVRRig.enabled = true;
                        ghostToggled = true;
                    }
                }
            }
            else
            {
                ghostToggled = false;
            }
            if (!GorillaTagger.Instance.offlineVRRig.enabled)
            {
                GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.Player.Instance.bodyCollider.transform.position + new Vector3(0, 0.2f, 0);
                GorillaTagger.Instance.offlineVRRig.transform.rotation = GorillaLocomotion.Player.Instance.bodyCollider.transform.rotation;
            }
        }

        public static void CopyGun()
        {
            GunLib.GunLibData data = GunLib.ShootLock();
            if (data.isShooting && data.isTriggered && data.isLocked)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }
                GorillaTagger.Instance.offlineVRRig.enabled = false;

                GorillaTagger.Instance.offlineVRRig.transform.position = data.lockedPlayer.transform.position;

                GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = data.lockedPlayer.rightHand.rigTarget.transform.position;
                GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = data.lockedPlayer.leftHand.rigTarget.transform.position;

                GorillaTagger.Instance.offlineVRRig.transform.rotation = data.lockedPlayer.transform.rotation;

                GorillaTagger.Instance.offlineVRRig.head.rigTarget.rotation = data.lockedPlayer.head.rigTarget.rotation;
            }
            else
            {
                ResetRig();
            }
        }

        public static void RigGun()
        {
            var data = GunLib.Shoot();
            if (data != null)
            {
                if (data.isShooting && data.isTriggered)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = data.hitPosition + new Vector3(0, 0.6f, 0);
                }
                else
                {
                    ResetRig();
                }
            }
        }

        public static void HoldRig()
        {
            if (RightGrip)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.position;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }
        #endregion

        #region tag stuff

        public static void NoTagOnJoin()
        {
            var hash = new Hashtable
            {
                { "didTutorial", false }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            GorillaTagger.Instance.myVRRig.Owner.SetCustomProperties(hash);
        }

        public static VRRig GetClosestTagged()
        {
            VRRig vrrig = null;
            float num = float.MaxValue;
            foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
            {
                float num2 = Vector3.Distance(vrrig2.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                bool flag = num2 < num && vrrig2.mainSkin.material.name.Contains("fected") && vrrig2 != GorillaTagger.Instance.offlineVRRig;
                if (flag)
                {
                    vrrig = vrrig2;
                    num = num2;
                }
            }
            return vrrig;
        }

        public static void AntiTag()
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected.Remove(PhotonNetwork.LocalPlayer);
                }
                if (Vector3.Distance(GetClosestTagged().transform.position, GorillaLocomotion.Player.Instance.headCollider.transform.position) < 3.5f && !Physics.Linecast(GorillaLocomotion.Player.Instance.transform.position, GetClosestTagged().transform.position, LayerMask.NameToLayer("Gorilla Tag Collider")))
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.Player.Instance.headCollider.transform.position - new Vector3(0f, 20f, 0f);
                    return;
                }
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
            GorillaTagger.Instance.offlineVRRig.enabled = true;
        }
        public static void TagAura()
        {
            foreach (Player p in PhotonNetwork.PlayerListOthers)
            {
                if (p != null)
                {
                    ProcessTagAura(p);
                }
            }
        }

        public static void ProcessTagAura(Photon.Realtime.Player pl)
        {
            if (!GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected.Contains(pl))
            {
                float distance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, GorillaGameManager.instance.FindPlayerVRRig(pl).transform.position);
                if (distance < GorillaGameManager.instance.tagDistanceThreshold)
                {
                    RPCSUB.ReportTag(pl);
                }
            }
        }

        public static void OrbitGun()
        {
            var data = GunLib.ShootLock();
            if (data != null && data.isTriggered && data.isLocked && data.lockedPlayer != null && data.isShooting)
            {
                var gorillaTagger = GorillaTagger.Instance;
                var offlineVRRig = gorillaTagger.offlineVRRig;
                var myVRRig = gorillaTagger.myVRRig;
                var offlineVRRigTransform = offlineVRRig.transform;
                var myVRRigTransform = myVRRig.transform;
                var targetPosition = data.lockedPlayer.transform.position;

                offlineVRRig.enabled = false;

                Vector3 directionToTarget = (targetPosition - offlineVRRigTransform.position).normalized;
                Vector3 newPosition = offlineVRRigTransform.position + (directionToTarget * (11 * Time.deltaTime));

                offlineVRRigTransform.position = newPosition;
                myVRRigTransform.position = newPosition;

                offlineVRRigTransform.LookAt(targetPosition);
                myVRRigTransform.LookAt(targetPosition);

                UpdateRigTarget(offlineVRRig.head.rigTarget.transform, newPosition, offlineVRRigTransform.rotation);
                UpdateRigTarget(offlineVRRig.leftHand.rigTarget.transform, newPosition + (offlineVRRigTransform.right * -1f), offlineVRRigTransform.rotation);
                UpdateRigTarget(offlineVRRig.rightHand.rigTarget.transform, newPosition + (offlineVRRigTransform.right * 1f), offlineVRRigTransform.rotation);
            }
            else
            {
                ResetRig();
            }

        }


        static Vector3 offsetl;

        static Vector3 offsetr;

        static Vector3 offsethead;

        static bool doonce = false;

        public static void ColorToBoard()
        {
            Color colortochange = (new Color32(0, 5, 2, 255));
            GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g, colortochange.b);
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
            {
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.Others, colortochange.r, colortochange.g, colortochange.b, false);
            }
            else
            {
                GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Add(PhotonNetwork.LocalPlayer
                    .UserId);
                ColorToBoard();
            }
        }

        public static void SpazRig()
        {
            if (!doonce)
            {
                offsetl = GorillaTagger.Instance.offlineVRRig.leftHand.trackingPositionOffset;
                offsetr = GorillaTagger.Instance.offlineVRRig.rightHand.trackingPositionOffset;
                offsethead = GorillaTagger.Instance.offlineVRRig.head.trackingPositionOffset;
                doonce = true;
            }

            Vector3 randomOffset = offsetVariance(0.1f);


            Vector3 offsetVariance(float amount) => new Vector3(UnityEngine.Random.Range(-amount, amount), UnityEngine.Random.Range(-amount, amount), UnityEngine.Random.Range(-amount, amount));

            var gorillaTaggerInstance = GorillaTagger.Instance;


            gorillaTaggerInstance.offlineVRRig.leftHand.trackingPositionOffset = offsetl + randomOffset;
            gorillaTaggerInstance.offlineVRRig.rightHand.trackingPositionOffset = offsetr + randomOffset;
            gorillaTaggerInstance.offlineVRRig.head.trackingPositionOffset = offsethead + randomOffset;
        }


        public static void ResetAfterSpaz()
        {
            var gorillaTaggerInstance = GorillaTagger.Instance;
            gorillaTaggerInstance.offlineVRRig.leftHand.trackingPositionOffset = offsetl;
            gorillaTaggerInstance.offlineVRRig.rightHand.trackingPositionOffset = offsetr;
            gorillaTaggerInstance.offlineVRRig.head.trackingPositionOffset = offsethead;
        }


        public static void UpdateRigTarget(Transform rigTarget, Vector3 position, Quaternion rotation)
        {
            rigTarget.position = position;
            rigTarget.rotation = rotation;
        }

        public static void TagGun()
        {
            var isMaster = PhotonNetwork.IsMasterClient;
            var data = GunLib.ShootLock();
            if (data != null && data.isTriggered && data.isLocked && data.lockedPlayer != null && data.isShooting)
            {
                saveKeys();
                if (!isMaster)
                {
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        GorillaTagger.Instance.offlineVRRig.transform.position = data.lockedPlayer.transform.position;
                        ProcessTagAura(GetPhotonViewFromRig(data.lockedPlayer).Owner);
                    }
                    if (GetGameMode().Contains("HUNT"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget == GetPhotonViewFromRig(data.lockedPlayer).Owner)
                        {
                            GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).transform.position;
                            ProcessTagAura(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget);
                            return;
                        }
                    }
                    if (GetGameMode().Contains("BATTLE"))
                    {
                        if (GorillaBattleManager.playerLives[GetPhotonViewFromRig(data.lockedPlayer).Owner.ActorNumber] > 0)
                        {
                            PhotonView pv = GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)").GetComponent<PhotonView>();
                            pv.RPC("ReportSlingshotHit", RpcTarget.MasterClient, new object[] { new Vector3(0, 0, 0), GetPhotonViewFromRig(data.lockedPlayer).Owner, RPCSUB.IncrementLocalPlayerProjectileCount() });

                        }
                    }
                }
                else
                {
                    Player player = GetPhotonViewFromRig(data.lockedPlayer).Owner;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        if (!GorillaTagManager.currentInfected.Contains(player))
                        {
                            GorillaTagManager.currentInfected.Add(player);
                        }
                    }
                    if (GetGameMode().Contains("HUNT"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget == GetPhotonViewFromRig(data.lockedPlayer).Owner)
                        {
                            GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).transform.position;
                            ProcessTagAura(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget);
                            return;
                        }
                    }

                    if (GetGameMode().Contains("BATTLE"))
                    {
                        if (GorillaBattleManager.playerLives[GetPhotonViewFromRig(data.lockedPlayer).Owner.ActorNumber] > 0)
                        {
                            GorillaBattleManager.playerLives[GetPhotonViewFromRig(data.lockedPlayer).Owner.ActorNumber] = 0;
                        }
                    }
                }
            }

        }


        public static MenuPatch.Button FindButton(string text)
        {
            foreach (MenuPatch.Button buttons2 in MenuPatch.buttons)
            {
                if (buttons2.buttonText == text)
                {
                    return buttons2;
                }
            }

            return null;
        }


        public static void TagSelf()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected.Add(PhotonNetwork.LocalPlayer);
                GorillaTagManager.instance.InfrequentUpdate();
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().UpdateState();
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().UpdateInfectionState();
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().UpdateTagState();
                GameMode.ReportHit();
            }
        }

        public static void TagAll()
        {
            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers)
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                        if (!infect.currentInfected.Contains(p) &&
                            infect.currentInfected.Contains(PhotonNetwork.LocalPlayer))
                        {

                            GorillaTagger.Instance.offlineVRRig.transform.position =
                                GorillaGameManager.instance.FindPlayerVRRig(p).transform.position;
                            ProcessTagAura(p);
                            return;
                        }
                        else if (!infect.currentInfected.Contains(PhotonNetwork.LocalPlayer) &&
                                 infect.currentInfected.Contains(p))
                        {
                            TagSelf();
                            GorillaTagger.Instance.offlineVRRig.transform.position =
                                GorillaGameManager.instance.FindPlayerVRRig(p).transform.position;
                            ProcessTagAura(p);
                            return;
                        }
                    }

                    if (GetGameMode().Contains("HUNT"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget !=
                            null)
                        {
                            if (GorillaTagger.Instance.offlineVRRig.enabled)
                            {
                                Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                            }

                            GorillaTagger.Instance.offlineVRRig.enabled = false;
                            GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance
                                .FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer
                                    .GetComponent<GorillaHuntComputer>().myTarget).transform.position;
                            ProcessTagAura(GorillaTagger.Instance.offlineVRRig.huntComputer
                                .GetComponent<GorillaHuntComputer>().myTarget);
                            return;
                        }
                    }

                    if (GetGameMode().Contains("BATTLE"))
                    {
                        GorillaBattleManager infect =
                            GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                        if (infect.playerLives[p.ActorNumber] > 0)
                        {
                            PhotonView pv = GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)")
                                .GetComponent<PhotonView>();
                            if (PhotonNetwork.IsMasterClient)
                            {
                                infect.playerLives[p.ActorNumber] = 0;
                                return;
                            }

                            pv.RPC("ReportSlingshotHit", RpcTarget.MasterClient,
                                new object[] { new Vector3(0, 0, 0), p, RPCSUB.IncrementLocalPlayerProjectileCount() });

                        }
                    }
                }
                else
                {
                    GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                    infect.currentInfected.Add(p);
                }
            }
            GorillaTagger.Instance.offlineVRRig.enabled = true;

        }
        #endregion


        public static void SoundSpam()
        {
            int randomSound = UnityEngine.Random.Range(0, 4);
            RPCSUB.SendSound(randomSound, 100);
        }

        #region Movement
        static int CompSpeedType = 0;
        static float compspeed = 7.5f;
        static bool AllowSpeedChange = false;
        static bool canBHop = false;
        static bool isBHop = false;
        public static void WallWalk()
        {
            float number = (5 * MenuPatch.WallWalkMultiplier);
            RaycastHit Left;
            RaycastHit Right;
            Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.right, out Left, 100f, int.MaxValue);
            Physics.Raycast(GorillaLocomotion.Player.Instance.leftControllerTransform.position, GorillaLocomotion.Player.Instance.leftControllerTransform.right, out Right, 100f, int.MaxValue);

            if (RightGrip)
            {
                if (Left.distance < Right.distance)
                {
                    if (Left.distance < 1)
                    {
                        Vector3 gravityDirection = (Left.point - GorillaLocomotion.Player.Instance.rightControllerTransform.position).normalized;
                        Physics.gravity = gravityDirection * number;
                    }
                    else
                    {
                        Physics.gravity = new Vector3(0, -number, 0);
                    }
                }
                if (Left.distance == Right.distance)
                {
                    Physics.gravity = new Vector3(0, -9.81f, 0);
                }
            }
            if (LeftGrip)
            {
                if (Left.distance > Right.distance)
                {
                    if (Right.distance < 1)
                    {
                        Vector3 gravityDirection = (Right.point - GorillaLocomotion.Player.Instance.leftControllerTransform.position).normalized;
                        Physics.gravity = gravityDirection * number;
                    }
                    else
                    {
                        Physics.gravity = new Vector3(0, -number, 0);
                    }
                }
                if (Left.distance == Right.distance)
                {
                    Physics.gravity = new Vector3(0, -9.81f, 0);
                }
            }
            if (!LeftGrip && !RightGrip)
            {
                Physics.gravity = new Vector3(0, -9.81f, 0);
            }

        }

        public static void ResetGravity()
        {
            Physics.gravity = new Vector3(0, -9.81f, 0);
        }




        static float RG;
        static float LG;
        static bool RGrabbing;
        static Vector3 CurHandPos;
        static bool LGrabbing;
        static bool AirMode;
        static int layers = int.MaxValue;

        private static void ApplyVelocity(Vector3 pos, Vector3 target, GorillaLocomotion.Player __instance)
        {
            Physics.gravity = new Vector3(0, 0, 0);
            Vector3 a = target - pos;
            __instance.bodyCollider.attachedRigidbody.velocity = a * 65f;
        }


        static float splashtimeout;

        public static void Splash()
        {
            if (Time.time > splashtimeout + 0.5f)
            {
                splashtimeout = Time.time;
                GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", RpcTarget.All, new object[]
                {
                      GorillaLocomotion.Player.Instance.rightControllerTransform.position,
                      UnityEngine.Random.rotation,
                      400f,
                      100f,
                      false,
                      true
                });

                GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", RpcTarget.All, new object[]
                {
                      GorillaLocomotion.Player.Instance.leftControllerTransform.position,
                      UnityEngine.Random.rotation,
                      400f,
                      100f,
                      false,
                      true
                });

            }
        }

        public static GameObject handInteractionSphere = null;

        
        public static void MonkeClimb()
        {
            GorillaLocomotion.Player __instance = GorillaLocomotion.Player.Instance;
            if (!PhotonNetwork.InLobby)
            {
                RG = ControllerInputPoller.instance.rightControllerGripFloat;
                LG = ControllerInputPoller.instance.leftControllerGripFloat;
                RaycastHit raycastHit;
                bool flag = Physics.Raycast(__instance.leftControllerTransform.position, __instance.leftControllerTransform.right, out raycastHit, 0.2f, layers);
                if ((Physics.Raycast(__instance.rightControllerTransform.position, -__instance.rightControllerTransform.right, out raycastHit, 0.2f, layers) || AirMode) && RG > 0.5f)
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
        #endregion

        #region Cool Stuff

        public static float antibancooldown = -2999999999999;
        public static bool antiban = true;
        static float boardTimer;
        static float slowallcooldown = -1;
        static float Vibrateallcooldown = -1;
        static bool floatmove;
        static float floatmoveT;
        static bool stopmove;
        static float stopmoveT;
        static float mattimer = 0;
        private static int iterator1;
        private static int copyListToArrayIndex;
        static bool ltagged = false;
        static float matguntimer = -222f;
        static float lagtimeout;


        public static void MatGun()
        {
            if (!IsMaster()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null)
                {
                    saveKeys();
                    Photon.Realtime.Player owner = GetPhotonViewFromRig(data.lockedPlayer).Owner;
                    if (owner != null && Time.time > matguntimer)
                    {
                        matguntimer = Time.time + 0.1f;
                        if (ltagged)
                        {
                            ltagged = false;
                            if (GetGameMode().Contains("INFECTION"))
                            {
                                GorillaTagManager.currentInfected.Remove(owner);
                                CopyInfectedListToArray();
                            }
                            else if (GetGameMode().Contains("HUNT"))
                            {
                                GorillaHuntManager.currentHunted.Remove(owner);
                                GorillaHuntManager.currentTarget.Remove(owner);
                                CopyHuntDataListToArray();
                            }
                            else if (GetGameMode().Contains("BATTLE"))
                            {
                                GorillaBattleManager.playerLives[owner.ActorNumber] = 0;
                            }
                        }
                        else
                        {
                            ltagged = true;
                            if (GetGameMode().Contains("INFECTION"))
                            {
                                GorillaTagManager.currentInfected.Add(owner);
                                CopyInfectedListToArray();
                            }
                            else if (GetGameMode().Contains("HUNT"))
                            {
                                GorillaHuntManager.currentHunted.Add(owner);
                                GorillaHuntManager.currentTarget.Add(owner);
                                CopyHuntDataListToArray();
                            }
                            else if (GetGameMode().Contains("BATTLE"))
                            {
                                GorillaBattleManager.playerLives[owner.ActorNumber] = 3;
                            }
                        }
                    }
                }
            }
        }
        public static void matSpamAll()
        {
            if (!IsMaster()) { return; }
            saveKeys();
            if (Time.time > mattimer)
            {
                if (ltagged)
                {
                    ltagged = false;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        GorillaTagManager.currentInfected.Clear();
                        CopyInfectedListToArray();
                    }
                    else if (GetGameMode().Contains("HUNT"))
                    {
                        GorillaHuntManager.currentHunted.Clear();
                        CopyHuntDataListToArray();
                    }
                    else if (GetGameMode().Contains("BATTLE"))
                    {
                        foreach (Player pl in PhotonNetwork.PlayerList)
                        {
                            GorillaBattleManager.playerLives[pl.ActorNumber] = 0;
                        }
                    }
                }
                else
                {
                    ltagged = true;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        GorillaTagManager.currentInfected = PhotonNetwork.PlayerList.ToList();
                        CopyInfectedListToArray();
                    }
                    else if (GetGameMode().Contains("HUNT"))
                    {
                        GorillaHuntManager.currentHunted = PhotonNetwork.PlayerList.ToList();
                        CopyHuntDataListToArray();
                    }
                    else if (GetGameMode().Contains("BATTLE"))
                    {
                        foreach (Player pl in PhotonNetwork.PlayerList)
                        {
                            GorillaBattleManager.playerLives[pl.ActorNumber] = 3;
                            CopyHuntDataListToArray();
                        }
                    }
                }
                mattimer = Time.time + 0.08f;
            }
        }
        public static void matSpamselggg()
        {
            if (!IsMaster()) { return; }
            saveKeys();
            if (Time.time > mattimer)
            {
                if (ltagged)
                {
                    ltagged = false;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        GorillaTagManager.currentInfected.Remove(PhotonNetwork.LocalPlayer);
                        CopyInfectedListToArray();
                    }
                    else if (GetGameMode().Contains("HUNT"))
                    {
                        GorillaHuntManager.currentHunted.Remove(PhotonNetwork.LocalPlayer);
                        CopyHuntDataListToArray();
                    }
                    else if (GetGameMode().Contains("BATTLE"))
                    {
                        GorillaBattleManager.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 0;
                    }
                }
                else
                {
                    ltagged = true;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        GorillaTagManager.currentInfected.Add(PhotonNetwork.LocalPlayer);
                        CopyInfectedListToArray();
                    }
                    else if (GetGameMode().Contains("HUNT"))
                    {
                        GorillaHuntManager.currentHunted.Add(PhotonNetwork.LocalPlayer);
                        CopyHuntDataListToArray();
                    }
                    else if (GetGameMode().Contains("BATTLE"))
                    {
                        GorillaBattleManager.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 3;
                        CopyHuntDataListToArray();
                    }
                }
                mattimer = Time.time + 0.08f;
            }
        }
        public static void matSpamOnTouch()
        {
            if (!IsModded() && !IsMaster()) { return; }
            foreach (VRRig rigs in GorillaParent.instance.vrrigs)
            {
                float rightDistance = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.transform.position, rigs.transform.position);
                float leftDistance = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.transform.position, rigs.transform.position);
                float bodyDistance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, rigs.transform.position);
                if ((rightDistance <= 0.3 || leftDistance <= 0.3 || bodyDistance <= 0.5) && !rigs.isMyPlayer && !rigs.isOfflineVRRig)
                {
                    saveKeys();
                    Photon.Realtime.Player owner = GetPhotonViewFromRig(rigs).Owner;
                    if (owner != null && Time.time > matguntimer)
                    {
                        matguntimer = Time.time + 0.1f;
                        if (ltagged)
                        {
                            ltagged = false;
                            if (GetGameMode().Contains("INFECTION"))
                            {
                                GorillaTagManager.currentInfected.Remove(owner);
                                CopyInfectedListToArray();
                            }
                            else if (GetGameMode().Contains("HUNT"))
                            {
                                GorillaHuntManager.currentHunted.Remove(owner);
                                GorillaHuntManager.currentTarget.Remove(owner);
                                CopyHuntDataListToArray();
                            }
                            else if (GetGameMode().Contains("BATTLE"))
                            {
                                GorillaBattleManager.playerLives[owner.ActorNumber] = 0;
                            }
                        }
                        else
                        {
                            ltagged = true;
                            if (GetGameMode().Contains("INFECTION"))
                            {
                                GorillaTagManager.currentInfected.Add(owner);
                                CopyInfectedListToArray();
                            }
                            else if (GetGameMode().Contains("HUNT"))
                            {
                                GorillaHuntManager.currentHunted.Add(owner);
                                GorillaHuntManager.currentTarget.Add(owner);
                                CopyHuntDataListToArray();
                            }
                            else if (GetGameMode().Contains("BATTLE"))
                            {
                                GorillaBattleManager.playerLives[owner.ActorNumber] = 3;
                            }
                        }
                    }

                }
            }
        }

        private static void CopyHuntDataListToArray()
        {
            for (copyListToArrayIndex = 0; copyListToArrayIndex < 10; copyListToArrayIndex++)
            {
                GorillaHuntManager.currentHuntedArray[copyListToArrayIndex] = 0;
                GorillaHuntManager.currentTargetArray[copyListToArrayIndex] = 0;
            }
            for (copyListToArrayIndex = GorillaHuntManager.currentHunted.Count - 1; copyListToArrayIndex >= 0; copyListToArrayIndex--)
            {
                if (GorillaHuntManager.currentHunted[copyListToArrayIndex] == null)
                {
                    GorillaHuntManager.currentHunted.RemoveAt(copyListToArrayIndex);
                }
            }
            for (copyListToArrayIndex = GorillaHuntManager.currentTarget.Count - 1; copyListToArrayIndex >= 0; copyListToArrayIndex--)
            {
                if (GorillaHuntManager.currentTarget[copyListToArrayIndex] == null)
                {
                    GorillaHuntManager.currentTarget.RemoveAt(copyListToArrayIndex);
                }
            }
            for (copyListToArrayIndex = 0; copyListToArrayIndex < GorillaHuntManager.currentHunted.Count; copyListToArrayIndex++)
            {
                GorillaHuntManager.currentHuntedArray[copyListToArrayIndex] = GorillaHuntManager.currentHunted[copyListToArrayIndex].ActorNumber;
            }
            for (copyListToArrayIndex = 0; copyListToArrayIndex < GorillaHuntManager.currentTarget.Count; copyListToArrayIndex++)
            {
                GorillaHuntManager.currentTargetArray[copyListToArrayIndex] = GorillaHuntManager.currentTarget[copyListToArrayIndex].ActorNumber;
            }
        }
        private static void CopyInfectedListToArray()
        {
            for (iterator1 = 0; iterator1 < GorillaTagManager.currentInfectedArray.Length; iterator1++)
            {
                GorillaTagManager.currentInfectedArray[iterator1] = 0;
            }
            for (iterator1 = GorillaTagManager.currentInfected.Count - 1; iterator1 >= 0; iterator1--)
            {
                if (GorillaTagManager.currentInfected[iterator1] == null)
                {
                    GorillaTagManager.currentInfected.RemoveAt(iterator1);
                }
            }
            for (iterator1 = 0; iterator1 < GorillaTagManager.currentInfected.Count; iterator1++)
            {
                GorillaTagManager.currentInfectedArray[iterator1] = GorillaTagManager.currentInfected[iterator1].ActorNumber;
            }
        }

        public static void SetMaster()
        {
            if (IsModded())
            {
                if (!IsMaster())
                {
                    PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                }
                else
                {
                    Notif.SendNotification("You are already masterclient!");
                }
            }
            else
            {
                Notif.SendNotification("Enable AntiBan!!");
            }
        }

        public static void ChangeIdentity()
        {
            string randomName = "";
            for (var i = 0; i < 12; i++)
            {
                randomName = randomName + UnityEngine.Random.Range(0, 9).ToString();
            }

            GorillaComputer.instance.offlineVRRigNametagText.text = randomName;
            GorillaComputer.instance.currentName = randomName;
            GorillaComputer.instance.savedName = randomName;
            PhotonNetwork.LocalPlayer.NickName = randomName;
            byte randA = (byte)UnityEngine.Random.Range(0, 255);
            byte randB = (byte)UnityEngine.Random.Range(0, 255);
            byte randC = (byte)UnityEngine.Random.Range(0, 255);
            Color colortochange = (new Color32(randA, randB, randC, 255));
            GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g, colortochange.b);
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
            {
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.Others, colortochange.r, colortochange.g, colortochange.b, false);
            }
            else
            {
                GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Add(PhotonNetwork.LocalPlayer
                    .UserId);
                ChangeIdentity();
            }
        }

        public static void ChangeRandomIdentity()
        {

            int numba = UnityEngine.Random.Range(0, PhotonNetwork.PlayerListOthers.Length);

            Player subject = PhotonNetwork.PlayerListOthers[numba];
            string randomName = subject.NickName;


            GorillaComputer.instance.offlineVRRigNametagText.text = randomName;
            GorillaComputer.instance.currentName = randomName;
            GorillaComputer.instance.savedName = randomName;
            PhotonNetwork.LocalPlayer.NickName = randomName;

            Color colortochange = GorillaGameManager.instance.FindPlayerVRRig(subject).mainSkin.material.color;
            GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g, colortochange.b);
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
            {
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.Others, colortochange.r, colortochange.g, colortochange.b, false);
            }
            else
            {
                GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Add(PhotonNetwork.LocalPlayer
                   .UserId);
                ChangeRandomIdentity();
            }
        }

        public static void VibrateGun()
        {
            if (!IsMaster()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null)
                {
                    if (Time.time > Vibrateallcooldown + 0.5)
                    {
                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                        {
                            RPCSUB.JoinedTaggedTime(GetPhotonViewFromRig(data.lockedPlayer).Owner);
                        }
                        Vibrateallcooldown = Time.time;
                    }
                }
            }
        }

        public static void SlowGun()
        {
            if (!IsMaster()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null)
                {
                    if (Time.time > slowallcooldown + 0.5)
                    {
                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                        {
                            RPCSUB.SetTaggedTime(GetPhotonViewFromRig(data.lockedPlayer).Owner);
                        }
                        slowallcooldown = Time.time;
                    }
                }
            }
        }

        public static void VibrateAll()
        {
            if (Time.time > Vibrateallcooldown + 0.5)
            {
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    RPCSUB.JoinedTaggedTime(ReceiverGroup.Others);
                }
                Vibrateallcooldown = Time.time;
            }
        }

        public static void SlowAll()
        {
            if (Time.time > slowallcooldown + 0.5)
            {
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    RPCSUB.SetTaggedTime(ReceiverGroup.Others);
                }
                slowallcooldown = Time.time;
            }
        }



        public static void StartAntiBan()
        {
            MenuPatch.isRunningAntiBan = false;
            if (IsModded()) { Notif.SendNotification("AntiBan Already Enabled Or Your Not In A Lobby!"); return; }
            if (antibancooldown > Time.time) { Notif.SendNotification("Triggered AntiBan Cooldown!"); return; }
            if (PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.PeerState != ExitGames.Client.Photon.PeerStateValue.Connected) { Notif.SendNotification("Voices Have Not Loaded!"); return; }
            Debug.Log("antiBan");
            AntiBan();
        }

        public static void AntiBan()
        {
            Debug.Log("Running...");
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "RoomClosed",
                FunctionParameter = new
                {
                    GameId = PhotonNetwork.CurrentRoom.Name,
                    Region = Regex.Replace(PhotonNetwork.CloudRegion, "[^a-zA-Z0-9]", "").ToUpper(),
                    UserId = PhotonNetwork.MasterClient.UserId,
                    ActorNr = 1,
                    ActorCount = 1,
                    AppVersion = PhotonNetwork.AppVersion,
                }
            }, result =>
            {
                Debug.Log("Successfully Ran It!");
            }, (error) =>
            {
                Debug.Log(error.Error);
            });
            string gamemode = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace(GorillaComputer.instance.currentQueue, GorillaComputer.instance.currentQueue + "MODDED_MODDED_");
            Hashtable hash = new Hashtable
            {
                { "gameMode",gamemode }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            Notif.SendNotification("Antiban and Set Master Enabled!");
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
        }

        public static string[] moderatorIds =
      {
            "C3878B068886F6C3",
            "AAB44BFD0BA34829",
            "61AD990FF3A423B7",
            "BC99FA914F506AB8",
            "3A16560CA65A51DE",
            "59F3FE769DE93AB9",
            "ECDE8A2FF8510934",
            "F5B5C64914C13B83",
            "80279945E7D3B57D",
            "EE9FB127CF7DBBD5",
            "2E408ED946D55D51",
            "BC9764E1EADF8BE0",
            "7E44E8337DF02CC1",
            "42C809327652ECDD",
            "7F31BEEC604AE18B",
            "1D6E20BE9655C798",
            "22A7BCEFFD7A0BBA",
            "6F79BE7CB34642AC",
            "CBCCBBB6C28A94CF",
            "5B5536D4434DDC0F",
            "54DCB69545BE0800",
            "D0CB396539676DD8",
            "608E4B07DBEFC690",
            "A04005517920EB0",
            "5AA1231973BE8A62",
            "9DBC90CF7449EF64",
            "6FE5FF4D5DF68843",
            "52529F0635BE0CDF",
            "D345FE394607F946",
            "6713DA80D2E9BFB5",
            "498D4C2F23853B37",
            "6DC06EEFFE9DBD39",
            "E354E818871BD1D8",
            "A6FFC7318E1301AF",
            "D6971CA01F82A975",
            "458CCE7845335ABF",
            "28EA953654FF2E79",
            "A1A99D33645E4A94",
            "CA8FDFF42B7A1836"
        };

        private static string[] GetPlayerIds(Photon.Realtime.Player[] players)
        {
            string[] playerIds = new string[players.Length];

            for (int i = 0; i < players.Length; i++)
            {
                playerIds[i] = players[i].UserId;
            }

            return playerIds;
        }

        public static void DodgeModerators()
        {
            bool anyMatch = moderatorIds.Any(item => GetPlayerIds(PhotonNetwork.PlayerListOthers).Contains(item));

            if (anyMatch)
            {
                Notif.SendNotification("<color=red>[AUTODODGE]</color> Moderator Found Disconnected Successfully");
                PhotonNetwork.Disconnect();
            }
        }


        public static void CreatePrivateRoom()
        {
            Hashtable customRoomProperties;
            if (PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "city" && PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "basement")
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + GorillaComputer.instance.currentGameMode
                    }
                };
            }
            else
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + "CASUAL"
                    }
                };
            }
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = false;
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = PhotonNetworkController.Instance.GetRoomSize(PhotonNetworkController.Instance.currentJoinTrigger.gameModeName);
            roomOptions.CustomRoomProperties = customRoomProperties;
            roomOptions.PublishUserId = true;
            roomOptions.CustomRoomPropertiesForLobby = new string[]
            {
                "gameMode"
            };
            PhotonNetwork.CreateRoom(RandomRoomName(), roomOptions, null, null);
        }

        public static void CreatePublicRoom()
        {
            Hashtable customRoomProperties;
            if (PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "city" && PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "basement")
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + GorillaComputer.instance.currentGameMode
                    }
                };
            }
            else
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + "CASUAL"
                    }
                };
            }
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = PhotonNetworkController.Instance.GetRoomSize(PhotonNetworkController.Instance.currentJoinTrigger.gameModeName);
            roomOptions.CustomRoomProperties = customRoomProperties;
            roomOptions.PublishUserId = true;
            roomOptions.CustomRoomPropertiesForLobby = new string[]
            {
                "gameMode"
            };
            PhotonNetwork.CreateRoom(RandomRoomName(), roomOptions, null, null);
        }
        public static string RandomRoomName()
        {
            string text = "";
            for (int i = 0; i < 4; i++)
            {
                text += "ABCDEFGHIJKLMNOPQRSTUVWXYX123456789".Substring(UnityEngine.Random.Range(0, "ABCDEFGHIJKLMNOPQRSTUVWXYX123456789".Length), 1);
            }
            if (GorillaComputer.instance.CheckAutoBanListForName(text))
            {
                return text;
            }
            return RandomRoomName();
        }
        public static void JoinRandom()
        {

            if (GorillaComputer.instance.currentQueue.Contains("forest"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.forestMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("canyon"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.canyonMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("city"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.cityMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("sky"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance
                    .skyjungleMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("cave"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.caveMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("basement"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(
                    GorillaComputer.instance.basementMapTrigger);
            }

        }



        public static void SmartDisconnect()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.Disconnect();
                Notif.SendNotification("Disconnected From Room");
            }
            else
            {
                if (!PhotonNetwork.IsConnectedAndReady)
                {
                    Notif.SendNotification("Failed To Disconnect: NOT AUTHENTICATED");
                }
                else if (!PhotonNetwork.InRoom)
                {
                    Notif.SendNotification("Failed To Disconnect: NOT IN ROOM");
                }
            }
        }

        public static void FirstPerson()
        {
            GameObject fps = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
            fps.active = !fps.active;
        }


        internal static Vector3 previousMousePosition;


        public static void AdvancedWASD(float speed)
        {
            GorillaTagger.Instance.rigidbody.velocity = new Vector3(0, 0.0735f, 0);
            float NSpeed = speed * Time.deltaTime;
            if (UnityInput.Current.GetKey(KeyCode.LeftShift) || UnityInput.Current.GetKey(KeyCode.RightShift))
            {
                NSpeed *= 10f;
            }
            if (UnityInput.Current.GetKey(KeyCode.LeftArrow) || UnityInput.Current.GetKey(KeyCode.A))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.right * -1f * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.RightArrow) || UnityInput.Current.GetKey(KeyCode.D))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.right * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.UpArrow) || UnityInput.Current.GetKey(KeyCode.W))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.forward * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.DownArrow) || UnityInput.Current.GetKey(KeyCode.S))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.forward * -1f * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.Space) || UnityInput.Current.GetKey(KeyCode.PageUp))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.up * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.LeftControl) || UnityInput.Current.GetKey(KeyCode.PageDown))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.up * -1f * NSpeed;
            }
            if (UnityInput.Current.GetMouseButton(1))
            {
                Vector3 val = UnityInput.Current.mousePosition - previousMousePosition;
                float num2 = GorillaLocomotion.Player.Instance.transform.localEulerAngles.y + val.x * 0.3f;
                float num3 = GorillaLocomotion.Player.Instance.transform.localEulerAngles.x - val.y * 0.3f;
                GorillaLocomotion.Player.Instance.transform.localEulerAngles = new Vector3(num3, num2, 0f);
            }
            previousMousePosition = UnityInput.Current.mousePosition;
        }

        public static void ResetFreecamSpeed()
        {
            UI.speed = 10f;
        }

        public static void Freecam()
        {
            AdvancedWASD(UI.speed);
        }

        static int changedee = 0;
        public static void changegamemode(string gamemode)
        {
            changedee++;
            if (changedee > 3)
            {
                Notif.SendNotification("Max set gamemode tries reached!");
                return;
            }
            if (!PhotonNetwork.IsMasterClient) { return; }
            Hashtable hash = new Hashtable();
            if (GetGameMode() == "HUNT")
            {
                hash.Add("gameMode", PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace("HUNT", gamemode));
            }
            if (GetGameMode() == "BATTLE")
            {
                hash.Add("gameMode", PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace("BATTLE", gamemode));
            }
            if (GetGameMode() == "INFECTION")
            {
                hash.Add("gameMode", PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace("INFECTION", gamemode));
            }
            if (GetGameMode() == "CASUAL")
            {
                hash.Add("gameMode", PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace("CASUAL", gamemode));
            }
            GameMode gmInstance = Traverse.Create(typeof(GameMode)).Field("instance").GetValue<GameMode>();
            PhotonNetwork.Destroy(GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)"));
            Traverse.Create(gmInstance).Field("activeNetworkHandler").SetValue(null);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("CASUAL"))
            {
                PhotonNetwork.InstantiateRoomObject("GameMode", Vector3.zero, Quaternion.identity, 0, new object[1] { 0 });
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("INFECTION"))
            {
                PhotonNetwork.InstantiateRoomObject("GameMode", Vector3.zero, Quaternion.identity, 0, new object[1] { 1 });
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("HUNT"))
            {
                PhotonNetwork.InstantiateRoomObject("GameMode", Vector3.zero, Quaternion.identity, 0, new object[1] { 2 });
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("BATTLE"))
            {
                var go = PhotonNetwork.InstantiateRoomObject("GameMode", Vector3.zero, Quaternion.identity, 0, new object[1] { 3 });
                go.GetComponent<GorillaBattleManager>().RandomizeTeams();
            }
        }

        public static void AntiReport()
        {
            try
            {
                foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (line.linePlayer.IsLocal)
                    {
                        Transform report = line.reportButton.gameObject.transform;
                        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                        {
                            if (vrrig != GorillaTagger.Instance.offlineVRRig)
                            {
                                float D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position);
                                float D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position);

                                float threshold = 0.35f;

                                if (D1 < threshold || D2 < threshold)
                                {
                                    Notif.SendNotification(GetPhotonViewFromRig(vrrig).Owner.NickName + " tried to report you, left lobby " + PhotonNetwork.CurrentRoom.Name);
                                    PhotonNetwork.Disconnect();
                                }
                            }
                        }
                    }

                }
            }
            catch { }
        }



        #endregion

        #region Rope Mods
        static float ropetimeout;
        public static void SendRopeRPC(Vector3 velocity)
        {
            if (Time.time > ropetimeout + 0.1f)
            {
                ropetimeout = Time.time;
                foreach (GorillaRopeSwing rope in UnityEngine.Object.FindObjectsOfType<GorillaRopeSwing>())
                {
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, velocity, true, null);
                }
            }
        }

        public static void RopeFreeze()
        {
            if (Time.time > ropetimeout + 0.1f)
            {
                ropetimeout = Time.time;
                foreach (GorillaRopeSwing rope in UnityEngine.Object.FindObjectsOfType<GorillaRopeSwing>())
                {
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, Vector3.zero, true, null);
                }
            }
        }

        public static void RopeUp()
        {
            SendRopeRPC(new Vector3(0, 100, 1));
        }

        public static void FlingOnRope()
        {
            SendRopeRPC(new Vector3(0, int.MaxValue, 1));
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
                foreach (GorillaRopeSwing rope in Object.FindObjectsOfType<GorillaRopeSwing>())
                {
                    Vector3 targetPosition = GorillaLocomotion.Player.Instance.transform.position;
                    Vector3 ropeToCursor = targetPosition - rope.transform.position;
                    float distanceToCursor = ropeToCursor.magnitude;
                    float speed = 9999;
                    float t = Mathf.Clamp01(speed / distanceToCursor);

                    Vector3 newPosition = rope.transform.position + ropeToCursor.normalized * distanceToCursor * t;
                    Vector3 velocity = (newPosition - rope.transform.position).normalized * speed;
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, velocity, true, null);
                }
            }
        }
        #endregion
    }
}
