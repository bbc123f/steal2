using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace Steal.Background
{
    public class InputHandler : MonoBehaviour
    {
        public enum VrType
        {
            OpenVR = 0,
            Oculus = 1,
            WindowsMR = 2,
            MockHMD = 3,
            none = 4,
        }

        public static float sensitivity = 0.5f;

        public static bool RightSecondary;
        public static bool RightPrimary;
        public static bool RightTrigger;
        public static bool RightGrip;
        public static Vector2 RightJoystick;
        public static bool RightStickClick;

        public static bool LeftSecondary;
        public static bool LeftPrimary;
        public static bool LeftGrip;
        public static bool LeftTrigger;
        public static Vector2 LeftJoystick;
        public static bool LeftStickClick;

        VrType type;

        public UnityEngine.XR.InputDevice leftController;
        public UnityEngine.XR.InputDevice rightController;

        public void Awake()
        {
            type = HeadsetType();
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }

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

        private static bool CalculateGripState(float grabValue, float grabThreshold)
        {
            return grabValue >= grabThreshold;
        }

        public void LateUpdate()
        {
            if (ControllerInputPoller.instance != null)
            {
                var Poller = ControllerInputPoller.instance;
                if (type == VrType.OpenVR)
                {
                    RightSecondary = Poller.rightControllerPrimaryButton;
                    RightPrimary = Poller.rightControllerSecondaryButton;
                    RightTrigger = CalculateGripState(Poller.rightControllerIndexFloat, sensitivity);
                    RightGrip = CalculateGripState(Poller.rightControllerGripFloat, sensitivity);
                    RightJoystick = Poller.rightControllerPrimary2DAxis;
                    RightStickClick = SteamVR_Actions.gorillaTag_RightJoystickClick.GetState(SteamVR_Input_Sources.RightHand);

                    //------------------------------------------------------------------------

                    LeftSecondary = Poller.leftControllerPrimaryButton;
                    LeftPrimary = Poller.leftControllerSecondaryButton;
                    LeftTrigger = CalculateGripState(Poller.leftControllerIndexFloat, sensitivity);
                    LeftGrip = CalculateGripState(Poller.leftControllerGripFloat, sensitivity);
                    LeftJoystick = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis(SteamVR_Input_Sources.LeftHand);
                    LeftStickClick = SteamVR_Actions.gorillaTag_LeftJoystickClick.GetState(SteamVR_Input_Sources.LeftHand);
                    return;
                }
                rightController.TryGetFeatureValue(CommonUsages.primaryButton, out RightPrimary);
                rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out RightSecondary);
                rightController.TryGetFeatureValue(CommonUsages.triggerButton, out RightTrigger);
                rightController.TryGetFeatureValue(CommonUsages.gripButton, out RightGrip);
                rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out RightStickClick);
                rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out RightJoystick);

                leftController.TryGetFeatureValue(CommonUsages.primaryButton, out LeftPrimary);
                leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out LeftSecondary);
                leftController.TryGetFeatureValue(CommonUsages.triggerButton, out LeftTrigger);
                leftController.TryGetFeatureValue(CommonUsages.gripButton, out LeftGrip);
                leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out LeftStickClick);
                leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out LeftJoystick);
                /*
                RightSecondary = Poller.rightControllerSecondaryButton;
                RightPrimary = Poller.rightControllerPrimaryButton;
                RightTrigger = CalculateGripState(Poller.rightControllerIndexFloat, sensitivity);
                RightGrip = CalculateGripState(Poller.rightControllerGripFloat, sensitivity);
                RightJoystick = Poller.rightControllerPrimary2DAxis;
                RightStickClick = SteamVR_Actions.gorillaTag_RightJoystickClick.GetState(SteamVR_Input_Sources.RightHand);

                //------------------------------------------------------------------------

                LeftSecondary = Poller.leftControllerSecondaryButton;
                LeftPrimary = Poller.leftControllerPrimaryButton;
                LeftTrigger = CalculateGripState(Poller.leftControllerIndexFloat, sensitivity);
                LeftGrip = CalculateGripState(Poller.leftControllerGripFloat, sensitivity);
                LeftJoystick = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis(SteamVR_Input_Sources.LeftHand);
                LeftStickClick = SteamVR_Actions.gorillaTag_LeftJoystickClick.GetState(SteamVR_Input_Sources.LeftHand);*/

            }
        }
    }
}
