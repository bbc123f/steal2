using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Steal;
using Steal.Background;
using UnityEngine;
using UnityEngine.XR;


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
    // Set the text color based on the theme
    guiStyle.normal.textColor = MenuPatch.GetTheme(UI.Theme)[2];

    // Define the right edge for alignment and padding
    float rightEdge = Screen.width;
    float padding = 10f; // Padding from the right edge
    float boxPadding = 5.0f; // Padding for the background box around the text
    float rectWidth = 10.0f; // Width of the rectangle next to the mod name
    float spacing = 5.0f; // Reduced spacing between mods for a closer appearance

    // Starting Y position for the mods list
    float startPositionY = 10.0f;

    Color originalColor = GUI.color;

    var sortedMods = modsEnabled.OrderByDescending(m => m.Length).ToList();

    foreach (var modName in sortedMods)
    {
        GUIContent modContent = new GUIContent(modName);
        Vector2 modSize = guiStyle.CalcSize(modContent);
        
        float modPositionX = rightEdge - modSize.x - padding - rectWidth;
        
        GUI.color = new Color(0, 0, 0, 0.5f); 
        GUI.Box(new Rect(modPositionX - boxPadding, startPositionY - boxPadding / 2, modSize.x + (2 * boxPadding), modSize.y + boxPadding), GUIContent.none);
        
        GUI.color = originalColor;
        
        GUI.Label(new Rect(modPositionX, startPositionY, modSize.x, modSize.y), modName, guiStyle);
        
        GUI.backgroundColor = MenuPatch.GetTheme(UI.Theme)[0];
        GUI.Box(new Rect(rightEdge - padding, startPositionY, rectWidth, modSize.y), GUIContent.none);
        
        startPositionY += modSize.y + spacing;
    }
    
    GUI.color = originalColor;
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