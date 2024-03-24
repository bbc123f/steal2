using Steal;
using System;
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

        private static Rect _window = new Rect(20f, 20f, 710f, 600f);
        private static bool _enabled = true;
        private static MenuPatch.Category currentNuGuiPage = Category.Movement;
        private static Vector2 scrollPosition;

        private void OnGUI()
        {
            if (_enabled == false)
                return;
            _window = GUI.Window(7859, _window, WindowMain, "", new GUIStyle());
        }


        private void Update()
        {
            if (Keyboard.current[Key.RightShift].wasPressedThisFrame)
                _enabled = !_enabled;
        }

        private void Start()
        {
            Initialize();
            GiveModToolTip();
        }


        private static void WindowMain(int id)
        {
            DrawWindow(new Rect(0f, 0f, _window.width, _window.height), 8);
            float currentTabPos = 10f;
            foreach (Category page in Enum.GetValues(typeof(Category)))
            {
                if (page != Category.Base && page != Category.Settings)
                {
                    Tab(currentTabPos, page);
                    currentTabPos += 140;
                }
            }

            var buttons = GetButtonInfoByPage(currentNuGuiPage);
            int modsCount = buttons.Count;
            float contentHeight = (modsCount / 3 + (modsCount % 3 > 0 ? 1 : 0)) * 140f;
            Rect scrollViewRect = new Rect(20f, 90f, _window.width - 10f, _window.height - 110f);
            Rect scrollContentRect = new Rect(0, 0, scrollViewRect.width - 20f, contentHeight);

            scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, scrollContentRect, false, true);

            float currentCheatPosX = 0f;
            float currentCheatPosY = 0f;
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

            DrawTabBar(new Rect(0f, 530f, _window.width, 20f), new Vector4(8f, 0f, 0f, 8f));

            GUI.DragWindow();
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
        }


        private static void DrawMod(Rect rect, Button text, int borderRadius)
        {
            DrawTexture(rect, Box, 8);
            DrawTexture(new Rect(rect.x, rect.y + 25f, rect.width, 1f), BoxLine, 0);
            ModEnabledButton(new Rect(rect.x, rect.y + 100f, rect.width, 30f), text);
               DrawText(new Rect(rect.x + 5f, rect.y, rect.width, 25f), text.buttonText, 12, Color.white, FontStyle.Bold, false, true);
            if (text.toolTip != null)
                DrawText(new Rect(rect.x, rect.y + 30f, rect.width, 30f), text.toolTip, 12, Color.white, FontStyle.Normal, true, true);
        }



        private static void ModEnabledButton(Rect modRect, Button mod)
        {
            if (modRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                Toggle(mod);
            if (mod.Enabled)
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
            if (mod.Enabled)
                DrawText(modRect, "Enabled", 12, Color.white, FontStyle.Bold, true, true);
            else
                DrawText(modRect, "Disabled", 12, Color.white, FontStyle.Bold, true, true);
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
            Rect rect = new Rect(tabPos, 60f, 130f, 30f);
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                currentNuGuiPage = category;
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