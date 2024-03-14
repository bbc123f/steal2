using Steal.Background;
using System.IO;
using UnityEngine;
using UnityEngine.XR;


namespace Steal.Components
{
    public enum VrType 
    { 
        OpenVR=0,
        Oculus = 1,
        WindowsMR = 2,
        MockHMD = 3,
        none = 4,
    }


    internal class HeadsetManager : MonoBehaviour
    {
        public static VrType HeadsetType()
        {
            if (XRSettings.isDeviceActive)
            {
                if (XRSettings.loadedDeviceName.Contains("Oculus"))
                {
                    return VrType.Oculus;
                }
                if (XRSettings.loadedDeviceName.Contains("Windows"))
                {
                    return VrType.WindowsMR;
                }
                if (XRSettings.loadedDeviceName.Contains("Open"))
                {
                    return VrType.OpenVR;
                }
                return VrType.MockHMD;
            }
            else
            {
                ShowConsole.Log("No VR device detected.");
                return VrType.none;
            }
        }
    }
}
