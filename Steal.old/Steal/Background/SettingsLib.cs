using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Microsoft.Win32.SafeHandles;

namespace Steal.Background
{
    internal static class SettingsLib
    {
        public static class Settings
        {
            public static int PlatformColors = 0;

            public static int BackgroundColor = 0;

            public static float speedBoost = 6.5f;

            public static float flightSpeed = 10;

            public static float WallWalkGravity = -9.81f;

            public static bool ShowIdentifier = true;

            public static bool AutoAntiBan = true;
        }

        public static Color[] ConvertColors =
        {
            Color.black, Color.white, Color.red, Color.yellow, Color.green, Color.blue, Color.magenta
        };

        public static void Init()
        {
            Settings.speedBoost = PlayerPrefs.GetFloat("steal_Speed", 6.5f);
            Settings.flightSpeed = PlayerPrefs.GetFloat("steal_FlySpeed", 10.0f);
            Settings.PlatformColors = PlayerPrefs.GetInt("steal_PlatformsKey", 0);
            Settings.BackgroundColor = PlayerPrefs.GetInt("steal_backGround", 0);
            Settings.WallWalkGravity = PlayerPrefs.GetFloat("steal_GravityStrength", -9.81f);
            Settings.ShowIdentifier = PlayerPrefs.GetInt("steal_ShowIdentifier", 1) == 1 ? true : false;
            Settings.AutoAntiBan = PlayerPrefs.GetInt("steal_AutoAntiBan", 1) == 1 ? true : false;
        }

        public static void AddToValue(int value, string valueName)
        {
            PlayerPrefs.SetInt(valueName, PlayerPrefs.GetInt(valueName) + value);
            PlayerPrefs.Save();
        }

        public static void SetValue(bool value, string valueName)
        {
            PlayerPrefs.SetInt(valueName, value == true ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SetValue(int value, string valueName)
        {
            PlayerPrefs.SetInt(valueName, value);
            PlayerPrefs.Save();
        }

        public static void SetValue(float value, string valueName)
        {
            PlayerPrefs.SetFloat(valueName, value);
            PlayerPrefs.Save();
        }
    }
}
