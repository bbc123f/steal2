using HarmonyLib;
using PlayFab;
using PlayFab.Internal;
using Steal.Background;
using UnityEngine;

namespace Steal.Patchers.Playfab
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

    [HarmonyPatch(typeof(PlayFabDeviceUtil), "GetAdvertIdFromUnity")]
    internal class NoGetAdvertIdFromUnity : MonoBehaviour
    {
        private static bool Prefix()
        {
            ShowConsole.Log("NoGetAdvertIdFromUnity");
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayFabDeviceUtil), "DoAttributeInstall")]
    internal class NoDoAttributeInstall : MonoBehaviour
    {
        private static bool Prefix()
        {
            ShowConsole.Log("NoDoAttributeInstall");
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayFabClientInstanceAPI), "ReportDeviceInfo")]
    internal class NoDeviceInfo2 : MonoBehaviour
    {
        private static bool Prefix()
        {
            ShowConsole.Log("NoDeviceInfo2");
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayFabClientAPI), "ReportDeviceInfo")]
    internal class NoDeviceInfo1 : MonoBehaviour
    {
        private static bool Prefix()
        {
            ShowConsole.Log("NoDeviceInfo1");
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayFabDeviceUtil), "SendDeviceInfoToPlayFab")]
    internal class NoDeviceInfo : MonoBehaviour
    {
        private static bool Prefix()
        {
            ShowConsole.Log("NoDeviceInfo");
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayFabClientAPI), "AttributeInstall")]
    internal class NoAttributeInstall : MonoBehaviour
    {
        private static bool Prefix()
        {
            ShowConsole.Log("NoAttributeInstall");
            return false;
        }
    }
}
