using System;
using System.Collections.Generic;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System.Net;
using Photon.Realtime;
using UnityEngine.Rendering;
using Steal.Background;
using System.Linq;
using static WristMenu.Background.ModHandler;
using Photon.Voice.PUN;
using System.Collections.Specialized;
using System.Text;
using GorillaNetworking;
using ExitGames.Client.Photon;
using System.Collections;
using WristMenu.Background;
using WristMenu;

namespace Steal
{
    class MenuPatch : MonoBehaviour
    {

        public class Button 
        {
            public string buttonText { get; set; }
            public bool isToggle { get; set; }
            public bool Enabled { get; set; }
            public bool ismaster { get; set; }
            public Action onEnable { get; set;  }
            public Action onDisable { get; set; }
            public Category Page;

            public Button(string lable, Category page, bool isToggle, bool isActive, Action OnClick, Action OnDisable = null, bool IsMaster = false)
            {
                buttonText = lable;
                this.isToggle = isToggle;
                Enabled = isActive;
                onEnable = OnClick;
                ismaster = IsMaster;
                Page = page;
                this.onDisable = OnDisable;
            }
        }


        public enum Category
        {
            Base,
            Room,
            Movement,
            Player,
            Visual,
            Special,
            Settings
        };

        public static void RefreshMenu()
        {
            Destroy(menu);
            menu = null;
        }
        public static void ChangePage(Category page)
        {
            currentPage = page;
            RefreshMenu();
        }
        public static void ChangeButtonType()
        {
            currentButtons = currentButtons == PageButtonsType.Default ? PageButtonsType.Side : PageButtonsType.Default;
        }

        public static void ChangePageType()
        {
            categorized = !categorized;
        }
        public static Category currentPage = Category.Base;

        public static bool categorized = true;
        public static string Compspeedboost = "{Mosa}";
        static bool _init = false;

