using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using Steal;
using Steal.Background;
using Steal.Patchers;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using static Steal.MenuPatch;
using static WristMenu.UI;

namespace WristMenu
{
    public class NuGUI : MonoBehaviour
    {
        private static Texture2D Background;
        private static Texture2D TabBar;
        private static Texture2D TabNormal;
        private static Texture2D TabActive;
        private static Texture2D Box;
        private static Texture2D BoxLine;
        private static Texture2D Disabled;
        private static Texture2D Enabled;
        private static Texture2D Infect;
        private static Texture2D UnInfect;

        private static Rect _window = new Rect(20f, 20f, 710f, 600f);
        private static bool _enabled = true;
        private static MenuPatch.Category currentNuGuiPage = Category.Movement;
        private static Vector2 scrollPosition;
        private static string textFieldString = "Enter text here";
        private void OnGUI()
        {
            if (_enabled == false)
                return;
            _window = GUI.Window(7859, _window, WindowMain, "", new GUIStyle());
        }

        public static bool hasCammed = false;

        private void Update()
        {
            if (FreeCamMOd)
            {
                ModHandler.AdvancedWASD(10);
                hasCammed = true;
            }
            else if (hasCammed)
            {
                GorillaTagger.Instance.rigidbody.useGravity = true;
                hasCammed = false;
            }
            if (Keyboard.current[Key.RightShift].wasPressedThisFrame)
                _enabled = !_enabled;
        }

        private void Start()
        {
            Initialize();
            //GiveModToolTip();
        }


        private static void WindowMain(int id)
        {
            DrawWindow(new Rect(0f, 0f, _window.width, _window.height), 8);

            //SettingsMaker();

            float currentTabPos = 10f;

            Player();

            Freecma();


            foreach (Category page in Enum.GetValues(typeof(Category)))
            {
                if (page != Category.Base)
                {
                    Tab(currentTabPos, page);
                    currentTabPos += 140;
                }
            }

            if (!isInPlayer)
            {
                var buttons = GetButtonInfoByPage(currentNuGuiPage);
                int modsCount = buttons.Count;
                float contentHeight = (modsCount / 3 + (modsCount % 3 > 0 ? 1 : 0)) * 140f;
                Rect scrollViewRect = new Rect(20f, 90f, _window.width - 10f, _window.height - 110f);
                Rect scrollContentRect = new Rect(0, 0, scrollViewRect.width - 20f, contentHeight);

                scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, scrollContentRect, false, true);

                float currentCheatPosX = 0f;
                float currentCheatPosY = 0f;
                if  (currentNuGuiPage == Category.Room)
                {
                    textFieldString = GUI.TextField(new Rect((_window.width / 2) - 120, 400, 200, 50), textFieldString);
                    JoinRoomButton(new Rect((_window.width / 2) - 20, 460, 210f, 30f));
                    SetNameButton(new Rect((_window.width / 2) - 230, 460, 210f, 30f));
                }
                foreach (Button button in buttons)
                {
                    DrawMod(new Rect(currentCheatPosX, currentCheatPosY, 210f, 130f), button, 8);

                    if (currentCheatPosX < scrollViewRect.width - 100f * 2)
                    {
                        currentCheatPosX += 230f;
                    }
                    else
                    {
                        currentCheatPosX = 0f;
                        currentCheatPosY += 140f;
                    }
                }

                GUI.EndScrollView();
            }
            else if (isInPlayer)
            {
                if (PhotonNetwork.InRoom)
                {
                    var buttons = PhotonNetwork.PlayerListOthers;
                    int modsCount = buttons.Length;
                    float contentHeight = (modsCount / 3 + (modsCount % 3 > 0 ? 1 : 0)) * 140f;
                    Rect scrollViewRect = new Rect(20f, 90f, _window.width - 10f, _window.height - 110f);
                    Rect scrollContentRect = new Rect(0, 0, scrollViewRect.width - 20f, contentHeight);

                    scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, scrollContentRect, false, true);

                    float currentCheatPosX = 0f;
                    float currentCheatPosY = 0f;
                    foreach (Player button in buttons)
                    {
                        DrawPlayer(new Rect(currentCheatPosX, currentCheatPosY, 210f, 130f), button, 8);

                        if (currentCheatPosX < scrollViewRect.width - 100f * 2)
                        {
                            currentCheatPosX += 230f;
                        }
                        else
                        {
                            currentCheatPosX = 0f;
                            currentCheatPosY += 140f;
                        }
                    }

                    GUI.EndScrollView();

                }
            }



            DrawTabBar(new Rect(0f, 600f, _window.width, 20f), new Vector4(8f, 0f, 0f, 8f));

            GUI.DragWindow();

