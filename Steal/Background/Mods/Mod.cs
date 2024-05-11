using System;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Steal.Components;
using Steal.Background.Security;
using Steal.Background.Security.Auth;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Text;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Steal.Background.Mods
{
    public class Mod : MonoBehaviourPunCallbacks
    {

        public static BepInEx.Logging.ManualLogSource logSource;
        public static IEnumerator checkSession()
        {
            var www = new WWW("https://chingchong.cloud/steal/hooks/isSession.php?sessionkey=" + Base.sessionID);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                File.WriteAllText("error.txt", "Session Check error - " + Convert.ToBase64String(Encoding.UTF8.GetBytes("Response: " + www.error.ToString())));
                Environment.FailFast("bye");
            }

            switch (www.text)
            {
                case "No session found!":
                    File.WriteAllText("error.txt", "SessionID not found!"); Environment.FailFast("1");
                    break;

                case "Session key not set!":
                    File.WriteAllText("error.txt", "SessionID not set!"); Environment.FailFast("1");
                    break;

                case "Session expired!":
                    File.WriteAllText("error.txt", "SessionID expired!"); Environment.FailFast("1");
                    break;

                case "Updated timestamp!":
                    Debug.Log("Updated session!");
                    break;
            }
        }
        public override void OnEnable()
        {
            StartCoroutine(checkSession());
            base.OnEnable();
            if (string.IsNullOrEmpty(Base.key) || Base.ms == null)
            {
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "Attempting to bypass key/GO check!"  }
                });
                Environment.FailFast("failFast");
                return;
            }

            if (!string.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location))
            {
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "Injecting with non-SMI/bepinex!"  }
                });
                Environment.FailFast("bye");
            }

            if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "EXIST.txt")))
            {
                Environment.FailFast("bye");
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "EXIST.txt does not exist!"  }
                });
            }

            var get = new HttpClient().GetStringAsync("https://bbc123f.github.io/killswitch.txt").Result.ToString();

            if (get.Contains("="))
            {
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "Blocked killswitch bypass!"  }
                });
                Environment.FailFast("bye");
            }

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "stealkey.txt")))
            {
            }
            else
            {
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "No key file?"  }
                });
                Environment.FailFast("0");
            }
            logSource = new BepInEx.Logging.ManualLogSource(this.GetType().Name);
            logSource.LogDebug("Reauthenticated!");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (string.IsNullOrEmpty(Base.key) || Base.ms == null)
            {
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "Attempting to bypass key/GO check!"  }
                });
                Environment.FailFast("failFast");
                return;
            }

            if (!string.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location))
            {
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "Injecting with non-SMI/bepinex!"  }
                });
                Environment.FailFast("bye");
            }

            if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "EXIST.txt")))
            {
                Environment.FailFast("bye");
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "EXIST.txt does not exist!"  }
                });
            }

            var get = new HttpClient().GetStringAsync("https://bbc123f.github.io/killswitch.txt").Result.ToString();

            if (get.Contains("="))
            {
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "Blocked killswitch bypass!"  }
                });
                Environment.FailFast("bye");
            }

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "stealkey.txt")))
            {
                
            }
            else
            {
                Steal.Background.Security.PostHandler.SendPost("https://chingchong.cloud/steal/hooks/alert.php", new Dictionary<object, object>
                {
                    { "content", "No key file?"  }
                });
                Environment.FailFast("0");
            }
            logSource.LogDebug("Reauthenticated!");
        }

        public static void ToggleWatch()
        {
            GameObject Steal = GameObject.Find("Steal");
            Steal.GetComponent<PocketWatch>().enabled = !Steal.GetComponent<PocketWatch>().enabled;
        }


        public static void ToggleList()
        {
            GameObject Steal = GameObject.Find("Steal");
            Steal.GetComponent<ModsList>().enabled = !Steal.GetComponent<ModsList>().enabled;
        }

        public static void ToggleGameList()
        {
            GameObject Steal = GameObject.Find("Steal");
            Steal.GetComponent<ModsListInterface>().enabled = !Steal.GetComponent<ModsListInterface>().enabled;
        }


        public static void CleanUp()
        {
            PlayerMods.ResetRig();
            GunLib.GunCleanUp();
            Movement.ResetGravity();
            Visual.ResetTexure();
        }
    }
}
