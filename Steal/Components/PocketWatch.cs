using GorillaLocomotion;
using GorillaNetworking;
using Steal.Background;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using WristMenu;


namespace Steal.Components
{
    internal class PocketWatch : MonoBehaviour
    {
        internal static GameObject computer = null;

        internal static GameObject computerText = null;

        private GameObject computerMaterial = null;

        private int lastPage = 0;

        public static float lastButtonMovement = 0f;

        private int cooldown = 0;

        private bool doneRefresh = false;

        private MenuPatch.Button currentButton = MenuPatch.buttons[6];

        private int buttonCooldown = 0;

        public void OnDisable()
        {
            UnityEngine.Object.Destroy(computer);
            UnityEngine.Object.Destroy(computerText);
            UnityEngine.Object.Destroy(computerMaterial);
        }

        public void FixedUpdate()
        {
            if (GorillaTagger.Instance.offlineVRRig == null)
                return;

            if (computer == null)
            {
                computer = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/huntcomputer (1)");
                computer.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
                GameObject gameObject = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/huntcomputer (1)/HuntWatch_ScreenLocal/Canvas/Anchor");
                computerText = gameObject.transform.GetChild(0).gameObject;
                computerMaterial = gameObject.transform.GetChild(1).gameObject;
                gameObject.transform.GetChild(2).gameObject.SetActive(false);
                gameObject.transform.GetChild(3).gameObject.SetActive(false);
                gameObject.transform.GetChild(4).gameObject.SetActive(false);
                gameObject.transform.GetChild(5).gameObject.SetActive(false);
                gameObject.transform.GetChild(6).gameObject.SetActive(false);
                computerText.GetComponent<Text>().text = "Start Menu With RightSticks";
                this.computerMaterial.GetComponent<Image>().material = new Material(GorillaComputer.instance.pressedMaterial);
                this.computerMaterial.SetActive(true);
                Renderer renderer = computer.GetComponent<Renderer>();
                renderer.material.shader = Shader.Find("UI/Default");
                renderer.material.SetColor("_Color", MenuPatch.GetTheme(UI.Theme)[2]);


            }

            else if (computer != null && computerText != null && computerMaterial != null)
            {
                if (computer.GetComponent<GorillaHuntComputer>())
                {
                    if (!computer.activeSelf || computer.GetComponent<GorillaHuntComputer>().enabled)
                    {
                        computer.SetActive(true);
                        computer.GetComponent<GorillaHuntComputer>().enabled = false;
                    }
                }

                if (InputHandler.LeftJoystick.x < -0.75f && !InputHandler.LeftStickClick)
                {
                    if (Time.frameCount >= buttonCooldown + 20)
                    {
                        int buttonIndex = MenuPatch.buttons.ToList().FindIndex(b => b.buttonText == currentButton.buttonText);
                        buttonIndex -= 1;
                        currentButton = MenuPatch.buttons[buttonIndex];
                        buttonCooldown = Time.frameCount;
                    }
                }

                if (InputHandler.LeftJoystick.x > 0.75f && !InputHandler.LeftStickClick)
                {
                    if (Time.frameCount >= buttonCooldown + 20)
                    {
     
                        int buttonIndex = MenuPatch.buttons.ToList().FindIndex(b => b.buttonText == currentButton.buttonText);
                        buttonIndex += 1;
                        currentButton = MenuPatch.buttons[buttonIndex];
                        buttonCooldown = Time.frameCount;
                    }
                }
                else if (Time.frameCount >= buttonCooldown + 20 && InputHandler.LeftStickClick)
                {
                    MenuPatch.Toggle(currentButton);
                    buttonCooldown = Time.frameCount;
                }

                if (computerText.GetComponent<Text>() && MenuPatch.buttons[6] != null && currentButton != null && computerText != null)
                {
                    if (computerText.GetComponent<Text>().text == "Start Menu With RightSticks" && InputHandler.LeftStickClick)
                    {
                        currentButton = MenuPatch.buttons[6];
                    }
                    else
                    {
                        if (currentButton.Enabled)
                        {
                            this.computerMaterial.GetComponent<Image>().material.color = Color.green;
                            computerText.GetComponent<Text>().color = Color.white;
                        }
                        else
                        {
                            this.computerMaterial.GetComponent<Image>().material.color = Color.red;
                            computerText.GetComponent<Text>().color = Color.white;
                        }

                        computerText.GetComponent<Text>().text = currentButton.buttonText;
                    }
                }
            }
        }
    }
}