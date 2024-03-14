using BepInEx;
using BepInEx.Configuration;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using GorillaTag;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using Steal.Attributes;
using Steal.Components;
using Steal.Patchers.Misc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Valve.VR;
using static UnityEngine.UI.GridLayoutGroup;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Steal.Background
{
    internal class MainManager : MonoBehaviourPunCallbacks
    {
        public static MainManager Instance => _instance;

        static MainManager _instance;

        string[] itemKeys = new string[]
        {
            "BASIC SCARF",
            "BIG EYEBROWS",
            "NOSE RING",
            "BASIC EARRINGS",
            "TRIPLE EARRINGS",
            "EYEBROW STUD",
            "SKULL MASK",
            "RIGHT EYEPATCH",
            "LEFT EYEPATCH",
            "DOUBLE EYEPATCH",
            "SURGICAL MASK",
            "ROUND SUNGLASSES",
            "BANANA HAT",
            "CAT EARS",
            "PARTY HAT",
            "USHANKA",
            "SWEATBAND",
            "BASEBALL CAP",
            "PINEAPPLE HAT",
            "WITCH HAT",
            "COCONUT",
            "SUNHAT",
            "TOP HAT",
        };
        string oldRoom;

        Vector3 head_direction;
        Vector3 roll_direction;
        Vector2 left_joystick;
        ConfigEntry<float> Acceleration_con;
        ConfigEntry<float> Max_con;
        ConfigEntry<float> multi;
        float acceleration = 5f;
        float maxs = 10f;
        float distance = 0.35f;
        float multiplier = 1f;
        float speed = 0f;
        float maxDistance = 100f;
        float Spring;
        float Damper;
        float MassScale;
        Vector3 grapplePoint;
        Vector3 leftgrapplePoint;
        SpringJoint joint;
        SpringJoint leftjoint;
        LineRenderer lr;
        LineRenderer leftlr;
        Color grapplecolor = Color.red;
        float triggerpressed;
        float lefttriggerpressed;
        bool wackstart = false;
        bool cangrapple = true;

        public static bool DebugMode = false;

        bool canleftgrapple = true;
        public GorillaBattleManager GorillaBattleManager;
        public GorillaHuntManager GorillaHuntManager;
        public GorillaTagManager GorillaTagManager;
        int layers = int.MaxValue;
        float RG;
        float LG;
        bool RGrabbing;
        bool LGrabbing;
        bool AirMode;
        Vector3 CurHandPos;

        VRRig Tagger;
        VRRig CopingRig;

        float TagAura;
        float cosmeticcooldown;
        float chatgpt;
        float compspeed = 7.5f;

        int ProjType = 0;
        int Projhash = -820530352;
        int ProjGunType = 0;
        int ProjGunhash = -820530352;
        int ProjHaloType = 0;
        int ProjHalohash = -820530352;
        int ProjRainType = 0;
        int ProjRainhash = -820530352;
        int CompSpeedType = 0;


        bool AllowProjChange = false;
        bool AllowSpeedChange = false;

        internal static Vector3 previousMousePosition;

        static GameObject pointer;
        private GameObject LeftPlat;
        private GameObject RightPlat;
        private GameObject[] LeftPlat_Networked = new GameObject[9999];
        private GameObject[] RightPlat_Networked = new GameObject[9999];

        public Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);


        public bool isModded
        {
            get
            {
                return PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED");
            }
        }
        bool[] istagged = new bool[100000];
        bool IsTaggedSelf;

        public void Awake()
        {
            _instance = this;
        }

        public void saveKeys()
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
                    if (this.GorillaHuntManager == null)
                    {
                        this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
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

        public string GetGameMode()
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
        float timerere;
        public void CrashAll()
        {
            if (!PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED")) { return; }
            GorillaParentPatcher.isCrashing = true;
            if (Time.time > timerere)
            {
                timerere = Time.time + 0.05f;
                PhotonNetwork.SendAllOutgoingCommands();
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();

                UnityEngine.Object.Destroy(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.Destroy(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.Destroy(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.Destroy(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.Destroy(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyObject(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyObject(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyObject(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyObject(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyObject(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyObject(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyImmediate(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyImmediate(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyImmediate(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyImmediate(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyImmediate(GorillaTagger.Instance.myVRRig.gameObject);
                UnityEngine.Object.DestroyImmediate(GorillaTagger.Instance.myVRRig.gameObject);
                PhotonNetwork.Destroy(GorillaTagger.Instance.myVRRig.gameObject);
                PhotonNetwork.Destroy(GorillaTagger.Instance.myVRRig.gameObject);
                PhotonNetwork.Destroy(GorillaTagger.Instance.myVRRig.gameObject);
                PhotonNetwork.Destroy(GorillaTagger.Instance.myVRRig.gameObject);

                if (GorillaTagger.Instance.myVRRig.IsNull())
                {
                    typeof(PhotonNetwork).GetMethod("RaiseEventInternal", BindingFlags.Static | BindingFlags.NonPublic).Invoke(typeof(PhotonNetwork), new object[]
                    {
                    202,
                    new Hashtable
                    {
                        { 0, "GorillaPrefabs/Gorilla Player Networked"},
                        { 6, PhotonNetwork.ServerTimestamp },
                        { 7, PhotonNetwork.AllocateViewID(PhotonNetwork.LocalPlayer.ActorNumber) }
                    },
                    new RaiseEventOptions
                    {
                        CachingOption = EventCaching.AddToRoomCacheGlobal
                    },
                    SendOptions.SendUnreliable
                    });

                }
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            antiban = true;
            antibancooldown = Time.time + 2f;
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
            antibancooldown = Time.time + 1;

        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            Notif.SendNotification(otherPlayer.NickName + " Has Left Room: " + oldRoom);
        }

        public void AdvancedWASD(float speed)
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
        public void DisableNetworkTriggers()
        {
            string[] maps =
            {
                "beach", "city", "basement", "clouds", "forest", "mountains", "canyon", "cave"
            };
            string orgstr = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
            for (int i = 0; i < maps.Length; i++)
            {
                orgstr.AddIfNot(maps[i], out string res);
                orgstr = res;
            }
            var hash = new Hashtable
                {
                    { "gameMode", orgstr }
                };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetCustomPropertiesOfRoom(hash);
        }
        public void matSpamAll()
        {
            if (!PhotonNetwork.IsMasterClient) { return; }
            saveKeys();

            if (ltagged)
            {
                ltagged = false;
                if (GetGameMode().Contains("INFECTION"))
                {
                    GorillaTagManager.isCurrentlyTag = true;
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
            StealGUI.mattimer = Time.time + 0.08f;
        }
        private int iterator1;
        private int copyListToArrayIndex;
        private void CopyHuntDataListToArray()
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
        private void CopyInfectedListToArray()
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

        bool ltagged = false;
        public void RandomTryonHats()
        {
            if (Time.time > cosmeticcooldown + 0.1f)
            {
                cosmeticcooldown = Time.time;
                if (GorillaTagger.Instance.myVRRig != null)
                {
                    string[] array2 = new string[] { itemKeys[UnityEngine.Random.Range(0, itemKeys.Length)] };
                    GorillaTagger.Instance.myVRRig.RPC("UpdateCosmetics", RpcTarget.All, new object[]
                    {
                    array2
                    });
                }
            }
        }

        public string RandomRoomName()
        {
            string text = "";
            for (int i = 0; i < 4; i++)
            {
                text += PhotonNetworkController.Instance.roomCharacters.Substring(UnityEngine.Random.Range(0, PhotonNetworkController.Instance.roomCharacters.Length), 1);
            }
            if (GorillaComputer.instance.CheckAutoBanListForName(text))
            {
                return text;
            }
            return this.RandomRoomName();
        }

        public void CreatePublicRoom()
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

        public Vector3 GetMiddle(Vector3 vector)
        {
            return new Vector3(vector.x / 2f, vector.y / 2f, vector.z / 2f);
        }

        public PhotonView GetPhotonViewFromRig(VRRig rig)
        {
            PhotonView info = Traverse.Create(rig).Field("photonView").GetValue<PhotonView>();
            if (info != null)
            {
                return info;
            }

            return null;
        }

        public void ProcessTeleportGun()
        {
            if (Input.RightGrip && !Input.RightTrigger)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit, 500, GorillaLocomotion.Player.Instance.locomotionEnabledLayers) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = Color.green;
                }
                pointer.transform.position = raycastHit.point;
                pointer.GetComponent<Renderer>().material.color = Color.green;
                return;
            }
            if (Input.RightTrigger && Input.RightGrip)
            {
                pointer.GetComponent<Renderer>().material.color = Color.red;
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                GorillaLocomotion.Player.Instance.transform.position = pointer.transform.position;
            }
            if (!Input.RightTrigger)
            {
                pointer.GetComponent<Renderer>().material.color = Color.green;
            }
            if (!Input.RightGrip)
            {
                if (pointer != null)
                {
                    UnityEngine.GameObject.Destroy(pointer);
                }
            }
        }

        public void ProcessCheckPoint()
        {
            if (Input.RightGrip)
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
            if (!Input.RightGrip && Input.RightTrigger)
            {
                pointer.GetComponent<Renderer>().material.color = Color.green;
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                GorillaLocomotion.Player.Instance.transform.position = pointer.transform.position;
            }
            if (!Input.RightTrigger)
            {
                pointer.GetComponent<Renderer>().material.color = Color.red;
            }
        }

        public void MagicMonkey()
        {
            if (Input.RightGrip && !Input.RightTrigger)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                {
                    pointer.GetComponent<Renderer>().material.color = Color.red;
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }
                pointer.transform.position = raycastHit.point;
                pointer.GetComponent<Renderer>().material.color = Color.green;
                return;
            }
            if (Input.RightTrigger && Input.RightGrip)
            {
                pointer.GetComponent<Renderer>().material.color = Color.green;
                float step = 32f * Time.deltaTime;
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                GorillaLocomotion.Player.Instance.transform.position = Vector3.MoveTowards(GorillaLocomotion.Player.Instance.transform.position, pointer.transform.position, step);
                return;
            }
            else
            {
                if (pointer != null)
                {
                    pointer.GetComponent<Renderer>().material.color = Color.red;
                    return;
                }
            }
            UnityEngine.GameObject.Destroy(pointer);

        }

        public void PlatformNetwork(EventData data)
        {
            if (data.Code == 110)
            {
                object[] customshit = (object[])data.CustomData;
                RightPlat_Networked[data.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightPlat_Networked[data.Sender].transform.position = (Vector3)customshit[0];
                RightPlat_Networked[data.Sender].transform.rotation = (Quaternion)customshit[1];
                RightPlat_Networked[data.Sender].transform.localScale = (Vector3)customshit[2];
                RightPlat_Networked[data.Sender].GetComponent<BoxCollider>().enabled = false;
            }
            if (data.Code == 120)
            {
                object[] customshit = (object[])data.CustomData;
                LeftPlat_Networked[data.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftPlat_Networked[data.Sender].transform.position = (Vector3)customshit[0];
                LeftPlat_Networked[data.Sender].transform.rotation = (Quaternion)customshit[1];
                LeftPlat_Networked[data.Sender].transform.localScale = (Vector3)customshit[2];
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

        public void Platforms()
        {

            RaiseEventOptions safs = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others
            };
            if (Input.RightGrip)
            {
                if (RightPlat == null)
                {
                    RightPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    RightPlat.transform.position = new Vector3(0f, -0.0175f, 0f) + GorillaLocomotion.Player.Instance.rightControllerTransform.position;
                    RightPlat.transform.rotation = GorillaLocomotion.Player.Instance.rightControllerTransform.rotation;
                    RightPlat.transform.localScale = scale;

                    RightPlat.GetComponent<Renderer>().material.color = Color.yellow;
                    PhotonNetwork.RaiseEvent(110, new object[] { RightPlat.transform.position, RightPlat.transform.rotation, scale }, safs, SendOptions.SendReliable);
                }
            }
            else
            {
                if (RightPlat != null)
                {
                    PhotonNetwork.RaiseEvent(111, null, safs, SendOptions.SendReliable);
                    Destroy(RightPlat);
                    RightPlat = null;
                }
            }
            if (Input.LeftGrip)
            {
                if (LeftPlat == null)
                {
                    LeftPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    LeftPlat.transform.localScale = scale;
                    LeftPlat.transform.position = new Vector3(0f, -0.0175f, 0f) + GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                    LeftPlat.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
                    LeftPlat.GetComponent<Renderer>().material.color = Color.red;
                    PhotonNetwork.RaiseEvent(120, new object[] { LeftPlat.transform.position, LeftPlat.transform.rotation, scale }, safs, SendOptions.SendReliable);
                }
            }
            else
            {
                if (LeftPlat != null)
                {
                    PhotonNetwork.RaiseEvent(121, null, safs, SendOptions.SendReliable);
                    Destroy(LeftPlat);
                    LeftPlat = null;
                }
            }
        }

        private void ProcessTagAura(Photon.Realtime.Player pl)
        {
            saveKeys();
            if (!GorillaTagManager.currentInfected.Contains(pl))
            {
                float distance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, GorillaGameManager.instance.FindPlayerVRRig(pl).transform.position);
                if (distance < GorillaGameManager.instance.tagDistanceThreshold)
                {
                    RPCSUB.ReportTag(pl);
                }
            }
        }

        public void TagAll()
        {
            if (GorillaGameManager.instance != null)
            {
                if (GetGameMode().Contains("INFECTION"))
                {
                    if (this.GorillaTagManager == null)
                    {
                        this.GorillaTagManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                    }
                }
                else if (GetGameMode().Contains("HUNT"))
                {
                    if (this.GorillaHuntManager == null)
                    {
                        this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
                    }
                }
                else if (GetGameMode().Contains("BATTLE"))
                {
                    if (this.GorillaBattleManager == null)
                    {
                        this.GorillaBattleManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                    }
                }
            }
            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers)
            {
                if (GorillaTagManager != null)
                {
                    if (!this.GorillaTagManager.currentInfected.Contains(p))
                    {
                        GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance.FindPlayerVRRig(p).transform.position;
                        ProcessTagAura(p);
                        return;
                    }
                }
                if (GorillaHuntManager != null)
                {
                    if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget != null)
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).transform.position;
                        ProcessTagAura(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget);
                        return;
                    }
                }
                if (GorillaBattleManager != null)
                {
                    if (this.GorillaBattleManager.playerLives[p.ActorNumber] > 0)
                    {
                        PhotonView pv = GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)").GetComponent<PhotonView>();
                        if (PhotonNetwork.IsMasterClient)
                        {
                            this.GorillaBattleManager.playerLives[GetPhotonViewFromRig(Tagger).Owner.ActorNumber] = 0;
                            return;
                        }
                        pv.RPC("ReportSlingshotHit", RpcTarget.MasterClient, new object[] { new Vector3(0, 0, 0), GetPhotonViewFromRig(Tagger).Owner, RPCSUB.IncrementLocalPlayerProjectileCount() });

                    }
                }
            }
            GorillaTagger.Instance.offlineVRRig.enabled = true;
        }

        public void TagGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (Tagger == null)
                {
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (Input.RightTrigger)
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>() != null)
                        {
                            Tagger = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (Input.RightTrigger && Tagger != null)
                {
                    if (GorillaGameManager.instance != null)
                    {
                        if (GetGameMode().Contains("INFECTION"))
                        {
                            if (this.GorillaTagManager == null)
                            {
                                this.GorillaTagManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                            }
                        }
                        else if (GetGameMode().Contains("HUNT"))
                        {
                            if (this.GorillaHuntManager == null)
                            {
                                this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
                            }
                        }
                        else if (GetGameMode().Contains("BATTLE"))
                        {
                            if (this.GorillaBattleManager == null)
                            {
                                this.GorillaBattleManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                            }
                        }
                    }
                    pointer.transform.position = Tagger.transform.position;
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        GorillaTagger.Instance.offlineVRRig.transform.position = Tagger.transform.position;
                        ProcessTagAura(GetPhotonViewFromRig(Tagger).Owner);
                    }
                    if (GetGameMode().Contains("HUNT"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget == GetPhotonViewFromRig(Tagger).Owner)
                        {
                            GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).transform.position;
                            ProcessTagAura(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget);
                            return;
                        }
                    }

                    if (GetGameMode().Contains("BATTLE"))
                    {
                        if (this.GorillaBattleManager.playerLives[GetPhotonViewFromRig(Tagger).Owner.ActorNumber] > 0)
                        {
                            PhotonView pv = GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)").GetComponent<PhotonView>();
                            if (PhotonNetwork.IsMasterClient)
                            {
                                this.GorillaBattleManager.playerLives[GetPhotonViewFromRig(Tagger).Owner.ActorNumber] = 0;
                                return;
                            }
                            pv.RPC("ReportSlingshotHit", RpcTarget.MasterClient, new object[] { new Vector3(0, 0, 0), GetPhotonViewFromRig(Tagger).Owner, RPCSUB.IncrementLocalPlayerProjectileCount() });

                        }
                    }
                    return;
                }
                else
                {
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                    Tagger = null;
                }
                return;
            }
        }

        public void ProcessTagAura()
        {
            if (GorillaGameManager.instance != null)
            {
                if (GetGameMode().Contains("INFECTION"))
                {
                    if (this.GorillaTagManager == null)
                    {
                        this.GorillaTagManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                    }
                }
                else if (GetGameMode().Contains("HUNT"))
                {
                    if (this.GorillaHuntManager == null)
                    {
                        this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
                    }
                }
                else if (GetGameMode().Contains("BATTLE"))
                {
                    if (this.GorillaBattleManager == null)
                    {
                        this.GorillaBattleManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                    }
                }
            }
            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers)
            {
                if (!this.GorillaTagManager.currentInfected.Contains(p))
                {
                    ProcessTagAura(p);
                    return;
                }
            }
        }

        public void BetterLongArms()
        {
            if (Input.RightTrigger)
            {
                GorillaLocomotion.Player.Instance.transform.localScale += new Vector3(0.02f, 0.02f, 0.02f);
                if (GorillaLocomotion.Player.Instance.transform.localScale.x > 2f)
                {
                    GorillaLocomotion.Player.Instance.transform.localScale = new Vector3(2f, 2f, 2f);
                }
            }

            if (Input.LeftTrigger)
            {
                GorillaLocomotion.Player.Instance.transform.localScale -= new Vector3(0.02f, 0.02f, 0.02f);
                if (GorillaLocomotion.Player.Instance.transform.localScale.x < 0.2f)
                {
                    GorillaLocomotion.Player.Instance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                }
            }

            if (Input.LeftPrimary)
            {
                GorillaLocomotion.Player.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }

        public void BoneESP(bool disable)
        {

            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    if (vrrig != null)
                    {
                        if (disable)
                        {
                            for (int i = 0; i < bones.Count(); i += 2)
                            {
                                if (vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>())
                                {
                                    Destroy(vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>());
                                }
                            }
                            return;
                        }
                        Material material = new Material(Shader.Find("GUI/Text Shader"));
                        if (vrrig.mainSkin.material.name.Contains("fected"))
                        {
                            material.color = Color.red;
                        }
                        else
                        {
                            material.color = Color.green;
                        }
                        if (!vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                        {
                            vrrig.head.rigTarget.gameObject.AddComponent<LineRenderer>();
                        }

                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().endWidth = 0.015f;
                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().startWidth = 0.015f;
                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().material = material;
                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().SetPosition(0, vrrig.head.rigTarget.transform.position + new Vector3(0, 0.160f, 0));
                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().SetPosition(1, vrrig.head.rigTarget.transform.position - new Vector3(0, 0.4f, 0));



                        for (int i = 0; i < bones.Count(); i += 2)
                        {
                            if (!vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>())
                            {
                                vrrig.mainSkin.bones[bones[i]].gameObject.AddComponent<LineRenderer>();
                            }
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().endWidth = 0.015f;
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().startWidth = 0.015f;
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().material = material;
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().SetPosition(0, vrrig.mainSkin.bones[bones[i]].position);
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().SetPosition(1, vrrig.mainSkin.bones[bones[i + 1]].position);
                        }


                    }
                }
            }
        }
        public static int[] bones = { 4, 3, 5, 4, 19, 18, 20, 19, 3, 18, 21, 20, 22, 21, 25, 21, 29, 21, 31, 29, 27, 25, 24, 22, 6, 5, 7, 6, 10, 6, 14, 6, 16, 14, 12, 10, 9, 7 };

        public void BoxESP()
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
                    Destroy(go, Time.deltaTime);
                }
            }
        }

        public void HitBoxESP(VRRig rig)
        {

        }

        void HeadHitBox(VRRig rig)
        {
            GameObject go = new GameObject("box");
            go.transform.position = rig.head.headTransform.transform.position;
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
            top.transform.localScale = new Vector3(0.3f, 0.02f, 0.02f);
            bottom.transform.SetParent(go.transform);
            bottom.transform.localPosition = new Vector3(0f, (0f - 1f) / 2f + 0.02f / 2f, 0f);
            bottom.transform.localScale = new Vector3(0.3f, 0.02f, 0.02f);
            left.transform.SetParent(go.transform);
            left.transform.localPosition = new Vector3((0f - 1f) / 2f + 0.02f / 2f, 0f, 0f);
            left.transform.localScale = new Vector3(0.02f, 0.3f, 0.02f);
            right.transform.SetParent(go.transform);
            right.transform.localPosition = new Vector3(1f / 2f - 0.02f / 2f, 0f, 0f);
            right.transform.localScale = new Vector3(0.02f, 0.3f, 0.02f);

            top.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            bottom.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            left.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            right.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");

            go.transform.LookAt(go.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
            Destroy(go, Time.deltaTime);
        }

        void BodyHitBox(VRRig rig)
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
            top.transform.localScale = new Vector3(0.5f, 0.02f, 0.02f);
            bottom.transform.SetParent(go.transform);
            bottom.transform.localPosition = new Vector3(0f, (0f - 1f) / 2f + 0.02f / 2f, 0f);
            bottom.transform.localScale = new Vector3(0.5f, 0.02f, 0.02f);
            left.transform.SetParent(go.transform);
            left.transform.localPosition = new Vector3((0f - 1f) / 2f + 0.02f / 2f, 0f, 0f);
            left.transform.localScale = new Vector3(0.02f, 0.5f, 0.02f);
            right.transform.SetParent(go.transform);
            right.transform.localPosition = new Vector3(1f / 2f - 0.02f / 2f, 0f, 0f);
            right.transform.localScale = new Vector3(0.02f, 0.5f, 0.02f);

            top.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            bottom.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            left.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            right.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");

            go.transform.LookAt(go.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
            Destroy(go, Time.deltaTime);
        }

        public void NoTag()
        {
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig != null && !rig.isOfflineVRRig && !rig.isMyPlayer)
                {
                    if (rig.mainSkin.material.name.Contains("fected"))
                    {
                        if (Vector3.Distance(rig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) < 5)
                        {
                            if (GorillaTagger.Instance.offlineVRRig.enabled)
                            {
                                if (GorillaTagger.Instance.offlineVRRig.enabled)
                                {
                                    Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                                }
                                GorillaTagger.Instance.offlineVRRig.enabled = false;
                                GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.Player.Instance.transform.position - new Vector3(0, 100f, 0);
                            }
                            return;
                        }
                        else
                        {
                            if (!GorillaTagger.Instance.offlineVRRig.enabled)
                            {
                                GorillaTagger.Instance.offlineVRRig.enabled = true;
                            }
                        }
                    }
                }
            }
        }

        public void ProjectileSpam()
        {
            if (Input.RightPrimary)
            {
                if (AllowProjChange)
                {
                    AllowProjChange = false;
                    ProjType++;
                    if (ProjType == 0)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {SlingShot}";
                        Projhash = -820530352;
                        Notif.SendNotification("Changed Projectile: Slingshot");
                    }
                    if (ProjType == 1)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {Waterballoon}";
                        Projhash = -1674517839;
                        Notif.SendNotification("Changed Projectile: Waterballoon");
                    }
                    if (ProjType == 2)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {SnowBall}";
                        Projhash = -675036877;
                        Notif.SendNotification("Changed Projectile: Snowball");
                    }
                    if (ProjType == 3)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {DeadShot}";
                        Projhash = 693334698;
                        Notif.SendNotification("Changed Projectile: DeadShot");
                    }
                    if (ProjType == 4)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {Cloud}";
                        Projhash = 1511318966;
                        Notif.SendNotification("Changed Projectile: Cloud");
                    }
                    if (ProjType == 5)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {Cupid}";
                        Projhash = 825718363;
                        Notif.SendNotification("Changed Projectile: Cupid");
                    }
                    if (ProjType == 6)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {Ice}";
                        Projhash = -1671677000;
                        Notif.SendNotification("Changed Projectile: Ice");
                    }
                    if (ProjType == 7)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {Elf}";
                        Projhash = 1705139863;
                        Notif.SendNotification("Changed Projectile: Elf");
                    }
                    if (ProjType == 8)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {Rock}";
                        Projhash = PoolUtils.GameObjHashCode(GameObject.Find("Environment Objects/PersistentObjects_Prefab/GlobalObjectPools/LavaRockProjectile(Clone)"));
                        Notif.SendNotification("Changed Projectile: Rock");
                    }
                    if (ProjType == 9)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {Spider}";
                        Projhash = -790645151;
                        Notif.SendNotification("Changed Projectile: Spider");
                    }
                    if (ProjType == 10)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {Coal}";
                        Projhash = -1433634409;
                        Notif.SendNotification("Changed Projectile: Coal");
                    }
                    if (ProjType == 11)
                    {
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {CandyCane}";
                        Projhash = 2061412059;
                        Notif.SendNotification("Changed Projectile: CandyCane");
                    }
                    if (ProjType == 12)
                    {
                        int[] hash = new int[]
                        {
                                    -666337545,
                                    -160604350,
                                    -1433633837,
                        };
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {Present}";
                        Projhash = -666337545;
                        Notif.SendNotification("Changed Projectile: Present");
                    }
                    if (ProjType >= 13)
                    {
                        ProjType = 0;
                        Main.Instance.buttons[18].buttontext = "Projectile Spam {SlingShot}";
                        Projhash = -820530352;
                        Notif.SendNotification("Changed Projectile: Slingshot");
                    }

                    Destroy(Main.Instance.menu);
                    Main.Instance.menu = null;
                    Main.Instance.Draw();
                }
            }
            else
            {
                AllowProjChange = true;
            }

            if (Input.RightGrip)
            {
                if (GorillaLocomotion.Player.Instance.rightControllerTransform.gameObject.GetComponent<VelocityTracker>() == null)
                {
                    GorillaLocomotion.Player.Instance.rightControllerTransform.gameObject.AddComponent<VelocityTracker>();
                }
                RPCSUB.SendLaunchProjectile(GorillaLocomotion.Player.Instance.rightControllerTransform.position, GorillaLocomotion.Player.Instance.rightControllerTransform.gameObject.GetComponent<VelocityTracker>().velocity, Projhash, -1, false, false, 1f, 1f, 1f, 1f);

            }
        }

        public void ProjectileGun()
        {
            if (Input.RightPrimary)
            {
                if (AllowProjChange)
                {
                    AllowProjChange = false;
                    ProjGunType++;
                    if (ProjGunType == 0)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {SlingShot}";
                        ProjGunhash = -820530352;
                        Notif.SendNotification("Changed Projectile: Slingshot");
                    }
                    if (ProjGunType == 1)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {Waterballoon}";
                        ProjGunhash = -1674517839;
                        Notif.SendNotification("Changed Projectile: Waterballoon");
                    }
                    if (ProjGunType == 2)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {SnowBall}";
                        ProjGunhash = -675036877;
                        Notif.SendNotification("Changed Projectile: Snowball");
                    }
                    if (ProjGunType == 3)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {DeadShot}";
                        ProjGunhash = 693334698;
                        Notif.SendNotification("Changed Projectile: DeadShot");
                    }
                    if (ProjGunType == 4)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {Cloud}";
                        ProjGunhash = 1511318966;
                        Notif.SendNotification("Changed Projectile: Cloud");
                    }
                    if (ProjGunType == 5)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {Cupid}";
                        ProjGunhash = 825718363;
                        Notif.SendNotification("Changed Projectile: Cupid");
                    }
                    if (ProjGunType == 6)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {Ice}";
                        ProjGunhash = -1671677000;
                        Notif.SendNotification("Changed Projectile: Ice");
                    }
                    if (ProjGunType == 7)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {Elf}";
                        ProjGunhash = 1705139863;
                        Notif.SendNotification("Changed Projectile: Elf");
                    }
                    if (ProjGunType == 8)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {Rock}";
                        ProjGunhash = PoolUtils.GameObjHashCode(GameObject.Find("Environment Objects/PersistentObjects_Prefab/GlobalObjectPools/LavaRockProjectile(Clone)"));
                        Notif.SendNotification("Changed Projectile: Rock");
                    }
                    if (ProjGunType == 9)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {Spider}";
                        ProjGunhash = -790645151;
                        Notif.SendNotification("Changed Projectile: Spider");
                    }
                    if (ProjGunType == 10)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {Coal}";
                        ProjGunhash = -1433634409;
                        Notif.SendNotification("Changed Projectile: Coal");
                    }
                    if (ProjGunType == 11)
                    {
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {CandyCane}";
                        ProjGunhash = 2061412059;
                        Notif.SendNotification("Changed Projectile: CandyCane");
                    }
                    if (ProjGunType == 12)
                    {
                        int[] hash = new int[]
                        {
                                    -666337545,
                                    -160604350,
                                    -1433633837,
                        };
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {Present}";
                        ProjGunhash = -666337545;
                        Notif.SendNotification("Changed Projectile: Present");
                    }
                    if (ProjGunType >= 13)
                    {
                        ProjGunType = 0;
                        Main.Instance.buttons[19].buttontext = "Projectile Gun {SlingShot}";
                        ProjGunhash = -820530352;
                        Notif.SendNotification("Changed Projectile: Slingshot");
                    }

                    Destroy(Main.Instance.menu);
                    Main.Instance.menu = null;
                    Main.Instance.Draw();

                }
            }
            else
            {
                AllowProjChange = true;
            }

            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                Physics.Raycast(GorillaTagger.Instance.rightHandTriggerCollider.transform.position, GorillaTagger.Instance.rightHandTriggerCollider.transform.up, out raycastHit);
                Vector3 position = GorillaTagger.Instance.rightHandTriggerCollider.transform.position;
                Vector3 point = raycastHit.point;
                Vector3 vector = (point - position).normalized;
                float d = 100f;
                vector *= d;
                RPCSUB.SendLaunchProjectile(GorillaTagger.Instance.rightHandTriggerCollider.transform.position, vector, ProjGunhash, -1, false, false, 1, 1, 1, 1);
            }
        }

        public void ProjectileHalo()
        {
            if (Input.RightPrimary)
            {
                if (AllowProjChange)
                {
                    AllowProjChange = false;
                    ProjHaloType++;
                    if (ProjHaloType == 0)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {SlingShot}";
                        ProjHalohash = -820530352;
                        Notif.SendNotification("Changed Projectile: Slingshot");
                    }
                    if (ProjHaloType == 1)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {Waterballoon}";
                        ProjHalohash = -1674517839;
                        Notif.SendNotification("Changed Projectile: Waterballoon");
                    }
                    if (ProjHaloType == 2)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {SnowBall}";
                        ProjHalohash = -675036877;
                        Notif.SendNotification("Changed Projectile: Snowball");
                    }
                    if (ProjHaloType == 3)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {DeadShot}";
                        ProjHalohash = 693334698;
                        Notif.SendNotification("Changed Projectile: DeadShot");
                    }
                    if (ProjHaloType == 4)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {Cloud}";
                        ProjHalohash = 1511318966;
                        Notif.SendNotification("Changed Projectile: Cloud");
                    }
                    if (ProjHaloType == 5)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {Cupid}";
                        ProjHalohash = 825718363;
                        Notif.SendNotification("Changed Projectile: Cupid");
                    }
                    if (ProjHaloType == 6)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {Ice}";
                        ProjHalohash = -1671677000;
                        Notif.SendNotification("Changed Projectile: Ice");
                    }
                    if (ProjHaloType == 7)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {Elf}";
                        ProjHalohash = 1705139863;
                        Notif.SendNotification("Changed Projectile: Elf");
                    }
                    if (ProjHaloType == 8)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {Rock}";
                        ProjHalohash = PoolUtils.GameObjHashCode(GameObject.Find("Environment Objects/PersistentObjects_Prefab/GlobalObjectPools/LavaRockProjectile(Clone)"));
                        Notif.SendNotification("Changed Projectile: Rock");
                    }
                    if (ProjHaloType == 9)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {Spider}";
                        ProjHalohash = -790645151;
                        Notif.SendNotification("Changed Projectile: Spider");
                    }
                    if (ProjHaloType == 10)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {Coal}";
                        ProjHalohash = -1433634409;
                        Notif.SendNotification("Changed Projectile: Coal");
                    }
                    if (ProjHaloType == 11)
                    {
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {CandyCane}";
                        ProjHalohash = 2061412059;
                        Notif.SendNotification("Changed Projectile: CandyCane");
                    }
                    if (ProjHaloType == 12)
                    {
                        int[] hash = new int[]
                        {
                                    -666337545,
                                    -160604350,
                                    -1433633837,
                        };
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {Present}";
                        ProjHalohash = -666337545;
                        Notif.SendNotification("Changed Projectile: Present");
                    }
                    if (ProjHaloType >= 13)
                    {
                        ProjHaloType = 0;
                        Main.Instance.buttons[20].buttontext = "Projectile Halo {SlingShot}";
                        ProjHalohash = -820530352;
                        Notif.SendNotification("Changed Projectile: Slingshot");
                    }

                    Destroy(Main.Instance.menu);
                    Main.Instance.menu = null;
                    Main.Instance.Draw();

                }
            }
            else
            {
                AllowProjChange = true;
            }

            if (Input.RightGrip)
            {
                chatgpt += 21 * Time.deltaTime;
                float x = GorillaTagger.Instance.offlineVRRig.headConstraint.transform.position.x + 0.5f * Mathf.Cos(chatgpt);
                float y = GorillaTagger.Instance.offlineVRRig.headConstraint.transform.position.y + 0.25f;
                float z = GorillaTagger.Instance.offlineVRRig.headConstraint.transform.position.z + 0.5f * Mathf.Sin(chatgpt);
                RPCSUB.SendLaunchProjectile(new Vector3(x, y, z), new Vector3(0, 0, 0), ProjHalohash, -1, false, false, 1f, 1f, 1f, 1f);
            }
        }

        public void ProjectileRain()
        {
            if (Input.RightPrimary)
            {
                if (AllowProjChange)
                {
                    AllowProjChange = false;
                    ProjRainType++;
                    if (ProjRainType == 0)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {SlingShot}";
                        ProjRainhash = -820530352;
                        Notif.SendNotification("Changed Projectile: Slingshot");
                    }
                    if (ProjRainType == 1)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {Waterballoon}";
                        ProjRainhash = -1674517839;
                        Notif.SendNotification("Changed Projectile: Waterballoon");
                    }
                    if (ProjRainType == 2)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {SnowBall}";
                        ProjRainhash = -675036877;
                        Notif.SendNotification("Changed Projectile: Snowball");
                    }
                    if (ProjRainType == 3)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {DeadShot}";
                        ProjRainhash = 693334698;
                        Notif.SendNotification("Changed Projectile: DeadShot");
                    }
                    if (ProjRainType == 4)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {Cloud}";
                        ProjRainhash = 1511318966;
                        Notif.SendNotification("Changed Projectile: Cloud");
                    }
                    if (ProjRainType == 5)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {Cupid}";
                        ProjRainhash = 825718363;
                        Notif.SendNotification("Changed Projectile: Cupid");
                    }
                    if (ProjRainType == 6)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {Ice}";
                        ProjRainhash = -1671677000;
                        Notif.SendNotification("Changed Projectile: Ice");
                    }
                    if (ProjRainType == 7)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {Elf}";
                        ProjRainhash = 1705139863;
                        Notif.SendNotification("Changed Projectile: Elf");
                    }
                    if (ProjRainType == 8)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {Rock}";
                        ProjRainhash = PoolUtils.GameObjHashCode(GameObject.Find("Environment Objects/PersistentObjects_Prefab/GlobalObjectPools/LavaRockProjectile(Clone)"));
                        Notif.SendNotification("Changed Projectile: Rock");
                    }
                    if (ProjRainType == 9)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {Spider}";
                        ProjRainhash = -790645151;
                        Notif.SendNotification("Changed Projectile: Spider");
                    }
                    if (ProjRainType == 10)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {Coal}";
                        ProjRainhash = -1433634409;
                        Notif.SendNotification("Changed Projectile: Coal");
                    }
                    if (ProjRainType == 11)
                    {
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {CandyCane}";
                        ProjRainhash = 2061412059;
                        Notif.SendNotification("Changed Projectile: CandyCane");
                    }
                    if (ProjRainType == 12)
                    {
                        int[] hash = new int[]
                        {
                                    -666337545,
                                    -160604350,
                                    -1433633837,
                        };
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {Present}";
                        ProjRainhash = -666337545;
                        Notif.SendNotification("Changed Projectile: Present");
                    }
                    if (ProjRainType >= 13)
                    {
                        ProjRainType = 0;
                        Main.Instance.buttons[21].buttontext = "Projectile Rain {SlingShot}";
                        ProjRainhash = -820530352;
                        Notif.SendNotification("Changed Projectile: Slingshot");
                    }

                    Destroy(Main.Instance.menu);
                    Main.Instance.menu = null;
                    Main.Instance.Draw();

                }
            }
            else
            {
                AllowProjChange = true;
            }

            if (Input.RightGrip)
            {
                RPCSUB.SendLaunchProjectile(GorillaTagger.Instance.offlineVRRig.transform.position + new Vector3(Random.Range(-3.8f, 3.8f), 5f, Random.Range(-3.8f, 3.8f)), new Vector3(0, 0, 0), ProjRainhash, -1, false, false, 1f, 1f, 1f, 1f);
            }
        }
        public bool following = false;
        public VRRig FollowingPlayer;
        public void FollowPLayer()
        {
            if (FollowingPlayer != null)
            {
                GorillaLocomotion.Player.Instance.transform.position = FollowingPlayer.transform.position + new Vector3(0, 0.13f, 0);
                GorillaLocomotion.Player.Instance.transform.rotation = FollowingPlayer.transform.rotation;

                GorillaLocomotion.Player.Instance.rightControllerTransform.position = FollowingPlayer.rightHandTransform.position;
                GorillaLocomotion.Player.Instance.leftControllerTransform.position = FollowingPlayer.leftHandTransform.position;

                GorillaLocomotion.Player.Instance.rightControllerTransform.rotation = FollowingPlayer.rightHandTransform.rotation;
                GorillaLocomotion.Player.Instance.leftControllerTransform.rotation = FollowingPlayer.leftHandTransform.rotation;
            }
        }

        float splashtimeout;
        public void Splash()
        {
            if (Time.time > splashtimeout + 0.5f)
            {
                if (Input.RightTrigger)
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
                }

                if (Input.LeftTrigger)
                {
                    splashtimeout = Time.time;
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
        }

        public void SizeableSplash()
        {
            if (Input.RightTrigger)
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

        public void SpashGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit, 100f, layers) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }
                pointer.transform.position = raycastHit.point;
                if (Input.RightTrigger)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = pointer.transform.position + new Vector3(0, 0.5f, 0);
                    if (Time.time > splashtimeout + 0.5f)
                    {
                        GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", 0, new object[]
                            {
                            pointer.transform.position,
                            UnityEngine.Random.rotation,
                            400f,
                            100f,
                            false,
                            true
                            });
                        splashtimeout = Time.time;
                    }

                    return;
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);
        }

        public void Piss()
        {
            if (Input.RightTrigger)
            {
                RPCSUB.SendLaunchProjectile(GorillaLocomotion.Player.Instance.bodyCollider.transform.position + new Vector3(0f, -0.1f, 0f),
                            GorillaLocomotion.Player.Instance.bodyCollider.transform.forward * Time.deltaTime * 500f,
                            -820530352,
                            -1,
                            true,
                            true,
                            255f,
                            105f,
                            0f,
                            1f);
            }
        }

        public void Cum()
        {
            if (Input.RightTrigger)
            {
                RPCSUB.SendLaunchProjectile(GorillaLocomotion.Player.Instance.bodyCollider.transform.position + new Vector3(0f, -0.1f, 0f),
                            GorillaLocomotion.Player.Instance.bodyCollider.transform.forward * Time.deltaTime * 500f,
                            -820530352,
                            -1,
                            true,
                            true,
                            45f,
                            25f,
                            22f,
                            1f);
            }
        }

        public void WallWalk()
        {
            RaycastHit Left;
            RaycastHit Right;
            Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.right, out Left, 100f, int.MaxValue);
            Physics.Raycast(GorillaLocomotion.Player.Instance.leftControllerTransform.position, GorillaLocomotion.Player.Instance.leftControllerTransform.right, out Right, 100f, int.MaxValue);

            if (Input.RightGrip)
            {
                if (Left.distance < Right.distance)
                {
                    if (Left.distance < 1)
                    {
                        Vector3 gravityDirection = (Left.point - GorillaLocomotion.Player.Instance.rightControllerTransform.position).normalized;
                        Physics.gravity = gravityDirection * 9.81f;
                    }
                    else
                    {
                        Physics.gravity = new Vector3(0, -9.81f, 0);
                    }
                }
                if (Left.distance == Right.distance)
                {
                    Physics.gravity = new Vector3(0, -9.81f, 0);
                }
            }
            if (Input.LeftGrip)
            {
                if (Left.distance > Right.distance)
                {
                    if (Right.distance < 1)
                    {
                        Vector3 gravityDirection = (Right.point - GorillaLocomotion.Player.Instance.leftControllerTransform.position).normalized;
                        Physics.gravity = gravityDirection * 9.81f;
                    }
                    else
                    {
                        Physics.gravity = new Vector3(0, -9.81f, 0);
                    }
                }
                if (Left.distance == Right.distance)
                {
                    Physics.gravity = new Vector3(0, -9.81f, 0);
                }
            }
            if (!Input.LeftGrip && !Input.RightGrip)
            {
                Physics.gravity = new Vector3(0, -9.81f, 0);
            }

        }

        bool ghostToggled = false;

        public void GhostMonkey()
        {
            if (Input.RightPrimary)
            {
                if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
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

        public void InvisMonkey()
        {
            if (Input.RightPrimary)
            {
                if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
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

        public void FreezeMonkey()
        {
            if (Input.RightPrimary)
            {
                if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
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

        public void CopyGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (CopingRig == null)
                {
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (Input.RightTrigger)
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            CopingRig = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (Input.RightTrigger && CopingRig != null)
                {
                    pointer.transform.position = CopingRig.transform.position;
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }
                    GorillaTagger.Instance.offlineVRRig.enabled = false;

                    GorillaTagger.Instance.offlineVRRig.transform.position = CopingRig.transform.position;

                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = CopingRig.rightHand.rigTarget.transform.position;
                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = CopingRig.leftHand.rigTarget.transform.position;

                    GorillaTagger.Instance.offlineVRRig.transform.rotation = CopingRig.transform.rotation;

                    GorillaTagger.Instance.offlineVRRig.head.rigTarget.rotation = CopingRig.head.rigTarget.rotation;
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    return;
                }
                else
                {
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                    CopingRig = null;
                }
                return;
            }
        }

        public void RigGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit, 100, GorillaLocomotion.Player.Instance.locomotionEnabledLayers) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                pointer.transform.position = raycastHit.point;

                if (Input.RightTrigger)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = raycastHit.point + new Vector3(0, 0.6f, 0);
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    return;
                }
                else
                {
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
                return;
            }
        }

        public void CompSpeedBoost()
        {
            if (Input.RightPrimary)
            {
                if (AllowSpeedChange)
                {
                    CompSpeedType++;
                    if (CompSpeedType == 0)
                    {
                        compspeed = 7.5f;
                        Main.Instance.buttons[30].buttontext = "Comp Speed Boost {Mosa}";
                        Notif.SendNotification("Changed Speed {Mosa}");
                    }
                    if (CompSpeedType == 1)
                    {
                        compspeed = 8.5f;
                        Main.Instance.buttons[30].buttontext = "Comp Speed Boost {Coke}";
                        Notif.SendNotification("Changed Speed {Coke}");
                    }
                    if (CompSpeedType == 2)
                    {
                        compspeed = 9.5f;
                        Main.Instance.buttons[30].buttontext = "Comp Speed Boost {Pixi}";
                        Notif.SendNotification("Changed Speed {Pixi}");
                    }
                    if (CompSpeedType >= 3)
                    {
                        compspeed = 7.5f;
                        Main.Instance.buttons[30].buttontext = "Comp Speed Boost {Mosa}";
                        Notif.SendNotification("Changed Speed {Mosa}");
                        CompSpeedType = 0;
                    }
                    AllowSpeedChange = false;
                }
            }
            else
            {
                AllowSpeedChange = true;
            }

            GorillaLocomotion.Player.Instance.maxJumpSpeed = compspeed;
        }

        public void FasterSwimming()
        {
            if (GorillaLocomotion.Player.Instance.InWater)
            {
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity * 1.013f;
            }
        }

        public void CompBalloonAura()
        {
            if (Time.time > TagAura + 0.1)
            {
                foreach (Player pl in PhotonNetwork.PlayerListOthers)
                {
                    float distance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, GorillaGameManager.instance.FindPlayerVRRig(pl).transform.position);
                    if (distance < (GorillaGameManager.instance.tagDistanceThreshold / 2))
                    {
                        PhotonView.Get(GorillaGameManager.instance.GetComponent<GorillaGameManager>()).RPC("ReportSlingshotHit", RpcTarget.MasterClient, new object[]
                        {
                                pl, new Vector3(0,0,0), RPCSUB.IncrementLocalPlayerProjectileCount()
                        });
                    }
                }
                TagAura = Time.time;
            }
        }

        public void CarMonkey()
        {
            left_joystick = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis(SteamVR_Input_Sources.LeftHand);
            RaycastHit raycastHit;
            bool flag43 = Physics.Raycast(GorillaLocomotion.Player.Instance.bodyCollider.transform.position, Vector3.down, out raycastHit, 10f, LayerMask.GetMask("Gorilla Object"));
            head_direction = GorillaLocomotion.Player.Instance.headCollider.transform.forward;
            roll_direction = Vector3.ProjectOnPlane(head_direction, raycastHit.normal);
            bool flag44 = left_joystick.y != 0f;
            if (flag44)
            {
                bool flag45 = left_joystick.y < 0f;
                if (flag45)
                {
                    bool flag46 = speed > -maxs;
                    if (flag46)
                    {
                        speed -= acceleration * Mathf.Abs(left_joystick.y) * Time.deltaTime;
                    }
                }
                else
                {
                    bool flag47 = speed < maxs;
                    if (flag47)
                    {
                        speed += acceleration * Mathf.Abs(left_joystick.y) * Time.deltaTime;
                    }
                }
            }
            else
            {
                bool flag48 = speed < 0f;
                if (flag48)
                {
                    speed += acceleration * Time.deltaTime * 0.5f;
                }
                else
                {
                    bool flag49 = speed > 0f;
                    if (flag49)
                    {
                        speed -= acceleration * Time.deltaTime * 0.5f;
                    }
                }
            }
            bool flag50 = speed > maxs;
            if (flag50)
            {
                speed = maxs;
            }
            bool flag51 = speed < -maxs;
            if (flag51)
            {
                speed = -maxs;
            }
            bool flag52 = speed != 0f && raycastHit.distance < distance;
            if (flag52)
            {
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = roll_direction.normalized * speed * multiplier;
            }
            bool flag53 = GorillaLocomotion.Player.Instance.IsHandTouching(true) || GorillaLocomotion.Player.Instance.IsHandTouching(false);
            if (flag53)
            {
                speed *= 0.1f;
            }

        }

        public void SpiderMonke()
        {
            lefttriggerpressed = ControllerInputPoller.instance.leftControllerIndexFloat;
            triggerpressed = ControllerInputPoller.instance.rightControllerIndexFloat;
            bool flag3 = !wackstart;
            if (flag3)
            {
                GameObject gameObject = new GameObject();
                Spring = 10f;
                Damper = 30f;
                MassScale = 12f;
                grapplecolor = Color.white;
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
            bool flag4 = triggerpressed > 0.1f;
            if (flag4)
            {
                bool flag5 = cangrapple;
                if (flag5)
                {
                    Spring = 10f;
                    StartGrapple(GorillaLocomotion.Player.Instance);
                    cangrapple = false;
                }
            }
            else
            {
                StopGrapple(GorillaLocomotion.Player.Instance);
            }
            bool flag6 = triggerpressed > 0.1f && lefttriggerpressed > 0.1f;
            if (flag6)
            {
                Spring /= 2f;
            }
            else
            {
                Spring = 10f;
            }
            bool flag7 = lefttriggerpressed > 0.1f;
            if (flag7)
            {
                bool flag8 = canleftgrapple;
                if (flag8)
                {
                    Spring = 10f;
                    LeftStartGrapple(GorillaLocomotion.Player.Instance);
                    canleftgrapple = false;
                }
            }
            else
            {
                LeftStopGrapple();
            }
        }

        private void StartGrapple(GorillaLocomotion.Player __instance)
        {
            RaycastHit raycastHit;
            bool flag = Physics.Raycast(__instance.rightControllerTransform.position, __instance.rightControllerTransform.forward, out raycastHit, maxDistance);
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

        private void DrawRope(GorillaLocomotion.Player __instance)
        {
            bool flag = !joint;
            if (!flag)
            {
                lr.SetPosition(0, __instance.rightControllerTransform.position);
                lr.SetPosition(1, grapplePoint);
            }
        }

        private void StopGrapple(GorillaLocomotion.Player __instance)
        {
            lr.positionCount = 0;
            UnityEngine.Object.Destroy(joint);
            cangrapple = true;
        }

        private void LeftStartGrapple(GorillaLocomotion.Player __instance)
        {
            RaycastHit raycastHit;
            bool flag = Physics.Raycast(__instance.leftControllerTransform.position, __instance.leftControllerTransform.forward, out raycastHit, maxDistance);
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

        private void LeftDrawRope(GorillaLocomotion.Player __instance)
        {
            bool flag = !leftjoint;
            if (!flag)
            {
                leftlr.SetPosition(0, __instance.leftControllerTransform.position);
                leftlr.SetPosition(1, leftgrapplePoint);
            }
        }

        private void LeftStopGrapple()
        {
            leftlr.positionCount = 0;
            UnityEngine.Object.Destroy(leftjoint);
            canleftgrapple = true;
        }

        public void MonkeClimb(GorillaLocomotion.Player __instance)
        {
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

        private void ApplyVelocity(Vector3 pos, Vector3 target, GorillaLocomotion.Player __instance)
        {
            Physics.gravity = new Vector3(0, 0, 0);
            Vector3 a = target - pos;
            __instance.bodyCollider.attachedRigidbody.velocity = a * 65f;
        }

        LineRenderer lineRenderer;
        RaycastHit hit;
        bool disablegrapple = false;
        public void GrappleHook()
        {

            if (Input.RightGrip && !Input.RightTrigger)
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
            if (Input.RightGrip && Input.RightTrigger && !disablegrapple)
            {
                if (Vector3.Distance(GorillaLocomotion.Player.Instance.bodyCollider.transform.position, hit.point) < 4)
                {
                    disablegrapple = true;
                    Destroy(lineRenderer); lineRenderer = null; return;
                }
                Vector3 dir2 = (hit.point - GorillaLocomotion.Player.Instance.bodyCollider.transform.position).normalized * 30;
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(dir2, ForceMode.Acceleration);
            }
            if (!Input.RightGrip && !Input.RightTrigger)
            {
                if (lineRenderer != null)
                {
                    Destroy(lineRenderer);
                }
            }
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, hit.point);
                lineRenderer.SetPosition(1, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
            }
        }

        public void SendRopeRPC(Vector3 velocity)
        {
            if (Time.time > ropetimeout + 0.1f)
            {
                ropetimeout = Time.time;
                foreach (GorillaRopeSwing rope in FindObjectsOfType<GorillaRopeSwing>())
                {
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, velocity, true, null);
                }
            }
        }

        float ropetimeout;
        public void RopeUp()
        {
            if (Input.RightTrigger)
            {
                SendRopeRPC(new Vector3(0, 100, 1));
            }
        }

        public void RopeDown()
        {
            if (Input.RightTrigger)
            {
                SendRopeRPC(new Vector3(0, -100, 1));
            }
        }

        public void RopeToSelf()
        {
            if (Input.RightTrigger)
            {
                if (Time.time > ropetimeout + 0.1f)
                {
                    ropetimeout = Time.time;
                    foreach (GorillaRopeSwing rope in FindObjectsOfType<GorillaRopeSwing>())
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
        }

        public void RopeGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                pointer.transform.position = raycastHit.point;

                if (Input.RightTrigger)
                {
                    if (Time.time > ropetimeout + 0.1f)
                    {
                        ropetimeout = Time.time;
                        foreach (GorillaRopeSwing rope in FindObjectsOfType<GorillaRopeSwing>())
                        {
                            Vector3 targetPosition = raycastHit.point;
                            Vector3 ropeToCursor = targetPosition - rope.transform.position;
                            float distanceToCursor = ropeToCursor.magnitude;
                            float speed = 9999;
                            float t = Mathf.Clamp01(speed / distanceToCursor);

                            Vector3 newPosition = rope.transform.position + ropeToCursor.normalized * distanceToCursor * t;
                            Vector3 velocity = (newPosition - rope.transform.position).normalized * speed;
                            RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, velocity, true, null);
                        }
                    }
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    return;
                }
                else
                {
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
        }

        public void RopeFreeze()
        {
            if (Time.time > ropetimeout + 0.05f && Input.RightTrigger)
            {
                ropetimeout = Time.time;
                SendRopeRPC(new Vector3(0, 0, 0));
            }
        }

        public void RopeSpaz()
        {
            if (Time.time > ropetimeout + 0.1f && Input.RightTrigger)
            {
                ropetimeout = Time.time;
                SendRopeRPC(new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100)));
            }
        }

        public void RGB()
        {
            GradientColorKey[] colorkeys = new GradientColorKey[7];
            colorkeys[0].color = Color.red;
            colorkeys[0].time = 0f;
            colorkeys[1].color = Color.yellow;
            colorkeys[1].time = 0.2f;
            colorkeys[2].color = Color.green;
            colorkeys[2].time = 0.3f;
            colorkeys[3].color = Color.cyan;
            colorkeys[3].time = 0.5f;
            colorkeys[4].color = Color.blue;
            colorkeys[4].time = 0.6f;
            colorkeys[5].color = Color.magenta;
            colorkeys[5].time = 0.8f;
            colorkeys[6].color = Color.red;
            colorkeys[6].time = 1f;
            Gradient gradient = new Gradient();
            gradient.colorKeys = colorkeys;
            float t = Mathf.PingPong(Time.time / 2f, 1f);
            var colortochange = gradient.Evaluate(t);

            GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g, colortochange.b, false);
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
            {
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.Others, colortochange.r, colortochange.g, colortochange.b, false);
            }
        }
        float colorFloat = 10f;
        public void Strobe()
        {
            colorFloat = Mathf.Repeat(colorFloat + Time.deltaTime * 40f, 50f);
            float r = Mathf.Cos(colorFloat * Mathf.PI * 2f) * 0.5f + 0.5f;
            float g = Mathf.Sin(colorFloat * Mathf.PI * 2f) * 0.5f + 0.5f;
            float b = Mathf.Cos(colorFloat * Mathf.PI * 2f + Mathf.PI / 2f) * 0.5f + 0.5f;
            GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(r, g, b, false);
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
            {
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.Others, r, g, b, false);
            }
        }

        VRRig KickPlayer;
        float kicktimer = -2;
        public void KickGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (KickPlayer == null)
                {
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (Input.RightTrigger)
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            KickPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (Input.RightTrigger && KickPlayer != null)
                {
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    pointer.transform.position = KickPlayer.transform.position;
                    Player owner = GetPhotonViewFromRig(KickPlayer).Owner;
                    if (owner != null && Time.time > kicktimer + 1f)
                    {
                        if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(owner.UserId))
                        {
                            PhotonNetworkController.Instance.shuffler = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                            PhotonNetworkController.Instance.keyStr = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                            RPCSUB.JoinPubWithFriends(owner);
                            kicktimer = Time.time;
                        }
                    }
                }
                else
                {
                    KickPlayer = null;
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);
        }

        public void TouchKick()
        {
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig != null && !rig.isOfflineVRRig)
                {
                    if (Vector3.Distance(rig.transform.position, GorillaTagger.Instance.offlineVRRig.rightHandPlayer.transform.position) > 1.5f)
                    {
                        Player owner = GetPhotonViewFromRig(rig).Owner;
                        if (kicktimer > Time.time)
                        {
                            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(owner.UserId))
                            {
                                PhotonNetworkController.Instance.shuffler = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                                PhotonNetworkController.Instance.keyStr = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                                RPCSUB.JoinPubWithFriends(owner);
                                kicktimer = Time.time + 0.5f;
                            }
                        }
                        return;
                    }
                    if (Vector3.Distance(rig.transform.position, GorillaTagger.Instance.offlineVRRig.leftHandPlayer.transform.position) > 1.5f)
                    {
                        Player owner = GetPhotonViewFromRig(rig).Owner;
                        if (kicktimer > Time.time)
                        {
                            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(owner.UserId))
                            {
                                PhotonNetworkController.Instance.shuffler = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                                PhotonNetworkController.Instance.keyStr = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                                RPCSUB.JoinPubWithFriends(owner);
                                kicktimer = Time.time + 0.5f;
                            }
                        }
                        return;
                    }
                }
            }
        }

        public void TouchCrash()
        {
            if (!PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED")) { return; }
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig != null && !rig.isOfflineVRRig)
                {
                    if (Vector3.Distance(rig.transform.position, GorillaTagger.Instance.offlineVRRig.rightHandPlayer.transform.position) > 1.5f)
                    {
                        Player owner = GetPhotonViewFromRig(rig).Owner;
                        MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                        object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        return;
                    }
                    if (Vector3.Distance(rig.transform.position, GorillaTagger.Instance.offlineVRRig.leftHandPlayer.transform.position) > 1.5f)
                    {
                        Player owner = GetPhotonViewFromRig(rig).Owner;
                        MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                        object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        return;
                    }
                }
            }
        }
        public float antibancooldown = 0;
        public bool antiban;
        public void StartAntiBan()
        {
            if (PhotonNetwork.InRoom && antiban && Time.time > antibancooldown && !PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
            {
                base.StartCoroutine(AntiBan());
                antibancooldown = Time.time + 2;
                antiban = false;
            }
        }
        private IEnumerator AntiBan()
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
            yield return new WaitForSeconds(0.5f);
            string gamemode = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace(GorillaComputer.instance.currentQueue, GorillaComputer.instance.currentQueue + "MODDED_MODDED_");
            Hashtable hash = new Hashtable
                    {
                        { "gameMode",gamemode }
                    };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            Notif.SendNotification("AntiBan Enabled For This Lobby");
            yield break;
        }

        static bool isnoclipped = false;
        public void NoClip()
        {
            if (Input.LeftTrigger)
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

        public static Hashtable removeFilter = new Hashtable();

        public static Hashtable ServerCleanDestroyEvent = new Hashtable();

        public static RaiseEventOptions ServerCleanOptions = new RaiseEventOptions();

        bool canBHop = false;
        bool isBHop = false;

        public void DesyncChecker()
        {
            if (Time.time - _previousEXTime >= (1 / 20))
            {
                _previousEXTime = Time.time;
                if (Time.time - _previousTime >= (PhotonNetwork.GetPing() / 1000))
                {
                    rigob = Instantiate(GorillaTagger.Instance.offlineVRRig.gameObject);
                    Destroy(rigob.GetComponent<Rigidbody>());
                    Destroy(rigob.GetComponent<VRRigReliableState>());
                    rigob.transform.position = _previousPosition;
                    rigob.transform.rotation = _previousRotation;
                    var vrrig = rigob.GetComponent<VRRig>();
                    vrrig.rightHandTransform.position = _previousRightPosition;
                    leftHand.transform.position = _previousLeftPosition;
                    vrrig.rightHandTransform.rotation = _previousRightRotation;
                    leftHand.transform.rotation = _previousLeftRotation;
                    vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                    vrrig.mainSkin.material.color = new Color32(242, 172, 7, 100);
                    vrrig.leftHandPlayer.enabled = false;
                    vrrig.rightHandPlayer.enabled = false;
                    vrrig.enabled = false;

                    // Update previous position and time
                    _previousPosition = GorillaTagger.Instance.offlineVRRig.transform.position;
                    _previousRotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;
                    _previousLeftPosition = GorillaTagger.Instance.offlineVRRig.leftHandTransform.position;
                    _previousRightPosition = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;
                    _previousLeftRotation = GorillaTagger.Instance.offlineVRRig.leftHandTransform.rotation;
                    _previousRightRotation = GorillaTagger.Instance.offlineVRRig.rightHandTransform.rotation;
                    _previousTime = Time.time;
                }
            }
            //MainManager.Instance.BreakAudioGunMouse();
        }
        public GameObject rigob;
        public GameObject leftHand;
        public GameObject rightHand;
        private Vector3 _previousPosition;
        private Quaternion _previousRotation;
        private Vector3 _previousRightPosition;
        private Vector3 _previousLeftPosition;
        private Quaternion _previousLeftRotation;
        private Quaternion _previousRightRotation;
        private float _previousTime;
        private float _previousEXTime;

        public void BHop()
        {
            if (Input.RightSecondary)
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
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(Vector3.up * 200f, ForceMode.Impulse);
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(GorillaTagger.Instance.offlineVRRig.rightHandPlayer.transform.right * 180f, ForceMode.Impulse);
                }
                if (GorillaLocomotion.Player.Instance.IsHandTouching(true))
                {
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(Vector3.up * 200f, ForceMode.Impulse);
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(-GorillaTagger.Instance.offlineVRRig.leftHandPlayer.transform.right * 180f, ForceMode.Impulse);
                }
            }
        }

        VRRig LagPlayer;
        public void CrashGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (LagPlayer == null)
                {
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (Input.RightTrigger)
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            LagPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (Input.RightTrigger && LagPlayer != null)
                {
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    pointer.transform.position = LagPlayer.transform.position;
                    Player owner = GetPhotonViewFromRig(LagPlayer).Owner;
                    if (owner != null)
                    {
                        MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                        object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                    }
                }
                else
                {
                    LagPlayer = null;
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);
        }
        float crashtimeout = -2192;
        public void FreezeAll()
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
            {
                Debug.Log("Crashing..");
                foreach (Player owner in PhotonNetwork.PlayerListOthers)
                {
                    MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                    object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                    method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                }

            }
        }

        VRRig CrashPlayer;
        public void FreezeGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (CrashPlayer == null)
                {
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (Input.RightTrigger)
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            CrashPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (Input.RightTrigger && CrashPlayer != null)
                {
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    pointer.transform.position = CrashPlayer.transform.position;
                    Player owner = GetPhotonViewFromRig(CrashPlayer).Owner;
                    if (owner != null && PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                    {
                        crashtimeout = Time.time;
                        MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                        object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                        method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                    }
                }
                else
                {
                    CrashPlayer = null;
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);
        }

        public void InvisGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                pointer.transform.position = raycastHit.point;
                if (Input.RightTrigger)
                {
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    Player owner = GetPhotonViewFromRig(raycastHit.collider.GetComponentInParent<VRRig>()).Owner;
                    if (owner != null)
                    {
                        PhotonNetwork.CurrentRoom.StorePlayer(owner);
                        PhotonNetwork.CurrentRoom.Players.Remove(owner.ActorNumber);
                        PhotonNetwork.OpRemoveCompleteCacheOfPlayer(owner.ActorNumber);
                    }
                }
                else
                {
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);
        }

        public void StartMatAll()
        {
            base.StartCoroutine(this.MatAll());
        }

        public void StartMatSelf()
        {
            base.StartCoroutine(this.MatSpamSelf());
        }

        private IEnumerator MatAll()
        {
            while (false)
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
                        if (this.GorillaHuntManager == null)
                        {
                            this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
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
                if (GorillaTagManager != null)
                {
                    if (GorillaTagManager.isCurrentlyTag)
                    {
                        foreach (Player owner in PhotonNetwork.PlayerList)
                        {
                            if (GorillaTagManager.currentIt == owner)
                            {
                                GorillaTagManager.currentIt = null;
                                yield return new WaitForSeconds(0.05f);
                            }
                            else
                            {
                                GorillaTagManager.currentIt = owner;
                                yield return new WaitForSeconds(0.05f);
                            }
                        }
                    }
                    else
                    {
                        foreach (Player owner in PhotonNetwork.PlayerList)
                        {
                            if (GorillaTagManager.currentInfected.Contains(owner))
                            {
                                istagged[owner.ActorNumber] = true;
                            }
                            else
                            {
                                istagged[owner.ActorNumber] = false;
                            }
                            if (istagged[owner.ActorNumber])
                            {
                                GorillaTagManager.currentInfected.Remove(owner);
                                GorillaTagManager.UpdateState();
                                yield return new WaitForSeconds(0.05f);
                            }
                            else
                            {
                                GorillaTagManager.currentInfected.Add(owner);
                                GorillaTagManager.UpdateState();
                                yield return new WaitForSeconds(0.05f);
                            }
                        }
                    }
                }
                if (this.GorillaHuntManager != null)
                {
                    foreach (Player owner in PhotonNetwork.PlayerList)
                    {
                        if (this.GorillaHuntManager.currentHunted.Contains(owner))
                        {
                            istagged[owner.ActorNumber] = true;
                        }
                        else
                        {
                            istagged[owner.ActorNumber] = false;
                        }
                        if (istagged[owner.ActorNumber])
                        {
                            this.GorillaHuntManager.currentHunted.Add(owner);
                            this.GorillaHuntManager.UpdateHuntState();
                            yield return new WaitForSeconds(0.1f);
                        }
                        else
                        {
                            this.GorillaHuntManager.currentHunted.Remove(owner);
                            this.GorillaHuntManager.UpdateHuntState();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                if (GorillaBattleManager != null)
                {
                    foreach (Player owner in PhotonNetwork.PlayerList)
                    {
                        if (GorillaBattleManager.playerLives[owner.ActorNumber] == 3)
                        {
                            istagged[owner.ActorNumber] = true;
                        }
                        else
                        {
                            istagged[owner.ActorNumber] = false;
                        }
                        if (istagged[owner.ActorNumber])
                        {
                            GorillaBattleManager.playerLives[owner.ActorNumber] = 0;
                            yield return new WaitForSeconds(0.05f);
                        }
                        else
                        {
                            GorillaBattleManager.playerLives[owner.ActorNumber] = 3;
                            yield return new WaitForSeconds(0.05f);
                        }
                    }
                }
            }
            yield break;
        }
        
        private IEnumerator MatSpamSelf()
        {
            while (Main.Instance.buttons[74].Active)
            {
                Photon.Realtime.Player owner = PhotonNetwork.LocalPlayer;
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
                        if (this.GorillaHuntManager == null)
                        {
                            this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
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
                if (GorillaTagManager != null)
                {
                    if (GorillaTagManager.isCurrentlyTag)
                    {
                        if (GorillaTagManager.currentIt == owner)
                        {
                            GorillaTagManager.currentIt = null;
                            yield return new WaitForSeconds(0.1f);
                        }
                        else
                        {
                            GorillaTagManager.currentIt = owner;
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                    {
                        if (GorillaTagManager.currentInfected.Contains(owner))
                        {
                            IsTaggedSelf = true;
                        }
                        else
                        {
                            IsTaggedSelf = false;
                        }
                        if (IsTaggedSelf)
                        {
                            GorillaTagManager.currentInfected.Remove(owner);
                            GorillaTagManager.UpdateState();
                            yield return new WaitForSeconds(0.1f);
                        }
                        else
                        {
                            GorillaTagManager.currentInfected.Add(owner);
                            GorillaTagManager.UpdateState();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                if (this.GorillaHuntManager != null)
                {
                    if (this.GorillaHuntManager.currentHunted.Contains(owner))
                    {
                        IsTaggedSelf = true;
                    }
                    else
                    {
                        IsTaggedSelf = false;
                    }
                    if (IsTaggedSelf)
                    {
                        this.GorillaHuntManager.currentHunted.Add(owner);
                        this.GorillaHuntManager.UpdateHuntState();
                        yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        this.GorillaHuntManager.currentHunted.Remove(owner);
                        this.GorillaHuntManager.UpdateHuntState();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                if (GorillaBattleManager != null)
                {
                    if (GorillaBattleManager.playerLives[owner.ActorNumber] == 3)
                    {
                        IsTaggedSelf = true;
                    }
                    else
                    {
                        IsTaggedSelf = false;
                    }
                    if (IsTaggedSelf)
                    {
                        GorillaBattleManager.playerLives[owner.ActorNumber] = 0;
                        yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        GorillaBattleManager.playerLives[owner.ActorNumber] = 3;
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
            yield break;
        }
        


        public IEnumerator CoolMatChanger()
        {
            while (PhotonNetwork.CurrentRoom != null)
            {
                string[] gamemodes = {
                "INFECTION",
                "HUNT",
                "BATTLE"
                };
                var gamemode = gamemodes[UnityEngine.Random.Range(0, gamemodes.Length)];

                changegamemode(gamemode);
                yield return new WaitForSeconds(0.5f);
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
                        if (this.GorillaHuntManager == null)
                        {
                            this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
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

                if (gamemode == "INFECTION")
                {
                    GorillaTagManager.isCurrentlyTag = true;
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        if (GorillaTagManager.currentInfected.Contains(p))
                        {
                            GorillaTagManager.currentInfected.Remove(p);
                            GorillaTagManager.UpdateState();
                        }
                        else
                        {
                            GorillaTagManager.currentInfected.Add(p);
                            GorillaTagManager.UpdateState();
                        }
                    }
                    if (GorillaTagManager.isCurrentlyTag)
                    {
                        Player pe = PhotonNetwork.PlayerList[UnityEngine.Random.Range(0, PhotonNetwork.PlayerList.Length)];
                        if (GorillaTagManager.currentIt != pe)
                        {
                            GorillaTagManager.ChangeCurrentIt(pe);
                        }
                    }
                    yield return new WaitForSeconds(0.5f);
                }
                if (gamemode == "HUNT")
                {
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        if (GorillaHuntManager.currentHunted.Contains(p))
                        {
                            GorillaHuntManager.currentHunted.Remove(p);
                            GorillaHuntManager.UpdateHuntState();
                        }
                        else
                        {
                            GorillaHuntManager.currentHunted.Add(p);
                            GorillaHuntManager.UpdateHuntState();
                        }
                    }
                    yield return new WaitForSeconds(0.5f);
                }
                if (gamemode == "BATTLE")
                {
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        if (GorillaBattleManager.playerLives[p.ActorNumber] > 0)
                        {
                            GorillaBattleManager.playerLives[p.ActorNumber] = 0;
                        }
                        if (GorillaBattleManager.playerLives[p.ActorNumber] == 0)
                        {
                            GorillaBattleManager.playerLives[p.ActorNumber] = 3;
                        }
                    }
                    yield return new WaitForSeconds(0.5f);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }


        VRRig MatPlayer;
        float matguntimer = -222f;
        public void MatGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (MatPlayer == null)
                {
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (Input.RightTrigger)
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            MatPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (Input.RightTrigger && MatPlayer != null)
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
                            if (this.GorillaHuntManager == null)
                            {
                                this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
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
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    pointer.transform.position = MatPlayer.transform.position;
                    Player owner = GetPhotonViewFromRig(MatPlayer).Owner;
                    if (owner != null && Time.time > matguntimer)
                    {
                        if (ltagged)
                        {
                            ltagged = false;
                            if (GetGameMode().Contains("INFECTION"))
                            {
                                GorillaTagManager.isCurrentlyTag = true;
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

                        matguntimer = Time.time + 0.1f;
                    }
                }
                else
                {
                    MatPlayer = null;
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);

        }

        VRRig SlowingRig;
        public void SlowGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (SlowingRig == null)
                {
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (Input.RightTrigger)
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            SlowingRig = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (Input.RightTrigger && SlowingRig != null)
                {
                    pointer.transform.position = SlowingRig.transform.position;
                    PhotonView Photonview = GetPhotonViewFromRig(SlowingRig);
                    Photon.Realtime.Player owner = Photonview.Owner;
                    if (Photonview != null || owner != null)
                    {
                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                        {
                            RPCSUB.SetTaggedTime(owner);
                        }
                        pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    }

                }
                else
                {
                    SlowingRig = null;
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);
        }
        public void SlowGunMouse()
        {
            if (UnityInput.Current.GetMouseButton(1))
            {
                RaycastHit raycastHit;
                if (SlowingRig == null)
                {
                    if (Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (UnityInput.Current.GetMouseButton(0))
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            SlowingRig = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (UnityInput.Current.GetMouseButton(0) && SlowingRig != null)
                {
                    pointer.transform.position = SlowingRig.transform.position;
                    PhotonView Photonview = GetPhotonViewFromRig(SlowingRig);
                    Photon.Realtime.Player owner = Photonview.Owner;
                    if (Photonview != null || owner != null)
                    {
                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                        {
                            RPCSUB.SetTaggedTime(owner);
                        }
                        pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    }

                }
                else
                {
                    SlowingRig = null;
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);
        }
        public void MatGunMouse()
        {
            if (UnityInput.Current.GetMouseButton(1))
            {
                RaycastHit raycastHit;
                if (MatPlayer == null)
                {
                    if (Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (UnityInput.Current.GetMouseButton(0))
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            MatPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (UnityInput.Current.GetMouseButton(0) && MatPlayer != null)
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
                            if (this.GorillaHuntManager == null)
                            {
                                this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
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
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    pointer.transform.position = MatPlayer.transform.position;
                    Player owner = GetPhotonViewFromRig(MatPlayer).Owner;
                    if (owner != null)
                    {
                        if (GorillaTagManager.currentInfected.Contains(owner))
                        {
                            GorillaTagManager.AddInfectedPlayer(owner);
                        }
                        else
                        {
                            GorillaTagManager.currentInfected.Remove(owner);
                        }
                    }
                }
                else
                {
                    MatPlayer = null;
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);

        }
        public void TagGunMouse()
        {
            if (Tagger == null)
            {
                Ray ray = GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition);

                RaycastHit raycastHit;
                Physics.Raycast(ray.origin, ray.direction, out raycastHit);
                if (pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                pointer.transform.position = raycastHit.point;
                if (UnityInput.Current.GetMouseButton(0))
                {
                    if (raycastHit.collider.GetComponentInParent<VRRig>())
                    {
                        Tagger = raycastHit.collider.GetComponentInParent<VRRig>();
                    }
                }
            }
            if (UnityInput.Current.GetMouseButton(0) && Tagger != null)
            {
                pointer.transform.position = Tagger.transform.position;
                pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                GorillaTagger.Instance.offlineVRRig.transform.position = Tagger.transform.position;
                ProcessTagAura(GetPhotonViewFromRig(Tagger).Owner);

                return;
            }
            else
            {
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                Tagger = null;
            }
        }
        public void RopeGunMouse()
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit) && pointer == null)
            {
                pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            }
            pointer.transform.position = raycastHit.point;
            if (UnityInput.Current.GetMouseButton(1))
            {
                if (Time.time > ropetimeout + 0.1f)
                {
                    ropetimeout = Time.time;
                    foreach (GorillaRopeSwing rope in FindObjectsOfType<GorillaRopeSwing>())
                    {
                        Vector3 targetPosition = raycastHit.point;
                        Vector3 ropeToCursor = targetPosition - rope.transform.position;
                        float distanceToCursor = ropeToCursor.magnitude;
                        float speed = 9999;
                        float t = Mathf.Clamp01(speed / distanceToCursor);

                        Vector3 newPosition = rope.transform.position + ropeToCursor.normalized * distanceToCursor * t;
                        Vector3 velocity = (newPosition - rope.transform.position).normalized * speed;
                        RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, velocity, true, null);
                    }
                }
                pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                return;
            }
            else
            {
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
            }

        }
        public void KickGunMouse()
        {
            RaycastHit raycastHit;
            if (KickPlayer == null)
            {
                if (Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                pointer.transform.position = raycastHit.point;
                if (UnityInput.Current.GetMouseButton(0))
                {
                    if (raycastHit.collider.GetComponentInParent<VRRig>())
                    {
                        KickPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                    }
                }
            }
            if (UnityInput.Current.GetMouseButton(0) && KickPlayer != null)
            {
                pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                pointer.transform.position = KickPlayer.transform.position;
                Player owner = GetPhotonViewFromRig(KickPlayer).Owner;
                if (owner != null && Time.time > kicktimer + 1)
                {
                    if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(owner.UserId))
                    {
                        PhotonNetworkController.Instance.shuffler = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                        PhotonNetworkController.Instance.keyStr = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                        RPCSUB.JoinPubWithFriends(owner);
                        kicktimer = Time.time;
                    }
                }
            }
            else
            {
                KickPlayer = null;
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
            }
        }
        public void rigspamtest()
        {
            GameObject rig = PhotonNetwork.PrefabPool.Instantiate("GorillaPrefabs/Gorilla Player Networked", GorillaLocomotion.Player.Instance.transform.position, GorillaLocomotion.Player.Instance.transform.rotation);
            rig.SetActive(true);
            PhotonNetwork.Destroy(Traverse.Create(rig.GetComponent<VRRig>()).Field("photonview").GetValue<PhotonView>());
        }


        public void FreezeGunMouse()
        {
            RaycastHit raycastHit;
            if (LagPlayer == null)
            {
                if (Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                pointer.transform.position = raycastHit.point;
                if (UnityInput.Current.GetMouseButton(0))
                {
                    if (raycastHit.collider.GetComponentInParent<VRRig>())
                    {
                        LagPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                    }
                }
            }
            if (UnityInput.Current.GetMouseButton(0) && LagPlayer != null)
            {
                pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                pointer.transform.position = LagPlayer.transform.position;
                Player owner = GetPhotonViewFromRig(LagPlayer).Owner;
                if (owner != null)
                {
                    MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                    object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                    method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                    PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                    {
                        TargetActors = new int[] { owner.ActorNumber }
                    }, SendOptions.SendReliable);
                    PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                    {
                        TargetActors = new int[] { owner.ActorNumber }
                    }, SendOptions.SendReliable);
                    PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                    {
                        TargetActors = new int[] { owner.ActorNumber }
                    }, SendOptions.SendReliable);
                }
            }
            else
            {
                LagPlayer = null;
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
            }
        }
        public void CrashGunMouse()
        {
            RaycastHit raycastHit;
            if (CrashPlayer == null)
            {
                if (Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                pointer.transform.position = raycastHit.point;
                if (UnityInput.Current.GetMouseButton(0))
                {
                    if (raycastHit.collider.GetComponentInParent<VRRig>())
                    {
                        CrashPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                    }
                }
            }
            if (UnityInput.Current.GetMouseButton(0) && CrashPlayer != null)
            {
                pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                pointer.transform.position = CrashPlayer.transform.position;
                Player owner = GetPhotonViewFromRig(CrashPlayer).Owner;
                if (owner != null)
                {
                    if (GorillaGameManager.instance != null && PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                    {
                        MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                        object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                    }
                }
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                CrashPlayer = null;
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
            }
        }
        public void InvisGunMouse()
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit) && pointer == null)
            {
                pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            }
            pointer.transform.position = raycastHit.point;
            if (UnityInput.Current.GetMouseButton(0))
            {
                pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                Player owner = GetPhotonViewFromRig(raycastHit.collider.GetComponentInParent<VRRig>()).Owner;
                if (owner != null)
                {
                    PhotonNetwork.CurrentRoom.StorePlayer(owner);
                    PhotonNetwork.CurrentRoom.Players.Remove(owner.ActorNumber);
                    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(owner.ActorNumber);
                }
            }
            else
            {
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
            }
        }
        public void BreakAudioGunMouse()
        {
            RaycastHit raycastHit;
            if (BAudioPlayer == null)
            {
                if (Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                pointer.transform.position = raycastHit.point;
                if (UnityInput.Current.GetMouseButton(0))
                {
                    if (raycastHit.collider.GetComponentInParent<VRRig>())
                    {
                        BAudioPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                    }
                }
            }
            if (UnityInput.Current.GetMouseButton(0) && BAudioPlayer != null)
            {
                pointer.transform.position = BAudioPlayer.transform.position;
                PhotonView Photonview = GetPhotonViewFromRig(BAudioPlayer);
                Photon.Realtime.Player owner = Photonview.Owner;
                if (Photonview != null || owner != null)
                {
                    GorillaTagger.Instance.myVRRig.RPC("PlayHandTap", owner, new object[]
                    {
                                94,
                                false,
                                9999f
                    });

                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                }

            }
            else
            {
                BAudioPlayer = null;
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
            }
        }

        VRRig VibratingRig;
        public void VibrateGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (VibratingRig == null)
                {
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (Input.RightTrigger)
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            VibratingRig = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (Input.RightTrigger && VibratingRig != null)
                {
                    pointer.transform.position = VibratingRig.transform.position;
                    PhotonView Photonview = GetPhotonViewFromRig(VibratingRig);
                    Photon.Realtime.Player owner = Photonview.Owner;
                    if (Photonview != null || owner != null)
                    {
                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                        {
                            RPCSUB.JoinedTaggedTime(owner);
                        }
                        pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    }

                }
                else
                {
                    VibratingRig = null;
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);
        }
        public void VibrateGunMouse()
        {
            RaycastHit raycastHit;
            if (VibratingRig == null)
            {
                if (Physics.Raycast(GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition), out raycastHit) && pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                }
                pointer.transform.position = raycastHit.point;
                if (UnityInput.Current.GetMouseButton(0))
                {
                    if (raycastHit.collider.GetComponentInParent<VRRig>())
                    {
                        VibratingRig = raycastHit.collider.GetComponentInParent<VRRig>();
                    }
                }
            }
            if (UnityInput.Current.GetMouseButton(0) && VibratingRig != null)
            {
                pointer.transform.position = VibratingRig.transform.position;
                PhotonView Photonview = GetPhotonViewFromRig(VibratingRig);
                Photon.Realtime.Player owner = Photonview.Owner;
                if (Photonview != null || owner != null)
                {
                    if (PhotonNetwork.LocalPlayer.IsMasterClient)
                    {
                        RPCSUB.JoinedTaggedTime(owner);
                    }
                    pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                }

            }
            else
            {
                VibratingRig = null;
                pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
            }
        }

        float slowallcooldown = -1;
        public void SlowAll()
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

        float Vibrateallcooldown = -1;
        public void VibrateAll()
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


        VRRig BAudioPlayer;
        public void BreakAudioGun()
        {
            if (Input.RightGrip)
            {
                RaycastHit raycastHit;
                if (BAudioPlayer == null)
                {
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                    {
                        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                        pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    }
                    pointer.transform.position = raycastHit.point;
                    if (Input.RightTrigger)
                    {
                        if (raycastHit.collider.GetComponentInParent<VRRig>())
                        {
                            BAudioPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                    }
                }
                if (Input.RightTrigger && BAudioPlayer != null)
                {
                    pointer.transform.position = BAudioPlayer.transform.position;
                    PhotonView Photonview = GetPhotonViewFromRig(BAudioPlayer);
                    Photon.Realtime.Player owner = Photonview.Owner;
                    if (Photonview != null || owner != null)
                    {
                        GorillaTagger.Instance.myVRRig.RPC("PlayHandTap", owner, new object[]
{
        94,
        false,
        9999f
});
                        pointer.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.7f);
                    }

                }
                else
                {
                    BAudioPlayer = null;
                    pointer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.7f);
                }
                return;
            }
            UnityEngine.GameObject.Destroy(pointer);
        }

        public void UnAcidSelf()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
                states[i].touchedLiquid = PhotonNetwork.PlayerList[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber ? false : states[i].touchedLiquid;
            }
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
        }

        public void UnAcidAll()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                states[i].touchedLiquid = false;
                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
            }
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
        }

        private IEnumerator changemode(string gamemode)
        {
            changegamemode(gamemode);
            yield return new WaitForSeconds(0.1f);
            changegamemode(gamemode);
            Notif.SendNotification("Gamemode Changed!");
            yield break;
        }

        public void changegamemode(string gamemode)
        {
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
            if (gmInstance)
            {
                PhotonNetwork.Destroy(gmInstance.GetComponent<PhotonView>());
            }
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

        public string getButtonType(int type)
        {
            if (type == 0)
            {
                return "Hate Speach";
            }
            if (type == 1)
            {
                return "Cheating";
            }
            if (type == 2)
            {
                return "Toxicity";
            }
            return "Other";
        }
    }

}