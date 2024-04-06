using GorillaGameModes;
using GorillaNetworking;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.XR;
using UnityEngine;
using HarmonyLib;
using ExitGames.Client.Photon;
using Player = Photon.Realtime.Player;
using static Steal.Background.InputHandler;

namespace Steal.Background.Mods
{
    internal class PlayerMods : Mod
    {
        public static GorillaBattleManager GorillaBattleManager;
        public static GorillaHuntManager GorillaHuntManager;
        public static GorillaTagManager GorillaTagManager;

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
            catch
            {
                throw;
            }

            return null;
        }
        public static void saveKeys()
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
                    if (GorillaHuntManager == null)
                    {
                        GorillaHuntManager = GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>();
                    }
                }
                else if (GetGameMode().Contains("BATTLE"))
                {
                    if (GorillaBattleManager == null)
                    {
                        GorillaBattleManager =
                            GorillaGameManager.instance.gameObject.GetComponent<GorillaBattleManager>();
                    }
                }
            }
        }

        public static void AntiFlap()
        {
            Traverse.Create(GorillaTagger.Instance.offlineVRRig).Field("speakingLoudness").SetValue(0);
            GorillaTagger.Instance.offlineVRRig.GetComponent<GorillaMouthFlap>().enabled = false;
        }

        public static void ReFlap()
        {
            GorillaTagger.Instance.offlineVRRig.GetComponent<GorillaMouthFlap>().enabled = true;
        }
        public static string getAntiReport()
        {
            return MenuPatch.antiReportCurrent;
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
        public static Vector3 GetMiddle(Vector3 vector)
        {
            return new Vector3(vector.x / 2f, vector.y / 2f, vector.z / 2f);
        }
        public static void Splash()
        {
            if (Time.time > splashtimeout + 0.5f)
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

        public static void SizeableSplash()
        {
            if (InputHandler.RightTrigger)
            {
                if (Time.time > splashtimeout + 0.5)
                {
                    GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", 0, new object[]
                    {
                        GetMiddle(GorillaLocomotion.Player.Instance.rightControllerTransform.position +
                                  GorillaLocomotion.Player.Instance.leftControllerTransform.position),
                        UnityEngine.Random.rotation,
                        Vector3.Distance(GorillaLocomotion.Player.Instance.rightControllerTransform.position,
                            GorillaLocomotion.Player.Instance.leftControllerTransform.position),
                        Vector3.Distance(GorillaLocomotion.Player.Instance.rightControllerTransform.position,
                            GorillaLocomotion.Player.Instance.leftControllerTransform.position),
                        false,
                        true
                    });
                    splashtimeout = Time.time;
                }
            }
        }

        public static void SplashGun()
        {
            var data = GunLib.Shoot();
            if (data != null)
            {
                if (data.isShooting && data.isTriggered)
                {
                    var tagger = GorillaTagger.Instance;

                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }

                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = data.hitPosition - new Vector3(0, 1f, 0);
                    if (Time.time > splashtimeout)
                    {
                        GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", RpcTarget.All, new object[]
                        {
                            data.hitPosition + new Vector3(0, 1, 0),
                            Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0, 360),
                                UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360))),
                            4f,
                            100f,
                            true,
                            false
                        });
                        splashtimeout = Time.time + 0.1f;
                    }
                }
                else
                {
                    ResetRig();
                }
            }
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
            GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g, colortochange.b);
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
            GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g, colortochange.b);
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

        #region Rig Mods

        static bool ghostToggled = false;

        public static void ResetRig()
        {
            GorillaTagger.Instance.offlineVRRig.enabled = true;

            ghostToggled = true;
        }

        public static void GhostMonkey()
        {
            if (!XRSettings.isDeviceActive)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }

                GorillaTagger.Instance.offlineVRRig.enabled = false;
                return;
            }

            if (RightPrimary)
            {
                if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
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
                    Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }

                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = new Vector3(999, 999, 999);
                return;
            }

            if (RightPrimary)
            {
                if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
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

        public static void FreezeMonkey()
        {
            if (RightPrimary)
            {
                if (!ghostToggled && GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
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
                GorillaTagger.Instance.offlineVRRig.transform.position =
                    GorillaLocomotion.Player.Instance.bodyCollider.transform.position + new Vector3(0, 0.2f, 0);
                GorillaTagger.Instance.offlineVRRig.transform.rotation =
                    GorillaLocomotion.Player.Instance.bodyCollider.transform.rotation;
            }
        }

        public static void CopyGun()
        {
            GunLib.GunLibData data = GunLib.ShootLock();
            if (data.isShooting && data.isTriggered && data.isLocked)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }

                GorillaTagger.Instance.offlineVRRig.enabled = false;

                GorillaTagger.Instance.offlineVRRig.transform.position = data.lockedPlayer.transform.position;

                GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position =
                    data.lockedPlayer.rightHand.rigTarget.transform.position;
                GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position =
                    data.lockedPlayer.leftHand.rigTarget.transform.position;

                GorillaTagger.Instance.offlineVRRig.transform.rotation = data.lockedPlayer.transform.rotation;

                GorillaTagger.Instance.offlineVRRig.head.rigTarget.rotation = data.lockedPlayer.head.rigTarget.rotation;
            }
            else
            {
                ResetRig();
            }
        }

        public static void RigGun()
        {
            var data = GunLib.Shoot();
            if (data != null)
            {
                if (data.isShooting && data.isTriggered)
                {
                    if (GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                    }

                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = data.hitPosition + new Vector3(0, 0.6f, 0);
                }
                else
                {
                    ResetRig();
                }
            }
        }

        public static void HoldRig()
        {
            if (RightGrip)
            {
                if (GorillaTagger.Instance.offlineVRRig.enabled)
                {
                    Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                }

                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position =
                    GorillaLocomotion.Player.Instance.rightControllerTransform.position;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }

        #endregion

        #region tag stuff

        public static void NoTagOnJoin()
        {
            var hash = new Hashtable
            {
                { "didTutorial", false }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            GorillaTagger.Instance.myVRRig.Owner.SetCustomProperties(hash);
        }

        static bool[] hasSentAlert = new bool[10];
        static float resetAlerts;

        public static void TagAlerts()
        {
            if (Time.time > resetAlerts)
            {
                resetAlerts = Time.time + 1;
                for (int i = 0; i < hasSentAlert.Length; i++)
                {
                    hasSentAlert[i] = false;
                }
            }

            for (int i = 0; i < GorillaParent.instance.vrrigs.Count; i++)
            {
                var rig = GorillaParent.instance.vrrigs[i];
                if (rig.isOfflineVRRig || rig == null || !rig.mainSkin.material.name.Contains("fect"))
                {
                    return;
                }

                if (Vector3.Distance(rig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <
                    3.5f)
                {
                    if (!hasSentAlert[i])
                    {
                        hasSentAlert[i] = true;
                        Notif.SendNotification(
                            $"Lava Monkey Detected {Vector3.Distance(rig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position)}M Away!",
                            Color.red);
                    }
                }
                else
                {
                    hasSentAlert[i] = false;
                }
            }
        }

        public static VRRig GetClosestUntagged()
        {
            VRRig vrrig = null;
            float num = float.MaxValue;
            foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
            {
                float num2 = Vector3.Distance(vrrig2.transform.position,
                    GorillaTagger.Instance.offlineVRRig.transform.position);
                bool flag = num2 < num && !vrrig2.mainSkin.material.name.Contains("fected") &&
                            vrrig2 != GorillaTagger.Instance.offlineVRRig;
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
                float num2 = Vector3.Distance(vrrig2.transform.position,
                    GorillaTagger.Instance.offlineVRRig.transform.position);
                bool flag = num2 < num && vrrig2.mainSkin.material.name.Contains("fected") &&
                            vrrig2 != GorillaTagger.Instance.offlineVRRig;
                if (flag)
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
                    GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected
                        .Remove(PhotonNetwork.LocalPlayer);
                }

                if (Vector3.Distance(GetClosestTagged().transform.position,
                        GorillaLocomotion.Player.Instance.headCollider.transform.position) < 3.5f && !Physics.Linecast(
                        GorillaLocomotion.Player.Instance.transform.position, GetClosestTagged().transform.position,
                        LayerMask.NameToLayer("Gorilla Tag Collider")))
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position =
                        GorillaLocomotion.Player.Instance.headCollider.transform.position - new Vector3(0f, 20f, 0f);
                    return;
                }

                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }

            GorillaTagger.Instance.offlineVRRig.enabled = true;
        }

        public static void TagAura()
        {
            foreach (Player p in PhotonNetwork.PlayerListOthers)
            {
                if (p != null)
                {
                    ProcessTagAura(p);
                }
            }
        }

        public static void ProcessTagAura(Photon.Realtime.Player pl)
        {
            if (!GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected.Contains(pl))
            {
                float distance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position,
                    GorillaGameManager.instance.FindPlayerVRRig(pl).transform.position);
                if (distance < GorillaGameManager.instance.tagDistanceThreshold)
                {
                    RPCSUB.ReportTag(pl);
                }
            }
        }

        public static void Helicopter()
        {
            var gorillaTaggerInstance = GorillaTagger.Instance;
            var offlineVRRig = gorillaTaggerInstance.offlineVRRig;

            offlineVRRig.enabled = false;

            Vector3 positionIncrement = new Vector3(0f, 0.05f, 0f);
            Vector3 rotationIncrement = new Vector3(0f, 10f, 0f);

            offlineVRRig.transform.position += positionIncrement;
            gorillaTaggerInstance.myVRRig.transform.position += positionIncrement;

            Quaternion newRotation = Quaternion.Euler(offlineVRRig.transform.rotation.eulerAngles + rotationIncrement);
            offlineVRRig.transform.rotation = newRotation;
            gorillaTaggerInstance.myVRRig.transform.rotation = newRotation;

            offlineVRRig.head.rigTarget.transform.rotation = newRotation;

            Vector3 leftHandPosition = offlineVRRig.transform.position - offlineVRRig.transform.right;
            Vector3 rightHandPosition = offlineVRRig.transform.position + offlineVRRig.transform.right;
            offlineVRRig.leftHand.rigTarget.transform.position = leftHandPosition;
            offlineVRRig.rightHand.rigTarget.transform.position = rightHandPosition;

            offlineVRRig.leftHand.rigTarget.transform.rotation = newRotation;
            offlineVRRig.rightHand.rigTarget.transform.rotation = newRotation;
        }

        public static void OrbitGun()
        {
            var data = GunLib.ShootLock();
            if (data != null && data.isTriggered && data.isLocked && data.lockedPlayer != null && data.isShooting)
            {
                var gorillaTagger = GorillaTagger.Instance;
                var offlineVRRig = gorillaTagger.offlineVRRig;
                var myVRRig = gorillaTagger.myVRRig;
                var offlineVRRigTransform = offlineVRRig.transform;
                var myVRRigTransform = myVRRig.transform;
                var targetPosition = data.lockedPlayer.transform.position;

                offlineVRRig.enabled = false;

                Vector3 directionToTarget = (targetPosition - offlineVRRigTransform.position).normalized;
                Vector3 newPosition = offlineVRRigTransform.position + (directionToTarget * (11 * Time.deltaTime));

                offlineVRRigTransform.position = newPosition;
                myVRRigTransform.position = newPosition;

                offlineVRRigTransform.LookAt(targetPosition);
                myVRRigTransform.LookAt(targetPosition);

                UpdateRigTarget(offlineVRRig.head.rigTarget.transform, newPosition, offlineVRRigTransform.rotation);
                UpdateRigTarget(offlineVRRig.leftHand.rigTarget.transform,
                    newPosition + (offlineVRRigTransform.right * -1f), offlineVRRigTransform.rotation);
                UpdateRigTarget(offlineVRRig.rightHand.rigTarget.transform,
                    newPosition + (offlineVRRigTransform.right * 1f), offlineVRRigTransform.rotation);
            }
            else
            {
                ResetRig();
            }

        }


        static Vector3 offsetl;

        static Vector3 offsetr;

        static Vector3 offsethead;

        static bool doonce = false;
        private static float splashtimeout;

        public static void ColorToBoard()
        {
            Color colortochange = (new Color32(0, 5, 2, 255));
            GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(colortochange.r, colortochange.g,
                colortochange.b);
            GorillaTagger.Instance.offlineVRRig.enabled = false;
            GorillaTagger.Instance.offlineVRRig.transform.position =
                GorillaComputer.instance.computerScreenRenderer.transform.position;
            GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.All, colortochange.r,
                colortochange.g, colortochange.b);
            GorillaTagger.Instance.offlineVRRig.transform.position =
                GorillaComputer.instance.computerScreenRenderer.transform.position;
            GorillaTagger.Instance.offlineVRRig.transform.position =
                GorillaComputer.instance.computerScreenRenderer.transform.position;
            GorillaTagger.Instance.offlineVRRig.transform.position =
                GorillaComputer.instance.computerScreenRenderer.transform.position;
            GorillaTagger.Instance.offlineVRRig.transform.position =
                GorillaComputer.instance.computerScreenRenderer.transform.position;
            GorillaTagger.Instance.offlineVRRig.transform.position =
                GorillaComputer.instance.computerScreenRenderer.transform.position;
            GorillaTagger.Instance.offlineVRRig.transform.position =
                GorillaComputer.instance.computerScreenRenderer.transform.position;
            GorillaTagger.Instance.offlineVRRig.transform.position =
                GorillaComputer.instance.computerScreenRenderer.transform.position;
            GorillaTagger.Instance.offlineVRRig.transform.position =
                GorillaComputer.instance.computerScreenRenderer.transform.position;
            GorillaTagger.Instance.offlineVRRig.enabled = true;
        }

        public static void SpazRig()
        {
            if (!doonce)
            {
                offsetl = GorillaTagger.Instance.offlineVRRig.leftHand.trackingPositionOffset;
                offsetr = GorillaTagger.Instance.offlineVRRig.rightHand.trackingPositionOffset;
                offsethead = GorillaTagger.Instance.offlineVRRig.head.trackingPositionOffset;
                doonce = true;
            }

            Vector3 randomOffset = offsetVariance(0.1f);


            Vector3 offsetVariance(float amount) => new Vector3(UnityEngine.Random.Range(-amount, amount),
                UnityEngine.Random.Range(-amount, amount), UnityEngine.Random.Range(-amount, amount));

            var gorillaTaggerInstance = GorillaTagger.Instance;


            gorillaTaggerInstance.offlineVRRig.leftHand.trackingPositionOffset = offsetl + randomOffset;
            gorillaTaggerInstance.offlineVRRig.rightHand.trackingPositionOffset = offsetr + randomOffset;
            gorillaTaggerInstance.offlineVRRig.head.trackingPositionOffset = offsethead + randomOffset;
        }


        public static void ResetAfterSpaz()
        {
            var gorillaTaggerInstance = GorillaTagger.Instance;
            gorillaTaggerInstance.offlineVRRig.leftHand.trackingPositionOffset = offsetl;
            gorillaTaggerInstance.offlineVRRig.rightHand.trackingPositionOffset = offsetr;
            gorillaTaggerInstance.offlineVRRig.head.trackingPositionOffset = offsethead;
        }


        public static void UpdateRigTarget(Transform rigTarget, Vector3 position, Quaternion rotation)
        {
            rigTarget.position = position;
            rigTarget.rotation = rotation;
        }

        public static void TagGun()
        {
            var isMaster = PhotonNetwork.IsMasterClient;
            var data = GunLib.ShootLock();
            if (data != null && data.isTriggered && data.isLocked && data.lockedPlayer != null && data.isShooting && GetPhotonViewFromRig(data.lockedPlayer) != null)
            {
                saveKeys();
                if (!isMaster)
                {
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }

                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        GorillaTagger.Instance.offlineVRRig.transform.position = data.lockedPlayer.transform.position;
                        ProcessTagAura(GetPhotonViewFromRig(data.lockedPlayer).Owner);
                    }

                    if (GetGameMode().Contains("HUNT"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }

                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>()
                                .myTarget == GetPhotonViewFromRig(data.lockedPlayer).Owner)
                        {
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
                        if (GorillaBattleManager.playerLives
                                [GetPhotonViewFromRig(data.lockedPlayer).Owner.ActorNumber] > 0)
                        {
                            PhotonView pv = GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)").GetComponent<PhotonView>();
                            pv.RPC("ReportSlingshotHit", RpcTarget.MasterClient,
                                new object[]
                                {
                                    new Vector3(0, 0, 0), GetPhotonViewFromRig(data.lockedPlayer).Owner,
                                    RPCSUB.IncrementLocalPlayerProjectileCount()
                                });

                        }
                    }

                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
                else
                {
                    Player player = GetPhotonViewFromRig(data.lockedPlayer).Owner;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        if (GorillaTagManager.isCurrentlyTag)
                        {
                            GorillaTagManager.ChangeCurrentIt(player);
                        }
                        else
                        {
                            if (!GorillaTagManager.currentInfected.Contains(player))
                            {
                                GorillaTagManager.currentInfected.Add(player);
                            }
                        }
                    }

                    if (GetGameMode().Contains("HUNT"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.enabled)
                        {
                            Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
                        }

                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget == GetPhotonViewFromRig(data.lockedPlayer).Owner)
                        {
                            GorillaTagger.Instance.offlineVRRig.transform.position = GorillaGameManager.instance.FindPlayerVRRig(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget).transform.position;
                            ProcessTagAura(GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>().myTarget);
                            return;
                        }
                    }

                    if (GetGameMode().Contains("BATTLE"))
                    {
                        if (GorillaBattleManager.playerLives
                                [GetPhotonViewFromRig(data.lockedPlayer).Owner.ActorNumber] > 0)
                        {
                            GorillaBattleManager.playerLives
                                [GetPhotonViewFromRig(data.lockedPlayer).Owner.ActorNumber] = 0;
                        }
                    }
                }
                if (!data.isShooting || !data.isTriggered)
                {
                    ResetRig();
                }
            }

        }
        public static void TagSelf()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().currentInfected
                    .Add(PhotonNetwork.LocalPlayer);
                GorillaTagManager.instance.InfrequentUpdate();
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().UpdateState();
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().UpdateInfectionState();
                GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().UpdateTagState();
                GameMode.ReportHit();
            }
        }

        public static void TagLag()
        {
            GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().tagCoolDown = 9999999999;
        }

        public static void RevertTagLag()
        {
            GorillaGameManager.instance.gameObject.GetComponent<GorillaTagManager>().tagCoolDown = 5f;
        }

        public static void TagAll()
        {
            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers)
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    if (GetGameMode().Contains("INFECTION"))
                    {

                    }

                    if (GetGameMode().Contains("HUNT"))
                    {
                        if (GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>()
                                .myTarget !=
                            null)
                        {
                            if (GorillaTagger.Instance.offlineVRRig.enabled)
                            {
                                Steal.Patchers.VRRigPatchers.OnDisable.Prefix(GorillaTagger.Instance.offlineVRRig);
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

            GorillaTagger.Instance.offlineVRRig.enabled = true;

        }

        #endregion

    }
}
