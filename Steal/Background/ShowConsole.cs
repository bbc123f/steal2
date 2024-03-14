using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft;

namespace Steal.Background
{
    class VersionJSON
    {
        public string Major { get; set; }
        public string Minor { get; set; }
        public string Revisions { get; set; }
    }
    internal class ShowConsole : MonoBehaviour
    {
        private IntPtr console;

        private StreamWriter writer;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        private async void Awake()
        {
            AllocConsole();
            console = GetConsoleWindow();
            ShowWindow(console, 5);
            writer = new StreamWriter(Console.OpenStandardOutput());
            writer.AutoFlush = true;
            Console.Title = "STEAL.LOL ON TOP";
            Console.SetOut(writer);
            Logo();
        }

        public void Logo()
        {
            string str = "  .     '     ,\r\n    _________\r\n _ /_|_____|_\\ _\r\n   '. \\   / .'\r\n     '.\\ /.'\r\n       '.'";
            Log(str);
            try
            {
                VersionJSON[] jsons = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionJSON[]>(new WebClient().DownloadString("https://tnuser.com/API/files/StealVersion.json"));
                var json = jsons[0];
                if (int.Parse(json.Revisions) > Version.Revision || int.Parse(json.Major) > Version.MajorVersion || int.Parse(json.Minor) > Version.MinorVersion)
                {
                    Log("Steal.lol Version: " + Version.MajorVersion + "." + Version.MinorVersion + "." + Version.Revision + " Update Available: TRUE");
                    return;
                }
                Log("Steal.lol Version: " + Version.MajorVersion + "." + Version.MinorVersion + "." + Version.Revision + " Update Available: FALSE");
            } catch(Exception ex) { Log(ex.ToString()); }
        }

        public static void Log(object message)
        {
            Console.WriteLine(message);
        }
        public static void LogERR(object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public static void LogError(object message)
        {
            //dismiss
        }
    }
}
