using HarmonyLib;
using Newtonsoft.Json;
using Steal.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Steal.Background.Security
{
    /*
    class Base
    {
        static void ExitProcess(int exitcode)
        {
            Environment.Exit(exitcode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Init()
        {
            if (Harmony.GetAllPatchedMethods() != null)
            {
                foreach (MethodBase mb in Harmony.GetAllPatchedMethods())
                {
                    if (mb != null)
                    {
                        foreach (string str in Harmony.GetPatchInfo(mb).Owners)
                        {
                            if (str.Contains("com.steal.lol") && !str.Contains("harmony-auto") && !str.Contains("bepinex"))
                            {
                                Debug.Log(str);
                                Harmony.UnpatchID(str);
                                return;
                            }
                        }
                    }
                } // Checking if any voids are already patched
            }

            if (Harmony.HasAnyPatches("com.steal.lol"))
            {
                Harmony.UnpatchID("com.steal.lol");
                return;
            } // Checking if any patchers have our id before we patch

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "login.json")))
            {
                var data = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "login.json"));
                var info = JsonConvert.DeserializeObject<Data>(data);
                using (HttpClient client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
  {
                        { "app_id", "44785593-b37b-47d8-9171-59abe616105c" },
                        { "username", info.Username },
                        { "password", info.Password }
  };

                    var content = new FormUrlEncodedContent(values);
                    var response = client.PostAsync("https://api.authware.org/user/auth", content);
                    response.Wait();
                    if (response.Result.StatusCode == (HttpStatusCode)200)
                    {
                        if (!GameObject.Find("steal"))
                        {
                            if (!new WebClient().DownloadString("https://tnuser.com/API/files/abcdefg").Contains("="))
                            {
                                GameObject ms = new GameObject("steal");
                                ms.AddComponent<StealGUI>();
                                ms.AddComponent<Main>();
                                ms.AddComponent<Notif>();
                                ms.AddComponent<Input>();
                                ms.AddComponent<AssetLoader>();
                                ms.AddComponent<MainManager>();
                                ms.AddComponent<HeadsetManager>();
                                ms.AddComponent<RPCSUB>();
                                ms.AddComponent<ShowConsole>();
                                Harmony harm = new Harmony("com.steal.lol");
                                harm.PatchAll();
                            }
                        }
                    }
                    else
                    {
                        File.WriteAllText("error.txt", response.Result.StatusCode.ToString());
                        ExitProcess(0);
                        return;
                    }
                }
            }
            else
            {
                File.WriteAllText("YOUR KEY FILE DOES NOT EXIST", "FILE DOES NOT EXIST");
                ExitProcess(0);
                return;
            }

        }
    }

    internal class Data
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    internal class Auth2
    {

    }*/
}
