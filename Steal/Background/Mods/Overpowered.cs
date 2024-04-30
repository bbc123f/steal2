using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using PlayFab.ClientModels;
using PlayFab;
using Steal.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Burst;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEngine.XR;
using HarmonyLib;
using GorillaTag;
using GorillaGameModes;
using System.Threading.Tasks;
using UnityEngine.Animations.Rigging;

namespace Steal.Background.Mods
{
    internal class Overpowered : Mod
    {
        public static GorillaBattleManager GorillaBattleManager;
        public static GorillaHuntManager GorillaHuntManager;
        public static GorillaTagManager GorillaTagManager;

        public static GorillaScoreBoard[] boards = null; 
        public static float antibancooldown = -2999999999999;
        public static bool antiban = true;
        static float boardTimer;
        static float slowallcooldown = -1;
        static float Vibrateallcooldown = -1;
        static bool floatmove;
        static float floatmoveT;
        static bool stopmove;
        static float stopmoveT;
        static float mattimer = 0;
        private static int iterator1;
        private static int copyListToArrayIndex;
        static bool ltagged = false;
        static float matguntimer = -222f;
        static float lagtimeout;
        public static float colorFloat = 0f;
        static Player crashedPlayer;

        public static void DisableNameOnJoin()
        {
            changeNameOnJoin = false;
        }

        public static void EnableNameOnJoin()
        {
            changeNameOnJoin = true;
        }

        public static void DisableStumpCheck()
        {
            StumpCheck = false;
        }

