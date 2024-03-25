using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using System.IO;
using System;
using System.Net;

namespace Steal.Background
{
    internal class SettingsLib : MonoBehaviour
    {
        public static bool hasInit = false;

        public static ConfigFile cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "stealconfig.cfg"), true);

        public static string bgURI;

        public static Color32 BGColor;
        public static Color32 ButtonColor;
        public static Color32 ButtonText;

        public static bool LegacyGhost;
        public static bool ShowConsole;

        public static bool Catagories;

        public static void Load()
        {
            hasInit = true;
            var urlpath = cfgFile.Bind<string>("Customization", "Custom_Background", "NONE", "You can make this either a URL or a file path!").Value;
            bgURI = urlpath;
            if (urlpath != null && urlpath != "NONE")
            {
                if (urlpath.Contains("https"))
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(urlpath);
                    request.Method = "HEAD";

                    bool exists;
                    try
                    {
                        request.GetResponse();
                        exists = true;
                    }
                    catch
                    {
                        exists = false;
                    }
                    if (exists)
                    {
                        bgURI = urlpath;
                    }
                    else
                    {
                        bgURI = "NONE";
                        Debug.Log("URL DOES NOT EXIST!");
                    }
                    
                }
                else
                {
                    if (!File.Exists(urlpath))
                    {
                        bgURI = "NONE";
                        Debug.Log("PATH DOES NOT EXIST!");
                    }
                    else
                    {
                        bgURI = urlpath;
                    }
                }
            }

            BGColor = cfgFile.Bind("Customization", "Custom_Background_Color", new Color(0, 0, 0, 0), "Set to 00000000 if you want it to the default theme!").Value;
            ButtonColor = cfgFile.Bind("Customization", "Custom_Button_Color", new Color(0, 0, 0, 0), "Set to 00000000 if you want it to the default theme!").Value;
            ButtonText = cfgFile.Bind("Customization", "Custom_Text_Color", new Color(0, 0, 0, 0), "Set to 00000000 if you want it to the default text theme!").Value;
            LegacyGhost = cfgFile.Bind("Customization", "Legacy_Ghost", true, "When turning on things like Invis Monkey or Ghost Monkey there will be another rig to take the place of the old one!").Value;
            ShowConsole = cfgFile.Bind("Customization", "Show_Update_Console", true, "When you start your game steal will open a console make this false to disable it!").Value;
            Catagories = cfgFile.Bind("Customization", "Catagories", true, "Enable or disable catagories on the menu! (does not effect GUI)").Value;

        }


        public void OnEnable()
        {
            Load();
            MenuPatch.categorized = Catagories;

        }

    }
}
