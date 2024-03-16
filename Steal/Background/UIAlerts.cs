using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.Background
{
    internal class UIAlerts : MonoBehaviour
    {
        public static bool ShowAlert = false;
        public static string Alert = "";

        public static void SendAlert(string alert)
        {
            Alert = alert;
            ShowAlert = true;
        }

        public void OnGUI()
        {
            if (ShowAlert)
            {
                GUILayout.Window(100, new Rect(Screen.width / 2, Screen.height / 2, 230, 100), Window, "ALERT");
            }
        }

        public void Window(int id)
        {
            if (ShowAlert)
            {
                GUILayout.Space(10);
                GUILayout.Label(Alert);
                GUILayout.Space(15);
                if (GUILayout.Button("Close"))
                {
                    ShowAlert = false;
                }
                GUI.DragWindow(new Rect(0, 0, 100000, 10000));
            }
        }
    }
}