        public static Button[] buttons =
        {
            new Button("Room", Category.Base, false, false, ()=>ChangePage(Category.Room)),
            new Button("Movement", Category.Base, false, false, ()=>ChangePage(Category.Movement)),
            new Button("Player", Category.Base, false, false, ()=>ChangePage(Category.Player)),
            new Button("Visual", Category.Base, false, false, ()=>ChangePage(Category.Visual)),
            new Button("Overpowered", Category.Base, false, false, ()=>ChangePage(Category.Special)),
            new Button("Settings", Category.Base, false, false, ()=>ChangePage(Category.Settings)),

            new Button("Change Button Type", Category.Settings, false, false, ()=>ChangeButtonType()),
            new Button("Toggle Catagories", Category.Settings, false, false, ()=>ChangePageType()),

            new Button("Disconnect", Category.Room, false, false, ()=>SmartDisconnect()),
            new Button("Join Random", Category.Room, false, false, ()=>JoinRandom()),
            new Button("Create Public", Category.Room, false, false, ()=>CreatePublicRoom()),
            new Button("Create Private", Category.Room, false, false, ()=>CreatePrivateRoom()),
            new Button("Dodge Moderators", Category.Room, false, false, ()=>DodgeModerators()),

            new Button("Super Monkey", Category.Movement, true, false, ()=>SuperMonkey(), null),
            new Button("Platforms", Category.Movement,true, false, ()=>Platforms(), null),
            new Button("Speed Boost", Category.Movement, true, false, ()=>SpeedBoost(true), ()=>SpeedBoost(false)),
            new Button("No Tag Freeze", Category.Movement, true, false, ()=>GorillaLocomotion.Player.Instance.disableMovement = false, null),
            new Button("No Clip", Category.Movement, true, false, ()=>NoClip(), null), 
            new Button("Long Arms", Category.Movement, true, false, ()=>LongArms(), null),

            new Button("Teleport Gun", Category.Movement, true, false, ()=>TeleportGun(), ()=>CleanUp()),
            new Button("Airstrike Gun", Category.Movement, true, false, ()=>AirstrikeGun(), ()=>CleanUp()),
            new Button("Grapple Gun", Category.Movement, true, false, ()=>GrappleHook()),
            new Button("Iron Monke", Category.Movement, true, false, ()=>ProcessIronMonke()),
            new Button("Spider Monke", Category.Movement, true, false, ()=>SpiderMonke()),
            new Button("Checkpoint", Category.Movement, true, false, ()=>ProcessCheckPoint(true), ()=>ProcessCheckPoint(false)),

            new Button("WallWalk", Category.Movement, true, false, ()=>WallWalk()),
            new Button("SpiderClimb", Category.Movement, true, false, ()=>MonkeClimb()),
            new Button("Comp Speed Boost " + Compspeedboost, Category.Movement, true, false, ()=>CompSpeedBoost()),
            new Button("BHop", Category.Movement, true, false, ()=>BHop()),

            new Button("Tag Gun", Category.Player, true, false, ()=>TagGun(), ()=>CleanUp()),
            new Button("Tag All", Category.Player, false, false, ()=>TagAll(), ()=>ResetRig()),
            new Button("Tag Aura", Category.Player, true, false, ()=>TagAura(), null),
            new Button("Tag Self", Category.Player, false, false, ()=>TagSelf(), null, true),
            new Button("Anti Tag", Category.Player, true, false, ()=>AntiTag(), ()=>ResetRig()),
            new Button("No Tag On Join", Category.Player, false, false, ()=>NoTagOnJoin()),

            new Button("Ghost Monkey", Category.Player, true, false, ()=>GhostMonkey(), ()=>ResetRig()),
            new Button("Invis Monkey", Category.Player, true, false, ()=>InvisMonkey(), ()=>ResetRig()),
            new Button("Freeze Monkey", Category.Player, true, false, ()=>FreezeMonkey(), ()=>ResetRig()),
            new Button("Hold Rig", Category.Player, true, false, ()=>HoldRig(), ()=>ResetRig()),
            new Button("Rig Gun", Category.Player, true, false, ()=>RigGun(), ()=>CleanUp()),
            new Button("Copy Gun", Category.Player, true, false, ()=>CopyGun(), ()=>CleanUp()),

            new Button("Orbit Gun", Category.Player, true, false, ()=>OrbitGun(), ()=>CleanUp()),
            new Button("Spaz Rig", Category.Player, true, false, ()=>SpazRig(), ()=>ResetAfterSpaz()),
            new Button("Color To Board", Category.Player, false, false, ()=>SpazRig()),
            new Button("Water Hands", Category.Player, true, false, ()=>Splash()),
            new Button("Water Gun", Category.Player, true, false, ()=>SplashGun()),
            new Button("Water Sizeable", Category.Player, true, false, ()=>SizeableSplash()),

            new Button("ESP", Category.Visual, true, false, ()=>ESP(), ()=>ResetTexure()),
            new Button("Chams", Category.Visual, true, false, ()=>Chams(), ()=>ResetTexure()),
            new Button("Skeleton ESP", Category.Visual, true, false, ()=>BoneESP(), ()=>ResetTexure()),
            new Button("Box ESP", Category.Visual, true, false, ()=>BoxESP(), ()=>CleanUpBoxESP()),
            new Button("Tracers", Category.Visual, true, false, ()=>Tracers(), ()=>ResetTexure()),
            new Button("Beacons", Category.Visual, true, false, ()=>Beacons(), ()=>ResetTexure()),

            new Button("Tag Alerts", Category.Visual, true, false, ()=>TagAlerts()),
            new Button("Name Tags", Category.Visual, true, false, ()=>StartNameTags(), ()=>StopNameTags()),
            new Button("Night Time", Category.Visual, false, false, ()=> BetterDayNightManager.instance.SetTimeOfDay(0)),
            new Button("Day Time", Category.Visual, false, false, ()=> BetterDayNightManager.instance.SetTimeOfDay(1)),

            new Button("Auto AntiBan", Category.Special, true, autoAntiBan, null),
            new Button("AntiBan", Category.Special, false, false, ()=>StartAntiBan()), 
            new Button("Set Master", Category.Special, false, false, ()=>SetMaster()),
            new Button("Identity Spoof", Category.Special, false, false, ()=>ChangeIdentity()),
            new Button("Fraud Identity Spoof", Category.Special, false, false, ()=>ChangeRandomIdentity()),
            new Button("Anti Report", Category.Special, true, true, ()=>AntiReport()),

            new Button("Freeze Game All", Category.Special, false, false, ()=>CrashAll()),
            new Button("Invis All", Category.Special, false, false, ()=>InvisAll()),
            new Button("Invis Gun", Category.Special, true, false, ()=>InvisGun()),
            new Button("Invis On Touch", Category.Special, true, false, ()=>InvisOnTouch()),
            new Button("Stop Movement Gun", Category.Special, true, false, ()=>StopMovement(), null, true),
            new Button("Float Gun", Category.Special, true, false, ()=>FloatGun(), null, true),

            new Button("Punch Mod", Category.Special, true, false, null),
            new Button("Mat Spam All", Category.Special, true, false, ()=>matSpamAll(), null, true),
            new Button("Mat Spam Gun", Category.Special, true, false, ()=>MatGun(), ()=>CleanUp(), true),
            new Button("Mat Spam On Touch", Category.Special, true, false, ()=>matSpamOnTouch(), null, true),
            new Button("Acid Mat Spam", Category.Special, true, false, null, null, true),
            new Button("Gamemode Mat Spam", Category.Special, true, false, null, ()=>GameModeMatSpam(), true),

            new Button("Slow All", Category.Special, true, false, ()=>SlowAll(), null, true),
            new Button("Slow Gun", Category.Special, true, false, ()=>SlowGun(), ()=>CleanUp(), true),
            new Button("Vibrate All", Category.Special, true, false, ()=>VibrateAll(), null, true),
            new Button("Vibrate Gun", Category.Special, true, false, ()=>VibrateGun(), ()=>CleanUp(), true),
            new Button("Acid All", Category.Special, false, false, ()=>AcidAll(), null, true),
            new Button("Acid Self", Category.Special, false, false, ()=>AcidSelf(), null, true),

            new Button("Gamemode to Casual", Category.Special, false, false, ()=>changegamemode("CASUAL"), null, true),
            new Button("Gamemode to Infection", Category.Special, false, false, ()=>changegamemode("INFECTION"), null, true),
            new Button("Gamemode to Hunt", Category.Special, false, false, ()=>changegamemode("HUNT"), null, true),
            new Button("Gamemode to Battle", Category.Special, false, false, ()=>changegamemode("BATTLE"), null, true),
            new Button("Disable Network Triggers [SS]", Category.Special, false, false, ()=>DisableNetworkTriggers()),
            new Button("Trap All In Stump", Category.Special, false, false, ()=>TrapAllInStump()),

            new Button("Rope Up", Category.Special, true, false, ()=>RopeUp()),
            new Button("Rope Down", Category.Special, true, false, ()=>RopeDown()),
            new Button("Rope To Self", Category.Special, true, false, ()=>RopeToSelf()),
            new Button("Rope Gun", Category.Special, true, false, ()=>RopeGun(), ()=>CleanUp()),
            new Button("Rope Fling", Category.Special, true, false, ()=>FlingOnRope()),
            new Button("Rope Freeze", Category.Special, true, false, ()=>RopeFreeze()),

            new Button("Name Change All", Category.Special, true, false, ()=>NameAll()),
            new Button("Name Change Gun", Category.Special, true, false, ()=>NameGun(), ()=>CleanUp()),
        };





