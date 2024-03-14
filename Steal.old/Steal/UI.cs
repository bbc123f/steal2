using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Photon.Pun;
using GorillaNetworking;
using Steamworks;
using Cinemachine;
using Facebook.WitAi.Lib;
using HarmonyLib;
using PlayFab;
using Steal.Background;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Valve.VR.InteractionSystem;
using Player = Photon.Realtime.Player;
using static Steal.Main;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Steal.Patchers;
using UnityEngine.Animations.Rigging;
using UnityEngine.Networking;

using UnityEngine.UI;
using Steal.Mods;
using UnityEngine.UIElements;
using GorillaTag.GuidedRefs;
using static UnityEngine.UI.GridLayoutGroup;

namespace Steal
{
    public class StealGUI : MonoBehaviour
    {
        public static string Name = "Steal";
        public static Rect Window = new Rect(0, 30, 415, 360);
        public static Vector2[] scroll = new Vector2[100];
        public static int Theme = 2;
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
        public static float speed = 10;
        public static Main.Category currentGUIPage = Main.Category.Room;
        private static float rotspeed;
        public static int tab = 0;
        private static float fov;
        private static bool campause;
        private static bool boner;
        private static bool psnma;
        private static int tagcooldown;
        private static bool matself;
        private static bool antiban;
        public static float mattimer = 0;

        public static List<Steal.Main.Button> settings = new List<Steal.Main.Button>
        {
            new Steal.Main.Button("Self-Destruct", () =>
                {
                        //UnityEngine.Object.Destroy(Steal.Background.Security.DevLoad.ms);
                    }, Category.Settings, false, null, false, false),
            new Steal.Main.Button("Toggle Pocketwatch", () =>
            {
                GameObject.Find("Steal").GetComponent<Steal.Components.PocketWatch>().enabled = !GameObject.Find("Steal").GetComponent<Steal.Components.PocketWatch>().enabled;
            }, Category.Settings, false, null, false, false),
            new Steal.Main.Button("Toggle InGame ModsList", () =>
            {
                GameObject.Find("Steal").GetComponent<ModsListInterface>().enabled = !GameObject.Find("Steal").GetComponent<ModsListInterface>().enabled;
            }, Category.Settings, false, null, false, false),
            new Steal.Main.Button("Toggle PC ModsList", () =>
            {
                GameObject.Find("Steal").GetComponent<ModsList>().enabled = !GameObject.Find("Steal").GetComponent<ModsList>().enabled;
            }, Category.Settings, false, null, false, false),

        };

        private void OnEnable()
        {
            Theme = PlayerPrefs.GetInt("steal_backGround", 1);
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

            if (freecam)
            {
                Mods.Mods.Freecam();
            }
            Mods.Mods.FollowPLayer();
        }
        private void OnGUI()
        {
            if (Show)
            {
                Window = GUI.Window(9999, Window, Win, "");
            }
        }

        public void changeGUIPage(Main.Category page)
        {
            currentGUIPage = page;
        }

        public static string[] tabs =
        {
            "Menu Mods",
            "Server",
            "Players",
            "Freecam",
            "Console",
            "Settings"
        };


        public void changeTab(int thing)
        {
            tab = thing;
        }


        public Texture2D infected;
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
                if (tabs[i] != null)
                {
                    if (ButtonManager.DrawPageButton(tabs[i], i, "button"))
                    {
                        changeTab(i);
                        AssetLoader.Instance.PlayClick();
                    }
                }
            }

            GUILayout.EndHorizontal();
            switch (tab)
            {
                case 0:
                    GUILayout.BeginArea(new Rect(15f, 80, Window.width - 30f, Window.height - 100));
                    GUILayout.BeginHorizontal();
                    foreach (Main.Category page in Enum.GetValues(typeof(Main.Category)))
                    {
                        if (page != Main.Category.Base && page != Category.Computer && page != Category.Settings)
                        {
                            if (ButtonManager.DrawPageButton(page.ToString(), page, new GUIStyle("button")))
                            {
                                changeGUIPage(page);
                            }
                        }
                    }

 

                    GUILayout.EndHorizontal();
                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);
                    GUILayout.BeginVertical();
                    GUILayout.Space(10f);


                    int buttonPageThing = 0;
                    int pageThing = 2;

                    int buttonthing = 0;
                    foreach (Main.Button button in Main.GetButtonInfoByPage(currentGUIPage))
                    {
                        if (buttonthing == 0 && currentGUIPage != Category.Room)
                        {
                            GUI.contentColor = DesignLibrary.textcolor;
                            GUILayout.Label("Page: " + 1);
                            GUI.contentColor = Color.white;
                        }
                        ButtonManager.DrawLayoutButton(button, new GUIStyle("button"));
                        buttonPageThing++;

                        if (buttonPageThing == Main.pagesize && buttonthing != Main.GetButtonInfoByPage(currentGUIPage).Count - 1)
                        {
                            GUI.contentColor = DesignLibrary.textcolor;
                            GUILayout.Label("Page: " + pageThing.ToString());
                            GUI.contentColor = Color.white;
                            pageThing++;
                            buttonPageThing = 0;
                        }

                        buttonthing++;
                    }


                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    break;

