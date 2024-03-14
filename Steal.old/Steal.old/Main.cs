using BepInEx;
using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaTag;
using HarmonyLib;
using Photon.Pun;
using Steal.Attributes;
using Steal.Background;
using Steal.Background.Security;
using Steal.Components;
using Steal.Patchers.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Valve.VR;
using static GorillaTag.ScienceExperimentManager;
using Button = Steal.Attributes.Button;

namespace Steal
{
    [BepInPlugin("asd", "asdd", "1.0.0")]
    public class s : BaseUnityPlugin
    {
        public void Update()
        {
            if (!GameObject.Find("Steal"))
            {
                Base.Init();
            }
        }
    }


    [HarmonyLib.HarmonyPatch(typeof(GorillaLocomotion.Player), "FixedUpdate", HarmonyLib.MethodType.Normal)]
    internal class Main : MonoBehaviourPunCallbacks
    {
        public static Main Instance { get; internal set; }
        public void Awake()
        {
            Instance = this;
        }
        private int pageSize = 6;
        private int pageNumber = 0;
        public GameObject menu = null;
        public GameObject menubg = null;
        public GameObject canvasObj = null;
        public GameObject menuClick;
        public int framePressCooldown = 0;
        static GameObject C4;
        static bool C4spawned;
        static VRRig oldTarget;
        static GradientColorKey[] colorKeys = 
        {
            new GradientColorKey(Color.black, 0f),
            new GradientColorKey(new Color32(169, 140, 189, 255), 0.5f),
            new GradientColorKey(Color.black, 1f),
        };

        #region Draw
        private void AddButton(float offset, Button button)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = Instance.menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0f, 0.58f - offset);
            var collider = gameObject.AddComponent<BtnCollider>();
            collider.button = button;
            gameObject.name = button.buttontext;
            int num = -1;

            for (int i = 0; i < Instance.buttons.Length; i++)
            {
                if (button.buttontext == Instance.buttons[i].buttontext)
                {
                    num = i;
                    break;
                }
            }

            if (!Instance.buttons[num].Active)
            {
                ColorChanger c = gameObject.AddComponent<ColorChanger>();
                Gradient h = new Gradient();
                h.colorKeys = colorKeys;
                c.gradient = h;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = new Color32(169, 140, 189, 255);
            }

            GameObject gameObject2 = new GameObject();
            gameObject2.transform.parent = canvasObj.transform;

            Text text2 = gameObject2.AddComponent<Text>();
            text2.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text2.text = button.buttontext;
            text2.fontSize = 1;
            text2.fontStyle = FontStyle.Italic;
            text2.alignment = TextAnchor.MiddleCenter;
            text2.resizeTextForBestFit = true;
            text2.resizeTextMinSize = 0;

