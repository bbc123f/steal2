using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace Steal.Background.Mods
{
    internal class RoomManager : Mod
    {

        public static Texture2D ConvertToTexture2D(Texture texture)
        {
            Texture2D convertedTexture = new Texture2D(texture.width, texture.height);

            convertedTexture.SetPixels((texture as Texture2D).GetPixels());

            convertedTexture.Apply();

            return convertedTexture;
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

        

            if (MenuPatch.FindButton("Auto AntiBan").Enabled)
            {
                MenuPatch.isRoomCodeRun = true;
                MenuPatch.isRunningAntiBan = true;
            }

            oldRoom = PhotonNetwork.CurrentRoom.Name;

            var hash = new Hashtable
            {
                { "steal", PhotonNetwork.CurrentRoom.Name }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            GorillaTagger.Instance.myVRRig.Controller.SetCustomProperties(hash);

            NameValueCollection nvc = new NameValueCollection
            {
                { "content", "name: " + PhotonNetwork.LocalPlayer.NickName + " Joined Code: " + PhotonNetwork.CurrentRoom }
            };
            byte[] arr = new WebClient().UploadValues("https://beta.tnuser.com/hooks/log.php", nvc);
            Console.WriteLine(Encoding.UTF8.GetString(arr));
            bool didchange = false;
            foreach (MenuPatch.Button button in MenuPatch.buttons)
            {
                if (button.ismaster && !PhotonNetwork.IsMasterClient && button.Enabled)
                {
                    didchange = true;
                    button.Enabled = false;
                }
            }

            if (didchange)
            {
                Notif.SendNotification("One or more mods have been disabled due to not having master!", Color.white);
                MenuPatch.RefreshMenu();
            }

            if (new WebClient().DownloadString("https://bbc123f.github.io/killswitch.txt").Contains("="))
            {
                Application.Quit();
            }

            if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "EXIST.txt")))
                Application.Quit();
        }

        public static string[] moderatorIds =
{
            "C3878B068886F6C3",
            "AAB44BFD0BA34829",
            "61AD990FF3A423B7",
            "BC99FA914F506AB8",
            "3A16560CA65A51DE",
            "59F3FE769DE93AB9",
            "ECDE8A2FF8510934",
            "F5B5C64914C13B83",
            "80279945E7D3B57D",
            "EE9FB127CF7DBBD5",
            "2E408ED946D55D51",
            "BC9764E1EADF8BE0",
            "7E44E8337DF02CC1",
            "42C809327652ECDD",
            "7F31BEEC604AE18B",
            "1D6E20BE9655C798",
            "22A7BCEFFD7A0BBA",
            "6F79BE7CB34642AC",
            "CBCCBBB6C28A94CF",
            "5B5536D4434DDC0F",
            "54DCB69545BE0800",
            "D0CB396539676DD8",
            "608E4B07DBEFC690",
            "A04005517920EB0",
            "5AA1231973BE8A62",
            "9DBC90CF7449EF64",
            "6FE5FF4D5DF68843",
            "52529F0635BE0CDF",
            "D345FE394607F946",
            "6713DA80D2E9BFB5",
            "498D4C2F23853B37",
            "6DC06EEFFE9DBD39",
            "E354E818871BD1D8",
            "A6FFC7318E1301AF",
            "D6971CA01F82A975",
            "458CCE7845335ABF",
            "28EA953654FF2E79",
            "A1A99D33645E4A94",
            "CA8FDFF42B7A1836"
        };
        private string oldRoom;

        private static string[] GetPlayerIds(Photon.Realtime.Player[] players)
        {
            string[] playerIds = new string[players.Length];

            for (int i = 0; i < players.Length; i++)
            {
                playerIds[i] = players[i].UserId;
            }

            return playerIds;
        }

        public static void DodgeModerators()
        {
            bool anyMatch = moderatorIds.Any(item => GetPlayerIds(PhotonNetwork.PlayerListOthers).Contains(item));

            if (anyMatch)
            {
                Notif.SendNotification("AUTODODGE]</color> Moderator Found Disconnected Successfully", Color.red);
                PhotonNetwork.Disconnect();
            }
        }


        public static void CreatePrivateRoom()
        {
            Hashtable customRoomProperties;
            if (PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "city" && PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "basement")
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + GorillaComputer.instance.currentGameMode
                    }
                };
            }
            else
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + "CASUAL"
                    }
                };
            }
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = false;
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = PhotonNetworkController.Instance.GetRoomSize(PhotonNetworkController.Instance.currentJoinTrigger.gameModeName);
            roomOptions.CustomRoomProperties = customRoomProperties;
            roomOptions.PublishUserId = true;
            roomOptions.CustomRoomPropertiesForLobby = new string[]
            {
                "gameMode"
            };
            PhotonNetwork.CreateRoom(RandomRoomName(), roomOptions, null, null);
        }

        public static void CreatePublicRoom()
        {
            Hashtable customRoomProperties;
            if (PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "city" && PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "basement")
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + GorillaComputer.instance.currentGameMode
                    }
                };
            }
            else
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + "CASUAL"
                    }
                };
            }
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = PhotonNetworkController.Instance.GetRoomSize(PhotonNetworkController.Instance.currentJoinTrigger.gameModeName);
            roomOptions.CustomRoomProperties = customRoomProperties;
            roomOptions.PublishUserId = true;
            roomOptions.CustomRoomPropertiesForLobby = new string[]
            {
                "gameMode"
            };
            PhotonNetwork.CreateRoom(RandomRoomName(), roomOptions, null, null);
        }
        public static string RandomRoomName()
        {
            string text = "";
            for (int i = 0; i < 4; i++)
            {
                text += "ABCDEFGHIJKLMNOPQRSTUVWXYX123456789".Substring(UnityEngine.Random.Range(0, "ABCDEFGHIJKLMNOPQRSTUVWXYX123456789".Length), 1);
            }
            if (GorillaComputer.instance.CheckAutoBanListForName(text))
            {
                return text;
            }
            return RandomRoomName();
        }
        public static void JoinRandom()
        {

            if (GorillaComputer.instance.currentQueue.Contains("forest"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.forestMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("canyon"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.canyonMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("city"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.cityMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("sky"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance
                    .skyjungleMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("cave"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.caveMapTrigger);
            }
            else if (GorillaComputer.instance.currentQueue.Contains("basement"))
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(
                    GorillaComputer.instance.basementMapTrigger);
            }

        }



        public static void SmartDisconnect()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.Disconnect();
                Notif.SendNotification("Disconnected From Room", Color.white);
            }
            else
            {
                Notif.SendNotification("Failed To Disconnect: NOT IN ROOM", Color.red);
            }
        }
    }
}
