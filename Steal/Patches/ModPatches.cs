
using HarmonyLib;
using PlayFab.Internal;
using Steal.Background;
using UnityEngine;

namespace Steal.Patchers.ModPatches
{
    [HarmonyPatch(typeof(PlayFabHttp), "InitializeScreenTimeTracker")]
    internal class NoInitializeScreenTimeTracker : MonoBehaviour
    {
        private static bool Prefix()
        {
            ShowConsole.Log("NoInitializeScreenTimeTracker");
            return false;
        }
    }
}