            RectTransform component = text2.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.064f, 0f, 0.231f - offset / 2.55f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }
        public void Draw()
        {
            Instance.menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(Instance.menu.GetComponent<Rigidbody>());
            Destroy(Instance.menu.GetComponent<BoxCollider>());
            Destroy(Instance.menu.GetComponent<Renderer>());
            Instance.menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);
            Instance.menu.name = "Instance.menu";

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<BoxCollider>());
            gameObject.transform.parent = Instance.menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.1f, 0.94f, 1.1575f);
            gameObject.name = "Instance.menucolor";
            gameObject.transform.position = new Vector3(0.054f, 0, 0.064f);
            ColorChanger c = gameObject.AddComponent<ColorChanger>();
            Gradient h = new Gradient();
            h.colorKeys = colorKeys;
            c.gradient = h;


            canvasObj = new GameObject();
            canvasObj.transform.parent = Instance.menu.transform;
            canvasObj.name = "canvas";
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 1000f;

            GameObject gameObject2 = new GameObject();
            gameObject2.transform.parent = canvasObj.transform;
            gameObject2.name = "Title";
            Text text = gameObject2.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.text = "Steal";
            text.fontSize = 1;
            text.fontStyle = FontStyle.Italic;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.28f, 0.05f);
            component.position = new Vector3(0.06f, 0f, 0.26f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            AddPageButtons();
            Button[] array2 = Instance.buttons.Skip(pageNumber * pageSize).Take(pageSize).ToArray();
            for (int i = 0; i < array2.Length; i++)
            {
                AddButton((float)i * 0.13f + 0.26f, array2[i]);
            }
        }
        private void AddPageButtons()
        {
            int num = (Instance.buttons.Length + pageSize - 1) / pageSize;
            int num2 = pageNumber + 1;
            int num3 = pageNumber - 1;
            if (num2 > num - 1)
            {
                num2 = 0;
            }
            if (num3 < 0)
            {
                num3 = num - 1;
            }

            int prevpage;
            if (pageNumber == 0)
            {
                prevpage = num;
            }
            else
            {
                prevpage = pageNumber - 1;
            }
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = Instance.menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0, 0.5264f);
            var cock123321 = gameObject.AddComponent<BtnCollider>();
            cock123321.button = new Button("PreviousPage", false, null);
            ColorChanger c = gameObject.AddComponent<ColorChanger>();
            Gradient h = new Gradient();
            h.colorKeys = colorKeys;
            c.gradient = h;
            gameObject.name = "back";

            GameObject gameObject2 = new GameObject();
            gameObject2.transform.parent = canvasObj.transform;
            gameObject2.name = "back";
            Text text = gameObject2.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.text = "{" + num3 + "} <<";
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;

            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.064f, 0f, 0.213f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            component.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            GameObject gameObject3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(gameObject3.GetComponent<Rigidbody>());
            gameObject3.GetComponent<BoxCollider>().isTrigger = true;
            gameObject3.transform.parent = Instance.menu.transform;
            gameObject3.transform.rotation = Quaternion.identity;
            gameObject3.name = "Next";
            gameObject3.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
            gameObject3.transform.localPosition = new Vector3(0.56f, 0, 0.4228f);
            var abc123 = gameObject3.AddComponent<BtnCollider>();
            abc123.button = new Button("NextPage", false, null);
            ColorChanger c3 = gameObject3.AddComponent<ColorChanger>();
            c3.gradient = h;

            GameObject gameObject4 = new GameObject();
            gameObject4.transform.parent = canvasObj.transform;
            gameObject4.name = "Next";

            Text text2 = gameObject4.AddComponent<Text>();
            text2.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text2.text = ">> {" + num2 + "}";
            text2.fontSize = 1;
            text2.alignment = TextAnchor.MiddleCenter;
            text2.resizeTextForBestFit = true;
            text2.resizeTextMinSize = 0;

            RectTransform component2 = text2.GetComponent<RectTransform>();
            component2.localPosition = Vector3.zero;
            component2.sizeDelta = new Vector2(0.2f, 0.03f);
            component2.localPosition = new Vector3(0.064f, 0f, 0.17f);
            component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            component2.localScale = new Vector3(0.9f, 0.9f, 0.9f);



            GameObject gameObject32 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(gameObject32.GetComponent<Rigidbody>());
            gameObject32.GetComponent<BoxCollider>().isTrigger = true;
            gameObject32.transform.parent = Instance.menu.transform;
            gameObject32.transform.rotation = Quaternion.identity;
            gameObject32.name = "Leave";
            gameObject32.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
            gameObject32.transform.localPosition = new Vector3(0.54f, 0f, 0.8f);
            BtnCollider sver = gameObject32.AddComponent<BtnCollider>();
            sver.button = new Button("cock", false, null);
            ColorChanger c32 = gameObject32.AddComponent<ColorChanger>();
            c32.gradient = h;

            GameObject gameObject4w = new GameObject();
            gameObject4w.transform.parent = canvasObj.transform;
            gameObject4w.name = "Leave";

            Text text90 = gameObject4w.AddComponent<Text>();
            text90.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text90.text = "Leave";
            text90.fontSize = 1;
            text90.alignment = TextAnchor.MiddleCenter;
            text90.resizeTextForBestFit = true;
            text90.resizeTextMinSize = 0;

            RectTransform component3 = text90.GetComponent<RectTransform>();
            component3.localPosition = Vector3.zero;
            component3.sizeDelta = new Vector2(0.28f, 0.05f);
            component3.position = new Vector3(0.06f, 0f, 0.324f);
            component3.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }
        public void Toggle(Button bt)
        {
            try
            {
                int num = (Instance.buttons.Length + pageSize - 1) / pageSize;
                if (bt.buttontext == "cock")
                {
                    PhotonNetwork.Disconnect();
                    return;
                }
                if (bt.buttontext == "NextPage")
                {
                    if (pageNumber < num - 1)
                    {
                        pageNumber++;
                    }
                    else
                    {
                        pageNumber = 0;
                    }
                    Destroy(Instance.menu);
                    Instance.menu = null;
                    Draw();
                    return;
                }
                if (bt.buttontext == "PreviousPage")
                {
                    if (pageNumber > 0)
                    {
                        pageNumber--;
                    }
                    else
                    {
                        pageNumber = num - 1;
                    }
                    Destroy(Instance.menu);
                    Instance.menu = null;
                    Draw();
                    return;
                }
                for (int i = 0; i < Instance.buttons.Length; i++)
                {
                    if (bt.buttontext == Instance.buttons[i].buttontext)
                    {
                        if (bt.isToggle)
                        {
                            buttons[i].Active = !buttons[i].Active;
                            Debug.Log($"{buttons[i].buttontext} Is Active: {buttons[i].Active}");
                            Destroy(Instance.menu);
                            Instance.menu = null;
                            Draw();
                        }
                        else
                        {
                            buttons[i].OnExecute();
                            Destroy(Instance.menu);
                            Instance.menu = null;
                            Draw();
                        }
                        if (!buttons[i].Active)
                        {
                            if (buttons[i].OnDisable != null)
                                buttons[i].OnDisable();
                        }
                    }
                }
                Destroy(Instance.menu);
                Instance.menu = null;
                Draw();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion

        /*
         
        Create Public
        Fly
        Platform
        Speed Boost
        No Tag Freeze
        Long Arms

        ESP
        Chams
        Skeleton ESP
        Tracers
        Box ESP
        Hit Box ESP

        Tag Gun
        Tag All
        Tag Aura
        Tag Self
        Anti Tag
        No Tag On Join
         
        */
        public Button[] buttons;
        private PredictFuturePosition PredictiveLR;
        private bool matself;

        internal void ComposeButtons()
        {
            Instance.buttons = new Attributes.Button[]
            {
                // ---- Page 1 Basic
                new Button("Create Public Room", false, () => MainManager.Instance.CreatePublicRoom()),
                new Button("Fly", true, () =>
                {
                    if (Input.RightPrimary)
                    {
                        GorillaLocomotion.Player.Instance.transform.position += (GorillaLocomotion.Player.Instance.headCollider.transform.forward * Time.deltaTime) * 15;
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    }
                }),
                new Button("Platforms", true, ()=>MainManager.Instance.Platforms()),
                new Button("Speed Boost", true, () =>
                {
                    GorillaLocomotion.Player.Instance.jumpMultiplier = 1.3f;
                    GorillaLocomotion.Player.Instance.maxJumpSpeed = 11;
                }),
                new Button("No Tag Freeze", true, ()=>{ GorillaLocomotion.Player.Instance.disableMovement = false; }),
                new Button("No Clip {LT}", true,()=>MainManager.Instance.NoClip()),

                // ---- Page 2 ESP
                
                new Button("ESP", true, () =>
                {
                    foreach (VRRig rig in GorillaParent.instance.vrrigs)
                    {
                        if (rig != null)
                        {
                            if (!rig.isOfflineVRRig && !rig.isMyPlayer)
                            {
                                GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                GameObject.Destroy(beacon.GetComponent<BoxCollider>());
                                GameObject.Destroy(beacon.GetComponent<Rigidbody>());
                                beacon.transform.rotation = rig.transform.rotation;
                                beacon.transform.localScale = new Vector3(0.4f, 0.86f, 0.4f);
                                beacon.transform.position = rig.transform.position;
                                beacon.GetComponent<MeshRenderer>().material = new Material(Shader.Find("GUI/Text Shader"));
                                if (rig.mainSkin.material.name.Contains("fected"))
                                {
                                    beacon.GetComponent<MeshRenderer>().material.color = Color.red;
                                }
                                else
                                {
                                    beacon.GetComponent<MeshRenderer>().material.color = Color.green;
                                }
                                Destroy(beacon, Time.deltaTime);
                            }
                        }
                    }
                }),
                new Button("Chams",true, () =>
                {
                        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                        {
                            if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer && vrrig.mainSkin.material.name.Contains("fected"))
                            {
                                vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                                vrrig.mainSkin.material.color = new Color(9f, 0f, 0f);
                            }
                            else if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                            {
                                vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                                vrrig.mainSkin.material.color = new Color(0f, 9f, 0f);
                            }
                        }
                },
                () =>
                {
                        foreach (VRRig vrrig in (VRRig[])UnityEngine.Object.FindObjectsOfType(typeof(VRRig)))
                        {
                            if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                            {
                                vrrig.ChangeMaterialLocal(vrrig.currentMatIndex);
                            }
                        }
                }),
                new Button("Bone Esp", true, () => MainManager.Instance.BoneESP(false), () =>
                {
                        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                        {
                            for (int i = 0; i < MainManager.bones.Count(); i += 2)
                            {
                                if (vrrig != null)
                                {
                                    if (vrrig.mainSkin.bones[MainManager.bones[i]].gameObject.GetComponent<LineRenderer>())
                                    {
                                        Destroy(vrrig.mainSkin.bones[MainManager.bones[i]].gameObject.GetComponent<LineRenderer>());
                                    }
                                    if (vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                                    {
                                        Destroy(vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>());
                                    }
                                }
                            }
                        }
                }),
                new Button("Tracers", true, () =>
                {
                    foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                    {
                        if (vrrig != null && !vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                        {
                            var gameobject = new GameObject("Line");
                            LineRenderer lineRenderer = gameobject.AddComponent<LineRenderer>();
                            lineRenderer.startColor = Color.green;
                            lineRenderer.endColor = Color.green;
                            lineRenderer.startWidth = 0.01f;
                            lineRenderer.endWidth = 0.01f;
                            lineRenderer.positionCount = 2;
                            lineRenderer.useWorldSpace = true;
                            lineRenderer.SetPosition(0, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
                            lineRenderer.SetPosition(1, vrrig.transform.position);
                            lineRenderer.material.shader = Shader.Find("GUI/Text Shader");
                            Destroy(lineRenderer, Time.deltaTime);
                        }
                    }
                }),
                new Button("Box Esp", true, ()=> MainManager.Instance.BoxESP()),
                new Button("Hit Box Esp", false, ()=>Notif.SendNotification("This Mod Doesnt Work!")),
                
                // ---- Page 3 Tag Mods

                new Button("Tag Gun", true, ()=> MainManager.Instance.TagGun()),
                new Button("Tag All", true, ()=> MainManager.Instance.TagAll()),
                new Button("Tag Aura", true, ()=>MainManager.Instance.ProcessTagAura()),
                new Button("Tag Self", false, () =>
                {
                    GameMode.ReportHit();
                }),
                new Button("Anti Tag", true, () => MainManager.Instance.NoTag()),
                new Button("No Tag On Join", false, () =>
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                }),

                // ---- Page 4 Projectile mods
                
                new Button("Projectile Spam {SlingShot}", true, ()=>MainManager.Instance.ProjectileSpam()),
                new Button("Projectile Gun {SlingShot}", true, ()=> MainManager.Instance.ProjectileGun()),
                new Button("Projectile Halo {SlingShot}", true, () => MainManager.Instance.ProjectileHalo()),
                new Button("Projectile Rain {SlingShot}", true, () => MainManager.Instance.ProjectileRain()),
                new Button("Piss", true, ()=>MainManager.Instance.Piss()),
                new Button("Cum", true, ()=> MainManager.Instance.Cum()),

                // ---- Page 5 Rig mods

                new Button("Ghost Monkey", true, ()=>MainManager.Instance.GhostMonkey()),
                new Button("Invisable Monkey", true, ()=>MainManager.Instance.InvisMonkey()),
                new Button("Freeze Monkey", true, ()=> MainManager.Instance.FreezeMonkey()),
                new Button("Copy Movement Gun", true, () => MainManager.Instance.CopyGun()),
                new Button("Rig Gun", true, ()=>MainManager.Instance.RigGun()),
                new Button("Hold Rig", true, () =>
                {
                    if (Input.RightGrip)
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.position;
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.enabled = true;
                    }
                }),

                // ---- Page 6 Comp Mods
                
                new Button("Comp Speed Boost {Mosa}", true, ()=>MainManager.Instance.CompSpeedBoost()),
                new Button("WallWalk {RG/LG}", true, ()=>MainManager.Instance.WallWalk(), ()=>{ Physics.gravity = new Vector3(0, -9.81f,0); }),
                new Button("Faster Swimming", true, ()=>MainManager.Instance.FasterSwimming()),
                new Button("B-Hop {KingOfNetflix}", true, () =>
                {
                    MainManager.Instance.BHop();
                }),
                new Button("SlingShot Predictions", true, () =>
                {
                    if (PredictiveLR == null)
                    {
                        var go = new GameObject("PredictiveLineRenderer");
                        var lr = go.AddComponent<LineRenderer>();
                        lr.startColor = Color.green;
                        lr.endColor = Color.green;
                        lr.startWidth = 0.01f;
                        lr.endWidth = 0.01f;
                        lr.positionCount = 2;
                        lr.useWorldSpace = true;
                        lr.material.shader = Shader.Find("GUI/Text Shader");
                        PredictiveLR = go.AddComponent<PredictFuturePosition>();
                        PredictiveLR.lineRenderer = lr;
                    }
                    float size = Mathf.Abs(GorillaTagger.Instance.offlineVRRig.slingshot.transform.lossyScale.x);
                    Vector3 position = GorillaTagger.Instance.offlineVRRig.slingshot.transform.position;
                    Vector3 vector = GorillaTagger.Instance.offlineVRRig.slingshot.centerOrigin.position - GorillaTagger.Instance.offlineVRRig.slingshot.center.position;
                    vector /= size;
                    Vector3 currentVelocity = GorillaLocomotion.Player.Instance.currentVelocity;
                    Vector3 velocity = Mathf.Min(GorillaTagger.Instance.offlineVRRig.slingshot.springConstant * GorillaTagger.Instance.offlineVRRig.slingshot.maxDraw, vector.magnitude * GorillaTagger.Instance.offlineVRRig.slingshot.springConstant) * vector.normalized;
                    velocity *= size;
                    velocity += currentVelocity;
                    PredictiveLR.Velocity = velocity;
                    PredictiveLR.Position = position;
                },
                () =>
                {
                    if (PredictiveLR != null)
                        Destroy(PredictiveLR.gameObject);
                }),
                new Button("Hunt Target Esp", true, () =>
                {
                    if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget != null)
                    {
                        GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                        GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).mainSkin.material.color = Color.red;
                        oldTarget = GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget);
                    }
                    else
                    {
                        oldTarget.ChangeMaterialLocal(oldTarget.currentMatIndex);
                    }
                }),

                // ---- Page 7 Fun Mods

                new Button("Car Monkey", true, ()=>MainManager.Instance.CarMonkey()),
                new Button("Spider Monkey {RT/LT}", true, ()=>MainManager.Instance.SpiderMonke()),
                new Button("Spider Climb {RG/LG}", true, ()=>MainManager.Instance.MonkeClimb(GorillaLocomotion.Player.Instance)),
                new Button("Grapple Gun", true, ()=> MainManager.Instance.GrappleHook()),
                new Button("Iron Monkey {RG/LG}", true, () =>
                {
                                        Rigidbody RB = GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody;
                    if (Input.RightGrip)
                    {
                        RB.AddForce(8f * GorillaLocomotion.Player.Instance.rightControllerTransform.right, ForceMode.Acceleration);
                        GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 50f * RB.velocity.magnitude, GorillaTagger.Instance.tapHapticDuration);
                    }
                    if (Input.LeftGrip)
                    {
                        RB.AddForce(-8f * GorillaLocomotion.Player.Instance.leftControllerTransform.right, ForceMode.Acceleration);
                        GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength / 50f * RB.velocity.magnitude, GorillaTagger.Instance.tapHapticDuration);
                    }
                    if (Input.LeftGrip | Input.RightGrip) RB.velocity = Vector3.ClampMagnitude(RB.velocity, 50f);
                }),
                new Button("C4 {RG}", true, () =>
                {
                    if (Input.RightGrip && !C4spawned && !Input.RightTrigger)
                    {
                        C4spawned = true;
                        C4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Destroy(C4.GetComponent<Rigidbody>());
                        Destroy(C4.GetComponent<BoxCollider>());
                        C4.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.transform.position +
                                                           new Vector3(0.2f, 0f, 0f);
                        C4.transform.localScale = new Vector3(0.2f, 0.1f, 0.4f);
                        C4.GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);
                        C4.GetComponent<Renderer>().material.color = Color.magenta;
                    }
                    if (C4spawned && Input.RightTrigger)
                    {
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddExplosionForce(80000f, C4.transform.position, 10f, 5f);
                        Destroy(C4);
                        C4spawned = false;
                    }
                }),

                // ---- Page 8 Fun Mods 2

                new Button("Teleport Gun", true, ()=>MainManager.Instance.ProcessTeleportGun()),
                new Button("Checkpoint", true, ()=>MainManager.Instance.ProcessCheckPoint()),
                new Button("Magic Monkey", true, ()=>MainManager.Instance.MagicMonkey()),
                new Button("Splash {RT/LT}", true, ()=>MainManager.Instance.Splash()),
                new Button("Sizeable Splash", true, ()=>MainManager.Instance.SizeableSplash()),
                new Button("Splash Gun", true, ()=>MainManager.Instance.SpashGun()),

                // ---- Page 9 Rope Mods
                
                new Button("Rope Up", true, ()=>MainManager.Instance.RopeUp()),
                new Button("Rope Down", true, ()=> MainManager.Instance.RopeDown()),
                new Button("Rope Spaz", true, ()=> MainManager.Instance.RopeSpaz()),
                new Button("Rope To Self", true, ()=>MainManager.Instance.RopeToSelf()),
                new Button("Freeze Ropes", true, ()=>MainManager.Instance.FreezeMonkey()),
                new Button("Rope Gun", true, ()=>MainManager.Instance.RopeGun()),

                // ---- Page 10 Stump Mods

                new Button("RGB {STUMP}", true, ()=>MainManager.Instance.RGB()),
                new Button("Strobe {STUMP}", true, ()=>MainManager.Instance.Strobe()),
                new Button("Kick Gun {STUMP} {PRIVATE}", true, ()=>MainManager.Instance.KickGun()),
                new Button("Kick All {STUMP} {PRIVATE}", false, () =>
                {
                    if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
                    {
                        GorillaComputer.instance.OnGroupJoinButtonPress(1,GorillaComputer.instance.friendJoinCollider);
                    }
                }),
                new Button("Touch Kick {STUMP} {PRIVATE}", false, () =>MainManager.Instance.TouchKick()),
                new Button("Random Cosmetics {Try On Room}", true, ()=>MainManager.Instance.RandomTryonHats()),

                // ---- Page 11 Op Mods

                new Button("AntiBan", false, () =>
                {
                    if (PhotonNetwork.InRoom)
                    {
                        MainManager.Instance.StartAntiBan();
                    }
                }),
                new Button("Set Master {AntiBan}", false, ()=>PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer)),
                new Button("Freeze All {AntiBan}", true, () =>
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                        MainManager.Instance.FreezeAll();
                }),
                new Button("Freeze Gun {AntiBan}", true, () =>
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                        MainManager.Instance.FreezeGun();
                }),
                new Button("Crash All {AntiBan}", true, () =>
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                        MainManager.Instance.CrashAll();
                }, ()=>GorillaParentPatcher.isCrashing = false),
                new Button("Crash Gun {AntiBan}", true, () =>
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                        MainManager.Instance.CrashGun();
                }),

                // ---- Page 12 Op Mods 2
                new Button("Touch Crash {AntiBan}", true, ()=>MainManager.Instance.TouchCrash()),
                new Button("Disable Nofications", true, ()=> {Notif.IsEnabled = false;}, ()=>{Notif.IsEnabled=true; }),
                new Button("Break Gamemode {M} {AntiBan}", false, () =>
                {
                    if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                    {
                                                string orgstr = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
                        if (orgstr.TryReplace("BATTLE", "", out string rforest))
                        {
                            orgstr = rforest;
                        }
                        if (orgstr.TryReplace("INFECTION", "", out string rcanton))
                        {
                            orgstr = rcanton;
                        }
                        if (orgstr.TryReplace("HUNT", "", out string rcacnton))
                        {
                            orgstr = rcacnton;
                        }
                        if (orgstr.TryReplace("CASUAL", "", out string rbasement))
                        {
                            orgstr = rbasement;
                        }
                        var hash = new Hashtable
                {
                    { "gameMode", orgstr }
                };
                        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                        PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetCustomPropertiesOfRoom(hash);
                        GameMode gmInstance = Traverse.Create(typeof(GameMode)).Field("instance").GetValue<GameMode>();
                        if (gmInstance)
                        {
                            PhotonNetwork.Destroy(gmInstance.GetComponent<PhotonView>());
                        }
                    }
                }),
                new Button("Sound Spam {M}", true, ()=>RPCSUB.SendSound(UnityEngine.Random.Range(0, 10), 1)),
                new Button("Invis All {Rejoin}", false, () =>
                {
                    foreach (Photon.Realtime.Player owner in PhotonNetwork.PlayerListOthers)
                    {
                        PhotonNetwork.CurrentRoom.StorePlayer(owner);
                        PhotonNetwork.CurrentRoom.Players.Remove(owner.ActorNumber);
                        PhotonNetwork.OpRemoveCompleteCacheOfPlayer(owner.ActorNumber);
                    }
                }),
                new Button("Invis Gun {Rejoin}", true, ()=>MainManager.Instance.InvisGun()),

                // Page 13 Op Mods 3

                new Button("Mat All {M}", true, () =>
                {
                        if (Time.time > StealGUI.mattimer)
                        {
                            MainManager.Instance.matSpamAll();
                        }
                }),
                new Button("Mat Gun {M}", true, ()=>MainManager.Instance.MatGun()),
                new Button("Mat Self {M}", true, () =>
                {
                    if (!matself)
                    {
                        if (PhotonNetwork.IsMasterClient)
                            MainManager.Instance.StartMatSelf();
                        matself = true;
                    }
                },
                ()=>{
                    matself = false;
                }),
                new Button("Tag Lag {M}", true, () =>
                {
                        if (GorillaGameManager.instance != null)
                        {
                            GorillaGameManager.instance.GetComponent<GorillaTagManager>().tagCoolDown = 999999;
                        }
                },
                () =>
                {
                            GorillaGameManager.instance.GetComponent<GorillaTagManager>().tagCoolDown = 5;
                }),
                new Button("Rock Game {M}", true, () =>
                {
                        if (GorillaGameManager.instance != null)
                        {
                            GorillaGameManager.instance.GetComponent<GorillaTagManager>().isCurrentlyTag = true;
                        }
                }),
                new Button("No Tag Cooldown {M}", true, () =>
                {
                        if (GorillaGameManager.instance != null)
                        {
                            GorillaGameManager.instance.GetComponent<GorillaTagManager>().tagCoolDown = 0;
                        }
                }, ()=> { if (GorillaGameManager.instance != null){ GorillaGameManager.instance.GetComponent<GorillaTagManager>().tagCoolDown = 5; } }),

                // Page 14 OP Mods 4

                new Button("Acid All {M}", true, () =>
                {
                        Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(11);
                        ScienceExperimentManager.PlayerGameState[] inGamePlayerStates = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
                        for (int i = 0; i < 10; i++)
                        {
                                PlayerGameState playerGameState = new PlayerGameState();
                                if (!playerGameState.touchedLiquid)
                                {
                                    playerGameState.touchedLiquidAtProgress = 1f;
                                }
                                playerGameState.touchedLiquid = true;
                                playerGameState.playerId = PhotonNetwork.PlayerList[i]==null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
                                inGamePlayerStates[i] = playerGameState;
                        }
                        Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(inGamePlayerStates);

                },
                ()=>{
                        Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(11);
                        ScienceExperimentManager.PlayerGameState[] inGamePlayerStates = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
                        for (int i = 0; i < 10; i++)
                        {
                                PlayerGameState playerGameState = new PlayerGameState();
                                if (!playerGameState.touchedLiquid)
                                {
                                    playerGameState.touchedLiquidAtProgress = -1f;
                                }
                                playerGameState.touchedLiquid = false;
                                playerGameState.playerId = PhotonNetwork.PlayerList[i]==null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
                                inGamePlayerStates[i] = playerGameState;
                        }
                        Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(inGamePlayerStates);
                }),
                new Button("Acid Self {M}", true, () =>
                {
                        Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(11);
                        ScienceExperimentManager.PlayerGameState[] inGamePlayerStates = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
                        for (int i = 0; i < 10; i++)
                        {
                                PlayerGameState playerGameState = new PlayerGameState();
                                playerGameState.playerId = PhotonNetwork.PlayerList[i]==null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
                                if (playerGameState.playerId == PhotonNetwork.LocalPlayer.ActorNumber)
                                {
                                    if (!playerGameState.touchedLiquid)
                                    {
                                        playerGameState.touchedLiquidAtProgress = 1f;
                                    }
                                    playerGameState.touchedLiquid = true;
                                }
                                inGamePlayerStates[i] = playerGameState;
                        }
                        Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(inGamePlayerStates);
                },
                () =>
                {
                        Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(11);
                        ScienceExperimentManager.PlayerGameState[] inGamePlayerStates = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
                        for (int i = 0; i < 10; i++)
                        {
                                PlayerGameState playerGameState = new PlayerGameState();
                                playerGameState.playerId = PhotonNetwork.PlayerList[i]==null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
                                if (playerGameState.playerId == PhotonNetwork.LocalPlayer.ActorNumber)
                                {
                                    if (!playerGameState.touchedLiquid)
                                    {
                                        playerGameState.touchedLiquidAtProgress = 1f;
                                    }
                                    playerGameState.touchedLiquid = false;
                                }
                                inGamePlayerStates[i] = playerGameState;
                        }
                        Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(inGamePlayerStates);
                }),
                new Button("Slow All {M}", true, () =>MainManager.Instance.SlowAll()),
                new Button("Slow Gun {M}", true, () =>MainManager.Instance.SlowGun()),
                new Button("Vibrate All {M}", true, () =>MainManager.Instance.VibrateAll()),
                new Button("Vibrate Gun {M}", true, () =>MainManager.Instance.VibrateGun()),

                // Page 15 OP Mods 5

                new Button("Change Gamemode to Casual {M}", false, () =>
                {
                    MainManager.Instance.changegamemode("CASUAL");
                }),
                new Button("Change Gamemode to Infection {M}", false, () =>
                {
                    MainManager.Instance.changegamemode("INFECTION");
                }),
                new Button("Change Gamemode to Hunt {M}", false, () =>
                {
                    MainManager.Instance.changegamemode("HUNT");
                }),
                new Button("Change Gamemode to Battle {M}", false, () =>
                {
                    MainManager.Instance.changegamemode("BATTLE");
                }),
                new Button("Set Room to Private {M}", false, ()=>{ PhotonNetwork.CurrentRoom.IsVisible = false; }),
                new Button("Set Room to Public {M}", false, ()=>{ PhotonNetwork.CurrentRoom.IsVisible = true; }),

                // Page 15 OP Mods 6

                new Button("Close Room {M}", true, ()=>PhotonNetwork.CurrentRoom.IsOpen = false, ()=>PhotonNetwork.CurrentRoom.IsVisible=true),
                new Button("Disable Network Triggers {SS} {AntiBan}", false, ()=>MainManager.Instance.DisableNetworkTriggers()),
                new Button("Trap All In Stump {AntiBan}", false, () =>
                {
                                        string orgstr = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
                    if (orgstr.TryReplace("forest", "", out string rforest))
                    {
                        orgstr = rforest;
                    }
                    if (orgstr.TryReplace("canyon", "", out string rcanton))
                    {
                        orgstr = rcanton;
                    }
                    if (orgstr.TryReplace("city", "", out string rcacnton))
                    {
                        orgstr = rcacnton;
                    }
                    if (orgstr.TryReplace("basement", "", out string rbasement))
                    {
                        orgstr = rbasement;
                    }
                    if (orgstr.TryReplace("clouds", "", out string rbacsement))
                    {
                        orgstr = rbacsement;
                    }
                    if (orgstr.TryReplace("mountain", "", out string r1basement))
                    {
                        orgstr = r1basement;
                    }
                    if (orgstr.TryReplace("beach", "", out string r1bdsement))
                    {
                        orgstr = r1bdsement;
                    }
                    if (orgstr.TryReplace("cave", "", out string r1bdsement1))
                    {
                        orgstr = r1bdsement1;
                    }
                    var hash = new Hashtable
                {
                    { "gameMode", orgstr }
                };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                    PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetCustomPropertiesOfRoom(hash);
                }),
                new Button("Desync Visualization", true, () => MainManager.Instance.DesyncChecker(), ()=>{ Destroy(MainManager.Instance.rigob); })
            };
        }

        
        static void Postfix()
        {
            try
            {
                if (instance == null) { return; }
                if (Input.LeftPrimary)
                {
                    if (Instance.menu == null)
                    {
                        Instance.Draw();
                        if (Instance.menuClick == null)
                        {
                            Instance.menuClick = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Instance.menuClick.transform.SetParent(GorillaLocomotion.Player.Instance.rightControllerTransform);
                            Instance.menuClick.name = "MenuClicker";
                            Instance.menuClick.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                            Instance.menuClick.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                        }
                    }
                    Instance.menu.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                    Instance.menu.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
                }
                else
                {
                    if (Instance.menu != null)
                    {
                        Destroy(Instance.menu);
                        Instance.menu = null;
                        Destroy(Instance.menuClick);
                        Instance.menuClick = null;
                    }
                }

                if (GorillaTagger.Instance.offlineVRRig != null && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaGameManager.instance != null)
                    {
                        GorillaTagger.Instance.offlineVRRig.ChangeMaterialLocal(GorillaGameManager.instance.MyMatIndex(PhotonNetwork.LocalPlayer));
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.ChangeMaterialLocal(0);
                    }
                }

                foreach (Button butt in Instance.buttons)
                {
                    if (butt.Active)
                    {
                        butt.OnExecute();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
        /*
        static void Prefix()
        {
            try {
                GorillaLocomotion.Player instance = GorillaLocomotion.Player.Instance;
                if (Input.LeftPrimary)
                {
                    if (Instance.menu == null)
                    {
                        Draw();
                        if (Instance.menuClick == null)
                        {
                            Instance.menuClick = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Instance.menuClick.transform.SetParent(GorillaLocomotion.Player.Instance.rightControllerTransform);
                            Instance.menuClick.name = "Instance.menuClicker";
                            Instance.menuClick.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                            Instance.menuClick.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                        }
                    }
                    Instance.menu.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                    Instance.menu.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
                }
                else
                {
                    if (Instance.menu != null)
                    {
                        Destroy(Instance.menu);
                        Instance.menu = null;
                        Destroy(Instance.menuClick);
                        Instance.menuClick = null;
                    }
                }

                if (GorillaTagger.Instance.offlineVRRig != null && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaGameManager.instance != null)
                    {
                        GorillaTagger.Instance.offlineVRRig.ChangeMaterialLocal(GorillaGameManager.instance.MyMatIndex(PhotonNetwork.LocalPlayer));
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.ChangeMaterialLocal(0);
                    }
                }

                // ---- PAGE 1 ---- Default

                if (Instance.buttonsActive[0])
                {
                    MainManager.Instance.CreatePublicRoom();
                    Instance.buttonsActive[0] = false;
                    Destroy(Instance.menu);
                    Instance.menu = null;
                }

                if (Instance.buttonsActive[1])
                {
                    if (Input.RightPrimary)
                    {
                        GorillaLocomotion.Player.Instance.transform.position += (GorillaLocomotion.Player.Instance.headCollider.transform.forward * Time.deltaTime) * 15;
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    }
                }

                if (Instance.buttonsActive[2])
                {
                    MainManager.Instance.Platforms();
                }

                if (Instance.buttonsActive[3])
                {
                    GorillaLocomotion.Player.Instance.jumpMultiplier = 1.3f;
                    GorillaLocomotion.Player.Instance.maxJumpSpeed = 11;
                }

                if (Instance.buttonsActive[4])
                {
                    GorillaLocomotion.Player.Instance.disableMovement = false;
                }

                if (Instance.buttonsActive[5])
                {
                    MainManager.Instance.NoClip();
                }

                // ---- PAGE 2 ---- ESP

                if (Instance.buttonsActive[6])
                {
                    foreach (VRRig rig in GorillaParent.instance.vrrigs)
                    {
                        if (rig != null)
                        {
                            if (!rig.isOfflineVRRig && !rig.isMyPlayer)
                            {
                                GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                GameObject.Destroy(beacon.GetComponent<BoxCollider>());
                                GameObject.Destroy(beacon.GetComponent<Rigidbody>());
                                beacon.transform.rotation = rig.transform.rotation;
                                beacon.transform.localScale = new Vector3(0.4f, 0.86f, 0.4f);
                                beacon.transform.position = rig.transform.position;
                                beacon.GetComponent<MeshRenderer>().material = new Material(Shader.Find("GUI/Text Shader"));
                                if (rig.mainSkin.material.name.Contains("fected"))
                                {
                                    beacon.GetComponent<MeshRenderer>().material.color = Color.red;
                                }
                                else
                                {
                                    beacon.GetComponent<MeshRenderer>().material.color = Color.green;
                                }
                                Destroy(beacon, Time.deltaTime);
                            }
                        }
                    }
                }

                if (Instance.buttonsActive[7])
                {
                    if (PhotonNetwork.CurrentRoom != null)
                    {
                        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                        {
                            if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer && vrrig.mainSkin.material.name.Contains("fected"))
                            {
                                vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                                vrrig.mainSkin.material.color = new Color(9f, 0f, 0f);
                            }
                            else if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                            {
                                vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                                vrrig.mainSkin.material.color = new Color(0f, 9f, 0f);
                            }
                        }
                        psnma = false;
                    }
                }
                else
                {
                    if (!psnma)
                    {
                        foreach (VRRig vrrig in (VRRig[])UnityEngine.Object.FindObjectsOfType(typeof(VRRig)))
                        {
                            if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                            {
                                vrrig.ChangeMaterialLocal(vrrig.currentMatIndex);
                            }
                        }
                        psnma = true;
                    }
                }

                if (Instance.buttonsActive[8])
                {
                    MainManager.Instance.BoneESP(false);
                    boner = true;
                }
                else
                {
                    if (boner)
                    {
                        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                        {
                            for (int i = 0; i < MainManager.bones.Count(); i += 2)
                            {
                                if (vrrig != null)
                                {
                                    if (vrrig.mainSkin.bones[MainManager.bones[i]].gameObject.GetComponent<LineRenderer>())
                                    {
                                        Destroy(vrrig.mainSkin.bones[MainManager.bones[i]].gameObject.GetComponent<LineRenderer>());
                                    }
                                    if (vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                                    {
                                        Destroy(vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>());
                                    }
                                }
                            }
                        }
                        boner = false;
                    }
                }

                if (Instance.buttonsActive[9])
                {
                    foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                    {
                        if (vrrig != null && !vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                        {
                            var gameobject = new GameObject("Line");
                            LineRenderer lineRenderer = gameobject.AddComponent<LineRenderer>();
                            lineRenderer.startColor = Color.green;
                            lineRenderer.endColor = Color.green;
                            lineRenderer.startWidth = 0.01f;
                            lineRenderer.endWidth = 0.01f;
                            lineRenderer.positionCount = 2;
                            lineRenderer.useWorldSpace = true;
                            lineRenderer.SetPosition(0, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
                            lineRenderer.SetPosition(1, vrrig.transform.position);
                            lineRenderer.material.shader = Shader.Find("GUI/Text Shader");
                            Destroy(lineRenderer, Time.deltaTime);
                        }
                    }
                }

                if (Instance.buttonsActive[10])
                {
                    foreach (VRRig rig in GorillaParent.instance.vrrigs)
                    {
                        if (rig != null && !rig.isOfflineVRRig && !rig.isMyPlayer)
                        {
                            MainManager.Instance.BoxESP(rig);
                        }
                    }
                }

                if (Instance.buttonsActive[11])
                {
                    foreach (VRRig rig in GorillaParent.instance.vrrigs)
                    {
                        if (rig != null && !rig.isOfflineVRRig && !rig.isMyPlayer)
                        {
                            MainManager.Instance.HitBoxESP(rig);
                        }
                    }
                }

                // ---- PAGE 3 ---- TAG Mods

                if (Instance.buttonsActive[12])
                {
                    MainManager.Instance.TagGun();
                }

                if (Instance.buttonsActive[13])
                {
                    MainManager.Instance.TagAll();
                }

                if (Instance.buttonsActive[14])
                {
                    MainManager.Instance.ProcessTagAura();
                }

                if (Instance.buttonsActive[15])
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        MainManager.Instance.saveKeys();
                        MainManager.Instance.GorillaTagManager.currentInfected.Add(PhotonNetwork.LocalPlayer);
                    }
                    Instance.buttonsActive[15] = false;
                    Destroy(Instance.menu);
                    Instance.menu = null;
                }

                if (Instance.buttonsActive[16])
                {
                    MainManager.Instance.NoTag();
                }

                if (Instance.buttonsActive[17])
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    Instance.buttonsActive[17] = false;
                    Destroy(Instance.menu);
                    Instance.menu = null;
                }

                // ---- PAGE 4 ---- Projectile mods

                if (Instance.buttonsActive[18])
                {
                    MainManager.Instance.ProjectileSpam();
                }

                if (Instance.buttonsActive[19])
                {
                    MainManager.Instance.ProjectileGun();
                }

                if (Instance.buttonsActive[20])
                {
                    MainManager.Instance.ProjectileHalo();
                }

                if (Instance.buttonsActive[21])
                {
                    MainManager.Instance.ProjectileRain();
                }

                if (Instance.buttonsActive[22])
                {
                    MainManager.Instance.Piss();
                }

                if (Instance.buttonsActive[23])
                {
                    MainManager.Instance.Cum();
                }

                // ---- PAGE 5 ---- Rig Mods

                if (Instance.buttonsActive[24])
                {
                    if (Input.RightTrigger)
                    {
                        if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            if (GorillaTagger.Instance.offlineVRRig.enabled)
                            {
                                Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                            }
                            GorillaTagger.Instance.offlineVRRig.enabled = false;
                            ghostToggled = true;
                        }
                        else
                        {
                            if (!ghostToggled && !GorillaTagger.Instance.offlineVRRig.enabled)
                            {
                                GorillaTagger.Instance.offlineVRRig.enabled = true;
                                ghostToggled = true;
                            }
                        }
                    }
                    else
                    {
                        ghostToggled = false;
                    }
                }

                if (Instance.buttonsActive[25])
                {
                    if (Input.RightTrigger)
                    {
                        if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            if (GorillaTagger.Instance.offlineVRRig.enabled)
                            {
                                Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                            }
                            GorillaTagger.Instance.offlineVRRig.enabled = false;
                            GorillaTagger.Instance.offlineVRRig.transform.position = new Vector3(999, 999, 999);
                            ghostToggled = true;
                        }
                        else
                        {
                            if (!ghostToggled && !GorillaTagger.Instance.offlineVRRig.enabled)
                            {
                                GorillaTagger.Instance.offlineVRRig.enabled = true;
                                ghostToggled = true;
                            }
                        }
                    }
                    else
                    {
                        ghostToggled = false;
                    }
                }

                if (Instance.buttonsActive[26])
                {
                    MainManager.Instance.FreezeMonkey();
                }

                if (Instance.buttonsActive[27])
                {
                    MainManager.Instance.CopyGun();
                }

                if (Instance.buttonsActive[28])
                {
                    MainManager.Instance.RigGun();
                }

                if (Instance.buttonsActive[29])
                {
                    if (Input.RightGrip)
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.position;
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.enabled = true;
                    }
                }

                // ---- PAGE 6 ---- Comp Mods

                if (Instance.buttonsActive[30])
                {
                    MainManager.Instance.CompSpeedBoost();
                }

                if (Instance.buttonsActive[31])
                {
                    RaycastHit Left;
                    RaycastHit Right;
                    Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.right, out Left, 100f, int.MaxValue);
                    Physics.Raycast(GorillaLocomotion.Player.Instance.leftControllerTransform.position, GorillaLocomotion.Player.Instance.leftControllerTransform.right, out Right, 100f, int.MaxValue);

                    if (Input.RightGrip)
                    {
                        if (Left.distance < Right.distance)
                        {
                            if (Left.distance < 1)
                            {
                                Vector3 gravityDirection = (Left.point - GorillaLocomotion.Player.Instance.rightControllerTransform.position).normalized;
                                Physics.gravity = gravityDirection * 9.81f;
                            }
                            else
                            {
                                Physics.gravity = new Vector3(0, -9.81f, 0);
                            }
                        }
                        if (Left.distance == Right.distance)
                        {
                            Physics.gravity = new Vector3(0, -9.81f, 0);
                        }
                    }
                    if (Input.LeftGrip)
                    {
                        if (Left.distance > Right.distance)
                        {
                            if (Right.distance < 1)
                            {
                                Vector3 gravityDirection = (Right.point - GorillaLocomotion.Player.Instance.leftControllerTransform.position).normalized;
                                Physics.gravity = gravityDirection * 9.81f;
                            }
                            else
                            {
                                Physics.gravity = new Vector3(0, -9.81f, 0);
                            }
                        }
                        if (Left.distance == Right.distance)
                        {
                            Physics.gravity = new Vector3(0, -9.81f, 0);
                        }
                    }
                    if (!Input.LeftGrip && !Input.RightGrip)
                    {
                        Physics.gravity = new Vector3(0, -9.81f, 0);
                    }

                }

                if (Instance.buttonsActive[32])
                {
                    MainManager.Instance.CompTagAura();
                }

                if (Instance.buttonsActive[33])
                {
                    if (Input.RightGrip)
                    {
                        MainManager.Instance.CompBalloonAura();
                    }
                }

                if (Instance.buttonsActive[34])
                {
                    if (PredictiveLR == null)
                    {
                        var go = new GameObject("PredictiveLineRenderer");
                        var lr = go.AddComponent<LineRenderer>();
                        lr.startColor = Color.green;
                        lr.endColor = Color.green;
                        lr.startWidth = 0.01f;
                        lr.endWidth = 0.01f;
                        lr.positionCount = 2;
                        lr.useWorldSpace = true;
                        lr.material.shader = Shader.Find("GUI/Text Shader");
                        PredictiveLR = go.AddComponent<PredictFuturePosition>();
                        PredictiveLR.lineRenderer = lr;
                    }
                    float size = Mathf.Abs(GorillaTagger.Instance.offlineVRRig.slingshot.transform.lossyScale.x);
                    Vector3 position = GorillaTagger.Instance.offlineVRRig.slingshot.transform.position;
                    Vector3 vector = GorillaTagger.Instance.offlineVRRig.slingshot.centerOrigin.position - GorillaTagger.Instance.offlineVRRig.slingshot.center.position;
                    vector /= size;
                    Vector3 currentVelocity = GorillaLocomotion.Player.Instance.currentVelocity;
                    Vector3 velocity = Mathf.Min(GorillaTagger.Instance.offlineVRRig.slingshot.springConstant * GorillaTagger.Instance.offlineVRRig.slingshot.maxDraw, vector.magnitude * GorillaTagger.Instance.offlineVRRig.slingshot.springConstant) * vector.normalized;
                    velocity *= size;
                    velocity += currentVelocity;
                    PredictiveLR.Velocity = velocity;
                    PredictiveLR.Position = position;
                }
                else
                {
                    if (PredictiveLR != null)
                        Destroy(PredictiveLR.gameObject);
                }

                if (Instance.buttonsActive[35])
                {
                    if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget != null)
                    {
                        GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                        GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).mainSkin.material.color = Color.red;
                        oldTarget = GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget);
                    }
                    else
                    {
                        oldTarget.ChangeMaterialLocal(oldTarget.currentMatIndex);
                    }
                }

                // ---- PAGE 7 ---- Fun Mods

                if (Instance.buttonsActive[36])
                {
                    MainManager.Instance.CarMonkey();
                }

                if (Instance.buttonsActive[37])
                {
                    MainManager.Instance.SpiderMonke();
                }

                if (Instance.buttonsActive[38])
                {
                    MainManager.Instance.MonkeClimb(instance);
                }

                if (Instance.buttonsActive[39])
                {
                    MainManager.Instance.GrappleHook();
                }

                if (Instance.buttonsActive[40])
                {
                    Rigidbody RB = GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody;
                    if (Input.RightGrip)
                    {
                        RB.AddForce(8f * GorillaLocomotion.Player.Instance.rightControllerTransform.right, ForceMode.Acceleration);
                        GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 50f * RB.velocity.magnitude, GorillaTagger.Instance.tapHapticDuration);
                    }
                    if (Input.LeftGrip)
                    {
                        RB.AddForce(-8f * GorillaLocomotion.Player.Instance.leftControllerTransform.right, ForceMode.Acceleration);
                        GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength / 50f * RB.velocity.magnitude, GorillaTagger.Instance.tapHapticDuration);
                    }
                    if (Input.LeftGrip | Input.RightGrip) RB.velocity = Vector3.ClampMagnitude(RB.velocity, 50f);
                }

                if (Instance.buttonsActive[41])
                {
                    if (Input.RightGrip && !C4spawned && !Input.RightTrigger)
                    {
                        C4spawned = true;
                        C4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Destroy(C4.GetComponent<Rigidbody>());
                        Destroy(C4.GetComponent<BoxCollider>());
                        C4.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.transform.position +
                                                           new Vector3(0.2f, 0f, 0f);
                        C4.transform.localScale = new Vector3(0.2f, 0.1f, 0.4f);
                        C4.GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);
                        C4.GetComponent<Renderer>().material.color = Color.magenta;
                    }
                    if (C4spawned && Input.RightTrigger)
                    {
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddExplosionForce(80000f, C4.transform.position, 10f, 5f);
                        Destroy(C4);
                        C4spawned = false;
                    }
                }

                // ---- PAGE 8 ---- Fun Mods 2

                if (Instance.buttonsActive[42])
                {
                    MainManager.Instance.ProcessTeleportGun();
                }

                if (Instance.buttonsActive[43])
                {
                    MainManager.Instance.ProcessCheckPoint();
                }

                if (Instance.buttonsActive[44])
                {
                    MainManager.Instance.MagicMonkey();
                }

                if (Instance.buttonsActive[45])
                {
                    MainManager.Instance.Splash();
                }

                if (Instance.buttonsActive[46])
                {
                    MainManager.Instance.SpashGun();
                }

                if (Instance.buttonsActive[47])
                {
                    MainManager.Instance.SizeableSplash();
                }

                // ---- PAGE 9 ---- Rope Mods

                if (Instance.buttonsActive[48])
                {
                    MainManager.Instance.RopeUp();
                }

                if (Instance.buttonsActive[49])
                {
                    MainManager.Instance.RopeDown();
                }

                if (Instance.buttonsActive[50])
                {
                    MainManager.Instance.RopeSpaz();
                }

                if (Instance.buttonsActive[51])
                {
                    MainManager.Instance.RopeToSelf();
                }

                if (Instance.buttonsActive[52])
                {
                    MainManager.Instance.RopeFreeze();
                }

                if (Instance.buttonsActive[53])
                {
                    MainManager.Instance.RopeGun();
                }

                // ---- PAGE 10 ---- Stump Mods

                if (Instance.buttonsActive[54])
                {
                    MainManager.Instance.RGB();
                }

                if (Instance.buttonsActive[55])
                {
                    MainManager.Instance.Strobe();
                }

                if (Instance.buttonsActive[56])
                {
                    MainManager.Instance.KickGun();
                }

                if (Instance.buttonsActive[57])
                {
                    if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
                    {
                        GorillaComputer.instance.OnGroupJoinButtonPress(1, GorillaComputer.instance.friendJoinCollider);
                    }
                }

                if (Instance.buttonsActive[58])
                {
                    MainManager.Instance.TouchKick();
                }

                if (Instance.buttonsActive[59])
                {
                    MainManager.Instance.RandomTryonHats();
                }

                // ---- PAGE 11 ---- OP Mods

                if (Instance.buttonsActive[60])
                {
                    if (PhotonNetwork.CurrentRoom != null)
                    {
                        if (!PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                        {
                            if (Time.time > MainManager.Instance.antibancooldown)
                            {
                                if (antiban)
                                {
                                    MainManager.Instance.StartAntiBan();
                                    antiban = false;
                                }
                            }
                            else
                            {
                                Notif.SendNotification("Please wait a few seconds before enabling antiban");
                            }
                        }
                    }
                    else
                    {
                        antiban = true;
                    }
                    Instance.buttonsActive[60] = false;
                    Destroy(Instance.menu);
                    Instance.menu = null;
                    Draw();

                }
                else
                {
                    antiban = true;
                }

                if (Instance.buttonsActive[61])
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                    {
                        PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                    }
                    Instance.buttonsActive[61] = false;
                    Destroy(Instance.menu);
                    Instance.menu = null;
                }

                if (Instance.buttonsActive[62])
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                        MainManager.Instance.CrashAll();
                }

                if (Instance.buttonsActive[63])
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                        MainManager.Instance.CrashGun();
                }

                if (Instance.buttonsActive[64])
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED") && PhotonNetwork.IsMasterClient)
                        MainManager.Instance.LagGun();
                }

                if (Instance.buttonsActive[65])
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED") && PhotonNetwork.IsMasterClient)
                    {
                        Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized + new Vector2(0.5f, 2f);
                        double ui = PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time);
                        ScienceExperimentManager.instance.photonView.RPC("SpawnSodaBubbleRPC", RpcTarget.Others, new object[] { vector, float.MinValue, 9999f, ui });
                        ScienceExperimentManager.instance.photonView.RPC("SpawnSodaBubbleRPC", RpcTarget.Others, new object[] { vector, float.MinValue, 9999f, ui });
                        ScienceExperimentManager.instance.photonView.RPC("SpawnSodaBubbleRPC", RpcTarget.Others, new object[] { vector, float.MaxValue, 9999f, ui });
                        ScienceExperimentManager.instance.photonView.RPC("SpawnSodaBubbleRPC", RpcTarget.Others, new object[] { vector, float.MinValue, 9999f, ui });
                    }
                }

                // ---- PAGE 12 ---- OP Mods 2

                if (Instance.buttonsActive[66])
                {
                    Notif.IsEnabled = false;
                }
                else
                {
                    Notif.IsEnabled = true;
                }

                if (Instance.buttonsActive[67])
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                    {
                        MainManager.Instance.PhotonDestroyAll2();
                    }
                }

                if (Instance.buttonsActive[68])
                {
                    if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
                    {
                        string orgstr = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
                        if (orgstr.TryReplace("BATTLE", "", out string rforest))
                        {
                            orgstr = rforest;
                        }
                        if (orgstr.TryReplace("INFECTION", "", out string rcanton))
                        {
                            orgstr = rcanton;
                        }
                        if (orgstr.TryReplace("HUNT", "", out string rcacnton))
                        {
                            orgstr = rcacnton;
                        }
                        if (orgstr.TryReplace("CASUAL", "", out string rbasement))
                        {
                            orgstr = rbasement;
                        }
                        var hash = new Hashtable
                {
                    { "gameMode", orgstr }
                };
                        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                        PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetCustomPropertiesOfRoom(hash);
                        GameMode gmInstance = Traverse.Create(typeof(GameMode)).Field("instance").GetValue<GameMode>();
                        if (gmInstance)
                        {
                            PhotonNetwork.Destroy(gmInstance.GetComponent<PhotonView>());
                        }
                        Instance.buttonsActive[68] = false;
                        Destroy(Instance.menu);
                        Instance.menu = null;
                    }
                }

                if (Instance.buttonsActive[69])
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        RPCSUB.SendSound(UnityEngine.Random.Range(0, 10), 1);
                    }
                }

                if (Instance.buttonsActive[70])
                {
                    foreach (Photon.Realtime.Player owner in PhotonNetwork.PlayerListOthers)
                    {
                        PhotonNetwork.CurrentRoom.StorePlayer(owner);
                        PhotonNetwork.CurrentRoom.Players.Remove(owner.ActorNumber);
                        PhotonNetwork.OpRemoveCompleteCacheOfPlayer(owner.ActorNumber);
                    }
                    Instance.buttonsActive[70] = false;
                    Destroy(Instance.menu);
                    Instance.menu = null;
                }

                if (Instance.buttonsActive[71])
                {
                    MainManager.Instance.InvisGun();
                }

                // ---- PAGE 13 ---- OP Mods 3

                if (Instance.buttonsActive[72])
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (GorillaGameManager.instance != null)
                        {
                            if (MainManager.Instance.GetGameMode().Contains("INFECTION"))
                            {
                                if (MainManager.Instance.GorillaTagManager == null)
                                {
                                    MainManager.Instance.GorillaTagManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                                }
                            }
                            else if (MainManager.Instance.GetGameMode().Contains("HUNT"))
                            {
                                if (MainManager.Instance.GorillaHuntManager == null)
                                {
                                    MainManager.Instance.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
                                }
                            }
                            else if (MainManager.Instance.GetGameMode().Contains("BATTLE"))
                            {
                                if (MainManager.Instance.GorillaBattleManager == null)
                                {
                                    MainManager.Instance.GorillaBattleManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                                }
                            }
                        }
                        if (Time.time > StealGUI.mattimer)
                        {
                            MainManager.Instance.matSpamAll();
                        }
                    }
                }

                if (Instance.buttonsActive[73])
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        MainManager.Instance.MatGun();
                    }
                }

                if (Instance.buttonsActive[74])
                {
                    if (!matself)
                    {
                        if (PhotonNetwork.IsMasterClient)
                            MainManager.Instance.StartMatSelf();
                        matself = true;
                    }
                }
                else
                {
                    matself = false;
                }
                if (PhotonNetwork.IsMasterClient)
                {
                    if (Instance.buttonsActive[75])
                    {
                        if (GorillaGameManager.instance != null)
                        {
                            tagcooldown = 999999;
                            GorillaGameManager.instance.GetComponent<GorillaTagManager>().tagCoolDown = 999999;
                        }
                    }
                    else
                    {
                        if (tagcooldown != 5 && !Instance.buttonsActive[77])
                        {
                            tagcooldown = 5;
                            GorillaGameManager.instance.GetComponent<GorillaTagManager>().tagCoolDown = 5;
                        }
                    }

                    if (Instance.buttonsActive[76])
                    {
                        if (GorillaGameManager.instance != null)
                        {
                            GorillaGameManager.instance.GetComponent<GorillaTagManager>().isCurrentlyTag = true;
                        }
                    }

                    if (Instance.buttonsActive[77])
                    {
                        if (GorillaGameManager.instance != null)
                        {
                            tagcooldown = 0;
                            GorillaGameManager.instance.GetComponent<GorillaTagManager>().tagCoolDown = 0;
                        }
                    }
                    else
                    {
                        if (tagcooldown != 5 && !Instance.buttonsActive[75])
                        {
                            tagcooldown = 5;
                            GorillaGameManager.instance.GetComponent<GorillaTagManager>().tagCoolDown = 5;
                        }
                    }

                    // ---- PAGE 14 ---- OP Mods 4

                    if (Instance.buttonsActive[78])
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            acid = false;
                            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
                            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
                            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                            {
                                states[i].touchedLiquid = true;
                                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
                            }
                            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
                        }
                    }
                    else
                    {
                        if (!acid && PhotonNetwork.IsMasterClient)
                        {
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            MainManager.Instance.UnAcidAll();
                            acid = true;
                        }
                    }

                    if (Instance.buttonsActive[79])
                    {
                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                        {
                            acid = false;
                            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
                            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
                            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                            {
                                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
                                states[i].touchedLiquid = PhotonNetwork.PlayerList[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber ? true : states[i].touchedLiquid;
                            }
                            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
                        }
                    }
                    else
                    {
                        if (!acid)
                        {
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            MainManager.Instance.UnAcidSelf();
                            acid = true;
                        }
                    }

                    if (Instance.buttonsActive[80])
                    {
                        MainManager.Instance.SlowAll();
                    }

                    if (Instance.buttonsActive[81]) { MainManager.Instance.SlowGun(); }

                    if (Instance.buttonsActive[82]) { MainManager.Instance.VibrateAll(); }

                    if (Instance.buttonsActive[83]) { MainManager.Instance.VibrateGun(); }
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    if (Instance.buttonsActive[84])
                    {
                        MainManager.Instance.changegamemode("CASUAL");
                        Instance.buttonsActive[84] = false;
                        Destroy(Instance.menu);
                        Instance.menu = null;
                    }

                    if (Instance.buttonsActive[85])
                    {
                        MainManager.Instance.changegamemode("INFECTION");
                        Instance.buttonsActive[85] = false;
                        Destroy(Instance.menu);
                        Instance.menu = null;
                    }

                    if (Instance.buttonsActive[86])
                    {
                        MainManager.Instance.changegamemode("HUNT");
                        Instance.buttonsActive[86] = false;
                        Destroy(Instance.menu);
                        Instance.menu = null;
                    }

                    if (Instance.buttonsActive[87])
                    {
                        MainManager.Instance.changegamemode("BATTLE");
                        Instance.buttonsActive[87] = false;
                        Destroy(Instance.menu);
                        Instance.menu = null;
                    }

                    if (Instance.buttonsActive[88])
                    {
                        PhotonNetwork.CurrentRoom.IsVisible = false;
                        Instance.buttonsActive[88] = false;
                        Destroy(Instance.menu);
                        Instance.menu = null;
                    }

                    if (Instance.buttonsActive[89])
                    {
                        PhotonNetwork.CurrentRoom.IsVisible = true;
                        Instance.buttonsActive[89] = false;
                        Destroy(Instance.menu);
                        Instance.menu = null;
                    }

                    if (Instance.buttonsActive[90])
                    {
                        PhotonNetwork.CurrentRoom.IsOpen = false;
                        AHHH = true;
                    }
                    else
                    {
                        if (AHHH)
                        {
                            PhotonNetwork.CurrentRoom.IsOpen = true;
                            AHHH = false;
                        }
                    }
                }

                if (Instance.buttonsActive[92])
                {
                    string orgstr = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
                    if (orgstr.TryReplace("forest", "", out string rforest))
                    {
                        orgstr = rforest;
                    }
                    if (orgstr.TryReplace("canyon", "", out string rcanton))
                    {
                        orgstr = rcanton;
                    }
                    if (orgstr.TryReplace("city", "", out string rcacnton))
                    {
                        orgstr = rcacnton;
                    }
                    if (orgstr.TryReplace("basement", "", out string rbasement))
                    {
                        orgstr = rbasement;
                    }
                    if (orgstr.TryReplace("clouds", "", out string rbacsement))
                    {
                        orgstr = rbacsement;
                    }
                    if (orgstr.TryReplace("mountain", "", out string r1basement))
                    {
                        orgstr = r1basement;
                    }
                    if (orgstr.TryReplace("beach", "", out string r1bdsement))
                    {
                        orgstr = r1bdsement;
                    }
                    if (orgstr.TryReplace("cave", "", out string r1bdsement1))
                    {
                        orgstr = r1bdsement1;
                    }
                    var hash = new Hashtable
                {
                    { "gameMode", orgstr }
                };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                    PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetCustomPropertiesOfRoom(hash);
                    Instance.buttonsActive[92] = false;
                    Destroy(Instance.menu);
                    Instance.menu = null;
                }
                if (Instance.buttonsActive[91])
                {
                    string orgstr = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
                    for (int i = 0; i < maps.Length; i++)
                    {
                        orgstr.AddIfNot(maps[i], out string res);
                        orgstr = res;
                    }
                    var hash = new Hashtable
                {
                    { "gameMode", orgstr }
                };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                    PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetCustomPropertiesOfRoom(hash);
                    Instance.buttonsActive[91] = false;
                    Destroy(Instance.menu);
                    Instance.menu = null;
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        static string[] maps =
        {
            "beach", "city", "basement", "clouds", "forest", "mountains", "canyon", "cave"
        };
        static float tagcooldown = 5f;
        static bool matself = false;
        public static bool antiban = false;
        static bool AHHH = false;
        static PredictFuturePosition PredictiveLR;
        static bool acid = true;*/
    }

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
                    Testtext.rectTransform.localScale = new Vector3(0.01f, 0.01f, 1f);
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

        public static void SendNotification(string NotificationText)
        {
            try
            {
                if (IsEnabled)
                {
                    if (!NotificationText.Contains("Joined") && !NotificationText.Contains("Left") && !NotificationText.Contains("has"))
                    {
                        NotificationText = "[<color=white>Menu</color>] : " + NotificationText;
                    }
                    else
                    {
                        NotificationText = "[<color=white>ROOM</color>] : " + NotificationText;
                    }
                    if (!NotificationText.Contains(Environment.NewLine)) { NotificationText = NotificationText + Environment.NewLine; }
                    NotifiText.text = NotifiText.text + NotificationText;
                    PreviousNotifi = NotificationText;
                    Testtext.color = Color.white;
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

    internal class Input : MonoBehaviour
    {
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
            type = HeadsetManager.HeadsetType();
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }

        private static bool CalculateGripState(float grabValue, float grabThreshold)
        {
            return grabValue >= grabThreshold;
        }

        public void Update()
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

    internal class ColorChanger : MonoBehaviour
    {
        Renderer renderer;
        public Gradient gradient;

        void Start()
        {
            renderer = GetComponent<Renderer>();
        }

        void Update()
        {
            if (renderer != null)
            {
                float t = Mathf.PingPong(Time.time / 2f, 1f);
                renderer.material.color = gradient.Evaluate(t);
            }

        }
    }
}