        public static void EnableStumpCheck()
        {
            StumpCheck = true;
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
        public static bool IsModded()
        {
            return PhotonNetwork.InRoom &&
                   PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED");
        }

        public static bool IsMaster()
        {
            return PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer.IsMasterClient;
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
            catch
            {
                throw;
            }

            return null;
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
        public static void switchAntiReport()
        {
            if (MenuPatch.antiReportCurrent == "Disconnect")
            {
                MenuPatch.antiReportCurrent = "Rejoin";
            }
            else if (MenuPatch.antiReportCurrent == "Rejoin")
            {
                MenuPatch.antiReportCurrent = "JoinRandom";
            }
            else if (MenuPatch.antiReportCurrent == "JoinRandom")
            {
                MenuPatch.antiReportCurrent = "Crash";
            }
            else if (MenuPatch.antiReportCurrent == "Crash")
            {
                MenuPatch.antiReportCurrent = "Float";
            }
            else if (MenuPatch.antiReportCurrent == "Float")
            {
                MenuPatch.antiReportCurrent = "Disconnect";
            }

        }
        public static void AntiReport()
        {
            try
            {
                MoveCrashHandler();
                foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (line.linePlayer.IsLocal)
                    {
                        Transform report = line.reportButton.gameObject.transform;
                        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                        {

                            if (vrrig != GorillaTagger.Instance.offlineVRRig && GetPhotonViewFromRig(vrrig) != null)
                            {
                                var owner = GetPhotonViewFromRig(vrrig).Owner;
                                float D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position);
                                float D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position);

                                float threshold = 0;

                                if (MenuPatch.antiReportCurrent == "Float")
                                {
                                    threshold = 6;
                                }
                                else if (MenuPatch.antiReportCurrent == "Crash")
                                {
                                    threshold = 6;
                                }
                                else
                                {
                                    threshold = 0.35f;
                                }

                                if (D1 < threshold || D2 < threshold || (IsVectorNear(vrrig.rightHandTransform.position, vrrig.leftHandTransform.position, .01f)))
                                {
                                    if (MenuPatch.antiReportCurrent == "Disconnect")
                                        PhotonNetwork.Disconnect();
                                    else if (MenuPatch.antiReportCurrent == "Rejoin")
                                    {
                                        string code = PhotonNetwork.CurrentRoom.Name;
                                        PhotonNetwork.Disconnect();
                                        PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code);
                                    }
                                    else if (MenuPatch.antiReportCurrent == "JoinRandom")
                                    {
                                        PhotonNetwork.Disconnect();
                                        PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.forestMapTrigger, false);
                                    }
                                    else if (MenuPatch.antiReportCurrent == "Crash")
                                    {
                                        if (!IsModded()) { return; }
                                        crashedPlayer = GetPhotonViewFromRig(vrrig).Owner;
                                        crashPlayerPosition = vrrig.transform.position;
                                        NameAll();
                                    }
                                    else if (MenuPatch.antiReportCurrent == "Float")
                                    {
                                        if (!IsMaster()) { return; }
                                        AngryBeeSwarm.instance.targetPlayer = owner;
                                        AngryBeeSwarm.instance.grabbedPlayer = owner;
                                        AngryBeeSwarm.instance.currentState = AngryBeeSwarm.ChaseState.Grabbing;
                                    }

                                    Notif.SendNotification(owner.NickName + " tried to report you, " + MenuPatch.antiReportCurrent + " " + PhotonNetwork.CurrentRoom.Name, Color.red);

                                }
                            }
                        }
                    }

                }
            }
            catch { }
        }

        static bool IsVectorNear(Vector3 vectorA, Vector3 vectorB, float threshold)
        {

            float distance = Vector3.Distance(vectorA, vectorB);

            return distance <= threshold;
        }
        public static void SoundSpam()
        {
            int randomSound = UnityEngine.Random.Range(0, 4);
            RPCSUB.SendSound(randomSound, 100);
        }
        public static bool isStumpChecking = false;
        public static float CheckedFor = 0;
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
            PhotonNetwork.Destroy(GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)"));
            Traverse.Create(gmInstance).Field("activeNetworkHandler").SetValue(null);
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

        public static void CheckForStump()
        {
            if (Time.time > CheckedFor + 1.5f)
            {
                StartAntiBan();
                CheckedFor = Time.time;
            }
        }
        public static void AcidMat(Photon.Realtime.Player player)
        {
            object obj;
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out obj);
            if (obj.ToString().Contains("MODDED"))
            {
                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount")
                    .SetValue(PhotonNetwork.CurrentRoom.PlayerCount);
                ScienceExperimentManager.PlayerGameState[] array = new ScienceExperimentManager.PlayerGameState[10];
                for (int i = 0; i < (int)PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    array[i].touchedLiquid = true;
                    array[i].playerId = player.ActorNumber;
                }

                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(array);
            }
        }

        public static void AcidMatAll(Photon.Realtime.Player player)
        {
            object obj;
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out obj);
            if (obj.ToString().Contains("MODDED"))
            {
                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount")
                    .SetValue(PhotonNetwork.CurrentRoom.PlayerCount);
                ScienceExperimentManager.PlayerGameState[] array = new ScienceExperimentManager.PlayerGameState[10];
                for (int i = 0; i < (int)PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    array[i].touchedLiquid = true;
                    array[i].playerId = PhotonNetwork.PlayerList[i] == null
                        ? 0
                        : PhotonNetwork.PlayerList[i].ActorNumber;
                }

                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(array);
            }
        }

        public static void AcidMatoff(Photon.Realtime.Player player = null)
        {
            object obj;
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out obj);
            if (obj.ToString().Contains("MODDED"))
            {
                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount")
                    .SetValue(PhotonNetwork.CurrentRoom.PlayerCount);
                ScienceExperimentManager.PlayerGameState[] array = new ScienceExperimentManager.PlayerGameState[10];
                for (int i = 0; i < (int)PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    array[i].touchedLiquid = true;
                    array[i].playerId = 900000000;
                }

                Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(array);
            }
        }
        public static void CrashGun()
        {
            if (!IsModded())
            {
                return;
            }
            CrashHandler();
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null && data.isLocked && GetPhotonViewFromRig(data.lockedPlayer) != null)
                {
                    crashedPlayer = GetPhotonViewFromRig(data.lockedPlayer).Owner;
                    crashPlayerPosition = data.lockedPlayer.transform.position;
                }
            }
        }

        public static void FloatGun()
        {
            if (!IsMaster()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null && data.isLocked && GetPhotonViewFromRig(data.lockedPlayer) != null)
                {
                    Player player = GetPhotonViewFromRig(data.lockedPlayer).Owner;
                    if (Time.time > floatmoveT)
                    {
                        floatmove = !floatmove;
                        floatmoveT = floatmove ? Time.time + 4f : Time.time + 0.2f;
                    }
                    if (floatmove)
                    {
                        AngryBeeSwarm.instance.targetPlayer = player;
                        AngryBeeSwarm.instance.grabbedPlayer = player;
                        AngryBeeSwarm.instance.currentState = AngryBeeSwarm.ChaseState.Grabbing;
                    }
                    else
                    {
                        AngryBeeSwarm.instance.targetPlayer = player;
                        AngryBeeSwarm.instance.grabbedPlayer = player;
                        AngryBeeSwarm.instance.currentState = AngryBeeSwarm.ChaseState.Dormant;
                    }
                }
            }
        }

        public static void GliderGun()
        {
            var data = GunLib.Shoot();
            if (data != null)
            {
                if (data.isShooting && data.isTriggered)
                {
                    foreach (Transform child in GameObject.Find("Environment Objects/PersistentObjects_Prefab/Gliders_Placement_Prefab/Root").transform)
                    {
                        foreach (Transform grandchild in child.transform)
                        {
                            GliderHoldable gh = grandchild.gameObject.GetComponent<GliderHoldable>();
                            gh.OnGrab(null, null);
                            gh.photonView.ControllerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
                            gh.photonView.OwnerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
                            GameObject.Find("Environment Objects/PersistentObjects_Prefab/Gliders_Placement_Prefab").transform.position = data.hitPosition;
                            GameObject.Find("Environment Objects/PersistentObjects_Prefab/Gliders_Placement_Prefab").transform.rotation = Quaternion.EulerAngles(UnityEngine.Random.Range(0.5f, 1.5f), UnityEngine.Random.Range(0.5f, 1.3f), UnityEngine.Random.Range(0.5f, 1.3f));
                            gh.gameObject.transform.position = data.hitPosition + new Vector3(UnityEngine.Random.Range(-3f, 3f), 2f, UnityEngine.Random.Range(-3f, 3f));

                        }
                    }
                }
            }
        }


        public static void GliderAll()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                foreach (Transform child in GameObject.Find("Environment Objects/PersistentObjects_Prefab/Gliders_Placement_Prefab/Root").transform)
                {
                    foreach (Transform grandchild in child.transform)
                    {
                        GliderHoldable gh = grandchild.gameObject.GetComponent<GliderHoldable>();
                        gh.OnGrab(null, null);
                        gh.photonView.ControllerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
                        gh.photonView.OwnerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
                        GameObject.Find("Environment Objects/PersistentObjects_Prefab/Gliders_Placement_Prefab").transform.position = grandchild.position;
                        GameObject.Find("Environment Objects/PersistentObjects_Prefab/Gliders_Placement_Prefab").transform.rotation = Quaternion.EulerAngles(UnityEngine.Random.Range(0.5f, 1.5f), UnityEngine.Random.Range(0.5f, 1.3f), UnityEngine.Random.Range(0.5f, 1.3f));
                        gh.gameObject.transform.position = vrrig.transform.position + new Vector3(UnityEngine.Random.Range(-3f, 3f), 2f, UnityEngine.Random.Range(-3f, 3f));

                    }
                }
            }

        }






        public static void StopMovement()
        {
            if (!IsMaster()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null && data.isLocked && GetPhotonViewFromRig(data.lockedPlayer) != null)
                {
                    Player player = GetPhotonViewFromRig(data.lockedPlayer).Owner;
                    if (Time.time > stopmoveT)
                    {
                        stopmoveT = Time.time + 0.4f;
                        stopmove = !stopmove;
                    }
                    if (stopmove)
                    {
                        AngryBeeSwarm.instance.targetPlayer = player;
                        AngryBeeSwarm.instance.grabbedPlayer = player;
                        AngryBeeSwarm.instance.currentState = AngryBeeSwarm.ChaseState.Grabbing;
                    }
                    else
                    {
                        AngryBeeSwarm.instance.targetPlayer = player;
                        AngryBeeSwarm.instance.grabbedPlayer = player;
                        AngryBeeSwarm.instance.currentState = AngryBeeSwarm.ChaseState.Dormant;
                    }
                }
            }
        }
        public static void VibrateGun()
        {
            if (!IsMaster()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null)
                {
                    if (Time.time > Vibrateallcooldown + 0.5 && GetPhotonViewFromRig(data.lockedPlayer) != null)
                    {
                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                        {
                            RPCSUB.JoinedTaggedTime(GetPhotonViewFromRig(data.lockedPlayer).Owner);
                        }
                        Vibrateallcooldown = Time.time;
                    }
                }
            }
        }

        public static void SlowGun()
        {
            if (!IsMaster()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null)
                {
                    if (Time.time > slowallcooldown + 0.5 && GetPhotonViewFromRig(data.lockedPlayer) != null)
                    {
                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                        {
                            RPCSUB.SetTaggedTime(GetPhotonViewFromRig(data.lockedPlayer).Owner);
                        }
                        slowallcooldown = Time.time;
                    }
                }
            }
        }

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

        public static void TrapAllInStump()
        {
            if (IsModded() && IsMaster())
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
            }
        }

        static string oldTriggers;

        public static void DisableNetworkTriggers()
        {
            if (IsMaster() && IsModded())
            {
                string[] maps =
                {
                "beach", "city", "basement", "clouds", "forest", "mountains", "canyon", "cave"
            };
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
                oldTriggers = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString();
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetCustomPropertiesOfRoom(hash);
            }
        }

        public static void EnableNetworkTriggers()
        {
            var hash = new Hashtable { { "gameMode", oldTriggers } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetCustomPropertiesOfRoom(hash);
        }

        public static void sscosmetic()
        {
            foreach (GorillaNetworking.CosmeticsController.CosmeticItem item in GorillaNetworking.CosmeticsController.instance.allCosmetics)
            {
                //CosmeticsController.instance.UnlockItem("LBAAK.");
                if (item.itemName == "LBAFV.")
                {
                    GorillaNetworking.CosmeticsController.instance.itemToBuy = item;
                }
                GorillaNetworking.CosmeticsController.instance.PurchaseItem();
                if (item.itemName == "LBAAK.")
                {
                    GorillaNetworking.CosmeticsController.instance.itemToBuy = item;
                }

                GorillaNetworking.CosmeticsController.instance.PurchaseItem();
            }
        }

        public static void NameAll()
        {
            if (!IsModded()) { return; }
            if (Time.time > boardTimer)
            {
                boardTimer = Time.time + 0.045f;
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable[byte.MaxValue] = PhotonNetwork.LocalPlayer.NickName;
                    Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
                    dictionary.Add(251, hashtable);
                    dictionary.Add(254, p.ActorNumber);
                    dictionary.Add(250, true);
                    PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer.SendOperation(252, dictionary, SendOptions.SendUnreliable);
                }
            }
        }

        public static void NameGun()
        {

            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null && GetPhotonViewFromRig(data.lockedPlayer) != null)
                {
                    if (Time.time > boardTimer)
                    {
                        boardTimer = Time.time + 0.06f;
                        Hashtable hashtable = new Hashtable();
                        hashtable[byte.MaxValue] = PhotonNetwork.LocalPlayer.NickName;
                        Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
                        dictionary.Add(251, hashtable);
                        dictionary.Add(254, GetPhotonViewFromRig(data.lockedPlayer).OwnerActorNr);
                        dictionary.Add(250, true);
                        PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer.SendOperation(252, dictionary, SendOptions.SendUnreliable);
                    }
                }
            }
        }
        public static bool InStumpCheck()
        {
            isStumpChecking = true;

            Transform objects = GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab")
                .transform;
            foreach (Player p in PhotonNetwork.PlayerListOthers)
            {
                if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(p.UserId))
                {
                    return false;
                }

                foreach (Transform child in objects)
                {
                    if (IsVectorNear(GorillaGameManager.instance.FindPlayerVRRig(p).transform.position, child.position,
                            6f))
                    {
                        return false;
                    }
                }


            }

            return true;
        }
        public static void StartAntiBan()
        {
            try
            {
                if (IsModded()) 
                {
                    PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                    Notif.SendNotification("AntiBan Already Enabled Or Your Not In A Lobby!", Color.white); 
                    return; 
                }


                if (StumpCheck)
                {
                    if (!InStumpCheck())
                    {
                        Notif.SendNotification(
                            "<color=red>A Player is about to leave/In stump!..</color> <color=green>Retrying..</color>", Color.white);
                        return;
                    }
                }

                if (PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.PeerState != ExitGames.Client.Photon.PeerStateValue.Connected) { Notif.SendNotification("Please wait until the lobby has fully loaded!", Color.white); return; }
                AntiBan();
            }
            catch
            {
                Debug.LogError("Unknown Error!");
                throw;
            }
        }

        public static void AntiBan()
        {
            isStumpChecking = false;
            MenuPatch.isRunningAntiBan = false;
            Debug.Log("Running...");
            string gamemode = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Replace(GorillaComputer.instance.currentGameMode.Value, "MODDED_MODDED" + GorillaComputer.instance.currentGameMode.Value);
            ExitGames.Client.Photon.Hashtable gamehash = new ExitGames.Client.Photon.Hashtable
            {
                { "gameMode", gamemode }
            };
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(gamehash, null, null);


            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "RoomClosed",
                FunctionParameter = new
                {
                    GameId = PhotonNetwork.CurrentRoom.Name,
                    Region = Regex.Replace(PhotonNetwork.CloudRegion, "[^a-zA-Z0-9]", "").ToUpper(),
                    ActorCount = 0,
                    AppVersion = PhotonNetwork.AppVersion,
                    AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime,
                    Type = "Close"
                }
            },
            delegate (ExecuteCloudScriptResult result)
            {

            }, error => { }, null, null);

            Notif.ClearAllNotifications();
            Notif.SendNotification("Antiban and Set Master Enabled!", Color.white);

            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);

            MenuPatch.isRunningAntiBan = false;
            antibancooldown = Time.time + 10;
        }

        public static void matSpamAll()
        {
            if (!IsMaster()) { return; }
            saveKeys();
            if (GetGameMode().Contains("INFECTION"))
            {
                MatAllInf();
                return;
            }
            else if (Time.time > mattimer)
            {
                if (ltagged)
                {
                    ltagged = false;
                    if (GetGameMode().Contains("INFECTION"))
                    {
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
                mattimer = Time.time + 0.08f;
            }
        }
        public static void matSpamselggg()
        {
            if (!IsMaster()) { return; }
            saveKeys();
            if (Time.time > mattimer)
            {
                if (ltagged)
                {
                    ltagged = false;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        GorillaTagManager.currentInfected.Remove(PhotonNetwork.LocalPlayer);
                        CopyInfectedListToArray();
                    }
                    else if (GetGameMode().Contains("HUNT"))
                    {
                        GorillaHuntManager.currentHunted.Remove(PhotonNetwork.LocalPlayer);
                        CopyHuntDataListToArray();
                    }
                    else if (GetGameMode().Contains("BATTLE"))
                    {
                        GorillaBattleManager.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 0;
                    }
                }
                else
                {
                    ltagged = true;
                    if (GetGameMode().Contains("INFECTION"))
                    {
                        GorillaTagManager.currentInfected.Add(PhotonNetwork.LocalPlayer);
                        CopyInfectedListToArray();
                    }
                    else if (GetGameMode().Contains("HUNT"))
                    {
                        GorillaHuntManager.currentHunted.Add(PhotonNetwork.LocalPlayer);
                        CopyHuntDataListToArray();
                    }
                    else if (GetGameMode().Contains("BATTLE"))
                    {
                        GorillaBattleManager.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 3;
                        CopyHuntDataListToArray();
                    }
                }
                mattimer = Time.time + 0.08f;
            }
        }
        public static void matSpamOnTouch()
        {
            if (!IsModded() && !IsMaster()) { return; }
            foreach (VRRig rigs in GorillaParent.instance.vrrigs)
            {
                float rightDistance = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.transform.position, rigs.transform.position);
                float leftDistance = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.transform.position, rigs.transform.position);
                float bodyDistance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, rigs.transform.position);
                if ((rightDistance <= 0.3 || leftDistance <= 0.3 || bodyDistance <= 0.5) && !rigs.isMyPlayer && !rigs.isOfflineVRRig)
                {
                    saveKeys();
                    Photon.Realtime.Player owner = GetPhotonViewFromRig(rigs).Owner;
                    if (owner != null && Time.time > matguntimer)
                    {
                        matguntimer = Time.time + 0.1f;
                        if (ltagged)
                        {
                            ltagged = false;
                            if (GetGameMode().Contains("INFECTION"))
                            {
                                GorillaTagManager.currentInfected.Remove(owner);
                                CopyInfectedListToArray();
                            }
                            else if (GetGameMode().Contains("HUNT"))
                            {
                                GorillaHuntManager.currentHunted.Remove(owner);
                                GorillaHuntManager.currentTarget.Remove(owner);
                                CopyHuntDataListToArray();
                            }
                            else if (GetGameMode().Contains("BATTLE"))
                            {
                                GorillaBattleManager.playerLives[owner.ActorNumber] = 0;
                            }
                        }
                        else
                        {
                            ltagged = true;
                            if (GetGameMode().Contains("INFECTION"))
                            {
                                GorillaTagManager.currentInfected.Add(owner);
                                CopyInfectedListToArray();
                            }
                            else if (GetGameMode().Contains("HUNT"))
                            {
                                GorillaHuntManager.currentHunted.Add(owner);
                                GorillaHuntManager.currentTarget.Add(owner);
                                CopyHuntDataListToArray();
                            }
                            else if (GetGameMode().Contains("BATTLE"))
                            {
                                GorillaBattleManager.playerLives[owner.ActorNumber] = 3;
                            }
                        }
                    }

                }
            }
        }

        private static void CopyHuntDataListToArray()
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
        private static void CopyInfectedListToArray()
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

        public static void SetMaster()
        {
            if (IsModded())
            {
                if (!IsMaster())
                {
                    PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                }
                else
                {
                    Notif.SendNotification("You are already masterclient!", Color.red);
                }
            }
            else
            {
                Notif.SendNotification("Enable AntiBan!!", Color.red);
            }
        }
        static bool shouldacidchange = false;
        static float canacidchange = -100;

        public static void AcidSpam()
        {
            if (Time.time > canacidchange)
            {
                canacidchange = Time.time + 0.8f;
                if (shouldacidchange)
                {
                    Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
                    ScienceExperimentManager.PlayerGameState[]
                        states = new ScienceExperimentManager.PlayerGameState[10];
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
                    ScienceExperimentManager.PlayerGameState[]
                        states = new ScienceExperimentManager.PlayerGameState[10];
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


        public static void AcidSelf()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance)
                .Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                {
                    states[i].playerId = PhotonNetwork.LocalPlayer.ActorNumber;
                    states[i].touchedLiquid = true;
                }
            }

            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
        }

        public static void UnAcidSelf()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(0);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance)
                .Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                {
                    states[i].playerId = PhotonNetwork.LocalPlayer.ActorNumber;
                    states[i].touchedLiquid = false;
                }
            }

            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
        }

        public static void UnAcidAll()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(0);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance)
                .Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                states[i].touchedLiquid = false;
                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
            }

            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
        }

        public static void AcidAll()
        {
            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerCount").SetValue(10);
            ScienceExperimentManager.PlayerGameState[] states = Traverse.Create(ScienceExperimentManager.instance)
                .Field("inGamePlayerStates").GetValue<ScienceExperimentManager.PlayerGameState[]>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                states[i].touchedLiquid = true;
                states[i].playerId = PhotonNetwork.PlayerList[i] == null ? 0 : PhotonNetwork.PlayerList[i].ActorNumber;
            }

            Traverse.Create(ScienceExperimentManager.instance).Field("inGamePlayerStates").SetValue(states);
        }

        private static GorillaTagManager manager = null;
        public static void MatAllInf()
        {
            if (manager == null)
            {
                manager = GameObject.Find("Gorilla Tag Manager").GetComponent<GorillaTagManager>();
            }
            if (Time.time > a + 0.084f)
            {
                foreach (var player in PhotonNetwork.PlayerListOthers)
                {
                    if (ewenum == 0)
                    {
                        UnAcidAll();
                        manager.currentInfected.Add(player);
                    }
                    else if (ewenum == 1)
                    {
                        AcidAll();
                    }
                    else if (ewenum == 2)
                    {
                        manager.currentInfected.Remove(player);
                    }
                }

                if (ewenum != 2)
                {
                    ewenum++;
                }
                else
                {
                    ewenum = 0;
                }

                a = Time.time;
            }
        }

        private static float a = 0;

        public static void CrashAll2()
        {
            if (!IsModded()) { return; }
            if (doorView == null)
            {
                doorView = GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/ChangeDoorState").GetComponent<PhotonView>();
            }

            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });

            if ((Mathf.RoundToInt(1f / UI.deltaTime) < 100))
            {
                doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
            }
        }


        private static Vector3 crashPlayerPosition = Vector3.zero;
        private static int ewenum;

        public static void MoveCrashHandler()
        {
            if (crashedPlayer != null)
            {
                if (crashedPlayer.InRoom())
                {
                    colorFloat = Mathf.Repeat(colorFloat + Time.deltaTime * float.PositiveInfinity, 1f);

                    if (doorView == null)
                    {
                        doorView = GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/ChangeDoorState").GetComponent<PhotonView>();
                    }

                    doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                    doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                    doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                }
                else
                {
                    crashedPlayer = null;
                }
            }
        }



        private static Dictionary<Player, Vector3> crashedPlayers = new Dictionary<Player, Vector3>();
        private static bool changeNameOnJoin;

        public static bool StumpCheck = true;

        public static void CrashHandlerMulti()
        {
            List<Player> playersToRemove = new List<Player>();
            Dictionary<Player, Vector3> playersToUpdate = new Dictionary<Player, Vector3>();

            foreach (var pair in crashedPlayers)
            {
                Player player = pair.Key;
                Vector3 lastKnownPosition = pair.Value;

                if (player == null || !player.InRoom())
                {
                    playersToRemove.Add(player);
                }
                else
                {
                    Vector3 currentPosition = GorillaGameManager.instance.FindPlayerVRRig(player).transform.position;
                    if (lastKnownPosition != currentPosition)
                    {
                        playersToUpdate[player] = currentPosition;
                        UpdatePlayerColor(player);
                        UpdatePlayerColor(player);
                    }
                }
            }

            foreach (var player in playersToRemove)
            {
                crashedPlayers.Remove(player);
            }

            foreach (var update in playersToUpdate)
            {
                crashedPlayers[update.Key] = update.Value;
            }
        }



        private static void UpdatePlayerColor(Player player)
        {
            if (doorView == null)
            {
                doorView = GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/ChangeDoorState").GetComponent<PhotonView>();
            }

            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
        }

        public static void CrashHandler()
        {
            if (!IsModded()) { return; }
            if (crashedPlayer != null)
            {
                if (crashedPlayer.InRoom())
                {
                    colorFloat = Mathf.Repeat(colorFloat + Time.deltaTime * float.PositiveInfinity, 1f);

                    if (doorView == null)
                    {
                        doorView = GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/ChangeDoorState").GetComponent<PhotonView>();
                    }

                    doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                    doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                    doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                    if (XRSettings.isDeviceActive && (Mathf.RoundToInt(1f / UI.deltaTime) < 100))
                    {
                        if (crashPlayerPosition != GorillaGameManager.instance.FindPlayerVRRig(crashedPlayer).transform.position)
                        {
                            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                            crashPlayerPosition = GorillaGameManager.instance.FindPlayerVRRig(crashedPlayer).transform.position;
                        }
                    }
                    else if ((Mathf.RoundToInt(1f / UI.deltaTime) < 100))
                    {
                        doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                        doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
                    }
                }
                else
                {
                    crashedPlayer = null;
                }
            }
        }

        public static void LagAl()
        {
            if (!IsModded()) { return; }
            Lag(RpcTarget.Others);
        }

        static PhotonView doorView = null;

        public static void Lag(Player target)
        {
            if (!IsModded()) { return; }

            if (doorView == null)
            {
                doorView = GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/ChangeDoorState").GetComponent<PhotonView>();
            }

            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
        }

        public static void Lag(RpcTarget target)
        {
            if (!IsModded()) { return; }

            if (doorView == null)
            {
                doorView = GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/ChangeDoorState").GetComponent<PhotonView>();
            }

            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
            doorView.RpcSecure("ChangeDoorState", RpcTarget.Others, true, new object[] { default(GTDoor.DoorState) });
        }

        public static void LagGun()
        {
            if (!IsModded()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null && data.isLocked && GetPhotonViewFromRig(data.lockedPlayer) != null)
                {
                    Lag(GetPhotonViewFromRig(data.lockedPlayer).Owner);
                }

            }


        }

        public static void CrashOnTouch()
        {
            if (!IsModded()) { return; }
            CrashHandler();
            foreach (VRRig rigs in GorillaParent.instance.vrrigs)
            {
                if (!rigs.isMyPlayer && !rigs.isOfflineVRRig)
                {
                    float rightDistance = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.transform.position, rigs.transform.position);
                    float leftDistance = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.transform.position, rigs.transform.position);
                    float bodyDistance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, rigs.transform.position);

                    float rightDistanceother = Vector3.Distance(rigs.rightHandTransform.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    float leftDistanceother = Vector3.Distance(rigs.leftHandTransform.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    float bodyDistanceother = Vector3.Distance(rigs.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    if (Time.time > lagtimeout + 0.002)
                    {
                        lagtimeout = Time.time;
                        if ((rightDistance <= 0.3 || leftDistance <= 0.3 || bodyDistance <= 0.5) || (rightDistanceother <= 0.3 || leftDistanceother <= 0.3 || bodyDistanceother <= 0.5))
                        {
                            if (rightDistance <= 0.3f)
                            {
                                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength / 2, GorillaTagger.Instance.tagHapticDuration / 2);
                            }
                            if (leftDistance <= 0.3f)
                            {
                                GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength / 2, GorillaTagger.Instance.tagHapticDuration / 2);
                            }
                            crashedPlayer = GetPhotonViewFromRig(rigs).Owner;
                            crashPlayerPosition = rigs.transform.position;
                        }
                    }
                }
            }

        }

        public static void LagOnTouch()
        {
            if (!IsModded()) { return; }
            foreach (VRRig rigs in GorillaParent.instance.vrrigs)
            {
                if (!rigs.isMyPlayer && !rigs.isOfflineVRRig && GetPhotonViewFromRig(rigs) != null)
                {
                    float rightDistance = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.transform.position, rigs.transform.position);
                    float leftDistance = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.transform.position, rigs.transform.position);
                    float bodyDistance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, rigs.transform.position);

                    float rightDistanceother = Vector3.Distance(rigs.rightHandTransform.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    float leftDistanceother = Vector3.Distance(rigs.leftHandTransform.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    float bodyDistanceother = Vector3.Distance(rigs.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    if (Time.time > lagtimeout + 0.002)
                    {
                        lagtimeout = Time.time;
                        if ((rightDistance <= 0.3 || leftDistance <= 0.3 || bodyDistance <= 0.5) || (rightDistanceother <= 0.3 || leftDistanceother <= 0.3 || bodyDistanceother <= 0.5))
                        {
                            if (rightDistance <= 0.3f)
                            {
                                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength / 2, GorillaTagger.Instance.tagHapticDuration / 2);
                            }
                            if (leftDistance <= 0.3f)
                            {
                                GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength / 2, GorillaTagger.Instance.tagHapticDuration / 2);
                            }
                            Lag(GetPhotonViewFromRig(rigs).Owner);

                        }
                    }
                }
            }

        }

        public static void StutterGun()
        {
            if (!IsModded()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null && data.isLocked && GetPhotonViewFromRig(data.lockedPlayer) != null)
                {
                    if (data.lockedPlayer != null)
                    {
                        var pv = GetPhotonViewFromRig(data.lockedPlayer);
                        if (pv != null)
                        {
                            PhotonNetwork.NetworkingClient.OpRaiseEvent((byte)204, new Hashtable
                            {
                                { 0, pv.ViewID }
                            }, new RaiseEventOptions
                            {
                                TargetActors = new int[]
                                {
                                    pv.Owner.ActorNumber
                                }
                            }, SendOptions.SendUnreliable);
                        }
                    }
                }
            }
        }

        public static void StutterOnTouch()
        {
            if (!IsModded()) { return; }
            foreach (VRRig rigs in GorillaParent.instance.vrrigs)
            {
                if (!rigs.isMyPlayer && !rigs.isOfflineVRRig && GetPhotonViewFromRig(rigs) != null)
                {
                    float rightDistance = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.transform.position, rigs.transform.position);
                    float leftDistance = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.transform.position, rigs.transform.position);
                    float bodyDistance = Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, rigs.transform.position);

                    float rightDistanceother = Vector3.Distance(rigs.rightHandTransform.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    float leftDistanceother = Vector3.Distance(rigs.leftHandTransform.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    float bodyDistanceother = Vector3.Distance(rigs.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);

                    if ((rightDistance <= 0.3 || leftDistance <= 0.3 || bodyDistance <= 0.5) || (rightDistanceother <= 0.3 || leftDistanceother <= 0.3 || bodyDistanceother <= 0.5))
                    {
                        if (rightDistance <= 0.3f)
                        {
                            GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength / 2, GorillaTagger.Instance.tagHapticDuration / 2);
                        }
                        if (leftDistance <= 0.3f)
                        {
                            GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength / 2, GorillaTagger.Instance.tagHapticDuration / 2);
                        }
                        if (rigs != null)
                        {
                            var pv = GetPhotonViewFromRig(rigs);
                            if (pv != null)
                            {
                                PhotonNetwork.NetworkingClient.OpRaiseEvent((byte)204, new Hashtable
                            {
                                { 0, pv.ViewID }
                            }, new RaiseEventOptions
                            {
                                TargetActors = new int[]
                                {
                                    pv.Owner.ActorNumber
                                }
                            }, SendOptions.SendUnreliable);
                            }
                        }
                    }
                }
            }
        }
        static float stutterTimeout;
        public static void StutterAll()
        {
            if (IsModded())
            {
                foreach (var rig in GorillaParent.instance.vrrigs)
                {
                    if (rig != null)
                    {
                        var pv = GetPhotonViewFromRig(rig);
                        if (pv != null && Time.time > stutterTimeout)
                        {
                            stutterTimeout = Time.time + 0.002f;
                            PhotonNetwork.SendRate = 1;
                            PhotonNetwork.NetworkingClient.OpRaiseEvent((byte)204, new Hashtable
                            {
                                { 0, pv.ViewID }
                            }, new RaiseEventOptions
                            {
                                Receivers = ReceiverGroup.Others
                            }, SendOptions.SendUnreliable);
                        }
                    }
                }
            }
        }

        public static void MatGun()
        {
            if (!IsMaster()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null)
                {
                    saveKeys();
                    Photon.Realtime.Player owner = GetPhotonViewFromRig(data.lockedPlayer).Owner;
                    if (owner != null && Time.time > matguntimer)
                    {
                        matguntimer = Time.time + 0.1f;
                        if (ltagged)
                        {
                            ltagged = false;
                            if (GetGameMode().Contains("INFECTION"))
                            {
                                GorillaTagManager.currentInfected.Remove(owner);
                                CopyInfectedListToArray();
                            }
                            else if (GetGameMode().Contains("HUNT"))
                            {
                                GorillaHuntManager.currentHunted.Remove(owner);
                                GorillaHuntManager.currentTarget.Remove(owner);
                                CopyHuntDataListToArray();
                            }
                            else if (GetGameMode().Contains("BATTLE"))
                            {
                                GorillaBattleManager.playerLives[owner.ActorNumber] = 0;
                            }
                        }
                        else
                        {
                            ltagged = true;
                            if (GetGameMode().Contains("INFECTION"))
                            {
                                GorillaTagManager.currentInfected.Add(owner);
                                CopyInfectedListToArray();
                            }
                            else if (GetGameMode().Contains("HUNT"))
                            {
                                GorillaHuntManager.currentHunted.Add(owner);
                                GorillaHuntManager.currentTarget.Add(owner);
                                CopyHuntDataListToArray();
                            }
                            else if (GetGameMode().Contains("BATTLE"))
                            {
                                GorillaBattleManager.playerLives[owner.ActorNumber] = 3;
                            }
                        }
                    }
                }
            }
        }

        static RequestableOwnershipGuard[] requestableOwnerships;

        public static void KickGun()
        {
            if (!IsModded()) { return; }
            var data = GunLib.ShootLock();
            if (data != null)
            {
                if (data.lockedPlayer != null && GetPhotonViewFromRig(data.lockedPlayer) != null)
                {
                    var ow = GetPhotonViewFromRig(data.lockedPlayer).Owner;
                    if (requestableOwnerships == null)
                    {
                        requestableOwnerships = Resources.FindObjectsOfTypeAll<RequestableOwnershipGuard>();
                    }
                    for (int i = 0; i < 251; i++)
                    {
                        var rnum = UnityEngine.Random.Range(0, requestableOwnerships.Length);
                        if (requestableOwnerships[rnum].isActiveAndEnabled && requestableOwnerships[rnum].photonView.AmOwner)
                        {
                            var t = requestableOwnerships[rnum];
                            t.currentState = NetworkingState.ForcefullyTakingOverWaitingForSight;
                            //t.photonView.RPC("SetOwnershipFromMasterClient", ow, PhotonNetwork.LocalPlayer, )
                        }
                    }
                }
            }
        }
    }
}