                case 1:
                    GUILayout.BeginArea(new Rect(15f, 80, Window.width - 30f, Window.height - 100));
                    scroll[1] = GUILayout.BeginScrollView(scroll[1]);
                    GUILayout.BeginVertical();
                    Code = GUILayout.TextField(Code);
                    ButtonManager.DrawLayoutButtonLegacy("Join Room", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(Code.ToUpper());
                    });
                    ButtonManager.DrawLayoutButtonLegacy("Set Name", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        PhotonNetwork.LocalPlayer.NickName = Code.ToUpper();
                        PlayerPrefs.SetString("playerName", Code.ToUpper());
                        GorillaComputer.instance.offlineVRRigNametagText.text = Code.ToUpper();
                        GorillaTagger.Instance.offlineVRRig.playerName = Code.ToUpper();
                        PlayerPrefs.Save();
                    });
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
                    GUILayout.BeginArea(new Rect(15f, 80, Window.width - 10f, Window.height - 100));
                    scroll[2] = GUILayout.BeginScrollView(scroll[2]);
                    GUILayout.BeginVertical();
                    if (PhotonNetwork.InRoom)
                    {
                        Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
                        for (int i = 0; i < playerListOthers.Length; i++)
                        {
                            Player player = playerListOthers[i];
                            Mods.Mods.selectedPerson = player;
                            GUILayout.BeginVertical();
                            GUILayout.Box(player.NickName);
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button("Teleport", bStyle, new GUILayoutOption[] { GUILayout.Width(70f), GUILayout.Height(30f) }))
                            {
                                TeleportationLib.Teleport(GorillaGameManager.instance.FindPlayerVRRig(player).transform.position);
                            }
                            if (GUILayout.Button("Tag", bStyle, new GUILayoutOption[] { GUILayout.Width(50f), GUILayout.Height(30f) }))
                            {
                                GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                                infect.currentInfected.Add(player);
                            }
                            if (GUILayout.Button("UnTag", bStyle, new GUILayoutOption[] { GUILayout.Width(50f), GUILayout.Height(30f) }))
                            {
                                GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                                infect.currentInfected.Remove(player);
                            }

                            if (GUILayout.RepeatButton("Lag", bStyle, new GUILayoutOption[] { GUILayout.Width(50f), GUILayout.Height(30f) }))
                            {
                                if (player != null && PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                                {
                                    MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                                    object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { player.ActorNumber });
                                    method.Invoke(typeof(PhotonNetwork), new object[1] { player.ActorNumber });

                                    GuidedRefHub.UnregisterTarget<VRRig>(GorillaGameManager.instance.FindPlayerVRRig(player), true);
                                }
                            }

                            if (GUILayout.Button("Follow Player", bStyle, new GUILayoutOption[] { GUILayout.Width(90f), GUILayout.Height(30f) }))
                            {
                                Mods.Mods.following = !Mods.Mods.following;
                                if (!Mods.Mods.following)
                                {
                                    Mods.Mods.FollowingPlayer = null;
                                }
                                else
                                {
                                    Mods.Mods.FollowingPlayer = GorillaGameManager.instance.FindPlayerVRRig(PhotonNetwork.PlayerListOthers[i]);
                                }
                            }


                            // faggorty
                            VRRig vrrig = GorillaGameManager.instance.FindPlayerVRRig(player);
                            if (!vrrig.mainSkin.material.name.Contains("fected"))
                            {
                                GUILayout.Label(ApplyColorFilter(vrrig.mainSkin.material.color), GUILayout.Width(30), GUILayout.Height(30));
                            }
                            else
                            {
                                Texture2D playertext = infectTexture;
                                GUILayout.Label(playertext, GUILayout.Width(30), GUILayout.Height(30));
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

                    ButtonManager.DrawLayoutButtonLegacy("Freecam Mode", freecam, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        freecam = isActive;
                        ShowConsole.Log("FreeCam : Is " + freecam.ToString());
                    });
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Speed: " + speed.ToString());
                    GUILayout.EndHorizontal();
                    speed = GUILayout.HorizontalSlider(speed, 0.1f, 100f);
                    GUILayout.BeginHorizontal();
                    ButtonManager.DrawLayoutButtonLegacy("Reset Speed", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        speed = 10;
                    });
                    GUILayout.EndHorizontal();
                    GUILayout.Label($"Camera Settings");
                    GUILayout.Label($"Camera FOV: {(int)GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>().fieldOfView}");
                    GUILayout.Label($"Current FOV: {(int)fov}");
                    fov = GUILayout.HorizontalSlider(fov, 1f, 179f);
                    ButtonManager.DrawLayoutButtonLegacy("First Person Camera", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        Mods.Mods.FirstPerson();
                    });
                    ButtonManager.DrawLayoutButtonLegacy("Set FOV", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>().fieldOfView = fov;
                        GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1").GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = fov;
                    });
                    ButtonManager.DrawLayoutButtonLegacy("Reset FOV", false, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>().fieldOfView = 60f;
                        GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1").GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = 60f;
                        fov = 60f;
                    });
                    ButtonManager.DrawLayoutButtonLegacy("Pause Camera", campause, false, true, new GUIStyle("button"), delegate (bool isActive)
                    {
                        campause = isActive;
                        ShowConsole.Log("Pause Camera : Is " + campause.ToString());
                    });
                    GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1").SetActive(!campause);
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    break;
                case 9:
                    /*
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
                    GUILayout.EndArea();*/
                    break;
                case 4:
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

                case 5:
                    scroll[1] = GUILayout.BeginScrollView(scroll[1]);
                    if (Theme == 0)
                    {
                        GUILayout.Label("Current Theme: Dark Mode");
                    }
                    if (Theme == 1)
                    {
                        GUILayout.Label("Current Theme: Pink");
                    }

                    if (Theme == 2)
                    {
                        GUILayout.Label("Current Theme: Purple");
                    }

                    if (Theme == 3)
                    {
                        GUILayout.Label("Current Theme: Light Mode");
                    }
                    if (Theme == 4)
                    {
                        GUILayout.Label("Current Theme: Blue");
                    }
                    if (GUILayout.Button("Change Menu Theme"))
                    {
                        Theme = Theme + 1;
                        if (Theme > 4)
                        {
                            Theme = 0;
                        }
                        SettingsLib.SetValue(Theme, "steal_backGround");
                        Main.RefreshMenu();
                    }
                    // foreach (Main.Button button in Main.buttons)
                    // {
                    //     if (button.changerThing != null)
                    //     {
                    //         string whaat = "1";
                    //         GUILayout.Label(button.ButtonText + "Changer");
                    //         GUILayout.TextField(whaat);
                    //         button.changerThing;
                    //     }
                    // }

                    //slider
                    // you know we have a change theme right? ik its right under i wasnt trying to remake itr lol
                    //give me a fucking slider :thumbsUp:



                    foreach (Steal.Main.Button button in settings)
                    {
                        ButtonManager.DrawLayoutButton(button, new GUIStyle("button"));
                    }//hello

                    GUILayout.Label("SpeedBoost Strength");
                    SettingsLib.Settings.speedBoost = GUILayout.HorizontalSlider(SettingsLib.Settings.speedBoost, 6.5f, 9.5f);// no wiat  what
                    if (GUILayout.Button("Reset Speed"))
                    {
                        SettingsLib.SetValue(6.5f, "steal_Speed");
                    }

                    GUILayout.Label("Flight Strength");
                    SettingsLib.Settings.flightSpeed = GUILayout.HorizontalSlider(SettingsLib.Settings.flightSpeed, 10f, 30f);
                    if (GUILayout.Button("Reset Flight"))
                    {
                        SettingsLib.SetValue(10f, "steal_FlySpeed");
                    }


                    GUILayout.Label("WallWalk Strength");
                    SettingsLib.Settings.WallWalkGravity = GUILayout.HorizontalSlider(SettingsLib.Settings.WallWalkGravity, -6.81f, -12.81f);
                    if (GUILayout.Button("Reset WallWalk"))
                    {
                        SettingsLib.SetValue(-6.81f, "steal_GravityStrength");
                    }

                    GUILayout.Label("FPS Limit: "+Application.targetFrameRate);
                    Application.targetFrameRate = (int)GUILayout.HorizontalSlider(Application.targetFrameRate, -1, 500);

                    GUILayout.EndScrollView();


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

        public Texture2D infectTexture;

        public Texture2D notInfectTexture;

        private Texture2D ApplyColorFilter(Color color)
        {
            Texture2D texture = new Texture2D(30, 30);

            Color[] colors = new Color[30 * 30];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
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
        public static Color textcolor = Color.white;
        public static Color activeTextColor = Color.green;


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
            if (theme == 0)
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
                activeTextColor = Color.white;
            }
            if (theme == 1)
            {
                button.SetPixel(0, 0, new Color(1f, 0.6f, 0.8f)); // Light pink
                button.Apply();
                buttonhovered.SetPixel(0, 0, new Color(1f, 0.6f, 0.8f)); // Light pink
                buttonhovered.Apply();
                buttonactive.SetPixel(0, 0, new Color(1f, 0.7f, 0.9f)); // Light pink
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
                activeTextColor = Color.black;
            }
            if (theme == 2)
            {
                button.SetPixel(0, 0, new Color32(99, 41, 143, 255));
                button.Apply();
                buttonhovered.SetPixel(0, 0, new Color32(114, 51, 161, 255));
                buttonhovered.Apply();
                buttonactive.SetPixel(0, 0, new Color32(99, 41, 143, 255));
                buttonactive.Apply();



                textarea.SetPixel(0, 0, new Color32(99, 41, 143, 255));
                textarea.Apply();
                textareahovered.SetPixel(0, 0, new Color32(114, 51, 161, 255));
                textareahovered.Apply();
                textareaactive.SetPixel(0, 0, new Color32(145, 68, 201, 255));
                textareaactive.Apply();
                label.SetPixel(0, 0, new Color32(114, 51, 161, 255));
                label.Apply();
                textField.SetPixel(0, 0, new Color32(114, 51, 161, 255));
                textField.Apply();
                toggle.SetPixel(0, 0, new Color32(114, 51, 161, 255));
                toggle.Apply();
                scrollView.SetPixel(0, 0, new Color32(114, 51, 161, 255));
                scrollView.Apply();
                dropdown.SetPixel(0, 0, new Color32(114, 51, 161, 255));
                dropdown.Apply();
                slider = CreateRoundedTexture(32, new Color32(114, 51, 161, 255));
                progressBar.SetPixel(0, 0, new Color32(114, 51, 161, 255));
                progressBar.Apply();
                box.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f));
                box.Apply();
                windowbackground = CreateRoundedTexture(25, new Color(0.1f, 0.1f, 0.1f));
                textcolor = Color.white;
                activeTextColor = Color.white;
            }

            if (theme == 3)
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
                activeTextColor = Color.black;
            }
            if (theme == 4)
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
                activeTextColor = Color.white;
            }
            if (theme == 5)
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
        public static void DrawLayoutButtonLegacy(string label, bool isActive, bool isToggle, bool PlaySound, GUIStyle style, Action<bool> onClick)
        {
            if (isActive)
            {
                GUI.backgroundColor = GUI.skin.button.active.background.GetPixel(0,0);
                GUI.contentColor = DesignLibrary.activeTextColor;
            }
            else
            {
                GUI.contentColor = DesignLibrary.textcolor;
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
                        AssetLoader.Instance.PlayClick();
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
            GUI.contentColor = DesignLibrary.textcolor;
            GUI.backgroundColor = Color.white;
        }

        public static void DrawLayoutButton(Main.Button button, GUIStyle style, Action optinalAction = null)
        {
            if (button.Enabled == true)
            {
                GUI.backgroundColor = GUI.skin.button.active.background.GetPixel(0, 0);
                GUI.contentColor = DesignLibrary.activeTextColor;
            }
            else
            {
                GUI.contentColor = DesignLibrary.textcolor;
            }
            if (GUILayout.Button(button.ButtonText, style))
            {
                Main.Toggle(button);
                AssetLoader.Instance.PlayClick();
            }


            GUI.contentColor = DesignLibrary.textcolor;
            GUI.backgroundColor = Color.white;
        }

        public static bool DrawPageButton(string lable, int i, GUIStyle style, Action optinalAction = null)
        {
            GUI.backgroundColor = Color.white;
            if (StealGUI.tab == i)
            {
                GUI.backgroundColor = GUI.skin.button.active.background.GetPixel(0, 0);
                GUI.contentColor = DesignLibrary.activeTextColor;
            }
            else
            {
                GUI.contentColor = DesignLibrary.textcolor;
            }
            bool result = false;
            if (GUILayout.Button(lable, style))
            {
                result = true;
                AssetLoader.Instance.PlayClick();
            }
            GUI.backgroundColor = Color.white;
            GUI.contentColor = DesignLibrary.textcolor;
            return result;
        }

        public static bool DrawPageButton(string lable, Steal.Main.Category i, GUIStyle style, Action optinalAction = null)
        {
            if (StealGUI.currentGUIPage == i)
            {
                GUI.backgroundColor = GUI.skin.button.active.background.GetPixel(0, 0);
                GUI.contentColor = DesignLibrary.activeTextColor;
            }
            else
            {
                GUI.contentColor = DesignLibrary.textcolor;
            }
            bool result = false;
            if (GUILayout.Button(lable, style))
            {
                result = true;
                AssetLoader.Instance.PlayClick();
            }

            GUI.contentColor = DesignLibrary.textcolor;
            GUI.backgroundColor = Color.white;
            return result;
        }
    }
}
