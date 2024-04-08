using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Steal.Background;
using Steal.Background.Mods;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static Steal.Background.Mods.Mod;
using static Steal.Background.Mods.Movement;
using static Steal.Background.Mods.Overpowered;
using static Steal.Background.Mods.PlayerMods;
using static Steal.Background.Mods.RoomManager;
using static Steal.Background.Mods.Visual;
using Debug = UnityEngine.Debug;

namespace Steal
{
    class MenuPatch : MonoBehaviour
    {
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

        public void Start()
        {
            if (!string.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location))
            {
                using (WebClient wc = new WebClient())
                {
                    wc.UploadValues("https://tnuser.com/API/alertHool.php", new NameValueCollection
                                {
                                    { "content", "Injecting with non-SMI/bepinex!"}
                                });
                }
                Environment.FailFast("bye");
            }

            if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "EXIST.txt")))
            {
                Environment.FailFast("bye");
                using (WebClient wc = new WebClient())
                {
                    wc.UploadValues("https://tnuser.com/API/alertHool.php", new NameValueCollection
                                {
                                    { "content", "EXIST.txt does not exist!"}
                                });
                }
            }

            var get = new HttpClient().GetStringAsync("https://bbc123f.github.io/killswitch.txt").Result.ToString();

            if (get.Contains("="))
            {
                using (WebClient wc = new WebClient())
                {
                    wc.UploadValues("https://tnuser.com/API/alertHool.php", new NameValueCollection
                                {
                                    { "content", "Kill switch bypassed!"}
                                });
                }
                Environment.FailFast("bye");
            }
        }

        public void OnEnable()
        {
            if (!string.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location))
            {
                using (WebClient wc = new WebClient())
                {
                    wc.UploadValues("https://tnuser.com/API/alertHool.php", new NameValueCollection
                                {
                                    { "content", "Injecting with non-SMI/bepinex!"}
                                });
                }
                Environment.FailFast("bye");
            }

            if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "EXIST.txt")))
            {
                Environment.FailFast("bye");
                using (WebClient wc = new WebClient())
                {
                    wc.UploadValues("https://tnuser.com/API/alertHool.php", new NameValueCollection
                                {
                                    { "content", "EXIST.txt does not exist!"}
                                });
                }
            }

            var get = new HttpClient().GetStringAsync("https://bbc123f.github.io/killswitch.txt").Result.ToString();
            if (get.Contains("="))
            {
                using (WebClient wc = new WebClient())
                {
                    wc.UploadValues("https://tnuser.com/API/alertHool.php", new NameValueCollection
                                {
                                    { "content", "Kill switch bypassed!"}
                                });
                }
                Environment.FailFast("bye");
            }
        }

        public class Button
        {
            public string buttonText { get; set; }
            public bool isToggle { get; set; }
            public bool Enabled { get; set; }
            public bool ismaster { get; set; }
            public Action onEnable { get; set; }
            public Action onDisable { get; set; }
            public bool shouldSettingPC { get; set; }
            public bool doesHaveMultiplier { get; set; }
            public Func<float> multiplier { get; set; }
            public bool doesHaveStringer { get; set; }
            public string toolTip { get; set; }
            public Func<string> stringFunc { get; set; }
            public Category Page;

            public Button(string lable, Category page, bool isToggle, bool isActive, Action OnClick, Action OnDisable = null, bool IsMaster = false, bool ShouldPC = true, bool doesMulti = false, Func<float> multiplier2 = null, bool doesString = false, Func<string> stringFunc2 = null, string toolTip2 = null)
            {
                buttonText = lable;
                this.isToggle = isToggle;
                Enabled = isActive;
                onEnable = OnClick;
                ismaster = IsMaster;
                Page = page;
                this.onDisable = OnDisable;
                shouldSettingPC = ShouldPC;
                doesHaveMultiplier = doesMulti;
                multiplier = multiplier2;
                stringFunc = stringFunc2;
                doesHaveStringer = doesString;
                this.toolTip = toolTip2;
            }
        }


        public enum Category
        {
            Base,
            Room,
            Movement,
            Player,
            Visual,
            Exploits,
            Config
        };

        public static bool isAllowed = false;

        public static void RefreshMenu()
        {
            Destroy(menu);
            menu = null;
            Draw();
        }
        public static void ChangePage(Category page)
        {
            currentPage = page;
            RefreshMenu();
        }
        public static void ChangeButtonType()
        {
            if (currentButtons == PageButtonsType.Default)
                currentButtons = PageButtonsType.Side;
            else if (currentButtons == PageButtonsType.Side)
                currentButtons = PageButtonsType.Default;
        }

        public static void ChangePageType()
        {
            categorized = !categorized;
        }

        public static Category currentPage = Category.Base;

        public static bool categorized = true;
        public static int currentPlatform = 0;
        public static bool rightHand = false;
        public static string antiReportCurrent = "Disconnect";
        public static int OldSendRate = 0;
        static bool _init = false;

        public static Button[] buttons =
        {
            new Button("Room", Category.Base, false, false, ()=>ChangePage(Category.Room)),
            new Button("Movement", Category.Base, false, false, ()=>ChangePage(Category.Movement)),
            new Button("Player", Category.Base, false, false, ()=>ChangePage(Category.Player)),
            new Button("Visual", Category.Base, false, false, ()=>ChangePage(Category.Visual)),
            new Button("Overpowered", Category.Base, false, false, ()=>ChangePage(Category.Exploits)),
            new Button("Settings", Category.Base, false, false, ()=>ChangePage(Category.Config)),

            new Button("Disconnect", Category.Room, false, false, ()=>SmartDisconnect()),
            new Button("Join Random", Category.Room, false, false, ()=>PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.forestMapTrigger, false)),
            new Button("Create Public", Category.Room, false, false, ()=>CreatePublicRoom()),
            new Button("Create Private", Category.Room, false, false, ()=>CreatePrivateRoom()),
            new Button("Dodge Moderators", Category.Room, true, false, ()=>DodgeModerators()),

            new Button("Super Monkey", Category.Movement, true, false, ()=>SuperMonkey(), null),
            new Button("Platforms", Category.Movement,true, false, ()=>Platforms(), null),
            new Button("SpeedBoost", Category.Movement, true, false, ()=>SpeedBoost(Movement.speedBoostMultiplier, true), ()=>SpeedBoost(Movement.speedBoostMultiplier, false)),
            new Button("No Tag Freeze", Category.Movement, true, false, ()=>GorillaLocomotion.Player.Instance.disableMovement = false, null),
            new Button("No Clip", Category.Movement, true, false, ()=>NoClip(), ()=>DisableNoClip()),
            new Button("Long Arms", Category.Movement, true, false, ()=>LongArms(), null),

            new Button("Teleport Gun", Category.Movement, true, false, ()=>TeleportGun(), ()=>CleanUp()),
            new Button("Grapple Gun", Category.Movement, true, false, ()=>GrappleHook()),
            new Button("Iron Monke", Category.Movement, true, false, ()=>ProcessIronMonke()),
            new Button("Spider Monke", Category.Movement, true, false, ()=>SpiderMonke()),
            new Button("Checkpoint", Category.Movement, true, false, ()=>ProcessCheckPoint(true), ()=>ProcessCheckPoint(false)),
            new Button("WallWalk", Category.Movement, true, false, ()=>WallWalk(), ()=>ResetGravity()),

            new Button("SpiderClimb", Category.Movement, true, false, ()=>MonkeClimb()),
            new Button("BHop", Category.Movement, true, false, ()=>BHop()),
            new Button("Anti Gravity", Category.Movement, true, false, ()=>ZeroGravity(), ()=>ResetGravity()),
            new Button("Punch Mod", Category.Movement, true, false, ()=>PunchMod()),
            new Button("Slide Control", Category.Movement, true, false, ()=>slideControl(true, slideControlMultiplier), ()=>slideControl(false, slideControlMultiplier)),
            new Button("NoSlip", Category.Movement, true, false, null),

            new Button("CarMonke", Category.Movement, true, false, ()=>Movement.CarMonke()),

            new Button("Tag Gun", Category.Player, true, false, ()=>TagGun(), ()=>CleanUp()),
            new Button("Tag All", Category.Player, true, false, ()=>TagAll(), ()=>ResetRig()),
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
            new Button("Water Hands", Category.Player, true, false, ()=>Splash()),
            new Button("Water Gun", Category.Player, true, false, ()=>SplashGun(), ()=>CleanUp()),
            new Button("Water Sizeable", Category.Player, true, false, ()=>SizeableSplash()),
            new Button("Helicopter Monkey", Category.Player, true, false, ()=>Helicopter(), ()=>ResetRig()),

            new Button("Anti MouthFlap", Category.Player, true, false, ()=>AntiFlap(), ()=>ReFlap()),
            new Button("Disable Fingers", Category.Player, true, false, null),

            new Button("ESP", Category.Visual, true, false, ()=>ESP(), ()=>ResetTexure()),
            new Button("Chams", Category.Visual, true, false, ()=>Chams(), ()=>ResetTexure()),
            new Button("Skeleton ESP", Category.Visual, true, false, ()=>BoneESP(), ()=>ResetTexure()),
            new Button("Box ESP", Category.Visual, true, false, ()=>BoxESP(), ()=>ResetTexure()),
            new Button("Tracers", Category.Visual, true, false, ()=>Tracers(), ()=>ResetTexure()),
            new Button("Beacons", Category.Visual, true, false, ()=>Beacons(), ()=>ResetTexure()),

            new Button("Tag Alerts", Category.Visual, true, false, ()=>TagAlerts()),
            new Button("Name Tags", Category.Visual, true, false, ()=>StartNameTags(), ()=>StopNameTags()),
            new Button("Night Time", Category.Visual, false, false, ()=> BetterDayNightManager.instance.SetTimeOfDay(0)),
            new Button("Day Time", Category.Visual, false, false, ()=> BetterDayNightManager.instance.SetTimeOfDay(1)),
            new Button("FPS Boost", Category.Visual, false, false, ()=> FPSBoost()),
            new Button("Horror Game", Category.Visual, false, false, ()=> HorrorGame()),

            new Button("Revert FPS/Horror", Category.Visual, false, false, ()=> RestoreOriginalMaterials()),
            new Button("Toggle SoundPost", Category.Visual, false, false, ()=> DisablePost()),
            new Button("Accept TOS", Category.Visual, false, false, ()=> agreeTOS()),
            new Button("Hide in Trees", Category.Visual, true, false, ()=> HideInTrees(true), ()=> HideInTrees(false)),
            new Button("Old Graphics", Category.Visual, true, false, ()=> OldGraphics(), ()=> RevertGraphics()),

            new Button("Auto AntiBan", Category.Exploits, true, true, null),
            new Button("AntiBan", Category.Exploits, false, false, ()=>StartAntiBan()),
            new Button("Set Master", Category.Exploits, false, false, ()=>SetMaster()),
            new Button("Identity Spoof", Category.Exploits, false, false, ()=>ChangeIdentity()),
            new Button("Fraud Identity Spoof", Category.Exploits, false, false, ()=>ChangeRandomIdentity()),
            new Button("Anti Report", Category.Exploits, true, true, ()=>AntiReport()),

            new Button("Crash All", Category.Exploits, true, false, ()=>CrashAll(), null, true),
            new Button("Crash Gun", Category.Exploits, true, false, ()=>CrashGun(), ()=>CleanUp(), true),
            new Button("Crash On Touch", Category.Exploits, true, false, ()=>CrashOnTouch(), null, true),
            new Button("Stutter All", Category.Exploits, true, false, ()=>StutterAll(), null, true),
            new Button("Stutter Gun", Category.Exploits, true, false, ()=>StutterGun(), ()=>CleanUp(), true),
            new Button("Stutter On Touch", Category.Exploits, true, false, ()=>StutterOnTouch(), null, true),

            new Button("Lag All", Category.Exploits, true, false, ()=>LagAl(), null, true),
            new Button("Lag Gun", Category.Exploits, true, false, ()=>LagGun(), ()=>CleanUp(), true),
            new Button("Lag On Touch", Category.Exploits, true, false, ()=>LagOnTouch(), null, true),
            new Button("Mat Spam All", Category.Exploits, true, false, ()=>matSpamAll(), null, true),
            new Button("Mat Spam Gun", Category.Exploits, true, false, ()=>MatGun(), ()=>CleanUp(), true),
            new Button("Mat Spam On Touch", Category.Exploits, true, false, ()=>matSpamOnTouch(), null, true),

            new Button("Slow All", Category.Exploits, true, false, ()=>SlowAll(), null, true),
            new Button("Slow Gun", Category.Exploits, true, false, ()=>SlowGun(), ()=>CleanUp(), true),
            new Button("Vibrate All", Category.Exploits, true, false, ()=>VibrateAll(), null, true),
            new Button("Vibrate Gun", Category.Exploits, true, false, ()=>VibrateGun(), ()=>CleanUp(), true),
            new Button("Acid All", Category.Exploits, false, false, ()=>AcidAll(), null, true),
            new Button("Acid Self", Category.Exploits, false, false, ()=>AcidSelf(), null, true),

            new Button("Gamemode to Casual", Category.Exploits, false, false, ()=>changegamemode("CASUAL"), null, true),
            new Button("Gamemode to Infection", Category.Exploits, false, false, ()=>changegamemode("INFECTION"), null, true),
            new Button("Gamemode to Hunt", Category.Exploits, false, false, ()=>changegamemode("HUNT"), null, true),
            new Button("Gamemode to Battle", Category.Exploits, false, false, ()=>changegamemode("BATTLE"), null, true),
            new Button("Disable Network Triggers [SS]", Category.Exploits, false, false, ()=>DisableNetworkTriggers()),
            new Button("Trap All In Stump", Category.Exploits, false, false, ()=>TrapAllInStump()),

            new Button("Rope Up", Category.Exploits, true, false, ()=>RopeUp()),
            new Button("Rope Down", Category.Exploits, true, false, ()=>RopeDown()),
            new Button("Rope To Self", Category.Exploits, true, false, ()=>RopeToSelf()),
            new Button("Rope Gun", Category.Exploits, true, false, ()=>RopeGun(), ()=>CleanUp()),
            new Button("Rope Spaz", Category.Exploits, true, false, ()=>FlingOnRope()),
            new Button("Rope Freeze", Category.Exploits, true, false, ()=>RopeFreeze()),

            new Button("Name Change All", Category.Exploits, true, false, ()=>NameAll()),
            new Button("Name Change Gun", Category.Exploits, true, false, ()=>NameGun(), ()=>CleanUp()),
            new Button("Glider Gun", Category.Exploits, true, false, ()=>GliderGun(), null, true),
            new Button("Glider All", Category.Exploits, true, false, ()=>GliderAll(), null, true),
            new Button("Sound Spam", Category.Exploits, true, false, ()=>SoundSpam(), null, true),
            new Button("Tag Lag", Category.Exploits, true, false, ()=>TagLag(), ()=>RevertTagLag(), true),

            new Button("Change Theme", Category.Config, false, false, ()=>ChangeTheme(), null, false, false),
            new Button("Change SpeedBoost ", Category.Config, false, false, ()=>SwitchSpeed(), null, false, true, true, ()=>getSpeedBoostMultiplier()),
            new Button("Change FlightSpeed ", Category.Config, false, false, ()=>SwitchFlight(), null, false, true, true, ()=>getFlightMultiplier()),
            new Button("Change WallWalk ", Category.Config, false, false, ()=>SwitchWallWalk(), null, false, true, true, ()=>getWallWalkMultiplier()),
            new Button("Change Platforms ", Category.Config, false, false, ()=>ChangePlatforms(), null, false, true, false, null, true, ()=>getPlats()),
            new Button("Change AntiReport ", Category.Config, false, false, ()=>switchAntiReport(), null, false, true, false, null, true, ()=>getAntiReport()),

            new Button("Change SlideControl ", Category.Config, false, false, ()=>SwitchSlide(), null, false, true, true, ()=>getSlideMultiplier()),
            new Button("Right Hand Menu", Category.Config, true, false, null),
            new Button("Random Name W AntiReport", Category.Config, true, false, ()=>EnableNameOnJoin(), ()=>DisableNameOnJoin()),
            new Button("Disable AntiBan StumpCheck [D]", Category.Config, true, false, ()=>DisableStumpCheck(), ()=>EnableStumpCheck()),
            new Button("Change Button Type", Category.Config, false, false, ()=>ChangeButtonType()),

            new Button("Toggle Categorys", Category.Config, false, false, ()=>ChangePageType()),
            new Button("Toggle Watch Menu", Category.Config, false, false, ()=>ToggleWatch()),
            new Button("Toggle Mod List", Category.Config, false, false, ()=>ToggleList()),
            new Button("Toggle VR Mod List", Category.Config, false, false, ()=>ToggleGameList()),
            new Button("Disable Notifications", Category.Config, false, false, ()=>Notif.IsEnabled = !Notif.IsEnabled),
            new Button("Clear Notifications", Category.Config, false, false, ()=>Notif.ClearAllNotifications()),
    };



        public static int page = 0;
        public static int pageSize = 6;

        static GameObject menu = null;
        static GameObject canvasObj = null;
        static GameObject referance = null;
        static GameObject titleObj = null;

        public static int framePressCooldown = 0;
        public static bool isRoomCodeRun = true;

        public static bool isRunningAntiBan = false;
        private float deltaTime = 0.0f;
        public static bool InLobbyCurrent = false;


        void LateUpdate()
        {
            try
            {
                //Movement.AdvancedWASD(10);
                if (!isAllowed)
                {
                    Application.Quit();
                    if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "EXIST.txt")))
                        Environment.FailFast("bye");
                    return;
                }
                if (RewindHelp > 0f && Time.frameCount > RewindHelp)
                {
                    RewindHelp = 0f;
                }
                if (MacroHelp > 0f && (float)Time.frameCount > MacroHelp)
                {
                    MacroHelp = 0f;
                }
                if (!InLobbyCurrent && PhotonNetwork.InRoom)
                {
                    InLobbyCurrent = true;
                }
                else if (!PhotonNetwork.InRoom)
                {
                    InLobbyCurrent = false;
                }

                if (isRunningAntiBan)
                {
                    if (PhotonVoiceNetwork.Instance.ClientState == ClientState.Joined)
                    {
                        if (MenuPatch.FindButton("Auto AntiBan").Enabled)
                        {
                            StartAntiBan();
                        }
                    }
                }

                if (PhotonNetwork.InRoom)
                {
                    if (isStumpChecking)
                    {
                        CheckForStump();
                    }
                }
                else
                {
                    isRunningAntiBan = false;
                    isStumpChecking = false;
                }

                bool rightHand2 = MenuPatch.FindButton("Right Hand Menu").Enabled;

                if ((InputHandler.LeftPrimary && !rightHand2) || (InputHandler.RightPrimary && rightHand2))
                {
                    if (menu == null)
                    {
                        Draw();
                        if (referance == null)
                        {
                            referance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            if (!rightHand2)
                            {
                                referance.transform.parent = GorillaLocomotion.Player.Instance.rightControllerTransform;
                            }
                            else
                            {
                                referance.transform.parent = GorillaLocomotion.Player.Instance.leftControllerTransform;
                            }

                            referance.transform.localPosition = new Vector3(0f, -0.1f, 0f) * GorillaLocomotion.Player.Instance.scale;
                            referance.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                        }
                    }
                    else
                    {
                        if (!rightHand2)
                        {
                            menu.transform.position =
                                GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                            menu.transform.rotation =
                                GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
                        }
                        else
                        {
                            menu.transform.RotateAround(menu.transform.position, menu.transform.forward, 180f);
                            menu.transform.position =
                                GorillaLocomotion.Player.Instance.rightControllerTransform.position;
                            menu.transform.rotation =
                                GorillaLocomotion.Player.Instance.rightControllerTransform.rotation;
                        }

                        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
                        float fps = 1.0f / deltaTime;

                        Text title = titleObj.GetComponent<Text>();
                        title.text = "Steal FPS-" + Mathf.Round(fps);
                    }
                }
                else if (menu == null)
                {
                    Destroy(menu);
                    menu = null;
                    GameObject.Destroy(referance);
                    referance = null;
                    Destroy(titleObj);
                    titleObj = null;
                }


                if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "EXIST.txt")))
                    Environment.FailFast("bye");


                foreach (Button bt in buttons)
                {
                    if (bt.Enabled)
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


        public static Texture2D MenuBackground
        {
            get
            {
                if (true)//SettingsLib.bgURI != "NONE")
                {
                    if (false)//SettingsLib.bgURI.Contains("http"))
                    {
                        return AssetLoader.DownloadBackround(SettingsLib.bgURI);
                    }
                    else
                    {
                        if (File.Exists(SettingsLib.bgURI))
                        {
                            Texture2D ImageTexture = new Texture2D(2, 2);
                            ImageConversion.LoadImage(ImageTexture, File.ReadAllBytes(SettingsLib.bgURI));
                            return ImageTexture;
                        }
                    }
                }
                return null;
            }
        }

        public static Color[] GetTheme(int Theme)
        {
            if (SettingsLib.hasInit)
            {
                if (SettingsLib.BGColor.a != 0)
                {
                    int r = Mathf.RoundToInt(SettingsLib.ButtonColor.r * 1.75f);
                    int g = Mathf.RoundToInt(SettingsLib.ButtonColor.g * 1.75f);
                    int b = Mathf.RoundToInt(SettingsLib.ButtonColor.b * 1.75f);
                    return new Color[]
                    {
                        SettingsLib.BGColor, SettingsLib.ButtonColor, new Color32((byte)r, (byte)g, (byte)b, SettingsLib.ButtonColor.a), SettingsLib.ButtonText // dark
                    };
                }
            }
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
                        new Color(0.7f, 0.7f, 0.7f), new Color(0.8f, 0.8f, 0.8f), new Color(0.9f, 0.9f, 0.9f), Color.white // Lite
                    };

                case 4:
                    return new Color[]
                    {
                        new Color32(59, 59, 59, 255), new Color(0.2f, 0.2f, 0.6f), new Color32(49, 0, 196, 255), Color.white // Blue
                    };

                case 5:
                    return new Color[]
                    {
                        new Color(36, 36, 31), new Color(255, 153, 51), new Color(205, 0, 0), Color.white // Red
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
            title.color = GetTheme(UI.Theme)[3];
            title.fontSize = 1;
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 0;
            RectTransform titleTransform = title.GetComponent<RectTransform>();
            titleTransform.localPosition = Vector3.zero;
            titleTransform.sizeDelta = new Vector2(0.2f, 0.03f);

            if (currentButtons == PageButtonsType.Default)
            {
                title.text = button;
                titleTransform.localPosition = button.Contains("<") ? new Vector3(0.064f, 0.0715f, -0.198f) : new Vector3(0.064f, -0.0685f, -0.198f);
                titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }
            else if (currentButtons == PageButtonsType.Side)
            {
                title.text = button.Contains("<") ? "<" : ">";
                titleTransform.position = new Vector3(0.064f, button.Contains("<") ? 0.186f : -0.186f, 0f);
                titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }


        }

        public static void ChangeTheme()
        {
            UI.Theme = UI.Theme + 1;
            if (UI.Theme > 4)
            {
                UI.Theme = 0;
            }
            PlayerPrefs.SetInt("steal_backGround", UI.Theme);
            MenuPatch.RefreshMenu();
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
            if (MenuBackground != null)
            {
                newBtn.GetComponent<Renderer>().material.shader = Shader.Find("UI/Default");
                newBtn.GetComponent<Renderer>().material.mainTexture = MenuBackground;
                newBtn.GetComponent<Renderer>().material.SetTexture("_MainTex", MenuBackground);
            }
            else
            {
                newBtn.GetComponent<Renderer>().material.color = GetTheme(UI.Theme)[1];
            }


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
            newBtn.transform.localScale = new Vector3(0.1f, 0.92f, 0.1f);
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
            if (button.doesHaveMultiplier)
            {
                title.text = button.buttonText + "[" + button.multiplier() + "]";
            }
            else if (button.doesHaveStringer)
            {
                title.text = button.buttonText + "[" + button.stringFunc() + "]";
            }
            else
            {
                title.text = button.buttonText;
            }
            title.fontSize = 1;
            title.color = GetTheme(UI.Theme)[3];
            title.fontStyle = FontStyle.Bold;
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
            try
            {
                menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(menu.GetComponent<Rigidbody>());
                GameObject.Destroy(menu.GetComponent<BoxCollider>());
                GameObject.Destroy(menu.GetComponent<Renderer>());
                menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f) * GorillaLocomotion.Player.Instance.scale;
                menu.name = "menu";

                GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(background.GetComponent<Rigidbody>());
                GameObject.Destroy(background.GetComponent<BoxCollider>());
                background.transform.parent = menu.transform;
                background.transform.rotation = Quaternion.identity;
                background.transform.localScale = new Vector3(0.1f, 1f, 1.1f);
                background.name = "menucolor";
                background.transform.position = new Vector3(0.05f, 0, -0.004f);
                if (false)
                {
                    background.GetComponent<Renderer>().material.shader = Shader.Find("UI/Default");
                    background.GetComponent<Renderer>().material.mainTexture = MenuBackground;
                    background.GetComponent<Renderer>().material.SetTexture("_MainTex", MenuBackground);
                }
                else
                {
                    background.GetComponent<Renderer>().material.color = GetTheme(UI.Theme)[0];
                }
                canvasObj = new GameObject();
                canvasObj.transform.parent = menu.transform;
                canvasObj.name = "canvas";
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                CanvasScaler canvasScale = canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasScale.dynamicPixelsPerUnit = 1000;

                titleObj = new GameObject();
                titleObj.transform.parent = canvasObj.transform;
                titleObj.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                Text title = titleObj.AddComponent<Text>();
                title.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                title.text = "Steal";
                title.fontStyle = FontStyle.Italic;
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
                    if (currentPage != Category.Base)
                    {
                        AddPageButton(">");
                        AddPageButton("<");
                        AddBackToStartButton();
                    }

                    var PageToDraw = GetButtonInfoByPage(currentPage).Skip(page * pageSize).Take(pageSize).ToArray();
                    for (int i = 0; i < PageToDraw.Length; i++)
                    {
                        AddButton(i * 0.13f, PageToDraw[i]);
                    }
                }
                else
                {
                    AddPageButton(">");
                    AddPageButton("<");
                    var UnPageToDraw = buttons.Skip(page * pageSize).Take(pageSize).ToArray();

                    for (int i = 0; i < UnPageToDraw.Length; i++)
                    {
                        if (UnPageToDraw[i].Page != Category.Base)
                        {
                            AddButton(i * 0.13f, UnPageToDraw[i]);
                        }
                    }
                }
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        public static List<Button> GetButtonInfoByPage(Category page)
        {
            return buttons.Where(button => button.Page == page).ToList();
        }


        public static void Toggle(Button button)
        {
            int totalPages = (buttons.Length + pageSize - 1) / pageSize;
            int totalCatagoriePages = (GetButtonInfoByPage(currentPage).Count + pageSize - 1) / pageSize;

            switch (button.buttonText)
            {
                case ">":

                    if (categorized)
                    {
                        if (totalCatagoriePages < page || (totalCatagoriePages - 1) == page)
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
                        if (page < totalPages - 1 || (totalPages - 1) == page)
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
                            page = totalPages - 1;
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
            if (button.ismaster && !PhotonNetwork.IsMasterClient)
            {
                Notif.SendNotification("You're Not Masterclient!", Color.red);
                button.Enabled = false;
                RefreshMenu();
                return;
            }

            if (!button.isToggle)
            {
                button.onEnable();

                if (button.Page == Category.Config)
                    RefreshMenu();

                ModsList.RefreshText();

                Notif.SendNotification("Executed non-toggle mod: " + button.buttonText + "!", Color.cyan);

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

            ModsList.RefreshText();
            RefreshMenu();
        }

        class BtnCollider : MonoBehaviour
        {
            public Button button;
            public float defaultZ;

            public void Awake()
            {
                defaultZ = transform.localScale.x;
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
                    transform.localScale = new Vector3(transform.localScale.x / 3, transform.localScale.y, transform.localScale.z);
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

            System.Collections.IEnumerator ResetYValue()
            {
                yield return new WaitForSeconds(0.65f);
                transform.localScale = new Vector3(defaultZ, transform.localScale.y, transform.localScale.z);
            }
        }

        public static void DrawRoundedEgg()
        {
            try
            {
                menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(menu.GetComponent<Rigidbody>());
                GameObject.Destroy(menu.GetComponent<BoxCollider>());
                GameObject.Destroy(menu.GetComponent<Renderer>());
                menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f) * GorillaLocomotion.Player.Instance.scale;
                menu.name = "menu";

                GameObject background = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GameObject.Destroy(background.GetComponent<Rigidbody>());
                GameObject.Destroy(background.GetComponent<SphereCollider>());
                background.transform.parent = menu.transform;
                background.transform.rotation = Quaternion.identity;
                background.transform.localScale = new Vector3(0.1f, 1f, 1.1f);
                background.name = "menucolor";
                background.transform.position = new Vector3(0.05f, 0, -0.004f);


                if (false)
                {
                    background.GetComponent<Renderer>().material.shader = Shader.Find("UI/Default");
                    background.GetComponent<Renderer>().material.mainTexture = MenuBackground;
                    background.GetComponent<Renderer>().material.SetTexture("_MainTex", MenuBackground);
                }
                else
                {
                    background.GetComponent<Renderer>().material.color = GetTheme(UI.Theme)[0];
                }
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
                title.fontStyle = FontStyle.BoldAndItalic;
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
                    if (currentPage != Category.Base)
                    {
                        AddPageButton(">");
                        AddPageButton("<");
                        AddBackToStartButton();
                    }

                    var PageToDraw = GetButtonInfoByPage(currentPage).Skip(page * pageSize).Take(pageSize).ToArray();
                    for (int i = 0; i < PageToDraw.Length; i++)
                    {
                        AddButton(i * 0.13f, PageToDraw[i]);
                    }
                }
                else
                {
                    AddPageButton(">");
                    AddPageButton("<");
                    var UnPageToDraw = buttons.Skip(page * pageSize).Take(pageSize).ToArray();

                    for (int i = 0; i < UnPageToDraw.Length; i++)
                    {
                        if (UnPageToDraw[i].Page != Category.Base)
                        {
                            AddButton(i * 0.13f, UnPageToDraw[i]);
                        }
                    }
                }
            }
            catch (Exception e) { Debug.LogException(e); }
        }

    }
}
