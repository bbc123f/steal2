using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using static GorillaNetworking.GorillaComputer;

namespace Steal.GorillaOS.Patchers
{
    [HarmonyPatch(typeof(GorillaComputer), "UpdateScreen", MethodType.Normal)]
    internal class UpdateScreenPatch
    {
        public static string CurrentStatus = "IDLE";


        static void Postfix()
        {
            CurrentStatus = "IDLE";
            switch (GorillaComputer.instance.currentState)
            {
                case ComputerState.Startup:
                    GorillaComputer.instance.screenText.Text = $"Welcome {PhotonNetwork.NickName}!\n\n" + PhotonNetwork.CountOfPlayers + " PLAYERS ONLINE\n\n" + PhotonNetwork.CountOfPlayersInRooms + " PLAYERS IN ROOMS\n\nPRESS ANY KEY TO CLOSE";
                    break;
                case ComputerState.Room:
                    {
                        GorillaComputer.instance.screenText.Text = $"ROOM   ::   {CurrentStatus}\n\nPRESS ENTER TO JOIN OR CREATE A CUSTOM ROOM WITH THE ENTERED CODE. PRESS OPTION 1 TO DISCONNECT FROM THE CURRENT ROOM.\n\nCURRENT ROOM: ";
                        if (PhotonNetwork.InRoom)
                        {
                            GorillaComputer.instance.screenText.Text += PhotonNetwork.CurrentRoom.Name;
                            GorillaText gorillaText3 = GorillaComputer.instance.screenText;
                            gorillaText3.Text = gorillaText3.Text + "\n\nPLAYERS IN ROOM: " + PhotonNetwork.CurrentRoom.PlayerCount;
                        }
                        else
                        {
                            GorillaComputer.instance.screenText.Text += "-NOT IN ROOM-";
                            GorillaText gorillaText4 = GorillaComputer.instance.screenText;
                            gorillaText4.Text = gorillaText4.Text + "\n\nPLAYERS IN ROOMS: " + PhotonNetwork.CountOfPlayersInRooms;
                        }
                        GorillaText gorillaText5 = GorillaComputer.instance.screenText;
                        gorillaText5.Text = gorillaText5.Text + "\n\nROOM TO JOIN: " + GorillaComputer.instance.roomToJoin;
                        if (GorillaComputer.instance.roomFull)
                        {
                            CurrentStatus = $"ROOM {instance.roomToJoin} IS FULL. JOIN ROOM FAILED.";
                            Steal.GorillaOS.GorillaOS.instance.Reload();
                        }
                        else if (GorillaComputer.instance.roomNotAllowed)
                        {
                            CurrentStatus = "CANNOT JOIN ROOM TYPE FROM HERE.";
                        }
                        break;
                    }
                case ComputerState.Support:
                    Traverse.Create(GorillaComputer.instance).Field("displaySupport").SetValue(true);
                    GorillaComputer.instance.screenText.Text = string.Concat(new string[]
                        {
                            $"MODS   ::   {CurrentStatus}\n",
                            "MODUALS:\n",
                            GorillaOS.list
                        });
                    break;
                case ComputerState.Loading:
                    GorillaComputer.instance.screenText.Text = "LOADING <color=red>GORILLA OS</color>...";
                    break;

                case ComputerState.Credits:
                    GorillaComputer.instance.screenText.Text = $"THEMES   ::   {CurrentStatus}\n\n1. GRAY\n2. DARK PURPLE\n3. CLEAR\n4. DEFAULT";
                    break;
            }


        }
    }
}
