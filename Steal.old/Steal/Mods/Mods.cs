using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.GuidedRefs;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using PlayFab;
using PlayFab.ClientModels;
using Steal.Background;
using Steal.Patchers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GorillaGameModes;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.UI.GridLayoutGroup;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;
using Object = UnityEngine.Object;

namespace Steal.Mods
{
    public class Mods : MonoBehaviour
    {
        private static GameObject LeftPlat;
        private static GameObject RightPlat;
        private GameObject[] LeftPlat_Networked = new GameObject[9999];
        private GameObject[] RightPlat_Networked = new GameObject[9999];

        public GorillaBattleManager GorillaBattleManager;
        public GorillaHuntManager GorillaHuntManager;
        public GorillaTagManager GorillaTagManager;
        public static GorillaScoreBoard[] boards = null;

        private static GameObject lObject = null;
        private static GameObject rObject = null;
        private static GameObject lastRightPos = null;
        private static GameObject lastLeftPos = null;
        private static bool setRightPosOnce = false;
        private static bool setLeftPosOnce = false;
        private static LineRenderer lString;
        private static LineRenderer rString;

        public static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);

        public static Color platformColor = Color.black;

        public static Mods instance;

        public static List<GameObject> leaves = new List<GameObject>
        {

        };

        public static List<GameObject> cosmetics = new List<GameObject>
        {

        };

        public void Awake()
        {
            instance = this;
        }

      

        public static void FirstPerson()
        {
            GameObject fps = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
            fps.active = !fps.active;
        }

        public static void TeleportGun()
        {
            RaycastHit raycastHit = (RaycastHit)GunLibraries.Shoot(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if ((object)raycastHit == null) return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButtonDown(0))
            {
                TeleportationLib.Teleport(raycastHit.point);
            }
        }