            DrawText(new Rect((_window.width / 2) - 35, -170, 400, 400f), "Steal", 30, Color.white, FontStyle.Italic, false, true);
        }


        public static void Initialize()
        {
            Background = CreateTexture(new Color32(10, 10, 10, 255));
            TabBar = CreateTexture(new Color32(15, 30, 55, 255));
            TabNormal = CreateTexture(new Color32(15, 30, 55, 255));
            TabActive = CreateTexture(new Color32(65, 80, 255, 255));
            Box = CreateTexture(new Color32(30, 30, 30, 255));
            BoxLine = CreateTexture(new Color32(65, 80, 255, 255));
            Disabled = CreateTexture(new Color32(90, 90, 90, 255));
            Enabled = CreateTexture(new Color32(65, 80, 255, 255));
            Infect = CreateTexture(new Color32(40, 0, 0, 255));
            UnInfect = CreateTexture(new Color32(40, 0, 0, 255));
        }


        private static void DrawMod(Rect rect, Button text, int borderRadius)
        {
            DrawTexture(rect, Box, 8);
            DrawTexture(new Rect(rect.x, rect.y + 25f, rect.width, 1f), BoxLine, 0);
            ModEnabledButton(new Rect(rect.x, rect.y + 100f, rect.width, 30f), text);

            string buttonName = "";
            if (text.doesHaveMultiplier)
            {
                buttonName = text.buttonText + "[" + text.multiplier() + "]";
            }
            else if (text.doesHaveStringer)
            {
                buttonName = text.buttonText + "[" + text.stringFunc() + "]";
            }
            else
            {
                buttonName = text.buttonText;
            }

            DrawText(new Rect(rect.x + 5f, rect.y, rect.width, 25f), buttonName, 12, Color.white, FontStyle.Bold, false, true);
            if (text.toolTip != null)
                DrawText(new Rect(rect.x, rect.y + 30f, rect.width, 30f), text.toolTip, 12, Color.white, FontStyle.Normal, true, true);
        }

        private static void DrawPlayer(Rect rect, Player text, int borderRadius)
        {
            DrawTexture(rect, Box, 8);
            DrawTexture(new Rect(rect.x, rect.y + 25f, rect.width, 1f), BoxLine, 0);
            PlayerTeleportButton(new Rect(rect.x, rect.y + 100f, rect.width, 30f), ()=>TeleportToPlayer(text));


            DrawText(new Rect(rect.x + 5f, rect.y, rect.width, 25f), text.NickName, 12, Color.white, FontStyle.Bold, false, true);

           
            VRRig vrrig = GorillaGameManager.instance.FindPlayerVRRig(text);
            if (vrrig.mainSkin.material.name.Contains("fected"))
            {
                PlayerUnTagButton(new Rect(rect.x, rect.y + 60f, rect.width, 30f), () => UnTagPlayer(text));
            }
            else
            {
                PlayerTagButton(new Rect(rect.x, rect.y + 60f, rect.width, 30f), text);
                //  Texture2D playertext = infectTexture;
                //GUILayout.Label(playertext, GUILayout.Width(30), GUILayout.Height(30));
            }
        }

        private static string TeleportToPlayer(Player player)
        {
            TeleportationLib.Teleport(GorillaGameManager.instance.FindPlayerVRRig(player).transform.position);
            return player.NickName;
        }

        private static void PlayerTeleportButton(Rect modRect, Action mod)
        {
            if (modRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                AssetLoader.Instance.PlayClick();
                mod();
            }
            DrawTexture(modRect, Enabled, 8);

            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
            labelstyle.fontSize = 12;
            labelstyle.fontStyle = FontStyle.Bold;
            DrawText(modRect, "Teleport", 12, Color.white, FontStyle.Bold, true, true);
        }

        public static void TagPlayer(Player player)
        {
            GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
            infect.currentInfected.Add(player);
        }

        private static void PlayerTagButton(Rect modRect, Player mod)
        {
            if (modRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                AssetLoader.Instance.PlayClick();
                TagPlayer(mod);
            }
            VRRig vrrig = GorillaGameManager.instance.FindPlayerVRRig(mod);

            UnInfect = ApplyColorFilter(vrrig.mainSkin.material.color);
            DrawTexture(modRect, UnInfect, 8);

            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
            labelstyle.fontSize = 12;
            labelstyle.fontStyle = FontStyle.Bold;
            DrawText(modRect, "Tag", 12, Color.white, FontStyle.Bold, true, true);
        }

        public static void UnTagPlayer(Player player)
        {
            GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
            infect.currentInfected.Remove(player);
        }

        private static void PlayerUnTagButton(Rect modRect, Action mod)
        {
            if (modRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                AssetLoader.Instance.PlayClick();
                mod();
            }
            DrawTexture(modRect, Infect, 8);

            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
            labelstyle.fontSize = 12;
            labelstyle.fontStyle = FontStyle.Bold;
            DrawText(modRect, "UnTag", 12, Color.white, FontStyle.Bold, true, true);
        }


        private static void ModEnabledButton(Rect modRect, Button mod)
        {
            if (modRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                AssetLoader.Instance.PlayClick();
                Toggle(mod);
            }
            if (mod.Enabled || !mod.isToggle)
            {
                DrawTexture(modRect, Enabled, 8);
            }
            else
            {
                DrawTexture(modRect, Disabled, 8);
            }
            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
            labelstyle.fontSize = 12;
            labelstyle.fontStyle = FontStyle.Bold;
            if (mod.Enabled && mod.isToggle)
                DrawText(modRect, "Enabled", 12, Color.white, FontStyle.Bold, true, true);
            else if (mod.isToggle)
                DrawText(modRect, "Disabled", 12, Color.white, FontStyle.Bold, true, true);
            else
                DrawText(modRect, "Run Mod", 12, Color.white, FontStyle.Bold, true, true);
        }

        private static void JoinRoomButton(Rect modRect)
        {
            if (modRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                AssetLoader.Instance.PlayClick();
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(textFieldString.ToUpper());
            }

            DrawTexture(modRect, Enabled, 8);

            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
            labelstyle.fontSize = 12;
            labelstyle.fontStyle = FontStyle.Bold;

            DrawText(modRect, "Join Room", 12, Color.white, FontStyle.Bold, true, true);
        }

        private static void SetNameButton(Rect modRect)
        {
            if (modRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                AssetLoader.Instance.PlayClick();
                PhotonNetwork.LocalPlayer.NickName = textFieldString.ToUpper();
                PlayerPrefs.SetString("playerName", textFieldString.ToUpper());
                GorillaComputer.instance.offlineVRRigNametagText.text = textFieldString.ToUpper();
                GorillaTagger.Instance.offlineVRRig.playerName = textFieldString.ToUpper();
                PlayerPrefs.Save();
            }

            DrawTexture(modRect, Enabled, 8);

            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
            labelstyle.fontSize = 12;
            labelstyle.fontStyle = FontStyle.Bold;

            DrawText(modRect, "Set Name", 12, Color.white, FontStyle.Bold, true, true);
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
        private static void Tab(float tabPos, MenuPatch.Category category)
        {
            Rect rect = new Rect();
            if (category != Category.Settings)
                rect = new Rect(tabPos, 50f, 130f, 30f);
            else
                rect = new Rect(570, 10f, 130f, 30f);
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                AssetLoader.Instance.PlayClick();
                currentNuGuiPage = category;
                isInPlayer = false;
            }
            if (currentNuGuiPage == category)
            {
                DrawTexture(rect, TabActive, 16);
            }
            else
            {
                DrawTexture(rect, TabNormal, 16);
            }
            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
            labelstyle.fontSize = 12;
            labelstyle.fontStyle = FontStyle.Bold;
            DrawText(rect, category.ToString(), 12, Color.white, FontStyle.Bold, true, true);
        }

        private static bool isInPlayer = false;
        private static void Player()
        {
            Rect rect = new Rect();
            rect = new Rect(10, 10f, 130f, 30f);
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                isInPlayer = !isInPlayer;
            }
            if (isInPlayer)
            {
                DrawTexture(rect, TabActive, 16);
            }
            else
            {
                DrawTexture(rect, TabNormal, 16);
            }
            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
            labelstyle.fontSize = 12;
            labelstyle.fontStyle = FontStyle.Bold;
            DrawText(rect, "Players", 12, Color.white, FontStyle.Bold, true, true);
        }


        public static bool FreeCamMOd = false;
        private static void Freecma()
        {
            Rect rect = new Rect();
            rect = new Rect(150, 10f, 130f, 30f);
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                FreeCamMOd = !FreeCamMOd;
            }
            if (FreeCamMOd)
            {
                DrawTexture(rect, TabActive, 16);
            }
            else
            {
                DrawTexture(rect, TabNormal, 16);
            }
            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
            labelstyle.fontSize = 12;
            labelstyle.fontStyle = FontStyle.Bold;
            DrawText(rect, "Freecam", 12, Color.white, FontStyle.Bold, true, true);
        }

        public static void DrawTabBar(Rect rect, Vector4 borderRadius)
        {
            DrawTexture(rect, TabBar, 0, borderRadius);
        }
        public static void DrawWindow(Rect rect, int borderRadius)
        {
            DrawTexture(rect, Background, borderRadius);
        }
        private static Texture2D CreateTexture(Color color)
        {
            Texture2D returnTexture = new Texture2D(1, 1);
            returnTexture.SetPixel(0, 0, color);
            returnTexture.Apply();
            return returnTexture;
        }

        private static void DrawTexture(Rect rect, Texture2D texture, int borderRadius, Vector4 borderRadius4 = default)
        {
            if (borderRadius4 == Vector4.zero)
                borderRadius4 = new Vector4(borderRadius, borderRadius, borderRadius, borderRadius);
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false, 0f, GUI.color, Vector4.zero, borderRadius4);
        }
    }
}