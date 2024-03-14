using Cinemachine;
using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaTag;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using Steal.Attributes;
using Steal.Background;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using static Photon.Voice.Unity.Recorder;

namespace Steal
{
    public class StealGUI : MonoBehaviour
    {
        public static string Name = "Steal";
        public static Rect Window = new Rect(0, 0, 415, 360);
        public static Vector2[] scroll = new Vector2[100];
        public static int Theme = 1;
        public static bool Show = true;
        public const int MaxLogs = 20;
        public static List<string> logs = new List<string>();
        public static int num = 1;
        public static string filename = "StealLogs" + num + ".txt";

        public static float deltaTime = 0;
        public static string Code = "text here";
        public static string ID = "text here";
        public static bool searched = false;

        public static bool freecam = false;
        public static bool[] GUIToggles = new bool[100];
        public static string Room;
        public static int RPCCount;
        private static float speed = 10;
        private static float rotspeed;


        public static string[] tabs =
        {
            "Main",
            "Server",
            "Players",
            "Freecam",
            "Misc",
            "Console",
            "Settings"
        };

        public static string[] monkeyboy =
        {
            "ESP",
            "Chams",
            "Skeleton ESP",
            "Tracers",
            "Box ESP",
            "Hit Box ESP",
            "Tag Gun",
            "Tag All",
            "Tag Self {M}",
            "Anti Tag",
            "No Tag On Join",
            "Rope Spaz {F}",
            "Rope Freeze {LAGGY}",
            "Rope Gun",
            "RGB {STUMP}",
            "Strobe {STUMP}",
            "Kick Gun {STUMP} {PRIVATE}",
            "Kick All {STUMP} {PRIVATE}",
            "Anti Ban",
            "Set Master {AntiBan}",
            "Crash Gun {AntiBan}",
            "Crash All {AntiBan}",
            "Freeze All {AntiBan}",
            "Freeze Gun {AntiBan}",
            "Break Room",
            "Break Gamemode {M}",
            "Sound Spam {M}",
            "Sound Spam 2 {M}",
            "Invis All {Rejoin}",
            "Invis Gun {Rejoin}",
            "Mat All {M}",
            "Mat Gun {M}",
            "Slow All {M}",
            "Slow Gun {M}",
            "Vibrate All {M}",
            "Virate Gun {M}",
            "Break Audio All {NW?}",
            "Break Audio Gun {NW?}",
        };

        public static bool[] Active = new bool[monkeyboy.Length];

        public static int tab = 0;
        private static float fov;
        private static bool campause;
        private static bool boner;
        private static bool psnma;
        private static int tagcooldown;
        private static bool matself;
        private static bool antiban;
        public static float mattimer = 0;

        private void OnEnable()
        {
            Application.logMessageReceived += new Application.LogCallback(HandleLog);
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= new Application.LogCallback(HandleLog);
        }

        private void Update()
        {
            if (Keyboard.current[Key.RightShift].wasPressedThisFrame)
            {
                Show = !Show;
            }
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            UpdateState();
            MainManager.Instance.FollowPLayer();
        }

