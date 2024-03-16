using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Steal;
using Steal.Background;
using UnityEngine;
using UnityEngine.XR;
using WristMenu;

public class ModsList : MonoBehaviour
{
    public static List<string> modsEnabled = new List<string>();

    private static string displayedText = String.Empty;
    private GUIStyle guiStyle;

    private static string displayedText2 = "steal.lol";
    private GUIStyle guiStyle2;

    private int highlightedIndex = 0;
    private float timer = 0.0f;
    private const float changeInterval = 0.2f;
    public static float deltaTime = 0;

    public void OnDisable()
    {
        displayedText = " ";
        displayedText = "";
        displayedText2 = "";
        guiStyle = null;
        guiStyle2 = null;
    }

    void OnEnable()
    {
        guiStyle = new GUIStyle();
        guiStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        guiStyle.normal.textColor = MenuPatch.GetTheme(UI.Theme)[2];
        guiStyle.fontSize = 20;
        guiStyle.wordWrap = true;

        guiStyle2 = new GUIStyle();
        guiStyle2.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        guiStyle2.normal.textColor = MenuPatch.GetTheme(UI.Theme)[2];
        guiStyle2.fontSize = 20;
        guiStyle2.wordWrap = true;
    }


    void Start()
    {
        guiStyle = new GUIStyle();
        guiStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        guiStyle.normal.textColor = MenuPatch.GetTheme(UI.Theme)[2];
        guiStyle.fontSize = 20;
        guiStyle.wordWrap = true;

        guiStyle2 = new GUIStyle();
        guiStyle2.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        guiStyle2.normal.textColor = MenuPatch.GetTheme(UI.Theme)[2];
        guiStyle2.fontSize = 20;
        guiStyle2.wordWrap = true;
    }

    void OnGUI()
    {
        guiStyle.normal.textColor = MenuPatch.GetTheme(UI.Theme)[2];
        guiStyle2.normal.textColor = MenuPatch.GetTheme(UI.Theme)[2];
        displayedText2 = "steal.lol - FPS:" + Mathf.RoundToInt(1f / deltaTime);
        GUIContent content = new GUIContent(displayedText);
        Vector2 size = guiStyle.CalcSize(content);


        GUI.Label(new Rect(Screen.width - size.x - 10, 10, size.x + 10, size.y), displayedText, guiStyle);

        Vector2 currentPosition = new Vector2(10, 10);

        for (int i = 0; i < displayedText2.Length; i++)
        {
            string character = displayedText2[i].ToString();
            guiStyle2.normal.textColor = (i == highlightedIndex) ? MenuPatch.GetTheme(UI.Theme)[2] * 1.6f : MenuPatch.GetTheme(UI.Theme)[2];

            Vector2 size2 = guiStyle2.CalcSize(new GUIContent(character));
            GUI.Label(new Rect(currentPosition.x, currentPosition.y, size2.x, size2.y), character, guiStyle2);

            currentPosition.x += size2.x;
        }
    }


    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        displayedText2 = "steal.lol - FPS:" + Mathf.RoundToInt(1f / deltaTime);

        timer += Time.deltaTime;

        if (timer > changeInterval)
        {
            timer = 0.0f;
            highlightedIndex = (highlightedIndex + 1) % displayedText2.Length;
        }


    }


    public static void RefreshText()
    {
        List<string> modsEnabled4 = new List<string>();
        foreach (MenuPatch.Button bt in MenuPatch.buttons)
        {
            if (bt.Enabled)
            {
                modsEnabled4.Add(bt.buttonText);
            }
        }

        modsEnabled = modsEnabled4;
        var sortedStrings = modsEnabled.OrderByDescending(s => s.Length);

        string newText = string.Join("\n", sortedStrings);
        displayedText = newText;
    }


}