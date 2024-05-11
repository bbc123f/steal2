using MonoMod.Utils;
using Steal.Background;
using Steal.Background.Security;
using Steal.Background.Security.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Steal.stealUI
{
    internal class Presets
    {
        public class Preset
        {
            public string title;
            public string[] buttons;

            public Preset(string name, string[] presetButtons)
            {
                title = name;
                this.buttons = presetButtons;
            }
        }

        public static Preset[] getPresets()
        {
            var response = PostHandler.PostReq2("https://chingchong.cloud/steal/presets/getUploads.php", new { Base.key }).Result.Content.ReadAsStringAsync().Result;
            if (response == "Key is not valid") { Environment.FailFast("uhh"); return null; }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Preset[]>(response);
        }

        public static void ActivatePreset(Preset preset)
        {
            foreach (var button in preset.buttons)
            {
                MenuPatch.FindButton(button).Enabled = true;
            }
            MenuPatch.RefreshMenu();
        }

        public static void UploadPreset(Preset preset)
        {
            var response = PostHandler.PostReq2("https://chingchong.cloud/steal/presets/upload.php", new
            {
                Base.key,
                title = preset.title,
                buttons = preset.buttons,
            }).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            Debug.Log("status: "+ response.StatusCode + " Response: "+ result);
            if (result == "Title length cannot be bigger than 8")
            {
                UIAlerts.SendAlert(result);
            }
            if (result == "Missing required fields in JSON.")
            {
                Debug.Log("this isnt supposed to happen, try again.");
            }
            if (result == "Key is not valid")
            {
                Debug.Log("Key isnt valid, your probably using a crack!");
                Environment.Exit(0);
            }
            if (result == "Rate limit exceeded. Please try again later.")
            {
                UIAlerts.SendAlert(result);
                Debug.Log("wait 12 seconds or so");
            }
        }
    }
}