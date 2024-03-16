using System;
using System.Collections.Generic;
using System.Text;
using GorillaNetworking;
using HarmonyLib;
using static GorillaNetworking.GorillaComputer;

namespace Steal.GorillaOS.Patchers
{
    [HarmonyPatch(typeof(GorillaComputer), "UpdateFunctionScreen", MethodType.Normal)]
    public class FunctionsPatch
    {
        static bool Prefix()
        {
            string str = string.Concat(new string[]
            {
                ((GorillaComputer.instance.currentState == ComputerState.Room) ? ">" : "")+" ROOM" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Name) ? ">" : "")+" NAME" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Color) ? ">" : "")+" COLOR" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Turn) ? ">" : "")+" TURN" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Mic) ? ">" : "")+" MIC" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Queue) ? ">" : "")+" QUEUE" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Group) ? ">" : "")+" GROUP" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Voice) ? ">" : "")+" AUDIO" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Visuals) ? ">" : "")+" ITEMS" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Credits) ? ">" : "")+" THEME" + "\n",

                ((GorillaComputer.instance.currentState == ComputerState.Support) ? ">" : "")+" MODS" + "\n"
            });
            GorillaComputer.instance.functionSelectText.Text = str;
            return false;
        }
    }
}
