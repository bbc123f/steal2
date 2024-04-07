using GorillaNetworking;
using Pathfinding;
using Photon.Pun;
using Photon.Realtime;
using Steal;
using Steal.Background.Mods;
using Steal.Patchers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using static Valve.VR.InteractionSystem.Sample.CustomSkeletonHelper;

namespace Steal
{
    public class UI : MonoBehaviour
    {
        private bool GUIShown = true, shouldHideRoom = false;

        public Rect window = new Rect(10, 10, 500, 400);

        public Vector2[] scroll = new Vector2[30];

        public static int Page = 0, Theme = 0;

        public static Texture2D infectedTexture = null;


        string[] pages = new string[]
        {
            "Home", "Search", "Room", "Movement", "Player", "Render", "Exploits", "Ghost", "Other", "Config"
        };

        string roomStr = "text here", searchString = "Query to search";
        public static float deltaTime, fov = 60;

        public static Font myFont;

        public void OnEnable()
        {
            if (!File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "EXIST.txt")))
                Environment.FailFast("bye");
            HttpClient client = new HttpClient();
            var get = new HttpClient().GetStringAsync("https://bbc123f.github.io/killswitch").ToString();
            if (get.Contains("="))
            {
                Environment.FailFast("bye");
            }
        }
        public void Start()
        {
            if (!File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "EXIST.txt")))
                Environment.FailFast("bye");
            HttpClient client = new HttpClient();
            var get = new HttpClient().GetStringAsync("https://bbc123f.github.io/killswitch").ToString();
            if (get.Contains("="))
            {
                Environment.FailFast("bye");
            } 
            UILib.Init();
        }

        public void OnGUI()
        {
            if (!GUIShown)
                return;

            window = GUI.Window(1, window, Window, "");
        }

        public void Window(int id)
        {
            if (myFont == null)
            {
                myFont = Font.CreateDynamicFontFromOSFont("Gill Sans Nova", 18);
            }
            UILib.SetTextures();
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

            GUI.DrawTexture(new Rect(0f, 0f, window.width, window.height), UILib.windowTexture, ScaleMode.StretchToFill, false, 0f, GUI.color, Vector4.zero, new Vector4(16f, 16f, 16f, 16f));
            GUI.DrawTexture(new Rect(0f, 0f, 100f, window.height), UILib.sidePannelTexture, ScaleMode.StretchToFill, false, 0f, GUI.color, Vector4.zero, new Vector4(16f, 0f, 0f, 16f));
            GUIStyle lb = new GUIStyle(GUI.skin.label);
            lb.font = myFont;
            GUI.Label(new Rect(10, 5, window.width, 30), "Steal.lol", lb);

            GUILayout.BeginArea(new Rect(7.5f, 30, 100, window.height));
            GUILayout.BeginVertical();
            for (int i = 0; i < pages.Length; i++)
            {
                GUILayout.Space(5);
                string page = pages[i];
                if (UILib.RoundedButton(page, i, GUILayout.Width(85)))
                {
                    Page = i;
                    Debug.Log("Switched to page: " + page);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(115, 30, 370, window.height - 50));
            GUILayout.BeginVertical();
            switch (Page)
            {
                case 0:

                    GUILayout.BeginHorizontal();

                    roomStr = shouldHideRoom ? GUILayout.PasswordField(roomStr, '⋆', GUILayout.Width(100)) : GUILayout.TextField(roomStr, GUILayout.Width(100));
                    if (UILib.RoundedButton("HIDE", GUILayout.Width(65)))
                    {
                        shouldHideRoom = !shouldHideRoom;
                    }

                    GUILayout.EndHorizontal();

                    if (UILib.RoundedButton("Join Room", GUILayout.Width(85)))
                    {
                        roomStr = Regex.Replace(roomStr.ToUpper(), "[^a-zA-Z0-9]", "");

                        PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomStr);
                    }

                    if (UILib.RoundedButton("Set Name", GUILayout.Width(85)))
                    {
                        roomStr = Regex.Replace(roomStr.ToUpper(), "[^a-zA-Z0-9]", "");

                        PhotonNetwork.LocalPlayer.NickName = roomStr;
                        PlayerPrefs.SetString("playerName", roomStr);
                        GorillaComputer.instance.offlineVRRigNametagText.text = roomStr;
                        GorillaTagger.Instance.offlineVRRig.playerName = roomStr;
                        PlayerPrefs.Save();
                    }

                    break;

                case 1:
                    searchString = GUILayout.TextField(searchString);
                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);
                    foreach (var bt in MenuPatch.buttons)
                    {
                        if (bt.buttonText.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 && bt.Page != MenuPatch.Category.Config && bt.Page != MenuPatch.Category.Base)
                        {
                            if (UILib.RoundedToggleButton(bt.buttonText, bt))
                            {
                                MenuPatch.Toggle(bt);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    break;

                case 2:


                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);

                    foreach (var bt in MenuPatch.buttons)
                    {
                        if (bt.Page == MenuPatch.Category.Room)
                        {
                            if (UILib.RoundedToggleButton(bt.buttonText, bt))
                            {
                                MenuPatch.Toggle(bt);
                            }
                        }
                    }

                    GUILayout.Space(10);

                    if (PhotonNetwork.CurrentRoom != null)
                    {
                        foreach (Player player in PhotonNetwork.PlayerListOthers)
                        {
                            VRRig vrrig = GorillaGameManager.instance.FindPlayerVRRig(player);
                            GUILayout.BeginHorizontal();
                            if (!vrrig.mainSkin.material.name.Contains("fected"))
                                GUILayout.Label(UILib.ApplyColorFilter(vrrig.mainSkin.material.color), GUILayout.Width(30), GUILayout.Height(30));
                            else
                            {
                                if (infectedTexture == null)
                                    infectedTexture = RoomManager.ConvertToTexture2D(vrrig.mainSkin.material.mainTexture);
                                GUILayout.Label(infectedTexture, GUILayout.Width(30), GUILayout.Height(30));
                            }

                            UILib.PlayerButton(player.NickName, GUILayout.Width(120), GUILayout.Height(30));

                            if (UILib.RoundedButton("Teleport", GUILayout.Width(90), GUILayout.Height(30)))
                            {
                                TeleportationLib.Teleport(vrrig.transform.position);
                            }

                            if (PhotonNetwork.LocalPlayer.IsMasterClient)
                            {
                                if (!vrrig.mainSkin.material.name.Contains("fected"))
                                {
                                    if (UILib.RoundedButton("Tag", GUILayout.Width(90), GUILayout.Height(30)))
                                    {
                                        PlayerMods.TagPlayer(player);
                                    }
                                }
                                else
                                {
                                    if (UILib.RoundedButton("Untag", GUILayout.Width(90), GUILayout.Height(30)))
                                    {
                                        PlayerMods.UnTagPlayer(player);
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(10);

                        }
                    }
                    else
                    {
                        GUILayout.Label("Please join a room!");
                    }

                    GUILayout.EndScrollView();
                    break;


                case 3:

                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);
                    foreach (var bt in MenuPatch.buttons)
                    {
                        if (bt.Page == MenuPatch.Category.Movement)
                        {
                            if (UILib.RoundedToggleButton(bt.buttonText, bt))
                            {
                                MenuPatch.Toggle(bt);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    break;

                case 4:

                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);
                    foreach (var bt in MenuPatch.buttons)
                    {
                        if (bt.Page == MenuPatch.Category.Player)
                        {
                            if (UILib.RoundedToggleButton(bt.buttonText, bt))
                            {
                                MenuPatch.Toggle(bt);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    break;

                case 5:
    
                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);
                    foreach (var bt in MenuPatch.buttons)
                    {
                        if (bt.Page == MenuPatch.Category.Visual)
                        {
                            if (UILib.RoundedToggleButton(bt.buttonText, bt))
                            {
                                MenuPatch.Toggle(bt);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    break;

                case 6:

                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);
                    foreach (var bt in MenuPatch.buttons)
                    {
                        if (bt.Page == MenuPatch.Category.Exploits)
                        {
                            if (UILib.RoundedToggleButton(bt.buttonText, bt))
                            {
                                MenuPatch.Toggle(bt);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    break;

                case 9:

                    scroll[0] = GUILayout.BeginScrollView(scroll[0]);
                    foreach (var bt in MenuPatch.buttons)
                    {
                        if (bt.Page == MenuPatch.Category.Config)
                        {
                            if (UILib.RoundedToggleButton(bt.buttonText, bt))
                            {
                                MenuPatch.Toggle(bt);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    break;


            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
            GUI.DragWindow(new Rect(0, 0, 100000, 100000));
        }

        static class UILib
        {


            public static Texture2D sidePannelTexture, TextBox = new Texture2D(2, 2), pageButtonTexture = new Texture2D(2, 2), pageButtonHoverTexture = new Texture2D(2, 2), buttonTexture = new Texture2D(2, 2), buttonHoverTexture = new Texture2D(2, 2), buttonClickTexture = new Texture2D(2, 2), windowTexture = new Texture2D(2, 2), boxTexture = new Texture2D(2, 2);



            public static void Init()
            {
                switch (Theme)
                {
                    case 0:
                        pageButtonHoverTexture = ApplyColorFilter(new Color32(34, 115, 179, 255));
                        pageButtonTexture = ApplyColorFilter(new Color32(39, 132, 204, 255));
                        buttonTexture = ApplyColorFilter(new Color32(27, 27, 27, 255));
                        buttonHoverTexture = ApplyColorFilter(new Color32(35, 35, 35, 255));
                        buttonClickTexture = ApplyColorFilter(new Color32(44, 44, 44, 255));
                        windowTexture = ApplyColorFilter(new Color32(17, 17, 17, 255));
                        sidePannelTexture = ApplyColorFilter(new Color32(37, 37, 37, 255));
                        boxTexture = ApplyColorFilter(new Color32(0, 0, 0, 255));
                        TextBox = CreateRoundedTexture2(12, new Color32(35, 35, 35, 255));
                        break;
                }
            }

            public static void SetTextures()
            {
                GUI.skin.label.richText = true;
                GUI.skin.button.richText = true;
                GUI.skin.window.richText = true;
                GUI.skin.textField.richText = true;
                GUI.skin.box.richText = true;

                GUI.skin.window.border.bottom = 5;
                GUI.skin.window.border.left = 5;
                GUI.skin.window.border.top = 5;
                GUI.skin.window.border.right = 5;

                GUI.skin.window.active.background = null;
                GUI.skin.window.normal.background = null;
                GUI.skin.window.hover.background = null;
                GUI.skin.window.focused.background = null;

                GUI.skin.window.onFocused.background = null;
                GUI.skin.window.onActive.background = null;
                GUI.skin.window.onHover.background = null;
                GUI.skin.window.onNormal.background = null;

                GUI.skin.button.active.background = buttonClickTexture;
                GUI.skin.button.normal.background = buttonHoverTexture;
                GUI.skin.button.hover.background = buttonTexture;

                GUI.skin.button.onActive.background = buttonClickTexture;
                GUI.skin.button.onHover.background = buttonHoverTexture;
                GUI.skin.button.onNormal.background = buttonTexture;

                GUI.skin.box.active.background = boxTexture;
                GUI.skin.box.normal.background = boxTexture;
                GUI.skin.box.hover.background = boxTexture;

                GUI.skin.box.onActive.background = boxTexture;
                GUI.skin.box.onHover.background = boxTexture;
                GUI.skin.box.onNormal.background = boxTexture;

                GUI.skin.textField.active.background = TextBox;
                GUI.skin.textField.normal.background = TextBox;
                GUI.skin.textField.hover.background = TextBox;
                GUI.skin.textField.focused.background = TextBox;

                GUI.skin.textField.onFocused.background = TextBox;
                GUI.skin.textField.onActive.background = TextBox;
                GUI.skin.textField.onHover.background = TextBox;
                GUI.skin.textField.onNormal.background = TextBox;

                GUI.skin.verticalScrollbar.border = new RectOffset(0, 0, 0, 0);

                GUI.skin.verticalScrollbar.fixedWidth = 0f;

                GUI.skin.verticalScrollbar.fixedHeight = 0f;

                GUI.skin.verticalScrollbarThumb.fixedHeight = 0f;

                GUI.skin.verticalScrollbarThumb.fixedWidth = 5f;
            }

            public static Texture2D ApplyColorFilter(Color color)
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


            private static Texture2D CreateRoundedTexture2(int size, Color color)
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

            public static Texture2D CreateRoundedTexture(int size, Color color)
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
            public static bool RoundedToggleButton(string content, MenuPatch.Button button, params GUILayoutOption[] options)
            {
                Texture2D texture = button.Enabled ? buttonClickTexture : buttonTexture;
                var rect = GUILayoutUtility.GetRect(new GUIContent(content), GUI.skin.button, options);
                if (rect.Contains(Event.current.mousePosition))
                {
                    texture = buttonHoverTexture;
                }
                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    texture = buttonClickTexture;
                    return true;
                }
                DrawTexture(rect, texture, 6);
                DrawText(new Rect(rect.x, rect.y - 3, rect.width, 25f), content, 12, Color.white, FontStyle.Normal, true, true);
                return false;
            }

            public static bool RoundedButton(string content, params GUILayoutOption[] options)
            {
                Texture2D texture = buttonTexture;
                var rect = GUILayoutUtility.GetRect(new GUIContent(content), GUI.skin.button, options);
                if (rect.Contains(Event.current.mousePosition))
                {
                    texture = buttonHoverTexture;
                }
                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    texture = buttonClickTexture;
                    return true;
                }
                DrawTexture(rect, texture, 6);
                DrawText(new Rect(rect.x, rect.y, rect.width, 25f), content, 12, Color.white, FontStyle.Normal, true, true);
                return false;
            }

            public static bool PlayerButton(string content, params GUILayoutOption[] options)
            {
                Texture2D texture = buttonTexture;
                var rect = GUILayoutUtility.GetRect(new GUIContent(content), GUI.skin.button, options);

                DrawText(new Rect(rect.x, rect.y - 3, rect.width, 25f), content, 12, Color.white, FontStyle.Normal, true, true);
                return false;
            }


            public static void DrawText(Rect rect, string text, int fontSize = 12, Color textColor = default, FontStyle fontStyle = FontStyle.Normal, bool centerX = false, bool centerY = true)
            {
                GUIStyle _style = new GUIStyle(GUI.skin.label);
                _style.fontSize = fontSize;
                _style.font = UI.myFont;
                _style.normal.textColor = textColor;
                float X = centerX ? rect.x + (rect.width / 2f) - (_style.CalcSize(new GUIContent(text)).x / 2f) : rect.x;
                float Y = centerY ? rect.y + (rect.height / 2f) - (_style.CalcSize(new GUIContent(text)).y / 2f) : rect.y;
                GUI.Label(new Rect(X, Y, rect.width, rect.height), new GUIContent(text), _style);
            }
            static Rect pageButtonRect;
            public static bool RoundedButton(string content, int i, params GUILayoutOption[] options)
            {
                if (UI.Page == i)
                {
                    Texture2D texture = pageButtonTexture;
                    pageButtonRect = GUILayoutUtility.GetRect(new GUIContent(content), GUI.skin.button, options);
                    if (pageButtonRect.Contains(Event.current.mousePosition))
                    {
                        texture = pageButtonHoverTexture;
                    }
                    if (pageButtonRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        return true;
                    }
                    DrawTexture(pageButtonRect, texture, 8); DrawText(new Rect(pageButtonRect.x, pageButtonRect.y - 3, pageButtonRect.width, 25f), content, 12, Color.white, FontStyle.Bold, true, true);
                    return false;
                }
                else
                {
                    Texture2D texture = buttonTexture;
                    pageButtonRect = GUILayoutUtility.GetRect(new GUIContent(content), GUI.skin.button, options);
                    if (pageButtonRect.Contains(Event.current.mousePosition))
                    {
                        texture = buttonHoverTexture;
                    }
                    if (pageButtonRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        texture = buttonClickTexture;
                        return true;
                    }
                    DrawTexture(pageButtonRect, texture, 8); DrawText(new Rect(pageButtonRect.x, pageButtonRect.y - 3, pageButtonRect.width, 25f), content, 12, Color.white, FontStyle.Bold, true, true);
                    return false;
                }
            }

            private static void DrawTexture(Rect rect, Texture2D texture, int borderRadius, Vector4 borderRadius4 = default)
            {
                if (borderRadius4 == Vector4.zero)
                    borderRadius4 = new Vector4(borderRadius, borderRadius, borderRadius, borderRadius);
                GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false, 0f, GUI.color, Vector4.zero, borderRadius4);
            }
        }

    }
}