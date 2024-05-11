using Steal.stealUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;


namespace Steal.Components
{
    public class ModsListInterface : MonoBehaviour
    {

        public void OnDisable()
        {
            Testtext.text = "";
            Testtext.gameObject.SetActive(false);
            HUDObj2ROOM = null;
            Destroy(HUDObj2ROOM);
            HUDObjROOM = null;
            Destroy(HUDObjROOM);
            HUDObj = null;
            Destroy(HUDObj);
            HUDObj2 = null;
            Destroy(HUDObj2);
            loaded1 = false;
        }

        private bool loaded1 = false;

        static GameObject HUDObjROOM;
        static GameObject HUDObj2ROOM;
        static GameObject MainCamera;
        static Text TesttextROOM;
        private static TextAnchor textAnchor = TextAnchor.UpperRight;
        static Material AlertText = new Material(Shader.Find("GUI/Text Shader"));
        static Text NotifiTextROOM;

        static GameObject HUDObj;
        static GameObject HUDObj2;
        static Text Testtext;
        static Text NotifiText;

        public static List<string> modsEnabled = ModsList.modsEnabled;

        public void LateUpdate()
        {
            if (!XRSettings.isDeviceActive)
            {
                this.enabled = false;
                return;
            }
            modsEnabled = ModsList.modsEnabled;
            if (!loaded1)
            {
                MainCamera = GameObject.Find("Main Camera");
                HUDObjROOM = new GameObject();
                HUDObj2ROOM = new GameObject();
                HUDObj2ROOM.name = "CLIENT_HUB_OVERLAYROOM";
                HUDObjROOM.name = "CLIENT_HUB_OVERLAYROOM";
                HUDObjROOM.AddComponent<Canvas>();
                HUDObjROOM.AddComponent<CanvasScaler>();
                HUDObjROOM.AddComponent<GraphicRaycaster>();
                HUDObjROOM.GetComponent<Canvas>().enabled = true;
                HUDObjROOM.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                HUDObjROOM.GetComponent<Canvas>().worldCamera = MainCamera.GetComponent<Camera>();
                HUDObjROOM.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 5);
                HUDObjROOM.GetComponent<RectTransform>().position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z);
                HUDObj2ROOM.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z - 4.6f);
                HUDObjROOM.transform.parent = HUDObj2ROOM.transform;
                HUDObjROOM.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 1.6f);
                var Temp = HUDObjROOM.GetComponent<RectTransform>().rotation.eulerAngles;
                Temp.y = -270f;
                HUDObjROOM.transform.localScale = new Vector3(1f, 1f, 1f);
                HUDObjROOM.GetComponent<RectTransform>().rotation = Quaternion.Euler(Temp);
                GameObject TestText = new GameObject();
                TestText.transform.parent = HUDObjROOM.transform;
                TesttextROOM = TestText.AddComponent<Text>();
                TesttextROOM.text = "";
                TesttextROOM.fontSize = 10;
                TesttextROOM.font = GameObject.Find("COC Text").GetComponent<Text>().font;
                TesttextROOM.rectTransform.sizeDelta = new Vector2(260, 300);
                TesttextROOM.rectTransform.localScale = new Vector3(0.01f, 0.01f, 0.7f);
                TesttextROOM.rectTransform.localPosition = new Vector3(-2.4f, -2f, 2f);
                TesttextROOM.material = AlertText;
                NotifiTextROOM = TesttextROOM;
                TesttextROOM.alignment = TextAnchor.UpperLeft;

                HUDObj = new GameObject();
                HUDObj2 = new GameObject();
                HUDObj2.name = "CLIENT_HUB_OVERLAY";
                HUDObj.name = "CLIENT_HUB_OVERLAY";
                HUDObj.AddComponent<Canvas>();
                HUDObj.AddComponent<CanvasScaler>();
                HUDObj.AddComponent<GraphicRaycaster>();
                HUDObj.GetComponent<Canvas>().enabled = true;
                HUDObj.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                HUDObj.GetComponent<Canvas>().worldCamera = MainCamera.GetComponent<Camera>();
                HUDObj.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 5);
                HUDObj.GetComponent<RectTransform>().position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z);
                HUDObj2.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z - 4.6f);
                HUDObj.transform.parent = HUDObj2.transform;
                HUDObj.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 1.6f);
                var Temp1 = HUDObj.GetComponent<RectTransform>().rotation.eulerAngles;
                Temp.y = -270f;
                HUDObj.transform.localScale = new Vector3(1f, 1f, 1f);
                HUDObj.GetComponent<RectTransform>().rotation = Quaternion.Euler(Temp);
                GameObject TestText1 = new GameObject();
                TestText1.transform.parent = HUDObj.transform;
                Testtext = TestText1.AddComponent<Text>();
                Testtext.text = "";
                Testtext.fontSize = 10;
                Testtext.font = GameObject.Find("COC Text").GetComponent<Text>().font;
                Testtext.rectTransform.sizeDelta = new Vector2(260, 300);
                Testtext.rectTransform.localScale = new Vector3(0.01f, 0.01f, 0.7f);
                Testtext.rectTransform.localPosition = new Vector3(-2.4f, -2f, 0f);
                Testtext.material = AlertText;
                NotifiText = Testtext;
                Testtext.alignment = TextAnchor.UpperRight;

                loaded1 = true;
            }
            HUDObj2.transform.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z);
            HUDObj2.transform.rotation = MainCamera.transform.rotation;
            HUDObj2ROOM.transform.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z);
            HUDObj2ROOM.transform.rotation = MainCamera.transform.rotation;

            string combinedString = string.Join("\n", modsEnabled);
            Testtext.color = MenuPatch.GetTheme(UI.Theme)[2];
            Testtext.text = $"Mods Enabled:\n<color=white>" + combinedString + "</color>";

        }
    }
}