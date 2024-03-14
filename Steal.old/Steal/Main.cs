using BepInEx;
using HarmonyLib;
using Photon.Pun;
using Photon.Voice.PUN;
using Steal.Background;
using Steal.Background.Security;
using Steal.Patchers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Steal.Mods.Mods;

namespace Steal
{
    /*
    [BepInPlugin("com.steal.isioans", "saipn", "1.0.0")]
    public class bepin : BaseUnityPlugin
    {
         public void LateUpdate()
         {
            return;
             if (!GameObject.Find("Steal"))
             {
                 DevLoad.Load();
                 Main.Draw();
    
             }
             else
             {
                 Destroy(this.gameObject);//penis
             }
         }
     }*/
    public class Main : MonoBehaviour
    {
        public static int pagesize = 6;
        public static int page = 0;

        internal static GameObject menu = null;
        static GameObject canvasObj = null;
        public static GameObject menuClick = null;


        public static Category currentPage = Category.Base;

        public bool shouldThing = false;

        public bool DoOnce = true;

        public string lastRoom;

        public void LateUpdate()
        {
            if (shouldThing)
            {
                if (PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.PeerState == ExitGames.Client.Photon.PeerStateValue.Connected)
                {
                    Calculations.SendHook();
                    Debug.Log("Photon Voice is fully loaded and connected.");
                    GameObject ms = GameObject.Find("Steal");
                    Notif.SendNotification("<color=green>[STEAL]</color><color=red> WAIT UNTIL NEXT NOTIFICATION FOR ANTIBAN TO ENABLE... </color>");
                    RPCSUB.thing();
                    shouldThing = false;
                }
            }


            if (RPCSUB.IsAntiBaaaaa)
            {
                RPCSUB.AntiBan2();
            }
            if (ControllerInput.LeftPrimary)
            {
                if (menu == null)
                {
                    Draw();
                }
                else
                {
                    menu.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                    menu.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
                    if (menuClick == null)
                    {
                        menuClick = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        menuClick.name = "buttonPresser";
                    }

                    menuClick.transform.parent = GorillaLocomotion.Player.Instance.rightControllerTransform;
                    menuClick.transform.localPosition =
                        new Vector3(0f, -0.1f, 0f) * GorillaLocomotion.Player.Instance.scale;
                    menuClick.transform.localScale =
                        new Vector3(0.01f, 0.01f, 0.01f) * GorillaLocomotion.Player.Instance.scale;
                }
            }
            else if (menu != null)
            {
                DestroyMenu();
                // menu.SetActive(false);
            }


            foreach (Button buttonInfo in buttons)
            {
                if (buttonInfo.OnClickAction == null) continue;

                if (buttonInfo.Enabled == true)
                {
                    buttonInfo.OnClickAction.Invoke();
                }
            }

            if (GorillaTagger.Instance.offlineVRRig != null && GorillaTagger.Instance.offlineVRRig.enabled)
            {
                if (GorillaGameManager.instance != null)
                {
                    GorillaTagger.Instance.offlineVRRig.ChangeMaterialLocal(GorillaGameManager.instance.MyMatIndex(PhotonNetwork.LocalPlayer));
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.ChangeMaterialLocal(0);
                }
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
            Computer,
            Settings
        };

        public static List<Button> buttons = new List<Button>
        {
            new Button ("Room", () => ChangePage(Category.Room), Category.Base, false),
            new Button ("Movement", () => ChangePage(Category.Movement), Category.Base, false),
            new Button ("Player", () => ChangePage(Category.Player), Category.Base, false),
            new Button ("Visual", () => ChangePage(Category.Visual), Category.Base, false),
            new Button ("Special", () => ChangePage(Category.Special), Category.Base, false),

            //Room
            new Button ("Disconnect", () => PhotonNetwork.Disconnect(), Category.Room, false),
            new Button ("Join Random", () => PhotonNetwork.JoinRandomRoom(), Category.Room, false),
            new Button ("Create Public", () => CreatePublicRoom(), Category.Room, false),
            new Button ("Create Private", () => CreatePrivateRoom(), Category.Room, false),
            new Button ("Dodge Moderators", () => DodgeModerators(), Category.Room),

            //Movement
            new Button ("Speed Boost", () => SpeedBoost(),Category.Movement, true,  () => ResetSpeed()),
            new Button ("Flight", () => Flight(), Category.Movement),
            new Button ("WallWalk", () => WallWalk(), Category.Movement),
            new Button ("Platforms", () => Platforms(), Category.Movement),
            new Button ("Teleport Gun", () => TeleportGun(), Category.Movement, true, ()=>GunLibraries.GunCleanUp()),
            new Button ("Airstrike Gun", () => AirStrike(), Category.Movement, true, ()=>GunLibraries.GunCleanUp()),

            //Movement Page 2
            new Button ("Iron Monke", () => ProcessIronMonke(), Category.Movement),
            new Button ("Spider Monke", () => SpiderMonke(true), Category.Movement, true, () => SpiderMonke(false)),
            new Button ("Up And Down", () => UpAndDown(), Category.Movement),
            new Button ("LongArms", () => LongArms(true), Category.Movement, true, () => LongArms(false)),
            new Button ("FastSwim", () => FasterSwimming(), Category.Movement),
            new Button ("Bunny Hop", () => BHop(), Category.Movement),
            
            //Movement Page 3
            new Button ("Grapple Hook", () => GrappleHook(), Category.Movement),
            new Button ("Spider Climb", () => MonkeClimb(GorillaLocomotion.Player.Instance), Category.Movement),
            new Button ("Checkpoint", () => ProcessCheckPoint(), Category.Movement),
            new Button ("Magic Monkey", () => MagicMonkey(), Category.Movement, true, ()=>GunLibraries.GunCleanUp()),
            new Button ("Slingshot [TRIG]", () => Slingshot(), Category.Movement),
            new Button ("NoClip", () => NoClip(true), Category.Movement, true, () => NoClip(false)),

            //Movement Page 4
            new Button ("Punch", () => PunchMod(), Category.Movement),
            
            //Player 
            new Button ("Tag Gun", () => TagGun(PhotonNetwork.LocalPlayer.IsMasterClient), Category.Player, true, () =>{ResetMonke(); GunLibraries.GunCleanUp(); }),
            new Button ("Tag All", () => TagAll(PhotonNetwork.LocalPlayer.IsMasterClient), Category.Player, true, () =>{ResetMonke(); GunLibraries.GunCleanUp(); }),
            new Button ("Tag Self", () => TagSelf(), Category.Player),
            new Button ("Tag Aura", () => TagAura(), Category.Player),
            new Button ("Anti Tag", () => AntiTag(), Category.Player),
            new Button ("No Tag On Join", () => NoTagOnJoin(), Category.Player),

            //Player Page 2
            new Button ("Ghost Monke", () => GhostMonkey(), Category.Player, true, () => ResetMonke()),
            new Button ("Invis Monke", () => InvisMonkey(), Category.Player, true, () => ResetMonke()),
            new Button ("Copy Gun", () => CopyGun(), Category.Player, true, () => ResetMonke()),
            new Button ("Rig Gun", () => RigGun(), Category.Player, true, () => {ResetMonke(); GunLibraries.GunCleanUp(); }),
            new Button ("Freeze Monkey", () => FreezeMonkey(), Category.Player, true, () => ResetMonke()),
            new Button ("Big Monke", () => BigMonke(), Category.Player, true, () => ResetMonke()),
            
            //Player Page 3
            new Button ("Splash [TRIG]", () => Splash(), Category.Player),
            new Button ("Sizeable Splash", () => SizeableSplash(), Category.Player),
            new Button ("Splash Gun", () => SpashGun(), Category.Player, true, ()=>GunLibraries.GunCleanUp()),
            new Button ("RGB", () => RGB(), Category.Player),
            new Button ("Strobe", () => Strobe(), Category.Player),
            new Button ("C4", () => C4Boom(), Category.Player),

            //Player Page 4
            new Button ("SpinHead X", () => SpinHeadX(), Category.Player, true, () => ResetHead()),
            new Button ("SpinHead Y", () => SpinHeadY(), Category.Player, true, () => ResetHead()),
            new Button ("SpinHead Z", () => SpinHeadZ(), Category.Player, true, () => ResetHead()),
            new Button ("Upsidedown Head", () => HeadUpsideDown(), Category.Player, true, () => ResetHead()),
            new Button ("No Tap Cooldown", () => EnableInstantHandTaps(), Category.Player, true, () => DisableInstantHandTaps()),
            new Button ("Loud Tap", () => LoudHandTaps(), Category.Player, true, () => FixHandTaps()),

            //Player Page 5
            new Button ("Impossible Color", () => ImpossibleColors(), Category.Player, false),
            new Button ("Untag All", () => UntagAll(), Category.Player, false),
            new Button ("Untag Gun", () => UnTagGun(), Category.Player, false),
            
            //Visual
            new Button ("ESP", () => ESP(), Category.Visual, true, () => ResetTexure()),
            new Button ("Chams", () => Chams(), Category.Visual, true, () => ResetTexure()),
            new Button ("Skeleton ESP", () => BoneESP(), Category.Visual, true, () => ResetTexure()),
            new Button ("Box ESP", () => BoxESP(), Category.Visual, true, () => ResetTexure()),
            new Button ("Tracers", () => Tracers(), Category.Visual, true, () => ResetTexure()),
            new Button ("Beacons", () => Beacons(), Category.Visual, true, () => ResetTexure()),

            //Visual Page 2
            
            new Button ("Uncap FPS", () => SetFPS(-1),Category.Visual, true, () => SetFPS(144)),
            new Button ("42Hz", () => SetFPS(42), Category.Visual, true,  () => SetFPS(144)),
            new Button ("Disable Leaves", () => Mods.Mods.RemoveLeaves(), Category.Visual, true, () => AddLeaves()),
            new Button ("Disable Cosmetics", () => Mods.Mods.EnableCosmetics(), Category.Visual, true, () => EnableCosmetics()),
            new Button ("Day Time", () => Mods.Mods.DayTime(), Category.Visual, false),
            new Button ("Night Time", () => Mods.Mods.NightTime(), Category.Visual, false),
          
            
            //Special
            new Button ("Vibrate All", () => VibrateAll(), Category.Special, true, null, true),
            new Button ("Vibrate Gun", () => VibrateGun(), Category.Special, true, ()=>GunLibraries.GunCleanUp(), true),
            new Button ("Slow All", () => SlowAll(), Category.Special, true, null, true),
            new Button ("Slow Gun", () => SlowGun(), Category.Special, true, ()=>GunLibraries.GunCleanUp(), true),
            new Button ("Mat All", () => instance.matSpamAll(), Category.Special, true, null, true),
            new Button ("Mat Gun", () => MatGun(), Category.Special, true, ()=>GunLibraries.GunCleanUp(), true),
            
            //Special Page 2
            new Button ("Set Room To Private", () => PhotonNetwork.CurrentRoom.IsVisible = false, Category.Special, true, () => PhotonNetwork.CurrentRoom.IsVisible = true, true),
            new Button ("Close Room", () => PhotonNetwork.CurrentRoom.IsOpen = false, Category.Special, true, () => PhotonNetwork.CurrentRoom.IsOpen = true, true),
            new Button ("Change Lobby To Casual", () => changegamemode("CASUAL"), Category.Special, false, null, true),
            new Button ("Change Lobby To Infection", () => changegamemode("INFECTION"), Category.Special, false, null, true),
            new Button ("Change Lobby To Hunt", () => changegamemode("HUNT"), Category.Special, false, null, true),
            new Button ("Change Lobby To Battle", () => changegamemode("BATTLE"), Category.Special, false, null, true),
            
            //Special Page 3
            new Button ("Rope Up", () => RopeUp(), Category.Special),
            new Button ("Rope Down", () => RopeDown(), Category.Special),
            new Button ("Rope To Self", () => RopeToSelf(), Category.Special),
            new Button ("Rope Gun", () => RopeGun(), Category.Special, true, ()=>GunLibraries.GunCleanUp()),
            new Button ("Rope Fling", () => FlingOnRope(), Category.Special),
            new Button ("Rope Freeze", () => RopeFreeze(), Category.Special),

            //Special Page 5
            new Button ("Kick All [PRIVATE]", () => kickAll(), Category.Special, true, null, true),
            new Button ("Kick Gun [PRIVATE]", ()=>KickGun(), Category.Special, true, ()=>GunLibraries.GunCleanUp(), true),
            new Button ("Freeze All", () => FreezeAll(), Category.Special, true, null, true),
            new Button ("Freeze Gun", () => FreezeGun(), Category.Special, true, ()=>GunLibraries.GunCleanUp(), true),
            new Button ("Crash All", () => CrashAll(), Category.Special, true, null, true),
            new Button ("Crash Gun", () => CrashGun(), Category.Special, true, ()=>GunLibraries.GunCleanUp(), true),

            //Special Page 5
            new Button ("Identity Spoof", () => ChangeIdentity(), Category.Special, false),
            new Button ("Fraud Identity Spoof", () => ChangeRandomIdentity(), Category.Special, false),
            new Button ("Anti Report", () => AntiReport(), Category.Special, true, null, false, true),
            new Button ("Acid All", () => AcidAll(), Category.Special, true, ()=>UnAcidAll()),
            new Button ("Acid Self", () => AcidSelf(), Category.Special, true, ()=>UnAcidSelf()),
            new Button ("Acid Gun", () => AcidGun(), Category.Special, true, ()=>rpcReset()),
            
            new Button ("UnAcid Gun", () => UnAcidGun(), Category.Special, true, ()=>rpcReset()),
            new Button ("Acid Spam [D?]", () => AcidSpam(), Category.Special, true, ()=>rpcReset()),
            new Button ("Change Name All", ()=>ChangeAllNames(), Category.Special),
            
            //Special Page 7
            new Button ("Freeze Aura", ()=>FreezeAura(), Category.Special)
        };


        #region Draw_Funct
        public static void Toggle(Button button)
        {
            int totalPages = (buttons.Count + pagesize - 1) / pagesize;

            switch (button.ButtonText)
            {
                case "NextPage":
                    page = (page < totalPages - 1) ? page + 1 : 0;
                    break;

                case "PreviousPage":
                    if (page == 0)
                        currentPage = Category.Base;
                    else
                        page--;
                    break;

                case "DisconnectingButton":
                    PhotonNetwork.Disconnect();
                    return;

                case "BackToStart":
                    currentPage = Category.Base;
                    page = 0;
                    RefreshMenu();
                    return;

                default:
                    ToggleButton(button.ButtonText);
                    break;
            }
            RefreshMenu();
        }


        public static void ToggleButton(string buttonText)
        {
            int buttonIndex = buttons.FindIndex(b => b.ButtonText == buttonText);
            if (buttons[buttonIndex].isMaster && !PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                ModsList.modsEnabled.Remove(buttons[buttonIndex].ButtonText);
                buttons[buttonIndex].Enabled = false;
                Notif.SendNotification("<color=red> DISABLING BECAUSE NO MASTER </color>");
                return;
            }
            else if (buttonIndex != -1)
            {
                if (buttons[buttonIndex].Toggle == false)
                {
                    buttons[buttonIndex].OnClickAction();
                    return;
                }
                else
                {
                    buttons[buttonIndex].Enabled = !buttons[buttonIndex].Enabled;
                }

                if (buttons[buttonIndex].Enabled == true)
                {
                    ModsList.modsEnabled.Add(buttons[buttonIndex].ButtonText);
                    ModsList.RefreshText();
                }
                else
                {
                    if (buttons[buttonIndex].OnDisableAction != null)
                    {
                        buttons[buttonIndex].OnDisableAction.Invoke();
                    }

                    ModsList.modsEnabled.Remove(buttons[buttonIndex].ButtonText);
                    ModsList.RefreshText();
                }
            }
        }


        //public static Color BackgroundColor = new Color(0f, 0f, 0f, 0.6f);

        internal static void Draw(bool refresh = true)
        {
            if (refresh)
            {
                menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(menu.GetComponent<Rigidbody>());
                Destroy(menu.GetComponent<BoxCollider>());
                Destroy(menu.GetComponent<Renderer>());
                menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);
                menu.name = "menu";

                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(gameObject.GetComponent<Rigidbody>());
                Destroy(gameObject.GetComponent<BoxCollider>());
                gameObject.transform.parent = menu.transform;
                gameObject.transform.rotation = Quaternion.identity;
                gameObject.transform.localScale = new Vector3(0.1f, 1.01f, 0.85f);
                gameObject.name = "menucolor";
                gameObject.transform.position = new Vector3(0.054f, 0, 0.01f);
                gameObject.GetComponent<Renderer>().material.color = Main.GetTheme(SettingsLib.Settings.BackgroundColor)[0];

                canvasObj = new GameObject();
                canvasObj.transform.parent = menu.transform;
                canvasObj.name = "canvas";
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasScaler.dynamicPixelsPerUnit = 1000f;
                GameObject gameObject2 = new GameObject();
                gameObject2.transform.parent = canvasObj.transform;
                gameObject2.name = "Title";
                Text text = gameObject2.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                text.text = "Steal X Moon";
                text.fontSize = 1;
                text.fontStyle = FontStyle.BoldAndItalic;
                text.alignment = TextAnchor.MiddleCenter;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;
                RectTransform component = text.GetComponent<RectTransform>();
                component.localPosition = Vector3.zero;
                component.sizeDelta = new Vector2(0.28f, 0.05f);
                component.position = new Vector3(0.06f, 0f, 0.1455f);
                component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }

            var array = GetButtonInfoByPage(currentPage).Skip(page * pagesize).Take(pagesize).ToArray();
            var array2 = GetButtonInfoByPage(currentPage).Skip((page + 1) * pagesize).Take(pagesize).ToArray();

            if (currentPage != Category.Base)
            {
                if (array2.Length == 0)
                {
                    AddPageButton(true);
                }
                else
                {
                    AddPageButton(false);
                    AddPageButton(true);
                }
                GameObject newButton3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Transform buttonTransform = newButton3.transform;

                buttonTransform.parent = menu.transform;
                buttonTransform.rotation = Quaternion.identity;
                buttonTransform.localScale = new Vector3(0.02f, 0.1f, 0.08f);
                buttonTransform.localPosition = new Vector3(0.6f, 0.7f, -.46f);

                Renderer buttonRenderer = newButton3.GetComponent<Renderer>();
                buttonRenderer.material.color = Main.GetTheme(SettingsLib.Settings.BackgroundColor)[0];

                BoxCollider buttonCollider = newButton3.GetComponent<BoxCollider>();
                buttonCollider.isTrigger = true;

                Destroy(newButton3.GetComponent<Rigidbody>());


                string stringthing = "BackToStart";
                Button button = new Button(stringthing);
                newButton3.AddComponent<BtnCollider>().clickedButton = button;

                GameObject titleObj3 = new GameObject();
                titleObj3.transform.parent = canvasObj.transform;
                Text title3 = titleObj3.AddComponent<Text>();
                title3.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                title3.fontSize = 1;
                title3.alignment = TextAnchor.MiddleCenter;
                title3.resizeTextForBestFit = true;
                title3.resizeTextMinSize = 0;
                title3.color = Main.GetTheme(SettingsLib.Settings.BackgroundColor)[2];
                title3.text = currentPage + " Page - " + (page + 1);
                RectTransform titleTransform3 = title3.GetComponent<RectTransform>();
                titleTransform3.localPosition = Vector3.zero;
                titleTransform3.sizeDelta = new Vector2(0.28f, 0.05f);
                titleTransform3.position = new Vector3(0.06f, 0f, -0.185f);

                titleTransform3.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }



            if (isCatagory)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    AddButton((float)i * 0.11f + 0.04f * 1f, array[i]);
                }
            }
            else
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    var array3 = buttons.Skip(page * pagesize).Take(pagesize).ToArray();
                    if (buttons[i].Page != Category.Base)
                    {
                        AddButton((float)i * 0.11f + 0.04f * 1f, array[i]);
                    }
                }
            }


        }

        static bool isCatagory = true;


        private static void AddButton(float offset, Button button)
        {
            if (button.ButtonText.Contains("Computer"))
            {
                return;
            }
            GameObject buttonOB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(buttonOB.GetComponent<Rigidbody>());
            buttonOB.GetComponent<BoxCollider>().isTrigger = true;
            buttonOB.transform.parent = menu.transform;
            buttonOB.transform.rotation = Quaternion.identity;
            buttonOB.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
            buttonOB.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - offset);
            buttonOB.AddComponent<BtnCollider>().clickedButton = button;
            buttonOB.name = button.ButtonText;


            GameObject textCanvas = new GameObject();
            textCanvas.transform.parent = canvasObj.transform;

            Text text = textCanvas.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            Renderer renderer = buttonOB.GetComponent<Renderer>();

            if (button.Enabled == false)
            {
                renderer.material.color = GetTheme(StealGUI.Theme)[0];
            }
            else
            {
                renderer.material.color = GetTheme(StealGUI.Theme)[1];
            }

            text.text = button.ButtonText;
            text.color = GetTheme(StealGUI.Theme)[2];
            text.fontSize = 1;
            text.fontStyle = FontStyle.Italic;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;

            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.localPosition = Vector3.zero;
            rectTransform.sizeDelta = new Vector2(0.2f, 0.03f);
            rectTransform.localPosition = new Vector3(0.064f, 0f, 0.11f - offset / 2.55f);
            rectTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        private static void AddPageButton(bool isLeft)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localPosition = new Vector3(0.56f, isLeft ? 0.625f : -0.625f, 0f);
            gameObject.transform.localScale = new Vector3(0.09f, 0.12f, 0.675f);
            string stringthing = isLeft ? "PreviousPage" : "NextPage";
            Button button = new Button(stringthing);
            gameObject.AddComponent<BtnCollider>().clickedButton = button;
            GameObject gameObject2 = new GameObject();
            gameObject2.transform.parent = canvasObj.transform;

            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.material.color = GetTheme(StealGUI.Theme)[0];

            Text text = gameObject2.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.text = button.ButtonText == "PreviousPage" ? "<" : ">";
            text.fontSize = 1;
            text.fontStyle = FontStyle.Italic;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;

            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.localPosition = Vector3.zero;
            rectTransform.sizeDelta = new Vector2(0.2f, 0.03f);
            rectTransform.position = new Vector3(0.064f, isLeft ? 0.186f : -0.186f, 0f);
            rectTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }



        #endregion

        #region out of site nigger

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

        public static string GetGravityStrength()
        {
            switch (SettingsLib.Settings.WallWalkGravity)
            {
                case -9.81f:
                    return "Normal";
                case -6.81f:
                    return "Weak";
                case -12.81f:
                    return "Strong";
                default:
                    return "??";
            }
        }

        public static void ChangePage(Category page)
        {
            currentPage = page;
            RefreshMenu();
        }

        public static void DestroyMenu()
        {
            if (menu == null || menuClick == null) { return; }
            Destroy(menu);
            menu = null;
            Destroy(menuClick);
            menuClick = null;

        }

        public static void RefreshMenu()
        {
            if (menu == null) { return; }
            Destroy(menu);
            menu = null;
            Draw(true);

        }

        public static List<Button> GetButtonInfoByPage(Category page)
        {
            return buttons.Where(button => button.Page == page).ToList();
        }

        public class Button
        {
            public string ButtonText = "noworkie";

            public bool Enabled = false;

            public bool Toggle = true;

            public Action OnClickAction;

            public Action OnDisableAction;

            public Action changerThing;

            public Category Page;

            public bool isMaster = false;

            public Button(string ButtonText2, Action OnClick2 = null, Category page2 = 0, bool shouldToggle2 = true,
                Action OnDisable2 = null, bool isMaster2 = false, bool Enabled2 = false, Action ChangerThing2 = null)
            {
                ButtonText = ButtonText2;
                OnClickAction = OnClick2;
                Page = page2;
                Toggle = shouldToggle2;
                OnDisableAction = OnDisable2;
                isMaster = isMaster2;
                Enabled = Enabled2;
                changerThing = ChangerThing2;
            }
        }


        internal class BtnCollider : MonoBehaviour
        {
            public static int framePressCooldown = 0;

            private void OnTriggerEnter(Collider collider)
            {
                if (Time.frameCount >= framePressCooldown + 20 && collider.gameObject == menuClick)
                {
                    AssetLoader.Instance.PlayClick();
                    GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.taggedHapticStrength, 0.001f);
                    Toggle(clickedButton);
                    framePressCooldown = Time.frameCount;
                }
            }

            public Button clickedButton;
        }
        #endregion
    }
}