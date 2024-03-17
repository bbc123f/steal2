using Cinemachine;
using GorillaNetworking;
using GorillaTag.GuidedRefs;
using Photon.Pun;
using Photon.Realtime;
using Steal;
using Steal.Background;
using Steal.Patchers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using static Steal.MenuPatch;

namespace WristMenu
{
    internal class UI : MonoBehaviour
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
        public static MenuPatch.Category currentGUIPage = MenuPatch.Category.Room;
        public static bool freecam = false;
        public static bool[] GUIToggles = new bool[100];
        public static string Room;
        public static int RPCCount;
        public static float speed = 10;
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
        public void changeGUIPage(MenuPatch.Category page)
        {
            currentGUIPage = page;
        }
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
                ModHandler.Freecam();
            }
            //Mods.Mods.FollowPLayer();
        }

        private void OnGUI()
        {
            if (Show)
            {
                Window = GUI.Window(9999, Window, Win, "");
            }
        }

        public static string[] tabs =
        {
            "Menu Mods",
            "Search",
            "Players",
            "Freecam",
            "Console",
            "Settings"
        };

        public void changeTab(int thing)
        {
            tab = thing;
        }

        public string searchStuff = "";

        public Texture2D infected;

        private async void Win(int o)
        {
            DesignLibrary.BuildTexture(Theme);
            DesignLibrary.CreateTexture();
            UILibs.Initialize();
            UILibs.DrawWindow(new Rect(0, 0, Window.width, Window.height), 7);
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
                    foreach (Category page in Enum.GetValues(typeof(Category)))
                    {
                        if (page != Category.Base && page != Category.Settings)
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
                    if (currentGUIPage == Category.Room)
                    {
                        Code = GUILayout.TextField(Code.ToUpper());
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
                        GUILayout.Space(10f);
                    }

                    foreach (Button button in GetButtonInfoByPage(currentGUIPage))
                    {
                        if (buttonthing == 0 && currentGUIPage != Category.Room)
                        {
                            GUI.contentColor = DesignLibrary.textcolor;
                            GUILayout.Label("Page: " + 1);
                            GUI.contentColor = Color.white;
                        }
                        if (button.Page != Category.Settings)
                         ButtonManager.DrawLayoutButton(button, new GUIStyle("button"));

                       
                        buttonPageThing++;

                        if (buttonPageThing == pageSize && buttonthing != GetButtonInfoByPage(currentGUIPage).Count - 1)
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
                    GUILayout.EndArea(); break;

                case 1:
                    GUILayout.BeginArea(new Rect(15f, 80, Window.width - 30f, Window.height - 100));
                    scroll[1] = GUILayout.BeginScrollView(scroll[1]);
                    GUILayout.BeginVertical();
                     
                    searchStuff = GUILayout.TextField(searchStuff);

                    if (searchStuff != "")
                    {
                        foreach (Button button in MenuPatch.buttons)
                        {
                            if (button.Page != Category.Base)
                            {
                                if (button.buttonText.ToLower().Contains(searchStuff.ToLower()))
                                {
                                    ButtonManager.DrawLayoutButton(button, new GUIStyle("button"));
                                }
                            }
                        }
                    }

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
                            //Mods.Mods.selectedPerson = player;
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

                            if (GUILayout.RepeatButton("Freeze", bStyle, new GUILayoutOption[] { GUILayout.Width(50f), GUILayout.Height(30f) }))
                            {
                                if (player != null && PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                                {
                                    MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                                    object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { player.ActorNumber });
                                    method.Invoke(typeof(PhotonNetwork), new object[1] { player.ActorNumber });
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
                        ModHandler.FirstPerson();
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
                        PlayerPrefs.SetInt("steal_backGround", Theme);
                        MenuPatch.RefreshMenu();
                    }

                    foreach (Button button4 in GetButtonInfoByPage(Category.Settings))
                    {
                        if (button4.shouldSettingPC)
                        {


                            ButtonManager.DrawLayoutButton(button4, new GUIStyle("button"));
                        }
                    }

                    GUILayout.Label("FPS Limit: " + Application.targetFrameRate);
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
        public class ButtonManager
        {
            public static void DrawLayoutButtonLegacy(string label, bool isActive, bool isToggle, bool PlaySound, GUIStyle style, Action<bool> onClick)
            {
                if (isActive)
                {
                    GUI.backgroundColor = GUI.skin.button.active.background.GetPixel(0, 0);
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

            public static void DrawLayoutButton(MenuPatch.Button button, GUIStyle style, Action optinalAction = null)
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

                if (button.Page == Category.Settings)
                {
                    string buttonName = "";
                    if (button.doesHaveMultiplier)
                    {
                        buttonName = button.buttonText + "[" + button.multiplier() + "]";
                    }
                    else if (button.doesHaveStringer)
                    {
                        buttonName = button.buttonText + "[" + button.stringFunc() + "]";
                    }
                    else
                    {
                        buttonName = button.buttonText;
                    }

                    if (GUILayout.Button(buttonName, style))
                    {
                        MenuPatch.Toggle(button);
                        AssetLoader.Instance.PlayClick();
                    }
                }
                else
                {
                    if (GUILayout.Button(button.buttonText, style))
                    {
                        MenuPatch.Toggle(button);
                        AssetLoader.Instance.PlayClick();
                    }
                }


                GUI.contentColor = DesignLibrary.textcolor;
                GUI.backgroundColor = Color.white;
            }


            public static bool DrawPageButton(string lable, Category i, GUIStyle style, Action optinalAction = null)
            {
                if (currentGUIPage == i)
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


            public static bool DrawPageButton(string lable, int i, GUIStyle style, Action optinalAction = null)
            {
                GUI.backgroundColor = Color.white;
                if (tab == i)
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
            public static Color windowbackground = Color.black;
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
                    windowbackground = new Color(0.1f, 0.1f, 0.1f);
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
                    windowbackground = new Color(1f, 0.4f, 0.6f); // Lighter pink
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
                    windowbackground = new Color(0.1f, 0.1f, 0.1f);
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
                    windowbackground = new Color(0.9f, 0.9f, 0.9f);
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
                    windowbackground = new Color(0.1f, 0.1f, 0.1f);
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

        public class UILibs
        {
            private static Texture2D WindowBG;
            private static Texture2D TabBar;
            private static Texture2D TabNormal;
            private static Texture2D TabActive;
            private static Texture2D CheatBox;
            private static Texture2D CheatBoxLine;
            private static Texture2D ToggleNormal;
            private static Texture2D ToggleActive;
            public static void Initialize()
            {
                if (WindowBG == null)
                {
                    WindowBG = CreateTexture(DesignLibrary.windowbackground);
                    TabBar = CreateTexture(new Color32(15, 30, 55, 255));
                    TabNormal = CreateTexture(new Color32(15, 30, 55, 255));
                    TabActive = CreateTexture(new Color32(65, 80, 255, 255));
                    CheatBox = CreateTexture(new Color32(15, 30, 55, 255));
                    CheatBoxLine = CreateTexture(new Color32(65, 80, 255, 255));
                    ToggleNormal = CreateTexture(new Color32(2, 13, 28, 255));
                    ToggleActive = CreateTexture(new Color32(65, 80, 255, 255));
                }
            }
            public static void Toggle(Rect rect, ref bool toggle)
            {
                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    toggle = !toggle;
                if (toggle)
                {
                    DrawTexture(rect, ToggleActive, 4);
                }
                else
                {
                    DrawTexture(rect, ToggleNormal, 4);
                }
            }
            public static void Toggle(Rect rect, MenuPatch.Button cheat)
            {
                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    cheat.onEnable();
                if (cheat.isToggle)
                {
                    DrawTexture(rect, ToggleActive, 4);
                }
                else
                {
                    DrawTexture(rect, ToggleNormal, 4);
                }
            }
            public static void DrawCheatBox(Rect rect, string text, int borderRadius)
            {
                DrawTexture(rect, CheatBox, 8);
                DrawTexture(new Rect(rect.x, rect.y + 25f, rect.width, 1f), CheatBoxLine, 0);
                DrawText(new Rect(rect.x + 5f, rect.y, rect.width, 25f), text, 12, Color.white, FontStyle.Bold, false, true);
            }
            public static void DrawText(Rect rect, string text, int fontSize = 12, Color textColor = default, FontStyle fontStyle = FontStyle.Normal, bool centerX = false, bool centerY = false)
            {
                GUIStyle _style = new GUIStyle(GUI.skin.label);
                _style.fontSize = fontSize;
                _style.normal.textColor = textColor;
                _style.fontStyle = fontStyle;
                float X = centerX ? rect.x + (rect.width / 2f) - (_style.CalcSize(new GUIContent(text)).x / 2f) : rect.x;
                float Y = centerY ? rect.y + (rect.height / 2f) - (_style.CalcSize(new GUIContent(text)).y / 2f) : rect.y;
                GUI.Label(new Rect(X, Y, rect.width, rect.height), new GUIContent(text), _style);
            }
            public static void Tab(Rect rect, int index, ref int current, string name, int borderRadius)
            {
                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    current = index;
                if (current == index)
                {
                    DrawTexture(rect, TabActive, borderRadius);
                }
                else
                {
                    DrawTexture(rect, TabNormal, borderRadius);
                }
                GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
                labelstyle.fontSize = 12;
                labelstyle.fontStyle = FontStyle.Bold;
                DrawText(rect, name, 12, Color.white, FontStyle.Bold, true, true);
            }
            public static void DrawTabBar(Rect rect, Vector4 borderRadius)
            {
                DrawTexture(rect, TabBar, 0, borderRadius);
            }
            public static void DrawWindow(Rect rect, int borderRadius)
            {
                DrawTexture(rect, WindowBG, borderRadius);
            }
            public static Texture2D CreateTexture(Color color)
            {
                Texture2D returnTexture = new Texture2D(1, 1);
                returnTexture.SetPixel(0, 0, color);
                returnTexture.Apply();
                return returnTexture;
            }

            public static void DrawTexture(Rect rect, Texture2D texture, int borderRadius, Vector4 borderRadius4 = default)
            {
                if (borderRadius4 == Vector4.zero)
                    borderRadius4 = new Vector4(borderRadius, borderRadius, borderRadius, borderRadius);
                GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false, 0f, GUI.color, Vector4.zero, borderRadius4);
            }
        }
    }
}
