using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BepInEx;
using Photon.Pun;
using Steal.Background.Security.Auth;
using System.IO;
using Steal.Components;

namespace Steal.Background.Mods
{
    public class Mod : MonoBehaviourPunCallbacks
    {
        public static BepInEx.Logging.ManualLogSource logSource;

        public override void OnEnable()
        {
            base.OnEnable();
            if (string.IsNullOrEmpty(Base.key) || Base.ms == null)
            {
                Environment.FailFast("failFast");
                return;
            }

            auth GetAuth = new auth(
                name: "Steal",
                ownerid: "RovpqveRf3",
                secret: "28dd3f3d424e86309e9d467c19b5936e61cc0abbd55e3360a04334e6044b9144",
                version: "1.0"
            );

            GetAuth.init();
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "stealkey.txt")))
            {
                var data = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "stealkey.txt"));
                GetAuth.license2(data);
            };
            logSource = new BepInEx.Logging.ManualLogSource(this.GetType().Name);
            logSource.LogDebug("Reauthenticated!");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (string.IsNullOrEmpty(Base.key) || Base.ms == null)
            {
                Environment.FailFast("failFast");
                return;
            }

            auth GetAuth = new auth(
                name: "Steal",
                ownerid: "RovpqveRf3",
                secret: "28dd3f3d424e86309e9d467c19b5936e61cc0abbd55e3360a04334e6044b9144",
                version: "1.0"
            );

            GetAuth.init();
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "stealkey.txt")))
            {
                var data = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "stealkey.txt"));
                GetAuth.license2(data);
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
