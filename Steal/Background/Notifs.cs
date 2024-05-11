using Steal.Background;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Steal.Background;

internal class Notif : MonoBehaviour
{
    public static Notif instance
    {
        get
        {
            return _instance;
        }
    }
    private static Notif _instance;
    GameObject HUDObj;
    GameObject HUDObj2;
    GameObject MainCamera;
    static Text Testtext;
    Material AlertText = new Material(Shader.Find("GUI/Text Shader"));
    int NotificationDecayTime = 180;
    int NotificationDecayTimeCounter = 0;
    string[] Notifilines;
    string newtext;
    public static string PreviousNotifi;
    bool HasInit = false;
    static Text NotifiText;
    public static bool IsEnabled = true;
    private void Awake()
    {
        _instance = this;
    }
    private void Init()
    {
        try
        {
            if (GameObject.Find("Main Camera"))
            {
                MainCamera = GameObject.Find("Main Camera");
                HUDObj = new GameObject();//GameObject.CreatePrimitive(PrimitiveType.Cube);
                HUDObj2 = new GameObject();
                HUDObj2.name = "NOTIFICATIONLIB_HUD_OBJ2";
                HUDObj.name = "NOTIFICATIONLIB_HUD_OBJ";
                HUDObj.AddComponent<Canvas>();
                HUDObj.AddComponent<CanvasScaler>();
                HUDObj.AddComponent<GraphicRaycaster>();
                HUDObj.GetComponent<Canvas>().enabled = true;
                HUDObj.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                HUDObj.GetComponent<Canvas>().worldCamera = MainCamera.GetComponent<Camera>();
                HUDObj.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 5);
                //HUDObj.CreatePrimitive()
                HUDObj.GetComponent<RectTransform>().position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z);//new Vector3(-67.151f, 11.914f, -82.749f);
                HUDObj2.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z - 4.6f);
                HUDObj.transform.parent = HUDObj2.transform;
                HUDObj.GetComponent<RectTransform>().localPosition = new Vector3(0.55f, 0f, 1.6f);
                var Temp = HUDObj.GetComponent<RectTransform>().rotation.eulerAngles;
                Temp.y = -270f;
                HUDObj.transform.localScale = new Vector3(1f, 1f, 1f);
                HUDObj.GetComponent<RectTransform>().rotation = Quaternion.Euler(Temp);
                GameObject TestText = new GameObject();
                TestText.transform.parent = HUDObj.transform;
                Testtext = TestText.AddComponent<Text>();
                Testtext.text = "";
                Testtext.fontSize = 10;
                Testtext.font = GameObject.Find("COC Text").GetComponent<Text>().font;
                Testtext.rectTransform.sizeDelta = new Vector2(260, 70);
                Testtext.alignment = TextAnchor.LowerLeft;
                if (!XRSettings.isDeviceActive)
                {
                    Testtext.rectTransform.localScale = new Vector3(0.02f, 0.02f, 2f);
                }
                else
                {
                    Testtext.rectTransform.localScale = new Vector3(0.01f, 0.01f, 1f);
                }

                Testtext.rectTransform.localPosition = new Vector3(-1.5f, -.9f, -.6f);
                Testtext.material = AlertText;
                NotifiText = Testtext;
            }
        }
        catch (Exception ex)
        {
            ShowConsole.LogERR(ex);
            throw;
        }
    }
    public void Update()
    {
        try
        {

            if (!HasInit)
            {
                if (GameObject.Find("Main Camera") != null)
                {
                    Init();
                    HasInit = true;
                }
            }
            //This is a bad way to do this, but i do not want to rely on utila
            if (HasInit)
            {
                HUDObj2.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z);
                HUDObj2.transform.rotation = MainCamera.transform.rotation;
                //HUDObj.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 1.6f);
                if (Testtext.text != "") //THIS CAUSES A MEMORY LEAK!!!!! -no longer causes a memory leak
                {
                    NotificationDecayTimeCounter++;
                    if (NotificationDecayTimeCounter > NotificationDecayTime)
                    {
                        Notifilines = null;
                        newtext = "";
                        NotificationDecayTimeCounter = 0;
                        Notifilines = Testtext.text.Split(Environment.NewLine.ToCharArray()).Skip(1).ToArray();
                        foreach (string Line in Notifilines)
                        {
                            if (Line != "")
                            {
                                newtext = newtext + Line + "\n";
                            }
                        }

                        Testtext.text = newtext;
                    }
                }
                else
                {
                    NotificationDecayTimeCounter = 0;
                }
            }
        }
        catch (Exception ex)
        {
            ShowConsole.LogERR(ex);
            throw;
        }

    }

    
    public static void SendNotification(string NotificationText, Color color)
    {
        try
        {
            if (IsEnabled)
            {
                if (!NotificationText.Contains(Environment.NewLine)) { NotificationText = NotificationText + Environment.NewLine; }
                NotifiText.text = NotifiText.text + NotificationText;
                PreviousNotifi = NotificationText;
                Testtext.color = color;
            }
        }
        catch
        {
            throw;
        }
    }

    public static void ClearAllNotifications()
    {
        NotifiText.text = "";
    }
    public static void ClearPastNotifications(int amount)
    {
        string[] Notifilines = null;
        string newtext = "";
        Notifilines = NotifiText.text.Split(Environment.NewLine.ToCharArray()).Skip(amount).ToArray();
        foreach (string Line in Notifilines)
        {
            if (Line != "")
            {
                newtext = newtext + Line + "\n";
            }
        }

        NotifiText.text = newtext;
    }

}