        private static void UpdateState()
        {
            if (freecam)
            {
                MainManager.Instance.AdvancedWASD(speed);
            }
            if (Active[0]) // ESP
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
                            GameObject.Destroy(beacon.GetComponent<CapsuleCollider>());
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
                            Destroy(beacon, Time.deltaTime);
                        }
                    }
                }
            }

            if (Active[1]) // Chams
            {
                if (PhotonNetwork.CurrentRoom != null)
                {
                    foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                    {
                        if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer && vrrig.mainSkin.material.name.Contains("fected"))
                        {
                            vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                            vrrig.mainSkin.material.color = new Color(9f, 0f, 0f);
                        }
                        else if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                        {
                            vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                            vrrig.mainSkin.material.color = new Color(0f, 9f, 0f);
                        }
                    }
                    psnma = false;
                }
            }
            else
            {
                if (!psnma)
                {
                    foreach (VRRig vrrig in (VRRig[])UnityEngine.Object.FindObjectsOfType(typeof(VRRig)))
                    {
                        if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                        {
                            vrrig.ChangeMaterialLocal(vrrig.currentMatIndex);
                        }
                    }
                    psnma = true;
                }
            }

            if (Active[2]) // Skeleton ESP
            {
                MainManager.Instance.BoneESP(false);
                boner = true;
            }
            else
            {
                if (boner)
                {
                    MainManager.Instance.BoneESP(true);
                    foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                    {
                        if (vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                        {
                            Destroy(vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>());
                        }
                    }
                    boner = false;
                }
            }

            if (Active[3]) // Tracers
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
                        Destroy(lineRenderer, Time.deltaTime);
                    }
                }
            }

            if (Active[4]) // Box ESP
            {
                foreach (VRRig rig in GorillaParent.instance.vrrigs)
                {
                    if (rig != null && !rig.isOfflineVRRig && !rig.isMyPlayer)
                    {
                        MainManager.Instance.BoxESP();
                    }
                }
            }

            if (Active[5]) // Hit Box ESP
            {
                foreach (VRRig rig in GorillaParent.instance.vrrigs)
                {
                    if (rig != null && !rig.isOfflineVRRig && !rig.isMyPlayer)
                    {
                        MainManager.Instance.HitBoxESP(rig);
                    }
                }
            }
            if (Active[6]) // Tag Gun
            {
                MainManager.Instance.TagGunMouse();
            }

            if (Active[7]) // Tag All
            {
                MainManager.Instance.TagAll();
            }

            if (Active[8]) // Tag self
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    MainManager.Instance.saveKeys();
                    MainManager.Instance.GorillaTagManager.currentInfected.Add(PhotonNetwork.LocalPlayer);
                }
                Active[8] = false;
            }

            if (Active[9]) // Anti Tag
            {
                MainManager.Instance.NoTag();
            }

            if (Active[10]) // No Tag On Join
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Active[10] = false;
            }

            if (Active[11]) // Rope Spaz
            {
                if (Keyboard.current[Key.F].wasPressedThisFrame)
                {
                    MainManager.Instance.RopeSpaz();
                }
            }
            if (Active[12]) // Rope Freeze
            {
                MainManager.Instance.RopeFreeze();
            }
            if (Active[13]) // Rope Gun
            {
                MainManager.Instance.RopeGunMouse();
            }
            if (Active[14]) // RGB
            {
                MainManager.Instance.RGB();
            }

            if (Active[15]) // Strobe
            {
                MainManager.Instance.Strobe();
            }

            if (Active[16]) // Kick Gun
            {
                MainManager.Instance.KickGunMouse();
            }

            if (Active[17]) // Kick All
            {
                if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
                {
                    GorillaComputer.instance.OnGroupJoinButtonPress(1, GorillaComputer.instance.friendJoinCollider);
                }
            }

            if (Active[18]) // Anti Ban
            {
                if (PhotonNetwork.CurrentRoom == null)
                {
                    MainManager.Instance.antiban = true;
                }
                MainManager.Instance.StartAntiBan();
            }

            if (Active[19]) // Set Master
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                {
                    PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                }
                Active[19] = false;
            }

            if (Active[20]) // Freeze Gun
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                {
                    MainManager.Instance.FreezeGunMouse();
                }
            }

            if (Active[21]) // Freeze All
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                {
                    MainManager.Instance.FreezeAll();
                }
            }

            if (Active[22]) // Crash All
            {
                MainManager.Instance.CrashAll();
            }

            if (Active[23]) // Crash Gun
            {
                MainManager.Instance.CrashGunMouse();
            }

            if (Active[24]) // Break Room
            {
                if (!PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                {
                    PlayFabClientAPI.ExecuteCloudScript(new PlayFab.ClientModels.ExecuteCloudScriptRequest
                    {
                        FunctionName = "RoomClosed",
                        FunctionParameter = new
                        {
                            GameId = PhotonNetwork.CurrentRoom.Name,
                            Region = Regex.Replace(PhotonNetwork.CloudRegion, "[^a-zA-Z0-9]", "").ToUpper()
                        }
                    }, result =>
                    {
                        ShowConsole.Log("Successfully Ran It");
                    }, null);
                }
            }

            if (Active[25]) // Break Gamemode
            {
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                {
                    string orgstr = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
                    if (orgstr.TryReplace("BATTLE", "", out string rforest))
                    {
                        orgstr = rforest;
                    }
                    if (orgstr.TryReplace("INFECTION", "", out string rcanton))
                    {
                        orgstr = rcanton;
                    }
                    if (orgstr.TryReplace("HUNT", "", out string rcacnton))
                    {
                        orgstr = rcacnton;
                    }
                    if (orgstr.TryReplace("CASUAL", "", out string rbasement))
                    {
                        orgstr = rbasement;
                    }
                    var hash = new Hashtable
                {
                    { "gameMode", orgstr }
                };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                    PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetCustomPropertiesOfRoom(hash);
                }
            }

            if (Active[26]) // Sound Spam
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    RPCSUB.SendSound(UnityEngine.Random.Range(0, 10), 1);
                }
            }

            if (Active[27]) // Sound Spam 2
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    RPCSUB.SendSound(UnityEngine.Random.Range(0, 10), 1f);
                }
                Active[27] = false;
            }

            if (Active[28]) // Invis All
            {
                foreach (Photon.Realtime.Player owner in PhotonNetwork.PlayerListOthers)
                {
                    PhotonNetwork.CurrentRoom.StorePlayer(owner);
                    PhotonNetwork.CurrentRoom.Players.Remove(owner.ActorNumber);
                    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(owner.ActorNumber);
                }
            }

            if (Active[29]) // Invis Gun
            {
                MainManager.Instance.InvisGunMouse();
            }


            if (Active[30]) // Mat All
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (GorillaGameManager.instance != null)
                    {
                        if (MainManager.Instance.GetGameMode().Contains("INFECTION"))
                        {
                            if (MainManager.Instance.GorillaTagManager == null)
                            {
                                MainManager.Instance.GorillaTagManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                            }
                        }
                        else if (MainManager.Instance.GetGameMode().Contains("HUNT"))
                        {
                            if (MainManager.Instance.GorillaHuntManager == null)
                            {
                                MainManager.Instance.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
                            }
                        }
                        else if (MainManager.Instance.GetGameMode().Contains("BATTLE"))
                        {
                            if (MainManager.Instance.GorillaBattleManager == null)
                            {
                                MainManager.Instance.GorillaBattleManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                            }
                        }
                    }
                    if (Time.time > mattimer)
                    {
                        MainManager.Instance.matSpamAll();
                    }
                }
            }

            if (Active[31]) // Mat Gun
            {
                MainManager.Instance.MatGunMouse();
            }

            if (Active[32]) // Slow All
            {
                MainManager.Instance.SlowAll();
            }

            if (Active[33]) // Slow Gun
            {
                MainManager.Instance.SlowGunMouse();
            }

            if (Active[34]) // Vibrate All
            {
                MainManager.Instance.VibrateAll();
            }

            if (Active[35]) // Vibrate Gun 
            {
                MainManager.Instance.VibrateGunMouse();
            }

            if (Active[36]) // Break Audio All
            {
                GorillaTagger.Instance.myVRRig.RPC("PlayHandTap", RpcTarget.Others, new object[]
{
        94,
        false,
        9999f
});
            }

            if (Active[37]) // Break Audio Gun
            {
                MainManager.Instance.BreakAudioGunMouse();
            }
        }

        static string[] maps =
{
            "beach", "city", "basement", "clouds", "forest", "mountains", "canyon", "cave"
        };
        private void OnGUI()
        {
            if (Show)
            {
                Window = GUI.Window(9999, Window, Win, "");
            }
        }

        private async void Win(int o)
        {
            DesignLibrary.BuildTexture(Theme);
            DesignLibrary.CreateTexture();

            GUIStyle lb = new GUIStyle(GUI.skin.label);
            lb.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(Name + " | FPS: " + Mathf.RoundToInt(1f / deltaTime), lb);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < tabs.Length; i++)
            {
                ButtonManager.DrawLayoutButton(tabs[i], false, false, true, new GUIStyle("button"), delegate (bool isActive)
                {
                    tab = i;
                });
            }
            GUILayout.EndHorizontal();
            switch (tab)
            {
                case 0:
                    GUILayout.BeginArea(new Rect(15f, 80, Window.width - 30f, Window.height - 100));
                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);
                    GUILayout.BeginVertical();
                    GUILayout.Label("Main", lb);
                    GUILayout.Space(10f);
                    Code = GUILayout.TextField(Code);
                    ButtonManager.DrawLayoutButton("Join Room", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(Code);
                    });
                    ButtonManager.DrawLayoutButton("Change Name", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        PhotonNetwork.LocalPlayer.NickName = Code.ToLower();
                        PlayerPrefs.SetString("playerName", Code.ToLower());
                        GorillaComputer.instance.offlineVRRigNametagText.text = Code.ToLower();
                        GorillaTagger.Instance.offlineVRRig.playerName = Code.ToLower();
                        PlayerPrefs.Save();
                    });
                    ButtonManager.DrawLayoutButton("Create Public", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        MainManager.Instance.CreatePublicRoom();
                    });
                    ButtonManager.DrawLayoutButton("Disconnect", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        PhotonNetwork.Disconnect();
                    });
                    for (int c = 0; c < monkeyboy.Length; c++)
                    {
                        ButtonManager.DrawLayoutButton(monkeyboy[c], Active[c], false, true, new GUIStyle("button"), delegate (bool isActive)
                        {
                            Active[c] = isActive;
                        });
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    break;

                case 1:
                    GUILayout.BeginArea(new Rect(15f, 80, Window.width - 30f, Window.height - 100));
                    scroll[1] = GUILayout.BeginScrollView(scroll[1]);
                    GUILayout.BeginVertical();
                    GUILayout.Label("Server Information", lb);

                    if (PhotonNetwork.InRoom)
                    {
                        GUILayout.Label($"Server Name: {PhotonNetwork.CurrentRoom.Name}");
                        GUILayout.Label($"Server Players: {PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString()}");
                        GUILayout.Label($"Server Region: {PhotonNetwork.CloudRegion}");
                        GUILayout.Label($"Server Host: {PhotonNetwork.MasterClient.NickName}");
                        GUILayout.Label($"Server Ping: {PhotonNetwork.GetPing()}");
                        GUILayout.Label($"Server IP: {PhotonNetwork.ServerAddress}");
                        GUILayout.Label($"Server is Public: {PhotonNetwork.CurrentRoom.IsVisible}");
                        GUILayout.Label($"Server is Joinable: {PhotonNetwork.CurrentRoom.IsOpen}");
                    }
                    else
                    {
                        GUILayout.Label("Server Name: Not Connected");
                        GUILayout.Label("Server Players: Not Connected");
                        GUILayout.Label("Server Region: Not Connected");
                        GUILayout.Label("Server Host: Not Connected");
                        GUILayout.Label("Server Ping: Not Connected");
                        GUILayout.Label("Server IP: Not Connected");
                        GUILayout.Label("Server is Public: Not Connected");
                        GUILayout.Label("Server is Joinable: Not Connected");
                    }
                    GUILayout.Label("Player Information", lb);
                    GUILayout.Label($"Player Name: {PhotonNetwork.LocalPlayer.NickName}");
                    GUILayout.Label($"Player ID: {PhotonNetwork.LocalPlayer.UserId}");
                    GUILayout.Label($"Player Ping: {PhotonNetwork.GetPing()}");
                    GUILayout.Label($"Player is Host: {PhotonNetwork.LocalPlayer.IsMasterClient}");
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    break;

                case 2:
                    GUI.skin.scrollView.normal.background = DesignLibrary.scrollView;
                    GUI.skin.scrollView.hover.background = DesignLibrary.scrollView;
                    GUI.skin.scrollView.active.background = DesignLibrary.scrollView;
                    GUIStyle bStyle = new GUIStyle(GUI.skin.button);
                    bStyle.normal.background = DesignLibrary.box;
                    bStyle.active.background = DesignLibrary.buttonactive;
                    bStyle.hover.background = DesignLibrary.buttonhovered;
                    bStyle.onNormal.background = DesignLibrary.label;
                    bStyle.onHover.background = DesignLibrary.buttonhovered;
                    bStyle.onActive.background = DesignLibrary.buttonactive;
                    GUILayout.BeginArea(new Rect(15f, 80, Window.width - 30f, Window.height - 100));
                    scroll[2] = GUILayout.BeginScrollView(scroll[2]);
                    GUILayout.BeginVertical();
                    if (PhotonNetwork.InRoom)
                    {
                        Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
                        for (int i = 0; i < playerListOthers.Length; i++)
                        {
                            Player player = playerListOthers[i];
                            GUILayout.BeginVertical();
                            GUILayout.Box(player.NickName);
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button("Teleport", bStyle, new GUILayoutOption[] { GUILayout.Width(60f), GUILayout.Height(30f) }))
                            {
                                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().isKinematic = true;
                                foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                                {
                                    meshCollider.enabled = false;
                                }
                                GorillaLocomotion.Player.Instance.transform.position = GorillaGameManager.instance.FindPlayerVRRig(player).headMesh.gameObject.transform.position;
                                // wait till player is in position
                                await Task.Delay(100);
                                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                                foreach (MeshCollider meshCollider2 in Resources.FindObjectsOfTypeAll<MeshCollider>())
                                {
                                    meshCollider2.enabled = true;
                                }
                                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().isKinematic = false;
                            }
                            if (GUILayout.Button("Follow Player", bStyle, new GUILayoutOption[] { GUILayout.Width(90f), GUILayout.Height(30f) }))
                            {
                                MainManager.Instance.following = !MainManager.Instance.following;
                                if (!MainManager.Instance.following)
                                {
                                    MainManager.Instance.FollowingPlayer = null;
                                }
                                else
                                {
                                    MainManager.Instance.FollowingPlayer = GorillaGameManager.instance.FindPlayerVRRig(PhotonNetwork.PlayerListOthers[i]);
                                }
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                            num++;
                        }
                    }
                    else
                    {
                        GUILayout.Label("Not in a room.");

                    }
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    break;

                case 3:
                    GUILayout.BeginArea(new Rect(15f, 80, Window.width - 30f, Window.height - 100));
                    scroll[3] = GUILayout.BeginScrollView(scroll[3]);
                    GUILayout.BeginVertical();
                    GUILayout.Label("Freecam Settings");

                    ButtonManager.DrawLayoutButton("Freecam Mode", freecam, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        freecam = isActive;
                        ShowConsole.Log("FreeCam : Is " + freecam.ToString());
                    });
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Speed: " + speed.ToString());
                    GUILayout.EndHorizontal();
                    speed = GUILayout.HorizontalSlider(speed, 0.1f, 100f);
                    GUILayout.BeginHorizontal();
                    ButtonManager.DrawLayoutButton("Reset Speed", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        speed = 5f;
                    });
                    GUILayout.EndHorizontal();
                    GUILayout.Label($"Camera Settings");
                    GUILayout.Label($"Camera FOV: {(int)GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>().fieldOfView}");
                    GUILayout.Label($"Current FOV: {(int)fov}");
                    fov = GUILayout.HorizontalSlider(fov, 1f, 179f);
                    ButtonManager.DrawLayoutButton("Set FOV", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>().fieldOfView = fov;
                        GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1").GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = fov;
                    });
                    ButtonManager.DrawLayoutButton("Reset FOV", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>().fieldOfView = 60f;
                        GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1").GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = 60f;
                        fov = 60f;
                    });
                    ButtonManager.DrawLayoutButton("Pause Camera", campause, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        campause = isActive;
                        ShowConsole.Log("Pause Camera : Is " + campause.ToString());
                    });
                    GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1").SetActive(!campause);
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    break;
                case 4:
                    GUILayout.BeginArea(new Rect(15f, 80, Window.width - 30f, Window.height - 100));
                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);
                    GUILayout.BeginVertical();
                    GUILayout.Label("Player LookUp", lb);
                    GUILayout.Space(10f);
                    GUILayout.Box("Searched Information Here", new GUIStyle("box"));
                    if (searched)
                    {
                        GUILayout.Label("Name: " + Misc.name);
                        GUILayout.Label("ID: " + Misc.id);
                        GUILayout.Label("Created: " + Misc.created);
                        GUILayout.Label("Last Login: " + Misc.lastlogin);
                        GUILayout.Label("Banned: " + Misc.banned);
                    }
                    else
                    {

                    }
                    GUILayout.Space(10f);
                    ID = GUILayout.TextField(ID);
                    ButtonManager.DrawLayoutButton("Search", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        searched = true;
                        Misc.SearchID(ID);
                    });

                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    break;
                case 5:
                    GUI.skin.scrollView.normal.background = DesignLibrary.scrollView;
                    GUI.skin.scrollView.hover.background = DesignLibrary.scrollView;
                    GUI.skin.scrollView.active.background = DesignLibrary.scrollView;
                    scroll[6] = GUI.BeginScrollView(
                        new Rect(10f, 80f, Window.width - 20f, 200f),
                        scroll[6],
                        new Rect(0f, 0f, Window.width - 40f, (float)(20 * (logs.Count + 1)))
                    ); for (int i = 0; i < logs.Count; i++)
                    {
                        string text = logs[i];
                        GUI.Label(new Rect(0f, (float)(20 * i), Window.width - 40f, 20f), text);
                    }
                    GUI.EndScrollView();
                    GUILayout.Space(220f);
                    if (GUILayout.Button("Save Logs"))
                    {
                        while (File.Exists(filename))
                        {
                            num++;
                            filename = "StealLogs" + num + ".txt";
                        }

                        File.WriteAllLines(filename, logs.ToArray());
                    }

                    if (GUILayout.Button("Clear Logs"))
                    {
                        logs.Clear();
                    }
                    break;

                case 6:
                    if (Theme == 1)
                    {
                        GUILayout.Label("Current Theme: Dark Mode");
                    }
                    if (Theme == 2)
                    {
                        GUILayout.Label("Current Theme: nigger <333");
                        // GUILayout.Label("Current Theme: Blue");
                    }

                    if (Theme == 3)
                    {
                        GUILayout.Label("Current Theme: Rainbow");
                    }

                    if (Theme == 4)
                    {
                        GUILayout.Label("Current Theme: Light Mode");
                    }
                    if (Theme == 5)
                    {
                        GUILayout.Label("Current Theme: Blue");
                    }
                    if (GUILayout.Button("Change Theme"))
                    {
                        Theme = Theme + 1;
                        if (Theme > 5)
                        {
                            Theme = 1;
                        }
                    }
                    break;
            }
            GUI.DragWindow();
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string message = "[Steal] " + logString;
            switch (type)
            {
                case LogType.Log:
                    message = "<color=white>[Steal] [Log] " + logString + "</color>";
                    break;

                case LogType.Exception:
                    message = "<color=red>[Steal] [Exception] " + logString + "</color>";
                    break;

                case LogType.Error:
                    message = "<color=red>[Steal] [Error] " + logString + "</color>";
                    break;

                case LogType.Warning:
                    message = "<color=yellow>[Steal] [Warning] " + logString + "</color>";
                    break;
            }
            LogToConsole(message);
        }

        private static void LogToConsole(string message)
        {
            logs.Add(message);
            if (logs.Count > MaxLogs)
            {
                logs.RemoveAt(0);
            }
        }
    }

    public class RigManager : MonoBehaviour
    {
        public static VRRig GetPlayerFromVRRig(Player p)
        {
            GameObject gameObject = GameObject.Find("Rig Parent");

            VRRig result;
            if (p.IsInactive || p.IsLocal)
            {
                result = null;
            }
            else
            {
                if (gameObject)
                {
                    VRRig[] componentsInChildren = gameObject.GetComponentsInChildren<VRRig>();
                    foreach (VRRig vrrig in componentsInChildren)
                    {
                        if (!vrrig.isOfflineVRRig || vrrig != null)
                        {
                            if (vrrig.playerText.text == p.NickName)
                            {
                                return vrrig;
                            }
                        }
                    }
                }
                result = null;
            }
            return result;
        }
        public static PhotonView GetPhotonViewFromVRRig(VRRig p)
        {
            GameObject gameObject3 = GameObject.Find("Network Parent");
            PhotonView result;
            if (p == null || p.isOfflineVRRig)
            {
                result = null;
            }
            else
            {
                if (gameObject3)
                {
                    PhotonView[] componentsInChildren = gameObject3.GetComponentsInChildren<PhotonView>();
                    foreach (PhotonView photonView in componentsInChildren)
                    {
                        if (photonView.Owner.NickName == p.playerText.text)
                        {
                            return photonView;
                        }
                    }
                }
                result = null;
            }
            return result;
        }
        public static VRRig GetPlayersVRRig(Player p)
        {
            GameObject gameObject = GameObject.Find("Rig Parent");

            VRRig result;
            if (p.IsLocal)
            {
                result = GorillaTagger.Instance.offlineVRRig;
            }
            else
            {
                if (p != null && !p.IsInactive)
                {
                    if (gameObject)
                    {
                        VRRig[] componentsInChildren = gameObject.GetComponentsInChildren<VRRig>();
                        foreach (VRRig vrrig in componentsInChildren)
                        {
                            if (vrrig.playerText.text == p.NickName)
                            {
                                return vrrig;
                            }
                        }
                    }
                }
                result = null;
            }
            return result;
        }
        public static PhotonView GetPhotonViewFromRig(VRRig rig)
        {
            PhotonView value = Traverse.Create(rig).Field("photonView").GetValue<PhotonView>();
            PhotonView result;
            if (value != null)
            {
                result = value;
            }
            else
            {
                result = null;
            }
            return result;
        }
    }
    public class DesignLibrary : MonoBehaviour
    {
        public static Texture2D button = new Texture2D(1, 1);
        public static Texture2D buttonhovered = new Texture2D(1, 1);
        public static Texture2D buttonactive = new Texture2D(1, 1);
        public static Texture2D textarea = new Texture2D(1, 1);
        public static Texture2D textareahovered = new Texture2D(1, 1);
        public static Texture2D textareaactive = new Texture2D(1, 1);
        public static Texture2D label = new Texture2D(1, 1);
        public static Texture2D textField = new Texture2D(1, 1);
        public static Texture2D toggle = new Texture2D(1, 1);
        public static Texture2D scrollView = new Texture2D(1, 1);
        public static Texture2D dropdown = new Texture2D(1, 1);
        public static Texture2D slider = new Texture2D(1, 1);
        public static Texture2D progressBar = new Texture2D(1, 1);
        public static Texture2D box = new Texture2D(1, 1);
        public static Texture2D windowbackground = new Texture2D(1, 1);
        public static Color textcolor;


        /*
        private static Texture2D CreateRoundedTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            float radius = size / 2f;
            float radiusSquared = radius * radius;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float distanceSquared = (x - radius) * (x - radius) + (y - radius) * (y - radius);
                    if (distanceSquared <= radiusSquared)
                    {
                        float alpha = 1f - (distanceSquared / radiusSquared); // Smoothly fade out the edges
                        colors[y * size + x] = new Color(color.r, color.g, color.b, alpha);
                    }
                    else
                    {
                        colors[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }      
         */


        private static Texture2D CreateRoundedTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            for (int i = 0; i < size * size; i++)
            {
                int x = i % size;
                int y = i / size;
                if (Mathf.Sqrt((x - size / 2) * (x - size / 2) + (y - size / 2) * (y - size / 2)) <= size / 2)
                {
                    colors[i] = color;
                }
                else
                {
                    colors[i] = Color.clear;
                }
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        public static void BuildTexture(int theme)
        {
            if (theme == 1)
            {
                button.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
                button.Apply();
                buttonhovered.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f));
                buttonhovered.Apply();
                buttonactive.SetPixel(0, 0, new Color(0.4f, 0.4f, 0.4f));
                buttonactive.Apply();
                textarea.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
                textarea.Apply();
                textareahovered.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f));
                textareahovered.Apply();
                textareaactive.SetPixel(0, 0, new Color(0.4f, 0.4f, 0.4f));
                textareaactive.Apply();
                label.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
                label.Apply();
                textField.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
                textField.Apply();
                toggle.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
                toggle.Apply();
                scrollView.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
                scrollView.Apply();
                dropdown.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
                dropdown.Apply();
                slider = CreateRoundedTexture(32, new Color(0.2f, 0.2f, 0.2f));
                progressBar.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
                progressBar.Apply();
                box.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f));
                box.Apply();
                windowbackground = CreateRoundedTexture(25, new Color(0.1f, 0.1f, 0.1f));
                textcolor = Color.white;
            }
            if (theme == 2)
            {
                button.SetPixel(0, 0, new Color(1f, 0.6f, 0.8f)); // Light pink
                button.Apply();
                buttonhovered.SetPixel(0, 0, new Color(1f, 0.6f, 0.8f)); // Light pink
                buttonhovered.Apply();
                buttonactive.SetPixel(0, 0, new Color(1f, 0.6f, 0.8f)); // Light pink
                buttonactive.Apply();
                textarea.SetPixel(0, 0, new Color(0.9f, 0.9f, 1f)); // Light blue
                textarea.Apply();
                textareahovered.SetPixel(0, 0, new Color(0.8f, 0.9f, 1f)); // Light blue
                textareahovered.Apply();
                textareaactive.SetPixel(0, 0, new Color(0.7f, 0.8f, 1f)); // Light blue
                textareaactive.Apply();
                label.SetPixel(0, 0, new Color(1f, 0.6f, 0.8f)); // Light pink
                label.Apply();
                textField.SetPixel(0, 0, new Color(1f, 0.8f, 1f)); // Light purple
                textField.Apply();
                toggle.SetPixel(0, 0, new Color(1f, 0.6f, 0.6f)); // Light red
                toggle.Apply();
                scrollView.SetPixel(0, 0, new Color(1f, 0.6f, 0.8f)); // Light pink
                scrollView.Apply();
                dropdown.SetPixel(0, 0, new Color(1f, 0.6f, 0.8f)); // Light pink
                dropdown.Apply();
                slider = CreateRoundedTexture(32, new Color(1f, 0.8f, 0.6f)); // Light orange
                progressBar.SetPixel(0, 0, new Color(1f, 0.6f, 0.6f)); // Light red
                progressBar.Apply();
                box.SetPixel(0, 0, new Color(1f, 0.4f, 0.6f));  // Lighter pink
                box.Apply();
                windowbackground = CreateRoundedTexture(25, new Color(1f, 0.4f, 0.6f)); // Lighter pink
                textcolor = Color.black;
            }
            if (theme == 3)
            {
                button.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                button.Apply();
                buttonhovered.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                buttonhovered.Apply();
                buttonactive.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                buttonactive.Apply();
                textarea.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                textarea.Apply();
                textareahovered.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                textareahovered.Apply();
                textareaactive.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                textareaactive.Apply();
                label.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                label.Apply();
                textField.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                textField.Apply();
                toggle.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                toggle.Apply();
                scrollView.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                scrollView.Apply();
                dropdown.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                dropdown.Apply();
                slider = CreateRoundedTexture(32, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                progressBar.SetPixel(0, 0, Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));
                progressBar.Apply();
                windowbackground = CreateRoundedTexture(25, new Color(0.1f, 0.1f, 0.1f));
                textcolor = Color.white;
            }

            if (theme == 4)
            {
                button.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f));
                button.Apply();
                buttonhovered.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f));
                buttonhovered.Apply();
                buttonactive.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f));
                buttonactive.Apply();
                textarea.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f));
                textarea.Apply();
                textareahovered.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f));
                textareahovered.Apply();
                textareaactive.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f));
                textareaactive.Apply();
                label.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f));
                label.Apply();
                textField.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f));
                textField.Apply();
                toggle.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f));
                toggle.Apply();
                scrollView.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f));
                scrollView.Apply();
                dropdown.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f));
                dropdown.Apply();
                slider = CreateRoundedTexture(32, new Color(0.8f, 0.8f, 0.8f));
                progressBar.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f));
                progressBar.Apply();
                box.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f));
                box.Apply();
                windowbackground = CreateRoundedTexture(25, new Color(0.9f, 0.9f, 0.9f));
                textcolor = Color.black;
            }
            if (theme == 5)
            {
                button.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.6f));
                button.Apply();
                buttonhovered.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.6f));
                buttonhovered.Apply();
                buttonactive.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.6f));
                buttonactive.Apply();
                textarea.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.6f));
                textarea.Apply();
                textareahovered.SetPixel(0, 0, new Color(0.3f, 0.8f, 0.6f));
                textareahovered.Apply();
                textareaactive.SetPixel(0, 0, new Color(0.4f, 1f, 0.8f));
                textareaactive.Apply();
                label.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.6f));
                label.Apply();
                textField.SetPixel(0, 0, new Color(0.4f, 0.4f, 1f));
                textField.Apply();
                toggle.SetPixel(0, 0, new Color(0.6f, 0.2f, 0.2f));
                toggle.Apply();
                scrollView.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.6f));
                scrollView.Apply();
                dropdown.SetPixel(0, 0, new Color(0.4f, 0.2f, 0.6f));
                dropdown.Apply();
                slider = CreateRoundedTexture(32, new Color(0.8f, 0.4f, 0.6f));
                progressBar.SetPixel(0, 0, new Color(0.6f, 0.2f, 0.4f));
                progressBar.Apply();
                box.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f));
                box.Apply();
                windowbackground = CreateRoundedTexture(25, new Color(0.1f, 0.1f, 0.1f));
                textcolor = Color.white;
            }
            if (theme == 6)
            {

            }
        }

        public static void CreateTexture()
        {
            GUI.skin.button.normal.background = button;

            GUI.skin.button.hover.background = buttonhovered;

            GUI.skin.button.active.background = buttonactive;

            GUI.skin.button.onFocused.textColor = textcolor;

            GUI.skin.button.onNormal.textColor = textcolor;

            GUI.skin.button.onHover.textColor = textcolor;

            GUI.skin.button.onActive.textColor = textcolor;

            GUI.skin.button.normal.textColor = textcolor;

            GUI.skin.button.onHover.textColor = textcolor;

            GUI.skin.textArea.normal.background = textarea;

            GUI.skin.textArea.hover.background = textareahovered;

            GUI.skin.textArea.active.background = textareaactive;

            GUI.skin.textArea.focused.background = textareaactive;

            GUI.skin.textArea.onNormal.background = textarea;

            GUI.skin.textArea.onHover.background = textareahovered;

            GUI.skin.textArea.onActive.background = textareaactive;

            GUI.skin.textArea.onFocused.background = textareaactive;

            GUI.skin.textArea.onFocused.textColor = textcolor;

            GUI.skin.textArea.onNormal.textColor = textcolor;

            GUI.skin.textArea.onHover.textColor = textcolor;

            GUI.skin.textArea.onActive.textColor = textcolor;

            GUI.skin.textArea.normal.textColor = textcolor;

            GUI.skin.textArea.onHover.textColor = textcolor;

            GUI.skin.textArea.onActive.textColor = textcolor;

            GUI.skin.label.normal.background = label;

            GUI.skin.label.active.background = label;

            GUI.skin.label.hover.background = label;

            GUI.skin.label.normal.textColor = textcolor;

            GUI.skin.textField.normal.background = textField;

            GUI.skin.textField.hover.background = textField;

            GUI.skin.textField.active.background = textField;

            GUI.skin.textField.focused.background = textField;

            GUI.skin.textField.onNormal.background = textField;

            GUI.skin.textField.onActive.background = textField;

            GUI.skin.textField.onHover.background = textField;

            GUI.skin.textField.onFocused.background = textField;

            GUI.skin.textField.onFocused.textColor = textcolor;

            GUI.skin.textField.onNormal.textColor = textcolor;

            GUI.skin.textField.onHover.textColor = textcolor;

            GUI.skin.textField.onActive.textColor = textcolor;

            GUI.skin.textField.normal.textColor = textcolor;

            GUI.skin.textField.onHover.textColor = textcolor;

            GUI.skin.textField.onActive.textColor = textcolor;

            GUI.skin.textField.onHover.textColor = textcolor;

            GUI.skin.textField.onFocused.textColor = textcolor;

            GUI.skin.toggle.normal.background = toggle;

            GUI.skin.toggle.active.background = toggle;

            GUI.skin.toggle.hover.background = toggle;

            GUI.skin.toggle.focused.background = toggle;

            GUI.skin.toggle.focused.textColor = textcolor;

            GUI.skin.toggle.normal.textColor = textcolor;

            GUI.skin.scrollView.normal.background = scrollView;

            GUI.skin.box.normal.background = box;

            GUI.skin.box.active.background = box;

            GUI.skin.box.hover.background = box;

            GUI.skin.box.onNormal.background = box;

            GUI.skin.box.onFocused.background = box;

            GUI.skin.box.onHover.background = box;

            GUI.skin.box.onActive.background = box;

            GUI.skin.window.normal.background = windowbackground;

            GUI.skin.window.active.background = windowbackground;

            GUI.skin.window.hover.background = windowbackground;

            GUI.skin.window.focused.background = windowbackground;

            GUI.skin.window.onNormal.background = windowbackground;

            GUI.skin.window.onActive.background = windowbackground;

            GUI.skin.window.onHover.background = windowbackground;

            GUI.skin.window.onFocused.background = windowbackground;

            GUI.skin.window.normal.textColor = textcolor;

            GUI.skin.window.active.textColor = textcolor;

            GUI.skin.window.hover.textColor = textcolor;

            GUI.skin.window.focused.textColor = textcolor;

            GUI.skin.window.onNormal.textColor = textcolor;

            GUI.skin.window.onActive.textColor = textcolor;

            GUI.skin.window.onHover.textColor = textcolor;

            GUI.skin.window.onFocused.textColor = textcolor;

            GUI.skin.horizontalSlider.normal.background = slider;

            GUI.skin.horizontalSlider.hover.background = slider;

            GUI.skin.horizontalSlider.active.background = slider;

            GUI.skin.horizontalSlider.focused.background = slider;

            GUI.skin.horizontalSliderThumb.normal.background = slider;

            GUI.skin.horizontalSliderThumb.hover.background = slider;

            GUI.skin.horizontalSliderThumb.active.background = slider;

            GUI.skin.horizontalSliderThumb.focused.background = slider;

            GUI.skin.verticalSlider.normal.background = slider;

            GUI.skin.verticalSlider.hover.background = slider;

            GUI.skin.verticalSlider.active.background = slider;

            GUI.skin.verticalSlider.focused.background = slider;

            GUI.skin.verticalSliderThumb.normal.background = slider;

            GUI.skin.verticalSliderThumb.hover.background = slider;

            GUI.skin.verticalSliderThumb.active.background = slider;

            GUI.skin.verticalSliderThumb.focused.background = slider;

            GUI.skin.verticalScrollbarThumb.onActive.background = slider;

            GUI.skin.verticalScrollbarThumb.onFocused.background = slider;

            GUI.skin.verticalScrollbarThumb.onHover.background = slider;

            GUI.skin.verticalScrollbarThumb.onNormal.background = slider;

            GUI.skin.verticalScrollbar.border = new RectOffset(0, 0, 0, 0);
            GUI.skin.verticalScrollbar.fixedWidth = 0f;
            GUI.skin.verticalScrollbar.fixedHeight = 0f;

            // GUI.skin.verticalScrollbarThumb.border = new RectOffset(0, 0, 0, 0);

            GUI.skin.verticalScrollbarThumb.fixedHeight = 0f;

            GUI.skin.verticalScrollbarThumb.fixedWidth = 5f;

            GUI.skin.scrollView.normal.background = box;

            GUI.skin.scrollView.hover.background = box;

            GUI.skin.scrollView.active.background = box;

            GUI.skin.scrollView.focused.background = box;

            GUI.skin.scrollView.onNormal.background = box;

            GUI.skin.scrollView.onHover.background = box;

            GUI.skin.scrollView.onActive.background = box;

            GUI.skin.scrollView.onFocused.background = box;
        }
    }
    public class ButtonManager
    {
        public static void DrawLayoutButton(string label, bool isActive, bool isToggle, bool PlaySound, GUIStyle style, Action<bool> onClick)
        {
            if (isActive)
            {
                GUI.contentColor = Color.green;
            }
            else
            {
                GUI.contentColor = Color.white;
            }
            bool flag = false;
            if (isToggle)
            {
                flag = GUILayout.Toggle(isActive, label, style);
            }
            else
            {

                if (GUILayout.Button(label, style))
                {
                    flag = true;
                    if (PlaySound)
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, false, 0.25f);
                    }
                }
            }
            if (flag)
            {
                if (onClick != null)
                {
                    onClick.Invoke(!isActive);
                }
            }
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;
        }
    }

}