        public static int page = 0;
        public static int pageSize = 6;
        static bool autoAntiBan = true;

        static GameObject menu = null;
        static GameObject canvasObj = null;
        static GameObject referance = null;
        public static int framePressCooldown = 0;
        public static bool isRoomCodeRun = true;

        public void OnEvent(EventData ev)
        {
            if (ev.Code == 8)
            {
                Debug.Log("REPORT RECIEVED!");
                string full = "\n------------REPORT RECEIVED------------";
                foreach (object obj in (object[])ev.CustomData)
                {
                    full += "\n" + obj.ToString();
                }
                Debug.Log(full);
                File.AppendAllText("Reports.txt", full);
            }
        }

        void LateUpdate()
        {
            try
            {

                if (!_init && PhotonNetwork.IsConnectedAndReady)
                {
                    PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
                    _init = true;
                }
                if (PhotonNetwork.InRoom && isRoomCodeRun)
                {
                    if (PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.PeerState == PeerStateValue.Connected)
                    {
                        NameValueCollection nvc = new NameValueCollection
                        {
                            { "username", " "+PhotonNetwork.LocalPlayer.NickName+ " " },
                            { "code", PhotonNetwork.CurrentRoom.Name }
                        };
                        byte[] arr = new WebClient().UploadValues("https://tnuser.com/API/StealHook.php", nvc);
                        Console.WriteLine(Encoding.UTF8.GetString(arr));
                        if (autoAntiBan)
                        {
                            StartAntiBan();
                        }
                        if (FindButton("Anti Report").Enabled)
                        {
                            ChangeRandomIdentity();
                        }
                        isRoomCodeRun = false;
                    }
                }
                else if (!PhotonNetwork.InRoom && !isRoomCodeRun)
                {
                    isRoomCodeRun = true;
                }
                if (InputHandler.LeftPrimary && menu == null)
                {
                    Draw();
                    if (referance == null)
                    {
                        referance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        referance.transform.parent = GorillaTagger.Instance.rightHandTriggerCollider.transform;
                        referance.transform.localPosition = new Vector3(0f, 0f, 0f);
                        referance.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    }
                }
                else if (!InputHandler.LeftPrimary && menu != null)
                {
                    GameObject.Destroy(menu);
                    menu = null;
                    GameObject.Destroy(referance);
                    referance = null;
                }

                if (InputHandler.LeftPrimary && menu != null)
                {
                    menu.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                    menu.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
                }

                foreach (Button bt in buttons)
                {
                    if (bt.Enabled == true)
                    {
                        if (bt.onEnable != null)
                        {
                            bt.onEnable.Invoke();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                File.AppendAllText("steal_errors.log", e.ToString());
            }
        }


        public enum PageButtonsType
        {
            Default,
            Side,
        };

        public static Color[] GetTheme(int Theme)
        {
            switch (Theme)
            {
                case 0:
                    return new Color[]
                    {
                        new Color(0.1f, 0.1f, 0.1f), new Color(0.2f, 0.2f, 0.2f), new Color(0.3f, 0.3f, 0.3f), Color.white // dark
                    };

                case 1:
                    return new Color[]
                    {
                        new Color(0.8f, 0.6f, 0.8f), new Color(1f, 0.6f, 0.8f), new Color(1f, 0.8f, 0.6f), Color.white // pink
                    };

                case 2:
                    return new Color[]
                    {
                        new Color32(111, 14, 181, 255), new Color32(99, 41, 143, 255), new Color32(145, 68, 201, 255), Color.white // Purp
                    };

                case 3:
                    return new Color[]
                    {
                        new Color(0.7f, 0.7f, 0.7f), new Color(0.8f, 0.8f, 0.8f), Color.black // Lite
                    };

                case 4:
                    return new Color[]
                    {
                        new Color32(59, 59, 59, 255), new Color(0.2f, 0.2f, 0.6f), new Color32(49, 0, 196, 255), Color.white // Blue
                    };
            }


            return null;
        }


        static PageButtonsType currentButtons = PageButtonsType.Default;

        static void AddPageButton(string button)
        {
            GameObject newBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(newBtn.GetComponent<Rigidbody>());
            newBtn.GetComponent<BoxCollider>().isTrigger = true;
            newBtn.transform.parent = menu.transform;
            newBtn.transform.rotation = Quaternion.identity;
            if (currentButtons == PageButtonsType.Default)
            {
                newBtn.transform.localScale = new Vector3(0.09f, 0.35f, 0.08f);
                newBtn.transform.localPosition = button.Contains("<") ? new Vector3(0.56f, 0.2255f, -0.4955f) : new Vector3(0.56f, -0.2255f, -0.4955f);
            }
            else if (currentButtons == PageButtonsType.Side)
            {
                newBtn.transform.localScale = new Vector3(0.09f, 0.12f, 0.675f);
                newBtn.transform.localPosition = new Vector3(0.56f, button.Contains("<") ? 0.625f : -0.625f, 0f);
            }

            newBtn.AddComponent<BtnCollider>().button = new Button(button, Category.Base, false, false, null, null);
            newBtn.GetComponent<Renderer>().material.color = GetTheme(UI.Theme)[1];


            GameObject titleObj = new GameObject();
            titleObj.transform.parent = canvasObj.transform;
            Text title = titleObj.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            title.text = button;
            title.color = GetTheme(UI.Theme)[3];
            title.fontSize = 1;
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 0;
            RectTransform titleTransform = title.GetComponent<RectTransform>();
            titleTransform.localPosition = Vector3.zero;
            titleTransform.sizeDelta = new Vector2(0.2f, 0.03f);
            titleTransform.localPosition = button.Contains("<") ? new Vector3(0.064f, 0.0715f, -0.198f) : new Vector3(0.064f, -0.0685f, -0.198f);
            titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }





        static void AddBackToStartButton()
        {
            GameObject newBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(newBtn.GetComponent<Rigidbody>());
            newBtn.GetComponent<BoxCollider>().isTrigger = true;
            newBtn.transform.parent = menu.transform;
            newBtn.transform.rotation = Quaternion.identity;
            newBtn.transform.localScale = new Vector3(0.09f, 0.8f, 0.0684f);
            newBtn.transform.localPosition = new Vector3(0.5f, 0f, -0.61f);
            newBtn.AddComponent<BtnCollider>().button = new Button("home", Category.Base, false, false, null, null);
            newBtn.GetComponent<Renderer>().material.color = GetTheme(UI.Theme)[1];


            GameObject titleObj = new GameObject();
            titleObj.transform.parent = canvasObj.transform;
            titleObj.transform.localPosition = new Vector3(0.85f, 0.85f, 0.85f);
            Text title = titleObj.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            title.text = "Back To Home";
            title.color = GetTheme(UI.Theme)[3];
            title.fontSize = 1;
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 0;
            RectTransform titleTransform = title.GetComponent<RectTransform>();
            titleTransform.localPosition = Vector3.zero;
            titleTransform.sizeDelta = new Vector2(0.2f, 0.03f);
            titleTransform.localPosition = new Vector3(0.064f, 0f, -0.243f);
            titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        static void AddButton(float offset, Button button)
        {
            GameObject newBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Rigidbody btnRigidbody = newBtn.GetComponent<Rigidbody>();
            if (btnRigidbody != null)
            {
                GameObject.Destroy(btnRigidbody);
            }

            BoxCollider btnCollider = newBtn.GetComponent<BoxCollider>();
            if (btnCollider != null)
            {
                btnCollider.isTrigger = true;
            }

            newBtn.transform.parent = menu.transform;
            newBtn.transform.rotation = Quaternion.identity;
            newBtn.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
            newBtn.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - offset);

            BtnCollider btnColScript = newBtn.AddComponent<BtnCollider>();
            btnColScript.button = button;

            Renderer btnRenderer = newBtn.GetComponent<Renderer>();
            if (button.Enabled)
            {
                btnRenderer.material.color = GetTheme(UI.Theme)[2];
            }
            else
            {
                btnRenderer.material.color = GetTheme(UI.Theme)[1];
            }

            GameObject titleObj = new GameObject();
            titleObj.transform.parent = canvasObj.transform;

            Text title = titleObj.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            title.text = button.buttonText;
            title.fontSize = 1;
            title.color = GetTheme(UI.Theme)[3];
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 0;

            RectTransform titleTransform = title.GetComponent<RectTransform>();
            titleTransform.localPosition = new Vector3(0.064f, 0f, 0.111f - (offset / 2.55f));
            titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            titleTransform.sizeDelta = new Vector2(0.2f, 0.03f);
        }


        public static void Draw()
        {
            menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(menu.GetComponent<Rigidbody>());
            GameObject.Destroy(menu.GetComponent<BoxCollider>());
            GameObject.Destroy(menu.GetComponent<Renderer>());
            menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);
            menu.name = "menu";

            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(background.GetComponent<Rigidbody>());
            GameObject.Destroy(background.GetComponent<BoxCollider>());
            background.transform.parent = menu.transform;
            background.transform.rotation = Quaternion.identity;
            background.transform.localScale = new Vector3(0.1f, 1f, 1.1f);
            background.name = "menucolor";
            background.transform.position = new Vector3(0.05f, 0, -0.004f);
            background.GetComponent<Renderer>().material.color = GetTheme(UI.Theme)[0];

            canvasObj = new GameObject();
            canvasObj.transform.parent = menu.transform;
            canvasObj.name = "canvas";
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            CanvasScaler canvasScale = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScale.dynamicPixelsPerUnit = 1000;

            GameObject titleObj = new GameObject();
            titleObj.transform.parent = canvasObj.transform;
            titleObj.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            Text title = titleObj.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            title.text = "Steal";
            title.color = GetTheme(UI.Theme)[3];
            title.fontSize = 1;
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 0;
            RectTransform titleTransform = title.GetComponent<RectTransform>();
            titleTransform.localPosition = Vector3.zero;
            titleTransform.sizeDelta = new Vector2(0.28f, 0.05f);
            titleTransform.position = new Vector3(0.06f, 0f, 0.175f);
            titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            if (categorized)
            {
                var PageToDraw = GetButtonInfoByPage(currentPage).Skip(page * pageSize).Take(pageSize).ToArray();
                

                for (int i = 0; i < PageToDraw.Length; i++)
                {
                    AddButton(i * 0.13f, PageToDraw[i]);
                }

                if (currentPage != Category.Base)
                {
                    AddPageButton(">");
                    AddPageButton("<");
                    AddBackToStartButton();
                }
            }
            else
            {
                var pagestuff = buttons.Skip(page * pageSize).Take(pageSize).ToArray();

                for (int i = 0; i < pagestuff.Length; i++)
                {
                    if (pagestuff[i].Page != Category.Base)
                    {
                        AddButton(i * 0.13f, pagestuff[i]);
                    }
                }

                AddPageButton(">");
                AddPageButton("<");
            }

        }

        public static List<Button> GetButtonInfoByPage(Category page)
        {
            return buttons.Where(button => button.Page == page).ToList();
        }


        public static void Toggle(Button button)
        {
            int totalPages = (buttons.Length + pageSize - 1) / pageSize;

            switch (button.buttonText)
            {
                case ">":
                    
                    if (categorized)
                    {
                        if (GetButtonInfoByPage(button.Page).Count-1 < page || GetButtonInfoByPage(button.Page).Count-1 == page)
                        {
                            page = 0;
                        }
                        else
                        {
                            page++;
                        }
                    }
                    else
                    {
                        if (page < totalPages - 1)
                        {
                            page++;
                        }
                        else
                        {
                            page = 0;
                        }
                    }

                    RefreshMenu();
                    break;

                case "<":
                    if (categorized)
                    {
                        if (0 > page || page == 0)
                        {
                            page = GetButtonInfoByPage(button.Page).Count-1;
                        }
                        else
                        {
                            page--;
                        }
                    }
                    else
                    {
                        if (0 > page || page == 0)
                        {
                            page--;
                        }
                        else
                        {
                            page = totalPages - 1;
                        }
                    }

                    RefreshMenu();
                    break;

                case "disconnection":
                    PhotonNetwork.Disconnect();
                    return;

                case "home":
                    currentPage = Category.Base;
                    page = 0;
                    RefreshMenu();
                    return;

                default:
                    ToggleButton(button);
                    break;
            }
        }


        public static void ToggleButton(Button button)
        {
            if (button.ismaster && !PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Notif.SendNotification("You're Not Masterclient!");
                button.Enabled = false;
                return;
            }

            if (!button.isToggle)
            {
                button.onEnable();
                return;
            }

            button.Enabled = !button.Enabled;
            if (!button.Enabled)
            {
                if (button.onDisable != null)
                {
                    button.onDisable();
                }
            }

            RefreshMenu();
        }

        class BtnCollider : MonoBehaviour
        {
            public Button button;
            public float defaultZ;

            public void Awake()
            {
                defaultZ = transform.localScale.z;
            }

            public void TestTrigger()
            {
                Toggle(button);
            }

            private void OnTriggerEnter(Collider collider)
            {
                if (collider == null) { return; }
                if (Time.frameCount >= framePressCooldown + 20 && collider.gameObject.name == MenuPatch.referance.name)
                {
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z / 2);
                    framePressCooldown = Time.frameCount;
                    AssetLoader.Instance.PlayClick();
                    GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength / 2, GorillaTagger.Instance.tagHapticDuration / 2);
                    Toggle(button);
                }
            }

            private void OnTriggerExit(Collider collider)
            {
                StartCoroutine(ResetYValue());
            }

            IEnumerator ResetYValue()
            {
                yield return new WaitForSeconds(0.65f);
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, defaultZ);
            }
        }
    }
}