        public static void RopeGun()
        {
            RaycastHit raycastHit = (RaycastHit)GunLibraries.Shoot(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if ((object)raycastHit == null) return;

            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (Time.time > ropetimeout + 0.1f)
                {
                    ropetimeout = Time.time;
                    foreach (GorillaRopeSwing rope in Object.FindObjectsOfType<GorillaRopeSwing>())
                    {
                        Vector3 targetPosition = raycastHit.point;
                        Vector3 ropeToCursor = targetPosition - rope.transform.position;
                        float distanceToCursor = ropeToCursor.magnitude;
                        float speed = 9999;
                        float t = Mathf.Clamp01(speed / distanceToCursor);

                        Vector3 newPosition = rope.transform.position + ropeToCursor.normalized * distanceToCursor * t;
                        Vector3 velocity = (newPosition - rope.transform.position).normalized * speed;
                        RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, velocity, true, null);
                    }
                }
            }
        }

        public static void AirStrike()
        {
            RaycastHit raycastHit = (RaycastHit)GunLibraries.Shoot(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if ((object)raycastHit == null) return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                GorillaLocomotion.Player.Instance.transform.position = raycastHit.point;
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = new Vector3(0f, 55f, 0f);
            }
        }

        public static void AcidSelf()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = new ScienceExperimentManager.PlayerGameState[10];
            states[1].touchedLiquid = true;
            states[1].playerId = PhotonNetwork.LocalPlayer.ActorNumber;
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
            rpcReset();
        }

        public static void UnAcidAll()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                states[i].touchedLiquid = false;
                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
            }
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
            rpcReset();
        }


        public static void UnAcidSelf()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
                states[i].touchedLiquid = PhotonNetwork.PlayerList[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber ? false : states[i].touchedLiquid;
            }
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
            rpcReset();
        }

        public static void ProcessIronMonke()
        {

            if (ControllerInput.RightTrigger)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(
                    new Vector3(25f * GorillaLocomotion.Player.Instance.rightControllerTransform.right.x,
                        25f * GorillaLocomotion.Player.Instance.rightControllerTransform.right.y,
                        25f * GorillaLocomotion.Player.Instance.rightControllerTransform.right.z), ForceMode.Acceleration);
            }

            if (ControllerInput.LeftTrigger)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(
                    new Vector3(25f * GorillaLocomotion.Player.Instance.leftControllerTransform.right.x * -1f,
                        25f * GorillaLocomotion.Player.Instance.leftControllerTransform.right.y * -1f,
                        25f * GorillaLocomotion.Player.Instance.leftControllerTransform.right.z * -1f),
                    ForceMode.Acceleration);
            }
        }


        public static void ESP()
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
                        Object.Destroy(beacon, Time.deltaTime);
                    }
                }
            }
        }

        public static void NoTagOnJoin()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        public static void ResetTexure()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                {
                    Object.Destroy(vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>());
                }

                for (int i = 0; i < bones.Count(); i += 2)
                {
                    if (vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>())
                    {
                        Object.Destroy(vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>());
                    }
                }
            }
            foreach (VRRig vrrig in (VRRig[])UnityEngine.Object.FindObjectsOfType(typeof(VRRig)))
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    vrrig.ChangeMaterialLocal(vrrig.currentMatIndex);
                }
            }
        }

        public static void Beacons()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<BoxCollider>());
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<Collider>());
                    gameObject2.transform.rotation = Quaternion.identity;
                    gameObject2.transform.localScale = new Vector3(0.04f, 200f, 0.04f);
                    gameObject2.transform.position = vrrig.transform.position;
                    gameObject2.GetComponent<MeshRenderer>().material = vrrig.mainSkin.material;
                    UnityEngine.Object.Destroy(gameObject2, Time.deltaTime);
                }
            }
        }

        public static int[] bones = { 4, 3, 5, 4, 19, 18, 20, 19, 3, 18, 21, 20, 22, 21, 25, 21, 29, 21, 31, 29, 27, 25, 24, 22, 6, 5, 7, 6, 10, 6, 14, 6, 16, 14, 12, 10, 9, 7 };


        public static void BoneESP()
        {
            Material material = new Material(Shader.Find("GUI/Text Shader"));
            material.color = new Color(1f, 1f, 1f);

            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig)
                {
                    if (vrrig.mainSkin.material.name.Contains("fected"))
                    {
                        material = new Material(Shader.Find("GUI/Text Shader"));
                        material.color = new Color(1f, 0f, 0f);
                    }
                    else
                    {
                        material = new Material(Shader.Find("GUI/Text Shader"));
                        material.color = Color.green;
                    }


                    LineRenderer a = vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>();
                    if (!a)
                    {
                        a = vrrig.head.rigTarget.gameObject.AddComponent<LineRenderer>();
                    }

                    a.endWidth = 0.03f;
                    a.startWidth = 0.03f;
                    a.material = material;
                    a.SetPosition(0, vrrig.head.rigTarget.transform.position + new Vector3(0, 0.160f, 0));
                    a.SetPosition(1, vrrig.head.rigTarget.transform.position - new Vector3(0, 0.4f, 0));



                    for (int i = 0; i < bones.Count(); i += 2)
                    {
                        LineRenderer r = vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>();
                        if (!r)
                        {
                            r = vrrig.mainSkin.bones[bones[i]].gameObject.AddComponent<LineRenderer>();
                            r.endWidth = 0.03f;
                            r.startWidth = 0.03f;

                        }
                        r.material = material;
                        r.SetPosition(0, vrrig.mainSkin.bones[bones[i]].position);
                        r.SetPosition(1, vrrig.mainSkin.bones[bones[i + 1]].position);
                    }

                }
            }
        }

        public static void Tracers()
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
                    Object.Destroy(lineRenderer, Time.deltaTime);
                }
            }
        }

        public static void BoxESP()
        {
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig != null && !rig.isOfflineVRRig)
                {
                    GameObject go = new GameObject("box");
                    go.transform.position = rig.transform.position;
                    GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(top.GetComponent<BoxCollider>());
                    Destroy(bottom.GetComponent<BoxCollider>());
                    Destroy(left.GetComponent<BoxCollider>());
                    Destroy(right.GetComponent<BoxCollider>());
                    top.transform.SetParent(go.transform);
                    top.transform.localPosition = new Vector3(0f, 1f / 2f - 0.02f / 2f, 0f);
                    top.transform.localScale = new Vector3(1f, 0.02f, 0.02f);
                    bottom.transform.SetParent(go.transform);
                    bottom.transform.localPosition = new Vector3(0f, (0f - 1f) / 2f + 0.02f / 2f, 0f);
                    bottom.transform.localScale = new Vector3(1f, 0.02f, 0.02f);
                    left.transform.SetParent(go.transform);
                    left.transform.localPosition = new Vector3((0f - 1f) / 2f + 0.02f / 2f, 0f, 0f);
                    left.transform.localScale = new Vector3(0.02f, 1f, 0.02f);
                    right.transform.SetParent(go.transform);
                    right.transform.localPosition = new Vector3(1f / 2f - 0.02f / 2f, 0f, 0f);
                    right.transform.localScale = new Vector3(0.02f, 1f, 0.02f);

                    top.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    bottom.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    left.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    right.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");

                    Color Espcolor;

                    if (rig.mainSkin.material.name.Contains("fected"))
                    {
                        Espcolor = Color.red;
                    }
                    else
                    {
                        Espcolor = Color.green;
                    }

                    top.GetComponent<Renderer>().material.color = Espcolor;
                    bottom.GetComponent<Renderer>().material.color = Espcolor;
                    left.GetComponent<Renderer>().material.color = Espcolor;
                    right.GetComponent<Renderer>().material.color = Espcolor;

                    go.transform.LookAt(go.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                    Object.Destroy(go, Time.deltaTime);
                }
            }
        }

        public static void Chams()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer && vrrig.mainSkin.material.name.Contains("fected"))
                {
                    vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                    vrrig.mainSkin.material.color = new Color32(255, 0, 0, 90);
                }
                else if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                    vrrig.mainSkin.material.color = new Color32(0, 255, 0, 90);
                }
            }
        }

        static float additiveOffset = 0;
        static float longarmsChangeTimer = -10;

        public static void LongArms(bool enabled)
        {
            if (!enabled)
            {
                GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(0f, -0.02f, 0f);
                GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0f, -0.02f, 0f);
                return;
            }
            if (ControllerInput.LeftTrigger || ControllerInput.RightTrigger)
            {
                if (Time.time > longarmsChangeTimer)
                {
                    additiveOffset += -0.3f;
                    longarmsChangeTimer = Time.time + 0.2f;
                }
            }
            GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(0f, -0.02f + additiveOffset, 0f);
            GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0f, -0.02f + additiveOffset, 0f);
            GorillaLocomotion.Player.Instance.maxArmLength = 70f;
        }

        public static void UpAndDown()
        {
            if (ControllerInput.LeftTrigger)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(new Vector3(0f, 300f, 0f), ForceMode.Acceleration);
            }

            if (ControllerInput.RightTrigger)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(new Vector3(0f, -300f, 0f), ForceMode.Acceleration);
            }
        }

        public static void SpiderMonke(bool enable)
        {
            if (!enable)
            {
                Object.Destroy(rObject);
                rObject = null;
                Object.Destroy(lastRightPos);
                lastRightPos = null;
                setRightPosOnce = false;
                Object.Destroy(lObject);
                lObject = null;
                Object.Destroy(lastLeftPos);
                lastLeftPos = null;
                setLeftPosOnce = false;
            }
            else
            {
                RaycastHit raycastHit;
                Physics.Raycast(GorillaLocomotion.Player.Instance.leftControllerTransform.position + GorillaLocomotion.Player.Instance.leftControllerTransform.forward, GorillaLocomotion.Player.Instance.leftControllerTransform.forward, out raycastHit);
                RaycastHit raycastHit2;
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position + GorillaLocomotion.Player.Instance.rightControllerTransform.forward, GorillaLocomotion.Player.Instance.rightControllerTransform.forward, out raycastHit2);
                if (lObject == null)
                {
                    lObject = new GameObject("leftObject");
                    lString = lObject.AddComponent<LineRenderer>();
                    lString.startWidth = 0.04f;
                    lString.endWidth = 0.04f;
                    lString.material.shader = GorillaTagger.Instance.offlineVRRig.mainSkin.material.shader;
                    lString.endColor = Color.white;
                    lString.startColor = Color.white;
                }
                if (rObject == null)
                {
                    rObject = new GameObject("rightObject");
                    rString = rObject.AddComponent<LineRenderer>();
                    rString.startWidth = 0.04f;
                    rString.endWidth = 0.04f;
                    rString.material.shader = GorillaTagger.Instance.offlineVRRig.mainSkin.material.shader;
                    rString.endColor = Color.white;
                    rString.startColor = Color.white;
                }
                if (ControllerInput.RightGrip)
                {
                    if (setRightPosOnce)
                    {
                        rString.SetPosition(0, raycastHit2.point);
                        lastRightPos = GameObject.CreatePrimitive(0);
                        Object.Destroy(lastRightPos.GetComponent<Rigidbody>());
                        Object.Destroy(lastRightPos.GetComponent<SphereCollider>());
                        Object.Destroy(lastRightPos.GetComponent<Renderer>());
                        lastRightPos.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                        lastRightPos.transform.position = raycastHit2.point;
                        setRightPosOnce = true;
                    }
                    lastRightPos.transform.LookAt(GorillaLocomotion.Player.Instance.rightControllerTransform.position);
                    rString.SetPosition(1, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
                    GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(-lastRightPos.transform.forward * 1800f * Time.deltaTime, UnityEngine.ForceMode.Acceleration);
                }
                else
                {
                    Object.Destroy(rObject);
                    rObject = null;
                    Object.Destroy(rString);
                    rString = null;
                    Object.Destroy(lastRightPos);
                    lastRightPos = null;
                    setRightPosOnce = false;
                }
                if (ControllerInput.LeftGrip)
                {
                    if (setLeftPosOnce)
                    {
                        lString.SetPosition(0, raycastHit.point);
                        lastLeftPos = GameObject.CreatePrimitive(0);
                        Object.Destroy(lastLeftPos.GetComponent<Rigidbody>());
                        Object.Destroy(lastLeftPos.GetComponent<SphereCollider>());
                        Object.Destroy(lastLeftPos.GetComponent<Renderer>());
                        lastLeftPos.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                        lastLeftPos.transform.position = raycastHit.point;
                        setLeftPosOnce = true;
                    }
                    lastLeftPos.transform.LookAt(GorillaLocomotion.Player.Instance.leftControllerTransform.position);
                    lString.SetPosition(1, GorillaLocomotion.Player.Instance.leftControllerTransform.position);
                    GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(-lastLeftPos.transform.forward * 1800f * Time.deltaTime, UnityEngine.ForceMode.Acceleration);
                }
                else
                {
                    Object.Destroy(lObject);
                    lObject = null;
                    Object.Destroy(lString);
                    lString = null;
                    Object.Destroy(lastLeftPos);
                    lastLeftPos = null;
                    setLeftPosOnce = false;
                }
            }
        }

        public static void Platforms()
        {
            RaiseEventOptions safs = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others
            };
            if (ControllerInput.RightGrip)
            {
                if (RightPlat == null)
                {
                    RightPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    RightPlat.transform.position = new Vector3(0f, -0.0175f, 0f) + GorillaLocomotion.Player.Instance.rightControllerTransform.position;
                    RightPlat.transform.rotation = GorillaLocomotion.Player.Instance.rightControllerTransform.rotation;
                    RightPlat.transform.localScale = scale;

                    RightPlat.GetComponent<Renderer>().material.color = SettingsLib.ConvertColors[SettingsLib.Settings.PlatformColors];
                    PhotonNetwork.RaiseEvent(110, new object[] { RightPlat.transform.position, RightPlat.transform.rotation, scale, RightPlat.GetComponent<Renderer>().material.color }, safs, SendOptions.SendReliable);
                }
            }
            else
            {
                if (RightPlat != null)
                {
                    PhotonNetwork.RaiseEvent(111, null, safs, SendOptions.SendReliable);
                    Object.Destroy(RightPlat);
                    RightPlat = null;
                }
            }
            if (ControllerInput.LeftGrip)
            {
                if (LeftPlat == null)
                {
                    LeftPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    LeftPlat.transform.localScale = scale;
                    LeftPlat.transform.position = new Vector3(0f, -0.0175f, 0f) + GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                    LeftPlat.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
                    LeftPlat.GetComponent<Renderer>().material.color = SettingsLib.ConvertColors[SettingsLib.Settings.PlatformColors];
                    PhotonNetwork.RaiseEvent(120, new object[] { LeftPlat.transform.position, LeftPlat.transform.rotation, scale, RightPlat.GetComponent<Renderer>().material.color }, safs, SendOptions.SendReliable);
                }
            }
            else
            {
                if (LeftPlat != null)
                {
                    PhotonNetwork.RaiseEvent(121, null, safs, SendOptions.SendReliable);
                    Object.Destroy(LeftPlat);
                    LeftPlat = null;
                }
            }
        }

        public void PlatformNetwork(EventData data)
        {
            if (data.Code == 110)
            {
                object[] customshit = (object[])data.CustomData;
                RightPlat_Networked[data.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightPlat_Networked[data.Sender].transform.position = (Vector3)customshit[0];
                RightPlat_Networked[data.Sender].transform.rotation = (Quaternion)customshit[1];
                RightPlat_Networked[data.Sender].transform.localScale = (Vector3)customshit[2];
                RightPlat_Networked[data.Sender].GetComponent<Renderer>().material.color = (Color)customshit[3];
                RightPlat_Networked[data.Sender].GetComponent<BoxCollider>().enabled = false;
            }
            if (data.Code == 120)
            {
                object[] customshit = (object[])data.CustomData;
                LeftPlat_Networked[data.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftPlat_Networked[data.Sender].transform.position = (Vector3)customshit[0];
                LeftPlat_Networked[data.Sender].transform.rotation = (Quaternion)customshit[1];
                LeftPlat_Networked[data.Sender].transform.localScale = (Vector3)customshit[2];
                LeftPlat_Networked[data.Sender].GetComponent<Renderer>().material.color = (Color)customshit[3];
                LeftPlat_Networked[data.Sender].GetComponent<BoxCollider>().enabled = false;
            }
            if (data.Code == 110)
            {
                Destroy(RightPlat_Networked[data.Sender]);
                RightPlat_Networked[data.Sender] = null;
            }
            if (data.Code == 121)
            {
                Destroy(LeftPlat_Networked[data.Sender]);
                LeftPlat_Networked[data.Sender] = null;
            }
        }


        public static void SpeedBoost()
        {
            GorillaLocomotion.Player.Instance.maxJumpSpeed = SettingsLib.Settings.speedBoost;
        }

        public static void ResetSpeed()
        {
            GorillaLocomotion.Player.Instance.maxJumpSpeed = 1;
        }
        static float LavaTimer;
        static bool canchangelava;
        public static void SpazLava()
        {
            if (Time.time > LavaTimer)
            {
                if (canchangelava)
                {
                    RiseLava();
                    canchangelava = false;
                }
                else
                {
                    DrainLava();
                    canchangelava = true;
                }
                rpcReset();
                LavaTimer = Time.time + 0.2f;
            }
        }

        public static void MimicVoiceGun()
        {
            //manager.GetComponent<Photon.Voice.Unity.Recorder>();
        }
        static float boardTimer = -0.5f;
        static float OBTimer = -0.5f;
        static int plaaaaaa = 0;
        public static void SpeedBoostTest()
        {
            if (Time.time - OBTimer >= (1 / 20))
            {
                OBTimer = Time.time;
                for (plaaaaaa = 0; plaaaaaa < PhotonNetwork.PlayerListOthers.Length; plaaaaaa++)
                {
                    GorillaGameManager.instance.GetComponent<GorillaTagManager>().ChangeCurrentIt(PhotonNetwork.PlayerListOthers[plaaaaaa]);
                }
                if (plaaaaaa > PhotonNetwork.PlayerListOthers.Length)
                {
                    plaaaaaa = 0;
                }
            }
        }

        public static void ChangeAllNames()
        {
            if (Time.time > boardTimer + 0.2f)
            {
                boardTimer = Time.time;
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable[byte.MaxValue] = PhotonNetwork.LocalPlayer.NickName;
                    Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
                    dictionary.Add(251, hashtable);
                    dictionary.Add(254, p.ActorNumber);
                    dictionary.Add(250, true);
                    PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer.SendOperation(252, dictionary, SendOptions.SendUnreliable);
                    rpcReset();
                }
            }
        }

        public static void AudioCrash()
        {
            Recorder recorder = GameObject.Find("Photon Manager").GetComponent<Recorder>();

            recorder.SourceType = Recorder.InputSourceType.AudioClip;
            recorder.AudioClip = GameObject.Find("SoundPostForest").GetComponent<SynchedMusicController>().audioSource.clip;
            recorder.AudioClip = GameObject.Find("SoundPostForest").GetComponent<SynchedMusicController>().audioSource.clip;
            recorder.AudioClip = GameObject.Find("SoundPostForest").GetComponent<SynchedMusicController>().audioSource.clip;
            recorder.AudioClip = GameObject.Find("SoundPostForest").GetComponent<SynchedMusicController>().audioSource.clip;
            recorder.AudioClip = GameObject.Find("SoundPostForest").GetComponent<SynchedMusicController>().audioSource.clip;
            recorder.AudioClip = GameObject.Find("SoundPostForest").GetComponent<SynchedMusicController>().audioSource.clip;
            recorder.AutoStart = true;
            recorder.LoopAudioClip = true;

            recorder.SamplingRate = POpusCodec.Enums.SamplingRate.Sampling48000;
            recorder.Bitrate = 12000;
            recorder.ReliableMode = false;
            recorder.FrameDuration = Photon.Voice.OpusCodec.FrameDuration.Frame60ms;
            if (recorder.RequiresRestart || recorder.RequiresInit)
            {
                recorder.RestartRecording(true);

                recorder.ResetLocalAudio();
            }
        }

        public static void Flight()
        {
            if (ControllerInput.RightPrimary)
            {
                GorillaLocomotion.Player.Instance.transform.position += (GorillaLocomotion.Player.Instance.headCollider.transform.forward * Time.deltaTime) * SettingsLib.Settings.flightSpeed;
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }


        public static void Slingshot()
        {
            if (ControllerInput.RightTrigger)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 15, ForceMode.Acceleration);
            }
        }

        public static void CreatePublicRoom()
        {
            Hashtable customRoomProperties;
            if (PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "city" && PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "basement")
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + GorillaComputer.instance.currentGameMode
                    }
                };
            }
            else
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + "CASUAL"
                    }
                };
            }
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = PhotonNetworkController.Instance.GetRoomSize(PhotonNetworkController.Instance.currentJoinTrigger.gameModeName);
            roomOptions.CustomRoomProperties = customRoomProperties;
            roomOptions.PublishUserId = true;
            roomOptions.CustomRoomPropertiesForLobby = new string[]
            {
                "gameMode"
            };
            PhotonNetwork.CreateRoom(RandomRoomName(), roomOptions, null, null);
        }

        public static void WallWalk()
        {
            RaycastHit Left;
            RaycastHit Right;
            Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.right, out Left, 100f, int.MaxValue);
            Physics.Raycast(GorillaLocomotion.Player.Instance.leftControllerTransform.position, GorillaLocomotion.Player.Instance.leftControllerTransform.right, out Right, 100f, int.MaxValue);

            if (ControllerInput.RightGrip)
            {
                if (Left.distance < Right.distance)
                {
                    if (Left.distance < 1)
                    {
                        Vector3 gravityDirection = (Left.point - GorillaLocomotion.Player.Instance.rightControllerTransform.position).normalized;
                        Physics.gravity = gravityDirection * SettingsLib.Settings.WallWalkGravity;
                    }
                    else
                    {
                        Physics.gravity = new Vector3(0, SettingsLib.Settings.WallWalkGravity, 0);
                    }
                }
                if (Left.distance == Right.distance)
                {
                    Physics.gravity = new Vector3(0, SettingsLib.Settings.WallWalkGravity, 0);
                }
            }

            if (ControllerInput.LeftGrip)
            {
                if (Left.distance > Right.distance)
                {
                    if (Right.distance < 1)
                    {
                        Vector3 gravityDirection = (Right.point - GorillaLocomotion.Player.Instance.leftControllerTransform.position).normalized;
                        Physics.gravity = gravityDirection * SettingsLib.Settings.WallWalkGravity;
                    }
                    else
                    {
                        Physics.gravity = new Vector3(0, SettingsLib.Settings.WallWalkGravity, 0);
                    }
                }
                if (Left.distance == Right.distance)
                {
                    Physics.gravity = new Vector3(0, SettingsLib.Settings.WallWalkGravity, 0);
                }
            }

            if (!ControllerInput.LeftGrip && !ControllerInput.RightGrip)
            {
                Physics.gravity = new Vector3(0, -9.81f, 0);
            }

        }

        public static string[] moderatorIds =
        {
            "C3878B068886F6C3",
            "AAB44BFD0BA34829",
            "61AD990FF3A423B7",
            "BC99FA914F506AB8",
            "3A16560CA65A51DE",
            "59F3FE769DE93AB9",
            "ECDE8A2FF8510934",
            "F5B5C64914C13B83",
            "80279945E7D3B57D",
            "EE9FB127CF7DBBD5",
            "2E408ED946D55D51",
            "BC9764E1EADF8BE0",
            "7E44E8337DF02CC1",
            "42C809327652ECDD",
            "7F31BEEC604AE18B",
            "1D6E20BE9655C798",
            "22A7BCEFFD7A0BBA",
            "6F79BE7CB34642AC",
            "CBCCBBB6C28A94CF",
            "5B5536D4434DDC0F",
            "54DCB69545BE0800",
            "D0CB396539676DD8",
            "608E4B07DBEFC690",
            "A04005517920EB0",
            "5AA1231973BE8A62",
            "9DBC90CF7449EF64",
            "6FE5FF4D5DF68843",
            "52529F0635BE0CDF",
            "D345FE394607F946",
            "6713DA80D2E9BFB5",
            "498D4C2F23853B37",
            "6DC06EEFFE9DBD39",
            "E354E818871BD1D8",
            "A6FFC7318E1301AF",
            "D6971CA01F82A975",
            "458CCE7845335ABF",
            "28EA953654FF2E79",
            "A1A99D33645E4A94",
            "CA8FDFF42B7A1836"
        };

        internal static Vector3 previousMousePosition;

        public static void AdvancedWASD(float speed)
        {
            GorillaTagger.Instance.rigidbody.velocity = new Vector3(0, 0.0735f, 0);
            float NSpeed = speed * Time.deltaTime;
            if (UnityInput.Current.GetKey(KeyCode.LeftShift) || UnityInput.Current.GetKey(KeyCode.RightShift))
            {
                NSpeed *= 10f;
            }
            if (UnityInput.Current.GetKey(KeyCode.LeftArrow) || UnityInput.Current.GetKey(KeyCode.A))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.right * -1f * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.RightArrow) || UnityInput.Current.GetKey(KeyCode.D))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.right * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.UpArrow) || UnityInput.Current.GetKey(KeyCode.W))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.forward * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.DownArrow) || UnityInput.Current.GetKey(KeyCode.S))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.forward * -1f * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.Space) || UnityInput.Current.GetKey(KeyCode.PageUp))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.up * NSpeed;
            }
            if (UnityInput.Current.GetKey(KeyCode.LeftControl) || UnityInput.Current.GetKey(KeyCode.PageDown))
            {
                GorillaLocomotion.Player.Instance.transform.position += Camera.main.transform.up * -1f * NSpeed;
            }
            if (UnityInput.Current.GetMouseButton(1))
            {
                Vector3 val = UnityInput.Current.mousePosition - previousMousePosition;
                float num2 = GorillaLocomotion.Player.Instance.transform.localEulerAngles.y + val.x * 0.3f;
                float num3 = GorillaLocomotion.Player.Instance.transform.localEulerAngles.x - val.y * 0.3f;
                GorillaLocomotion.Player.Instance.transform.localEulerAngles = new Vector3(num3, num2, 0f);
            }
            previousMousePosition = UnityInput.Current.mousePosition;
        }

        private static string[] GetPlayerIds(Photon.Realtime.Player[] players)
        {
            string[] playerIds = new string[players.Length];

            for (int i = 0; i < players.Length; i++)
            {
                playerIds[i] = players[i].UserId;
            }

            return playerIds;
        }

        public static void ChangeIdentity()
        {
            string randomName = "";
            for (var i = 0; i < 12; i++)
            {
                randomName = randomName + UnityEngine.Random.Range(0, 9).ToString();
            }

            GorillaComputer.instance.offlineVRRigNametagText.text = randomName;
            GorillaComputer.instance.currentName = randomName;
            GorillaComputer.instance.savedName = randomName;
            PhotonNetwork.LocalPlayer.NickName = randomName;
            byte randA = (byte)UnityEngine.Random.Range(0, 255);
            byte randB = (byte)UnityEngine.Random.Range(0, 255);
            byte randC = (byte)UnityEngine.Random.Range(0, 255);
            Color colortochange = (new Color32(randA, randB, randC, 255));
        //    GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g, colortochange.b, false);
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
            {
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.Others, colortochange.r, colortochange.g, colortochange.b, false);
            }
            else
            {
                GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Add(PhotonNetwork.LocalPlayer
                    .UserId);
                ChangeIdentity();
            }
        }

        public static void ChangeRandomIdentity()
        {

            int numba = UnityEngine.Random.Range(0, PhotonNetwork.PlayerListOthers.Length);

            Player subject = PhotonNetwork.PlayerListOthers[numba];
            string randomName = subject.NickName;


            GorillaComputer.instance.offlineVRRigNametagText.text = randomName;
            GorillaComputer.instance.currentName = randomName;
            GorillaComputer.instance.savedName = randomName;
            PhotonNetwork.LocalPlayer.NickName = randomName;

            Color colortochange = GorillaGameManager.instance.FindPlayerVRRig(subject).mainSkin.material.color;
         //   GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g, colortochange.b, false);
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
            {
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.Others, colortochange.r, colortochange.g, colortochange.b, false);
            }
            else
            {
                GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Add(PhotonNetwork.LocalPlayer
                   .UserId);
                ChangeRandomIdentity();
            }
        }


        public static void DodgeModerators()
        {

            bool anyMatch = moderatorIds.Any(item => GetPlayerIds(PhotonNetwork.PlayerListOthers).Contains(item));

            if (anyMatch)
            {
                Notif.SendNotification("<color=red>[AUTODODGE]</color> Moderator Found Disconnected Successfully");
                PhotonNetwork.Disconnect();
            }
        }

        public static void Freecam()
        {
            AdvancedWASD(StealGUI.speed);
        }

        public static void ResetFreecamSpeed()
        {
            StealGUI.speed = 10f;
        }

        public static bool wackstart = false;

        static bool ghostToggled = false;
        public static void GhostMonkey()
        {
            if (!XRSettings.isDeviceActive)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                return;
            }
            if (ControllerInput.RightPrimary)
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

        public static void InvisMonkey()
        {
            if (!XRSettings.isDeviceActive)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }

                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = new Vector3(999, 999, 999);
                return;
            }
            if (ControllerInput.RightPrimary)
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


        public static void RigGun()
        {
            RaycastHit raycastHit = (RaycastHit)GunLibraries.Shoot(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if ((object)raycastHit == null) return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }
                GorillaTagger.Instance.offlineVRRig.enabled = false;

                GorillaTagger.Instance.offlineVRRig.transform.position = raycastHit.point + new Vector3(0, 0.6f, 0);
            }
        }

        public static void RemoveLeaves()
        {
            foreach (GameObject g in leaves)
            {
                g.SetActive(false);
            }
        }

        public static void DisableCosmetics()
        {
            Transform transform = GorillaTagger.Instance.offlineVRRig.mainCamera.transform.Find("Cosmetics");
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject v = transform.GetChild(i).gameObject;
                if (v.activeSelf)
                {
                    v.SetActive(false);
                    cosmetics.Add(v);
                }
            }
        }

        public static void ResetHead()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.x = 0f;
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 0f;
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.z = 0f;
        }

        public static void HeadUpsideDown()
        {
            GorillaTagger.Instance.offlineVRRig.head.rigTarget.eulerAngles = new Vector3(0, 0f, 180);
        }

        public static void SpinHeadX()
        {
            GorillaTagger.Instance.offlineVRRig.head.rigTarget.eulerAngles += new Vector3(0f, 10f, 0f);
        }

        public static void FixHandTaps()
        {
            GorillaTagger.Instance.handTapVolume = 0.1f;
        }

        public static void LoudHandTaps()
        {
            GorillaTagger.Instance.handTapVolume = int.MaxValue;
        }

        public static void SilentHandTaps()
        {
            GorillaTagger.Instance.handTapVolume = 0;
        }

        public static void EnableInstantHandTaps()
        {
            GorillaTagger.Instance.tapCoolDown = 0f;
        }

        public static void DisableInstantHandTaps()
        {
            GorillaTagger.Instance.tapCoolDown = 0.33f;
        }
        public static void SetFPS(int limit)
        {
            Application.targetFrameRate = limit;
        }

        public static void ImpossibleColors()
        {
            PlayerPrefs.SetFloat("redValue", -2147483648);
            PlayerPrefs.SetFloat("greenValue", -2147483648);
            PlayerPrefs.SetFloat("blueValue", -2147483648);

            GorillaTagger.Instance.UpdateColor(-2147483648, -2147483648, -2147483648);
            PlayerPrefs.Save();
            GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.All, new object[] { -2147483648, -2147483648, -2147483648, false });
        }

        public static void SpinHeadY()
        {
            GorillaTagger.Instance.offlineVRRig.head.rigTarget.eulerAngles += new Vector3(10, 0f, 0f);
        }

        public static void SpinHeadZ()
        {
            GorillaTagger.Instance.offlineVRRig.head.rigTarget.eulerAngles += new Vector3(0, 0f, 10f);
        }

        public static void NightTime()
        {
            BetterDayNightManager.instance.SetTimeOfDay(0);
        }

        public static void DayTime()
        {
            BetterDayNightManager.instance.SetTimeOfDay(1);
        }

        public static void EnableCosmetics()
        {
            foreach (GameObject c in cosmetics)
            {
                c.SetActive(true);
            }
            cosmetics.Clear();
        }


        public static void AddLeaves()
        {
            foreach (GameObject l in leaves)
            {
                l.SetActive(true);
            }
        }

        static GameObject pointer;
        public static void CopyGun()
        {
            if (PhotonNetwork.CurrentRoom == null) { return; }
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer != null)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }
                GorillaTagger.Instance.offlineVRRig.enabled = false;

                GorillaTagger.Instance.offlineVRRig.transform.position = GunLibraries.lockedOnPlayer.transform.position;

                GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = GunLibraries.lockedOnPlayer.rightHand.rigTarget.transform.position;
                GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = GunLibraries.lockedOnPlayer.leftHand.rigTarget.transform.position;

                GorillaTagger.Instance.offlineVRRig.transform.rotation = GunLibraries.lockedOnPlayer.transform.rotation;

                GorillaTagger.Instance.offlineVRRig.head.rigTarget.rotation = GunLibraries.lockedOnPlayer.head.rigTarget.rotation;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }


        public static void TagGun(bool isMaster)
        {
            if (PhotonNetwork.CurrentRoom == null) { return; }
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                instance.saveKeys();
                if (!isMaster)
                {
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        GorillaTagger.Instance.offlineVRRig.transform.position = GunLibraries.lockedOnPlayer.transform.position;
                        ProcessTagAura(GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner);
                    }
                    if (GetGameMode().Contains("HUNT"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget == GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner)
                        {
                            GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).transform.position;
                            ProcessTagAura(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget);
                            return;
                        }
                    }
                    if (GetGameMode().Contains("BATTLE"))
                    {
                        if (instance.GorillaBattleManager.playerLives[GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner.ActorNumber] > 0)
                        {
                            PhotonView pv = GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)").GetComponent<PhotonView>();
                            pv.RPC("ReportSlingshotHit", RpcTarget.MasterClient, new object[] { new Vector3(0, 0, 0), GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner, RPCSUB.IncrementLocalPlayerProjectileCount() });

                        }
                    }
                }
                else
                {
                    Player player = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        if (!instance.GorillaTagManager.currentInfected.Contains(player))
                        {
                            instance.GorillaTagManager.currentInfected.Add(player);
                        }
                    }
                    if (GetGameMode().Contains("HUNT"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget == GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner)
                        {
                            GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).transform.position;
                            ProcessTagAura(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget);
                            return;
                        }
                    }

                    if (GetGameMode().Contains("BATTLE"))
                    {
                        if (instance.GorillaBattleManager.playerLives[GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner.ActorNumber] > 0)
                        {
                            instance.GorillaBattleManager.playerLives[GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner.ActorNumber] = 0;
                        }
                    }
                }
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }


        public static void SetNoclip(bool StateDepender)
        {
            foreach (MeshCollider meshCollider in Object.FindObjectsOfType<MeshCollider>())
            {
                meshCollider.enabled = !StateDepender;
            }
        }

        public static void NoClip(bool enable)
        {
            if (enable)
            {
                SetNoclip(ControllerInput.LeftTrigger);
            }
            else
            {
                SetNoclip(false);
            }
        }


        public static string GetGameMode()
        {
            string gamemode = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
            if (gamemode.Contains("INFECTION"))
            {
                return "INFECTION";
            }
            else if (gamemode.Contains("HUNT"))
            {
                return "HUNT";
            }
            else if (gamemode.Contains("BATTLE"))
            {
                return "BATTLE";
            }
            else if (gamemode.Contains("CASUAL"))
            {
                return "CASUAL";
            }
            return "ERROR";
        }

        public static VRRig GetClosestUntagged()
        {
            VRRig vrrig = null;
            float num = float.MaxValue;
            foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
            {
                float num2 = Vector3.Distance(vrrig2.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                bool flag = num2 < num && !vrrig2.mainSkin.material.name.Contains("fected") && vrrig2 != GorillaTagger.Instance.offlineVRRig;
                if (flag)
                {
                    vrrig = vrrig2;
                    num = num2;
                }
            }
            return vrrig;
        }

        public static VRRig GetClosestTagged()
        {
            VRRig vrrig = null;
            float num = float.MaxValue;
            foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
            {
                float num2 = Vector3.Distance(vrrig2.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                bool flag = num2 < num && vrrig2.mainSkin.material.name.Contains("fected") && vrrig2 != GorillaTagger.Instance.offlineVRRig;
                if (flag)
                {
                    vrrig = vrrig2;
                    num = num2;
                }
            }
            return vrrig;
        }

        public static VRRig GetClosest()
        {
            VRRig vrrig = null;
            float num = float.MaxValue;
            foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
            {
                float num2 = Vector3.Distance(vrrig2.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                if (num2 < num && vrrig2 != GorillaTagger.Instance.offlineVRRig)
                {
                    vrrig = vrrig2;
                    num = num2;
                }
            }
            return vrrig;
        }

        public static void AntiTag()
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected.Remove(PhotonNetwork.LocalPlayer);
                }
                if (Vector3.Distance(GetClosestTagged().transform.position, GorillaLocomotion.Player.Instance.headCollider.transform.position) < 3.5f && !Physics.Linecast(GorillaLocomotion.Player.Instance.transform.position, GetClosestTagged().transform.position, LayerMask.NameToLayer("Gorilla Tag Collider")))
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.Player.Instance.headCollider.transform.position - new Vector3(0f, 20f, 0f);
                    return;
                }
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
            GorillaTagger.Instance.offlineVRRig.enabled = true;
        }


        public static void FreezeAura()
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected.Remove(PhotonNetwork.LocalPlayer);
                }
                else if (Vector3.Distance(GetClosest().transform.position, GorillaLocomotion.Player.Instance.headCollider.transform.position) < 3.5f && !Physics.Linecast(GorillaLocomotion.Player.Instance.transform.position, GetClosestTagged().transform.position, LayerMask.NameToLayer("Gorilla Tag Collider")))
                {
                    if (Time.time > slowallcooldown + 0.2)
                    {
                        if (GetPhotonViewFromRig(GetClosest()) == null) { return; }
                        crashtimeout = Time.time;
                        Player player = GetPhotonViewFromRig(GetClosest()).Owner;
                        MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                        object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { player.ActorNumber });
                        method.Invoke(typeof(PhotonNetwork), new object[1] { player.ActorNumber });
                        slowallcooldown = Time.time;
                    }
                    return;
                }
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
            GorillaTagger.Instance.offlineVRRig.enabled = true;
        }


        public static void FasterSwimming()
        {
            if (GorillaLocomotion.Player.Instance.InWater)
            {
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity * 1.013f;
            }
        }


        static bool canBHop = false;
        static bool isBHop = false;

        static float RG;
        static float LG;
        static bool RGrabbing;
        static bool LGrabbing;
        static bool AirMode;
        private static void ApplyVelocity(Vector3 pos, Vector3 target, GorillaLocomotion.Player __instance)
        {
            Physics.gravity = new Vector3(0, 0, 0);
            Vector3 a = target - pos;
            __instance.bodyCollider.attachedRigidbody.velocity = a * 65f;
        }
        static Vector3 CurHandPos;

        static int layers = int.MaxValue;

        static LineRenderer lr;

        static LineRenderer lineRenderer;

        static bool disablegrapple = false;

        static RaycastHit hit;

        private static bool C4Spawned = false;

        private static GameObject C4;


        public static Vector3 GetMiddle(Vector3 vector)
        {
            return new Vector3(vector.x / 2f, vector.y / 2f, vector.z / 2f);
        }


        static float splashtimeout;

        public static void SpashGun()
        {
            if (PhotonNetwork.CurrentRoom == null) { return; }
            RaycastHit raycastHit = (RaycastHit)GunLibraries.Shoot(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if ((object)raycastHit == null) return;

            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }

                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = pointer.transform.position + new Vector3(0, 0.5f, 0);
                if (Time.time > splashtimeout + 0.5f)
                {
                    GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", 0, new object[]
                        {
                            pointer.transform.position,
                            UnityEngine.Random.rotation,
                            400f,
                            100f,
                            false,
                            true
                        });
                    splashtimeout = Time.time;
                }

                return;
            }
            else
            {

                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
            return;
        }



        public static void RGB()
        {
            GradientColorKey[] colorkeys = new GradientColorKey[7];
            colorkeys[0].color = Color.red;
            colorkeys[0].time = 0f;
            colorkeys[1].color = Color.yellow;
            colorkeys[1].time = 0.2f;
            colorkeys[2].color = Color.green;
            colorkeys[2].time = 0.3f;
            colorkeys[3].color = Color.cyan;
            colorkeys[3].time = 0.5f;
            colorkeys[4].color = Color.blue;
            colorkeys[4].time = 0.6f;
            colorkeys[5].color = Color.magenta;
            colorkeys[5].time = 0.8f;
            colorkeys[6].color = Color.red;
            colorkeys[6].time = 1f;
            Gradient gradient = new Gradient();
            gradient.colorKeys = colorkeys;
            float t = Mathf.PingPong(Time.time / 2f, 1f);
            var colortochange = gradient.Evaluate(t);

            //GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g, colortochange.b, false);
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
            {
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.Others, colortochange.r, colortochange.g, colortochange.b, false);
            }
        }
        static float colorFloat = 10f;
        public static void Strobe()
        {
            // colorFloat = Mathf.Repeat(colorFloat + Time.deltaTime * 40f, 50f);
            // float r = Mathf.Cos(colorFloat * Mathf.PI * 2f) * 0.5f + 0.5f;
            // float g = Mathf.Sin(colorFloat * Mathf.PI * 2f) * 0.5f + 0.5f;
            // float b = Mathf.Cos(colorFloat * Mathf.PI * 2f + Mathf.PI / 2f) * 0.5f + 0.5f;
            // GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(r, g, b, false);
            // if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
            // {
            //     GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.Others, r, g, b, false);
            // }
        }


        public static void BigMonke()
        {
            GorillaLocomotion.Player.Instance.transform.localScale = new Vector3(4f, 4f, 4f);
        }

        public static void ResetMonke()
        {
            GorillaLocomotion.Player.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
            GorillaTagger.Instance.offlineVRRig.enabled = true;

        }

        public static void SizeableSplash()
        {
            if (ControllerInput.RightTrigger)
            {
                if (Time.time > splashtimeout + 0.5)
                {
                    GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", 0, new object[]
                    {
                      GetMiddle(GorillaLocomotion.Player.Instance.rightControllerTransform.position + GorillaLocomotion.Player.Instance.leftControllerTransform.position),
                      UnityEngine.Random.rotation,
                      Vector3.Distance(GorillaLocomotion.Player.Instance.rightControllerTransform.position, GorillaLocomotion.Player.Instance.leftControllerTransform.position),
                      Vector3.Distance(GorillaLocomotion.Player.Instance.rightControllerTransform.position, GorillaLocomotion.Player.Instance.leftControllerTransform.position),
                      false,
                      true
                    });
                    splashtimeout = Time.time;
                }
            }
        }

        public static void Splash()
        {
            if (Time.time > splashtimeout + 0.5f)
            {
                if (ControllerInput.RightTrigger)
                {
                    splashtimeout = Time.time;
                    GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", RpcTarget.All, new object[]
                    {
                      GorillaLocomotion.Player.Instance.rightControllerTransform.position,
                      UnityEngine.Random.rotation,
                      400f,
                      100f,
                      false,
                      true
                    });
                }

                if (ControllerInput.LeftTrigger)
                {
                    splashtimeout = Time.time;
                    GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", RpcTarget.All, new object[]
                    {
                      GorillaLocomotion.Player.Instance.leftControllerTransform.position,
                      UnityEngine.Random.rotation,
                      400f,
                      100f,
                      false,
                      true
                    });
                }
            }
        }


        public static void ProcessCheckPoint()
        {
            if (ControllerInput.RightGrip)
            {
                if (pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }
                pointer.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.position;
            }
            if (!ControllerInput.RightGrip && ControllerInput.RightTrigger)
            {
                pointer.GetComponent<Renderer>().material.color = Color.green;
                TeleportationLib.Teleport(pointer.transform.position);
            }
            if (!ControllerInput.RightTrigger)
            {
                pointer.GetComponent<Renderer>().material.color = Color.red;
            }
        }

        public static void MagicMonkey()
        {
            if (ControllerInput.RightGrip && !ControllerInput.RightTrigger)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position - GorillaLocomotion.Player.Instance.rightControllerTransform.up, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out raycastHit) && pointer == null)
                {
                    pointer.GetComponent<Renderer>().material.color = Color.red;
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                    pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }
                pointer.transform.position = raycastHit.point;
                pointer.GetComponent<Renderer>().material.color = Color.green;
                return;
            }
            if (ControllerInput.RightTrigger && ControllerInput.RightGrip)
            {
                pointer.GetComponent<Renderer>().material.color = Color.green;
                float step = 32f * Time.deltaTime;
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                GorillaLocomotion.Player.Instance.transform.position = Vector3.MoveTowards(GorillaLocomotion.Player.Instance.transform.position, pointer.transform.position, step);
                return;
            }
            else
            {
                if (pointer != null)
                {
                    pointer.GetComponent<Renderer>().material.color = Color.red;
                    return;
                }
            }
            UnityEngine.GameObject.Destroy(pointer);

        }

        public static void C4Boom()
        {
            if (ControllerInput.RightGrip && !C4Spawned && !ControllerInput.RightTrigger)
            {
                C4Spawned = true;
                C4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.Destroy(C4.GetComponent<Rigidbody>());
                Object.Destroy(C4.GetComponent<BoxCollider>());
                C4.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.transform.position +
                                        new Vector3(0.2f, 0f, 0f);
                C4.transform.localScale = new Vector3(0.2f, 0.1f, 0.4f);
                C4.GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);
                C4.GetComponent<Renderer>().material.color = Color.magenta;
            }
            if (C4Spawned && ControllerInput.RightTrigger)
            {
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddExplosionForce(80000f, C4.transform.position, 10f, 5f);
                Object.Destroy(C4);
                C4Spawned = false;
            }
        }
        public static void GrappleHook()
        {

            if (ControllerInput.RightGrip && !ControllerInput.RightTrigger)
            {
                disablegrapple = false;
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out hit);

                if (lineRenderer == null)
                {
                    GameObject ob = new GameObject("line");
                    lineRenderer = ob.AddComponent<LineRenderer>();
                    lineRenderer.startColor = new Color(0, 0, 0, 1f);
                    lineRenderer.endColor = new Color(0, 0, 0, 1f);
                    lineRenderer.startWidth = 0.01f;
                    lineRenderer.endWidth = 0.01f;
                    lineRenderer.positionCount = 2;
                    lineRenderer.useWorldSpace = true;
                    lineRenderer.material.shader = Shader.Find("GUI/Text Shader");
                }
            }
            if (ControllerInput.RightGrip && ControllerInput.RightTrigger && !disablegrapple)
            {
                if (Vector3.Distance(GorillaLocomotion.Player.Instance.bodyCollider.transform.position, hit.point) < 4)
                {
                    disablegrapple = true;
                    Object.Destroy(lineRenderer); lineRenderer = null; return;
                }
                Vector3 dir2 = (hit.point - GorillaLocomotion.Player.Instance.bodyCollider.transform.position).normalized * 30;
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(dir2, ForceMode.Acceleration);
            }
            if (!ControllerInput.RightGrip && !ControllerInput.RightTrigger)
            {
                if (lineRenderer != null)
                {
                    Object.Destroy(lineRenderer);
                }
            }
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, hit.point);
                lineRenderer.SetPosition(1, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
            }
        }
        public static void MonkeClimb(GorillaLocomotion.Player __instance)
        {
            if (!PhotonNetwork.InLobby)
            {
                RG = ControllerInputPoller.instance.rightControllerGripFloat;
                LG = ControllerInputPoller.instance.leftControllerGripFloat;
                RaycastHit raycastHit;
                bool flag = Physics.Raycast(__instance.leftControllerTransform.position, __instance.leftControllerTransform.right, out raycastHit, 0.2f, layers);
                if ((Physics.Raycast(__instance.rightControllerTransform.position, -__instance.rightControllerTransform.right, out raycastHit, 0.2f, layers) || AirMode) && RG > 0.5f)
                {
                    if (!RGrabbing)
                    {
                        CurHandPos = __instance.rightControllerTransform.position;
                        RGrabbing = true;
                        LGrabbing = false;
                    }
                    ApplyVelocity(__instance.rightControllerTransform.position, CurHandPos, __instance);
                    return;
                }
                if ((flag || AirMode) && LG > 0.5f)
                {
                    if (!LGrabbing)
                    {
                        CurHandPos = __instance.leftControllerTransform.position;
                        LGrabbing = true;
                        RGrabbing = false;
                    }
                    ApplyVelocity(__instance.leftControllerTransform.position, CurHandPos, __instance);
                    return;
                }
                if (LGrabbing || RGrabbing)
                {
                    Physics.gravity = new Vector3(0, -9.81f, 0);
                    LGrabbing = false;
                    RGrabbing = false;
                }
            }
        }

        public static void BHop()
        {
            if (ControllerInput.RightSecondary)
            {
                if (canBHop)
                {
                    isBHop = !isBHop;
                    canBHop = false;
                }
            }
            else
            {
                canBHop = true;
            }
            if (isBHop)
            {
                if (GorillaLocomotion.Player.Instance.IsHandTouching(false))
                {
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(Vector3.up * 220f, ForceMode.Impulse);
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(GorillaTagger.Instance.offlineVRRig.rightHandPlayer.transform.right * 170f, ForceMode.Impulse);
                }
                if (GorillaLocomotion.Player.Instance.IsHandTouching(true))
                {
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(Vector3.up * 220f, ForceMode.Impulse);
                    GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().AddForce(-GorillaTagger.Instance.offlineVRRig.leftHandPlayer.transform.right * 170f, ForceMode.Impulse);
                }
            }
        }


        public static void FreezeMonkey()
        {
            if (ControllerInput.RightPrimary)
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
            if (!GorillaTagger.Instance.offlineVRRig.enabled)
            {

                GorillaTagger.Instance.offlineVRRig.transform.position = GorillaLocomotion.Player.Instance.bodyCollider.transform.position + new Vector3(0, 0.2f, 0);
                GorillaTagger.Instance.offlineVRRig.transform.rotation = GorillaLocomotion.Player.Instance.bodyCollider.transform.rotation;
            }
        }


        public static void TagAura()
        {
            bool inRoom = PhotonNetwork.InRoom;
            if (inRoom)
            {
                bool flag = Vector3.Distance(GetClosestUntagged().transform.position, GorillaLocomotion.Player.Instance.headCollider.transform.position) < 3.5f;
                if (flag)
                {
                    GorillaLocomotion.Player.Instance.leftControllerTransform.gameObject.transform.position = GetClosestUntagged().transform.position;
                }
            }
        }

        public static void ProcessTagAura(Photon.Realtime.Player pl)
        {
            if (!GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected.Contains(pl))
            {
                float distance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, GorillaGameManager.instance.FindPlayerVRRig(pl).transform.position);
                if (distance < GorillaGameManager.instance.tagDistanceThreshold)
                {
                    RPCSUB.ReportTag(pl);
                }
            }
        }


        public static void TagSelf()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected
                    .Add(PhotonNetwork.LocalPlayer);
                GorillaTagManager.instance.InfrequentUpdate();
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().UpdateState();
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().UpdateInfectionState();
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().UpdateTagState();
                GameMode.ReportHit();
            }
            else
            {
                AntiBan();
            }
        }

        public static float antibancooldown = 0;
        public static bool antiban = true;



        public static async Task AntiBan()
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "RoomClosed",
                FunctionParameter = new
                {
                    GameId = PhotonNetwork.CurrentRoom.Name,
                    Region = Regex.Replace(PhotonNetwork.CloudRegion, "[^a-zA-Z0-9]", "").ToUpper(),
                    UserId = PhotonNetwork.MasterClient.UserId,
                    ActorNr = 1,
                    ActorCount = 1,
                    AppVersion = PhotonNetwork.AppVersion,
                }
            }, result =>
            {
                Debug.Log("Successfully Ran It!");
            }, (error) =>
            {
                Debug.Log(error.Error);
            });
            await Task.Delay(500);
            string gamemode = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace(GorillaComputer.instance.currentQueue, GorillaComputer.instance.currentQueue + "MODDED_MODDED_");
            Hashtable hash = new Hashtable
            {
                { "gameMode",gamemode }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
            await Task.CompletedTask;
            //Notif.SendNotification("AntiBan Enabled For This Lobby");
        }

        static float Vibrateallcooldown = -1;

        public static void VibrateAll()
        {
            if (Time.time > Vibrateallcooldown + 0.5)
            {
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    RPCSUB.JoinedTaggedTime(ReceiverGroup.Others);
                }

                
                Vibrateallcooldown = Time.time;
            }
        }

        public static void VibrateGun()
        {
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (GetPhotonViewFromRig(GunLibraries.lockedOnPlayer) == null) { return; }
                if (Time.time > Vibrateallcooldown + 0.5)
                {
                    Player player = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                    RPCSUB.JoinedTaggedTime(player);
                    Vibrateallcooldown = Time.time;
                }
            }
        }



        public static void AcidGun()
        {
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (GetPhotonViewFromRig(GunLibraries.lockedOnPlayer) == null) { return; }
                Photon.Realtime.Player player = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
                ScienceExperimentManager.PlayerGameState[] states = new ScienceExperimentManager.PlayerGameState[10];
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (player == PhotonNetwork.PlayerList[i])
                    {
                        states[i].touchedLiquid = true;
                        states[i].playerId = player.ActorNumber;
                    }
                }
                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
            }
        }

        public static void UnAcidGun()
        {
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (GetPhotonViewFromRig(GunLibraries.lockedOnPlayer) == null) { return; }
                Photon.Realtime.Player player = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
                ScienceExperimentManager.PlayerGameState[] states = new ScienceExperimentManager.PlayerGameState[10];
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (player == PhotonNetwork.PlayerList[i])
                    {
                        states[i].touchedLiquid = false;
                        states[i].playerId = player.ActorNumber;
                    }
                }
                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
            }
        }

        public static void BoardTesting()
        {
            if (boards == null)
            {
                boards = GameObject.FindObjectsOfType<GorillaScoreBoard>();
            }
            foreach (GorillaScoreBoard board in boards)
            {
                foreach (GorillaPlayerScoreboardLine line in board.lines)
                {
                    line.playerNameValue = "";
                    line.playerName.text = "";
                    line.UpdateLine();
                }
            }
        }

        public static void AntiReport()
        {
            try
            {
                if (boards == null)
                {
                    boards = GameObject.FindObjectsOfType<GorillaScoreBoard>();
                }
                foreach (GorillaScoreBoard board in boards)
                {
                    foreach (GorillaPlayerScoreboardLine line in board.lines)
                    {
                        if (line.linePlayer.UserId == PhotonNetwork.LocalPlayer.UserId)
                        {
                            Transform report = line.reportButton.gameObject.transform;
                            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                            {
                                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                                {
                                    float D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position);
                                    float D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position);

                                    float threshold = 0.35f;

                                    if (D1 < threshold || D2 < threshold)
                                    {
                                        Notif.SendNotification("<color=red>[REPORT]</color> " + GetPhotonViewFromRig(vrrig).Owner.NickName + " tried to report you, left lobby " + PhotonNetwork.CurrentRoom.Name);
                                        PhotonNetwork.Disconnect();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { } 
        }


        public static void BoardNameSpam()
        {
            int index = 0;
            foreach (GorillaScoreBoard board in UnityEngine.Object.FindObjectsOfType<GorillaScoreBoard>())
            {
                foreach (Player player in PhotonNetwork.PlayerList)
                    board.lines[index] = board.lines[index + 1];
            }
        }

        public static void AcidAll()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = new ScienceExperimentManager.PlayerGameState[10];
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                states[i].touchedLiquid = true;
                states[i].playerId = PhotonNetwork.PlayerList[i].ActorNumber;
            }
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
            rpcReset();
        }
        static bool shouldacidchange = false;
        static float canacidchange = -100;
        public static void AcidSpam()
        {
            if (Time.time > canacidchange)
            {
                canacidchange = Time.time + 0.5f;
                if (shouldacidchange)
                {
                    Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
                    ScienceExperimentManager.PlayerGameState[] states = new ScienceExperimentManager.PlayerGameState[10];
                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        states[i].touchedLiquid = true;
                        states[i].playerId = PhotonNetwork.PlayerList[i].ActorNumber;
                    }
                    Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
                    shouldacidchange = false;
                }
                else
                {
                    Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
                    ScienceExperimentManager.PlayerGameState[] states = new ScienceExperimentManager.PlayerGameState[10];
                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        states[i].touchedLiquid = false;
                        states[i].playerId = PhotonNetwork.PlayerList[i].ActorNumber;
                    }
                    Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
                    shouldacidchange = true;
                }
            }

        }

        public static Vector3[] lastLeft = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        public static Vector3[] lastRight = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        public static void PunchMod()
        {
            int index = -1;
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    index++;

                    Vector3 they = vrrig.rightHandTransform.position;
                    Vector3 notthem = GorillaTagger.Instance.offlineVRRig.head.rigTarget.position;
                    float distance = Vector3.Distance(they, notthem);

                    if (distance < 0.25)
                    {
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity += Vector3.Normalize(vrrig.rightHandTransform.position - lastRight[index]) * 10f;
                    }
                    lastRight[index] = vrrig.rightHandTransform.position;

                    they = vrrig.leftHandTransform.position;
                    distance = Vector3.Distance(they, notthem);

                    if (distance < 0.25)
                    {
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity += Vector3.Normalize(vrrig.leftHandTransform.position - lastLeft[index]) * 10f;
                    }
                    lastLeft[index] = vrrig.leftHandTransform.position;
                }
            }
        }
        public static void rpcReset()
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                GorillaGameManager.instance.OnPlayerLeftRoom(player);
                GorillaGameManager.instance.OnMasterClientSwitched(player);
                ScienceExperimentManager.instance.OnMasterClientSwitched(player);
                GorillaGameManager.instance.OnMasterClientSwitched(player);
                GameMode.ActiveGameMode.OnMasterClientSwitched(player);
                PhotonNetwork.RemoveCallbackTarget(player);
                PhotonNetwork.RemoveBufferedRPCs(player.ActorNumber);
                PhotonNetwork.RemoveRPCs(player);
            }
        }

        public static void DisableNetworkTriggers()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("gameMode", "forestcitybasementcanyonsmountainsbeachskycaves" + PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString());
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable, null, null);
        }

        public static void RiseLava()
        {
            InfectionLavaController controller = InfectionLavaController.Instance;
            System.Type type = controller.GetType();

            FieldInfo fieldInfo = type.GetField("reliableState", BindingFlags.NonPublic | BindingFlags.Instance);

            object reliableState = fieldInfo.GetValue(controller);

            FieldInfo stateFieldInfo = reliableState.GetType().GetField("state");
            stateFieldInfo.SetValue(reliableState, InfectionLavaController.RisingLavaState.Full);

            FieldInfo stateFieldInfo2 = reliableState.GetType().GetField("stateStartTime");
            stateFieldInfo2.SetValue(reliableState, PhotonNetwork.Time);

            fieldInfo.SetValue(controller, reliableState);
        }

        public static void DrainLava()
        {
            InfectionLavaController controller = InfectionLavaController.Instance;
            System.Type type = controller.GetType();

            FieldInfo fieldInfo = type.GetField("reliableState", BindingFlags.NonPublic | BindingFlags.Instance);

            object reliableState = fieldInfo.GetValue(controller);

            FieldInfo stateFieldInfo = reliableState.GetType().GetField("state");
            stateFieldInfo.SetValue(reliableState, InfectionLavaController.RisingLavaState.Drained);

            FieldInfo stateFieldInfo2 = reliableState.GetType().GetField("stateStartTime");
            stateFieldInfo2.SetValue(reliableState, PhotonNetwork.Time);

            fieldInfo.SetValue(controller, reliableState);
        }


        public static async void MatGun()
        {
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (GetPhotonViewFromRig(GunLibraries.lockedOnPlayer) == null) { return; }
                Player player = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                if (infect.currentInfected.Contains(player))
                {
                    await Task.Delay(300);
                    infect.currentInfected.Remove(player);
                }
                else
                {
                    await Task.Delay(300);
                    infect.currentInfected.Add(player);
                }
                rpcReset();
            }
        }


        static float slowallcooldown = -1;
        public static void SlowAll()
        {
            if (Time.time > slowallcooldown + 0.5)
            {
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    RPCSUB.SetTaggedTime(ReceiverGroup.Others);
                }
                slowallcooldown = Time.time;
            }
        }

        public static void FreezeAll()
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
            {
                if (Time.time > slowallcooldown + 0.5)
                {
                    foreach (Player owner in PhotonNetwork.PlayerListOthers)
                {

                        MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                        object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                        method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                        GuidedRefHub.UnregisterTarget<VRRig>(GorillaGameManager.instance.FindPlayerVRRig(owner), true);
                        slowallcooldown = Time.time;
                    }
                }

                slowallcooldown = Time.time;
            }
        }

        static float ropetimeout;
        public static void SendRopeRPC(Vector3 velocity)
        {
            if (Time.time > ropetimeout + 0.1f)
            {
                ropetimeout = Time.time;
                foreach (GorillaRopeSwing rope in UnityEngine.Object.FindObjectsOfType<GorillaRopeSwing>())
                {
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, velocity, true, null);
                }
            }
        }

        public static void RopeFreeze()
        {
            if (Time.time > ropetimeout + 0.1f)
            {
                ropetimeout = Time.time;
                foreach (GorillaRopeSwing rope in UnityEngine.Object.FindObjectsOfType<GorillaRopeSwing>())
                {
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, Vector3.zero, true, null);
                }
            }
        }

        public static void RopeUp()
        {
            SendRopeRPC(new Vector3(0, 100, 1));
        }

        public static void FlingOnRope()
        {
            SendRopeRPC(new Vector3(0, int.MaxValue, 1));
        }

        public static void RopeDown()
        {
            SendRopeRPC(new Vector3(0, -100, 1));
        }

        public static void RopeToSelf()
        {
            if (Time.time > ropetimeout + 0.1f)
            {
                ropetimeout = Time.time;
                foreach (GorillaRopeSwing rope in Object.FindObjectsOfType<GorillaRopeSwing>())
                {
                    Vector3 targetPosition = GorillaLocomotion.Player.Instance.transform.position;
                    Vector3 ropeToCursor = targetPosition - rope.transform.position;
                    float distanceToCursor = ropeToCursor.magnitude;
                    float speed = 9999;
                    float t = Mathf.Clamp01(speed / distanceToCursor);

                    Vector3 newPosition = rope.transform.position + ropeToCursor.normalized * distanceToCursor * t;
                    Vector3 velocity = (newPosition - rope.transform.position).normalized * speed;
                    RopeSwingManager.instance.photonView.RPC("SetVelocity", RpcTarget.All, rope.ropeId, 4, velocity, true, null);
                }
            }
        }

        static float crashtimeout = -2192;

        public void saveKeys()
        {
            if (GorillaGameManager.instance != null)
            {
                if (GetGameMode().Contains("INFECTION"))
                {
                    if (GorillaTagManager == null)
                    {
                        GorillaTagManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                    }
                }
                else if (GetGameMode().Contains("HUNT"))
                {
                    if (this.GorillaHuntManager == null)
                    {
                        this.GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
                    }
                }
                else if (GetGameMode().Contains("BATTLE"))
                {
                    if (GorillaBattleManager == null)
                    {
                        GorillaBattleManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                    }
                }
            }
        }


        public void matSpamAll()
        {
            saveKeys();
            if (Time.time > mattimer)
            {
                if (ltagged)
                {
                    ltagged = false;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        GorillaTagManager.isCurrentlyTag = true;
                        GorillaTagManager.currentInfected.Clear();
                        CopyInfectedListToArray();
                    }
                    else if (GetGameMode().Contains("HUNT"))
                    {
                        GorillaHuntManager.currentHunted.Clear();
                        CopyHuntDataListToArray();
                    }
                    else if (GetGameMode().Contains("BATTLE"))
                    {
                        foreach (Player pl in PhotonNetwork.PlayerList)
                        {
                            GorillaBattleManager.playerLives[pl.ActorNumber] = 0;
                        }
                    }
                }
                else
                {
                    ltagged = true;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        GorillaTagManager.currentInfected = PhotonNetwork.PlayerList.ToList();
                        CopyInfectedListToArray();
                    }
                    else if (GetGameMode().Contains("HUNT"))
                    {
                        GorillaHuntManager.currentHunted = PhotonNetwork.PlayerList.ToList();
                        CopyHuntDataListToArray();
                    }
                    else if (GetGameMode().Contains("BATTLE"))
                    {
                        foreach (Player pl in PhotonNetwork.PlayerList)
                        {
                            GorillaBattleManager.playerLives[pl.ActorNumber] = 3;
                            CopyHuntDataListToArray();
                        }
                    }
                }
                rpcReset();
                mattimer = Time.time + 0.08f;
            }
        }

        float mattimer = 0;
        private int iterator1;
        private int copyListToArrayIndex;
        private void CopyHuntDataListToArray()
        {
            for (copyListToArrayIndex = 0; copyListToArrayIndex < 10; copyListToArrayIndex++)
            {
                GorillaHuntManager.currentHuntedArray[copyListToArrayIndex] = 0;
                GorillaHuntManager.currentTargetArray[copyListToArrayIndex] = 0;
            }
            for (copyListToArrayIndex = GorillaHuntManager.currentHunted.Count - 1; copyListToArrayIndex >= 0; copyListToArrayIndex--)
            {
                if (GorillaHuntManager.currentHunted[copyListToArrayIndex] == null)
                {
                    GorillaHuntManager.currentHunted.RemoveAt(copyListToArrayIndex);
                }
            }
            for (copyListToArrayIndex = GorillaHuntManager.currentTarget.Count - 1; copyListToArrayIndex >= 0; copyListToArrayIndex--)
            {
                if (GorillaHuntManager.currentTarget[copyListToArrayIndex] == null)
                {
                    GorillaHuntManager.currentTarget.RemoveAt(copyListToArrayIndex);
                }
            }
            for (copyListToArrayIndex = 0; copyListToArrayIndex < GorillaHuntManager.currentHunted.Count; copyListToArrayIndex++)
            {
                GorillaHuntManager.currentHuntedArray[copyListToArrayIndex] = GorillaHuntManager.currentHunted[copyListToArrayIndex].ActorNumber;
            }
            for (copyListToArrayIndex = 0; copyListToArrayIndex < GorillaHuntManager.currentTarget.Count; copyListToArrayIndex++)
            {
                GorillaHuntManager.currentTargetArray[copyListToArrayIndex] = GorillaHuntManager.currentTarget[copyListToArrayIndex].ActorNumber;
            }
        }
        private void CopyInfectedListToArray()
        {
            for (iterator1 = 0; iterator1 < GorillaTagManager.currentInfectedArray.Length; iterator1++)
            {
                GorillaTagManager.currentInfectedArray[iterator1] = 0;
            }
            for (iterator1 = GorillaTagManager.currentInfected.Count - 1; iterator1 >= 0; iterator1--)
            {
                if (GorillaTagManager.currentInfected[iterator1] == null)
                {
                    GorillaTagManager.currentInfected.RemoveAt(iterator1);
                }
            }
            for (iterator1 = 0; iterator1 < GorillaTagManager.currentInfected.Count; iterator1++)
            {
                GorillaTagManager.currentInfectedArray[iterator1] = GorillaTagManager.currentInfected[iterator1].ActorNumber;
            }
        }

        bool ltagged = false;

        static float kicktimer = -1000;
        public static void KickGun()
        {
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (GetPhotonViewFromRig(GunLibraries.lockedOnPlayer) == null) { return; }
                Player owner = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                if (owner != null && Time.time > kicktimer + 1f)
                {
                    if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(owner.UserId))
                    {

                        PhotonNetworkController.Instance.shuffler = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                        PhotonNetworkController.Instance.keyStr = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                        RPCSUB.JoinPubWithFriends(owner);
                        kicktimer = Time.time;
                    }
                }
            }
        }

        public static void SlowGun()
        {
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));

            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (GetPhotonViewFromRig(GunLibraries.lockedOnPlayer) == null) { return; }
                Player player = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                RPCSUB.SetTaggedTime(player);
            }
        }

        public static void FreezeGun()
        {
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (Time.time > slowallcooldown + 0.2)
                {

                    if (GetPhotonViewFromRig(GunLibraries.lockedOnPlayer) == null) { return; }
                    crashtimeout = Time.time;
                    Player player = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                    MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                    object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { player.ActorNumber });
                    method.Invoke(typeof(PhotonNetwork), new object[1] { player.ActorNumber });
                    GuidedRefHub.UnregisterTarget<VRRig>(GunLibraries.lockedOnPlayer, true);
                    slowallcooldown = Time.time;
                }
            }
        }


        public static void CrashGun()
        {
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (Time.time > slowallcooldown + 0.1)
                {
                    if (GetPhotonViewFromRig(GunLibraries.lockedOnPlayer) == null) { return; }
                    Player player = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                    crashtimeout = Time.time;
                    MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                    object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner.ActorNumber });
                    method.Invoke(typeof(PhotonNetwork), new object[1] { GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner.ActorNumber });
                    PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                    {
                        TargetActors = new int[] { player.ActorNumber }
                    }, SendOptions.SendReliable);
                    PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                    {
                        TargetActors = new int[] { player.ActorNumber }
                    }, SendOptions.SendReliable);
                    PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                    {
                        TargetActors = new int[] { player.ActorNumber }
                    }, SendOptions.SendReliable);

                    GuidedRefHub.UnregisterTarget<VRRig>(GunLibraries.lockedOnPlayer, true);
                    slowallcooldown = Time.time;
                }
            }
        }


        public static void Slow()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                RPCSUB.SetTaggedTime(selectedPerson);
            }
        }

        public static Player selectedPerson;
        public static void Vibrate()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                RPCSUB.JoinedTaggedTime(selectedPerson);
                
            }
        }

        static float timerere;
        public static void CrashAll()
        {

            if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED"))
            {
                if (Time.time > slowallcooldown + 0.1)
                {
                    foreach (Player owner in PhotonNetwork.PlayerListOthers)
                {

                        MethodInfo method = typeof(PhotonNetwork).GetMethod("SendDestroyOfPlayer", BindingFlags.Static | BindingFlags.NonPublic);
                        object obj = method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                        method.Invoke(typeof(PhotonNetwork), new object[1] { owner.ActorNumber });
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);
                        PhotonNetwork.RaiseEvent(100, null, new RaiseEventOptions
                        {
                            TargetActors = new int[] { owner.ActorNumber }
                        }, SendOptions.SendReliable);

                        GuidedRefHub.UnregisterTarget<VRRig>(GorillaGameManager.instance.FindPlayerVRRig(owner), true);

                    }
                    rpcReset();
                    slowallcooldown = Time.time;
                }

            }
            /*
            if (!PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED")) { return; }
            Crasher.isCrashing = true;
            if (Time.time > timerere)
            {
                timerere = Time.time + 0.05f;
                PhotonNetwork.SendAllOutgoingCommands();
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
                PhotonNetwork.Destroy(GorillaTagger.Instance.myVRRig.gameObject);

                if (GorillaTagger.Instance.myVRRig.IsNull())
                {
                    typeof(PhotonNetwork).GetMethod("RaiseEventInternal", BindingFlags.Static | BindingFlags.NonPublic).Invoke(typeof(PhotonNetwork), new object[]
                    {
                    202,
                    new Hashtable
                    {
                        { 0, "GorillaPrefabs/Gorilla Player Networked"},
                        { 6, PhotonNetwork.ServerTimestamp },
                        { 7, PhotonNetwork.AllocateViewID(PhotonNetwork.LocalPlayer.ActorNumber) }
                    },
                    new RaiseEventOptions
                    {
                        CachingOption = EventCaching.AddToRoomCacheGlobal
                    },
                    SendOptions.SendUnreliable
                    });

                }
            }*/
        }

        public static void kickAll()
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (Time.time > slowallcooldown + 0.5)
                {
                    if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork
                        .LocalPlayer.UserId) &&
                    GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId))
                    {
                        PhotonNetworkController.Instance.shuffler =
                            UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                        PhotonNetworkController.Instance.keyStr =
                            UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                        RPCSUB.JoinPubWithFriends(player);
                    }
                    slowallcooldown = Time.time;
                }
            }
        }

        public static async void Mat()
        {
            GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
            if (infect.currentInfected.Contains(selectedPerson))
            {
                await Task.Delay(100);
                infect.currentInfected.Remove(selectedPerson);
            }
            else
            {
                await Task.Delay(100);
                infect.currentInfected.Add(selectedPerson);
            }
        }


        public static void changegamemode(string gamemode)
        {
            if (!PhotonNetwork.IsMasterClient) { return; }
            Hashtable hash = new Hashtable();
            if (GetGameMode() == "HUNT")
            {
                hash.Add("gameMode", PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace("HUNT", gamemode));
            }
            if (GetGameMode() == "BATTLE")
            {
                hash.Add("gameMode", PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace("BATTLE", gamemode));
            }
            if (GetGameMode() == "INFECTION")
            {
                hash.Add("gameMode", PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace("INFECTION", gamemode));
            }
            if (GetGameMode() == "CASUAL")
            {
                hash.Add("gameMode", PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace("CASUAL", gamemode));
            }
            GameMode gmInstance = Traverse.Create(typeof(GameMode)).Field("instance").GetValue<GameMode>();
            if (gmInstance)
            {
                PhotonNetwork.Destroy(gmInstance.GetComponent<PhotonView>());
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("CASUAL"))
            {
                PhotonNetwork.InstantiateRoomObject("GameMode", Vector3.zero, Quaternion.identity, 0, new object[1] { 0 });
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("INFECTION"))
            {
                PhotonNetwork.InstantiateRoomObject("GameMode", Vector3.zero, Quaternion.identity, 0, new object[1] { 1 });
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("HUNT"))
            {
                PhotonNetwork.InstantiateRoomObject("GameMode", Vector3.zero, Quaternion.identity, 0, new object[1] { 2 });
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("BATTLE"))
            {
                var go = PhotonNetwork.InstantiateRoomObject("GameMode", Vector3.zero, Quaternion.identity, 0, new object[1] { 3 });
                go.GetComponent<GorillaBattleManager>().RandomizeTeams();
            }
        }



        public static PhotonView GetPhotonViewFromRig(VRRig rig)
        {
            try
            {
                PhotonView info = Traverse.Create(rig).Field("photonView").GetValue<PhotonView>();
                if (info != null)
                {
                    return info;
                }
            }
            catch {
                throw;
            }

            return null;
        }

        public static bool following = false;
        public static VRRig FollowingPlayer;
        public static void FollowPLayer()
        {
            if (FollowingPlayer != null)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }
                GorillaTagger.Instance.offlineVRRig.enabled = false;

                GorillaTagger.Instance.offlineVRRig.transform.position = FollowingPlayer.transform.position;

                GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = FollowingPlayer.rightHand.rigTarget.transform.position;
                GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = FollowingPlayer.leftHand.rigTarget.transform.position;

                GorillaTagger.Instance.offlineVRRig.transform.rotation = FollowingPlayer.transform.rotation;

                GorillaTagger.Instance.offlineVRRig.head.rigTarget.rotation = FollowingPlayer.head.rigTarget.rotation;
            }
        }


        public static void TagAll(bool isMaster)
        {
            if (Time.time > slowallcooldown + 0.5)
            {
                foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers)
                {
                    if (!isMaster)
                    {
                        if (GetGameMode().Contains("INFECTION"))
                        {

                            GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                            if (!infect.currentInfected.Contains(p) &&
                                infect.currentInfected.Contains(PhotonNetwork.LocalPlayer))
                            {

                                GorillaTagger.Instance.offlineVRRig.transform.position =
                                    GorillaGameManager.instance.FindPlayerVRRig(p).transform.position;
                                ProcessTagAura(p);
                                return;
                            }
                            else if (!infect.currentInfected.Contains(PhotonNetwork.LocalPlayer) &&
                                     infect.currentInfected.Contains(p))
                            {
                                TagSelf();
                                GorillaTagger.Instance.offlineVRRig.transform.position =
                                    GorillaGameManager.instance.FindPlayerVRRig(p).transform.position;
                                ProcessTagAura(p);
                                return;
                            }
                        }

                        if (GetGameMode().Contains("HUNT"))
                        {
                            if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget !=
                                null)
                            {
                                if (GorillaTagger.Instance.offlineVRRig.enabled)
                                {
                                    Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                                }

                                GorillaTagger.Instance.offlineVRRig.enabled = false;
                                GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance
                                    .FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer
                                        .GetComponent<GorillaHuntComputer>().myTarget).transform.position;
                                ProcessTagAura(GorillaTagger.Instance.offlineVRRig.huntComputer
                                    .GetComponent<GorillaHuntComputer>().myTarget);
                                return;
                            }
                        }

                        if (GetGameMode().Contains("BATTLE"))
                        {
                            GorillaBattleManager infect =
                                GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                            if (infect.playerLives[p.ActorNumber] > 0)
                            {
                                PhotonView pv = GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)")
                                    .GetComponent<PhotonView>();
                                if (PhotonNetwork.IsMasterClient)
                                {
                                    infect.playerLives[p.ActorNumber] = 0;
                                    return;
                                }

                                pv.RPC("ReportSlingshotHit", RpcTarget.MasterClient,
                                    new object[] { new Vector3(0, 0, 0), p, RPCSUB.IncrementLocalPlayerProjectileCount() });

                            }
                        }
                    }
                    else
                    {
                        GorillaTagManager infect = GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>();
                        infect.currentInfected.Add(p);
                    }
                }
                slowallcooldown = Time.time;
                }
                GorillaTagger.Instance.offlineVRRig.enabled = true;
        }


        public static void UntagAll()
        {
            if (!PhotonNetwork.IsMasterClient) { return; }
            instance.saveKeys();
            if (GetGameMode().Contains("fect"))
            {
                instance.GorillaTagManager.currentInfected.Clear();
                instance.CopyInfectedListToArray();
            }
            if (GetGameMode().Contains("hunt"))
            {
                instance.GorillaHuntManager.currentHunted.Clear();
                instance.GorillaHuntManager.currentTarget = PhotonNetwork.PlayerList.ToList();
                instance.CopyHuntDataListToArray();
            }
            if (GetGameMode().Contains("bat"))
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    instance.GorillaBattleManager.playerLives[PhotonNetwork.PlayerList[i].ActorNumber] = 3;
                }
            }
        }
        public static void UnTagGun()
        {
            GunLibraries.ShootLock(ControllerInput.RightGrip, ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0));
            if (GunLibraries.lockedOnPlayer == null)
                return;
            if (ControllerInput.RightTrigger || UnityInput.Current.GetMouseButton(0))
            {
                if (GetPhotonViewFromRig(GunLibraries.lockedOnPlayer) == null) { return; }
                if (!PhotonNetwork.IsMasterClient) { return; }
                instance.saveKeys();
                Player owner = GetPhotonViewFromRig(GunLibraries.lockedOnPlayer).Owner;
                if (GetGameMode().Contains("fect"))
                {
                    instance.GorillaTagManager.currentInfected.Remove(owner);
                    instance.CopyInfectedListToArray();
                }
                if (GetGameMode().Contains("hunt"))
                {
                    instance.GorillaHuntManager.currentHunted.Remove(owner);
                    instance.GorillaHuntManager.currentTarget.Add(owner);
                    instance.CopyHuntDataListToArray();
                }
                if (GetGameMode().Contains("bat"))
                {
                    instance.GorillaBattleManager.playerLives[owner.ActorNumber] = 3;
                }
            }
        }

        public static void CreatePrivateRoom()
        {
            Hashtable customRoomProperties;
            if (PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "city" && PhotonNetworkController.Instance.currentJoinTrigger.gameModeName != "basement")
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + GorillaComputer.instance.currentGameMode
                    }
                };
            }
            else
            {
                customRoomProperties = new Hashtable
                {
                    {
                        "gameMode",
                        PhotonNetworkController.Instance.currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + "CASUAL"
                    }
                };
            }
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = false;
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = PhotonNetworkController.Instance.GetRoomSize(PhotonNetworkController.Instance.currentJoinTrigger.gameModeName);
            roomOptions.CustomRoomProperties = customRoomProperties;
            roomOptions.PublishUserId = true;
            roomOptions.CustomRoomPropertiesForLobby = new string[]
            {
                "gameMode"
            };
            PhotonNetwork.CreateRoom(RandomRoomName(), roomOptions, null, null);
        }

        
        public static string RandomRoomName()
        {

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[4];
            var random = new System.Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            string text = "";
            for (int i = 0; i < 4; i++)
            {
                text += new String(stringChars);
            }
            if (GorillaComputer.instance.CheckAutoBanListForName(text))
            {
                return text;
            }
            return RandomRoomName();
        }

    }
}