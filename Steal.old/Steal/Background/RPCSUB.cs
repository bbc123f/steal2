using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GorillaGameModes;
using Photon.Voice.PUN;
using Steal.Patchers.GorillaNotPatchers;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

namespace Steal.Background
{
    internal class RPCSUB : MonoBehaviourPunCallbacks
    {

        static byte PROJECTILE_ID = 0;
        static byte IMPACT_ID = 1;
        static byte EFFECT_ID = 2;
        static byte SOUND = 3;
        static byte JOINWITHFRIEND_ID = 4;
        static byte TAG_ID = 5;

        public static Type RoomSystemInstance;

        static int ProjectileCount = 0;

        static RaiseEventOptions Others = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            CachingOption = EventCaching.DoNotCache
        };

        static RaiseEventOptions All = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.DoNotCache
        };

        static RaiseEventOptions Master = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.MasterClient,
            CachingOption = EventCaching.DoNotCache
        };

        public override void OnConnected()
        {
            base.OnConnected();
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
            //PhotonNetwork.NetworkingClient.EventReceived += MainManager.Instance.PlatformNetwork;
        }

        public string getButtonType(int type)
        {
            if (type == 0)
            {
                return "Hate Speach";
            }

            if (type == 1)
            {
                return "Cheating";
            }

            if (type == 2)
            {
                return "Toxicity";
            }

            return "Other";
        }

        public void OnEvent(EventData ev)
        {
            if (ev.Code == 8)
            {
                object[] arr = (object[])ev.CustomData;
                string customdata = "";
                customdata = $"{arr[4].ToString()} was reported by the anticheat for {arr[5].ToString()}";
                ShowConsole.Log(customdata);
                Notif.SendNotification($"{arr[4].ToString()} was reported by the anticheat for {arr[5].ToString()}");
            }

            if (ev.Code == 50)
            {
                object[] arr = (object[])ev.CustomData;
                string customdata = "";
                customdata =
                    $"{arr[2].ToString()} was reported by {arr[3].ToString()} for {getButtonType(int.Parse(arr[3].ToString()))}";
                ShowConsole.Log(customdata);
                Notif.SendNotification(
                    $"{arr[2].ToString()} was reported by {arr[3].ToString()} for {getButtonType(int.Parse(arr[3].ToString()))}");
            }
        }

        public static int[] getActorNumbers(Player[] list)
        {
            List<int> result = new List<int>();
            foreach (Player p in list)
            {
                result.Add(p.ActorNumber);
            }

            return result.ToArray();
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            GameObject ms = GameObject.Find("Steal");
            ms.GetComponent<Main>().shouldThing = true;
            Mods.Mods.leaves.Clear();
           
            foreach (GameObject g in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (g.activeSelf && g.name.Contains("smallleaves"))
                {
                    Mods.Mods.leaves.Add(g);
                }
            }
        }

        public static Texture2D ConvertToTexture2D(Texture texture)
        {
            Texture2D convertedTexture = new Texture2D(texture.width, texture.height);

            convertedTexture.SetPixels((texture as Texture2D).GetPixels());

            convertedTexture.Apply();

            return convertedTexture;
        }

        private static StealGUI _stealGUI = new StealGUI();



        public static async void thing()
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            


            if (PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.PeerState ==
                ExitGames.Client.Photon.PeerStateValue.Connected)
            {
                banwait = Time.time;
                IsAntiBaaaaa = true;
                stopwatch.Stop();
            }
            
            
            GameObject ms = GameObject.Find("Steal");
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig.mainSkin.material.name.Contains("fected"))
                {
                    ms.GetComponent<StealGUI>().infectTexture =
                        (ConvertToTexture2D(vrrig.mainSkin.material.mainTexture));
                }
                else
                {
                    ms.GetComponent<StealGUI>().notInfectTexture =
                        (ConvertToTexture2D(vrrig.mainSkin.material.mainTexture));
                }
            }
        }

        public static bool IsAntiBaaaaa = false;
        public static void AntiBan2()
        {
            if (IsAntiBaaaaa)
            {
                if (Time.time > banwait + 7)
                {
                    AntiNot.gorillanot = GorillaNot.instance; 
                    AntiNot.antiNot = true;
                    Mods.Mods.AntiBan();
                    Notif.SendNotification(
                        "<color=green>[STEAL]</color><color=cyan> AntiBan And Master Set Automatically </color>");
                    IsAntiBaaaaa = false;
                    banwait = Time.time;
                }
            }
        }


        private static float banwait = 0;
        static float t;
        static int eventsraised = 0;
        internal static void SendEvent(in byte code, in object evData, in RaiseEventOptions r) //  This shit makes me so fucking mad
        {
            var s = new object[3];
            s[0] = PhotonNetwork.ServerTimestamp;
            s[1] = code;
            s[2] = evData;
            bool result = PhotonNetwork.NetworkingClient.OpRaiseEvent(3, s, r, SendOptions.SendReliable);// you can change this to relibale and it still works just how they did it
            ShowConsole.Log("EVENT SENT: " + code + " STATUS: " + result);// you can delete this i just did it for ShowConsoleing puoposes
            float cooldown = 0.3f;
            switch (code) // nutty as but it works (if you're haveing problems with the effects change #2's cooldown time)
            {
                case 0:
                    cooldown = 0.1f;
                    break;
                case 1:
                    cooldown = 0.1f;
                    break;
                case 2:
                    cooldown = 1.9f;
                    break;
                case 3:
                    cooldown = 0.4f;
                    break;
                case 4:
                    cooldown = -1;
                    break;
                case 5:
                    cooldown = -1;
                    break;
            }
            t = Time.time + cooldown;
        }

        #region EFFECTS

        public static void SetSlowedTime(ReceiverGroup group)
        {
            if (PhotonNetwork.IsMasterClient && Time.time > t)
            {
                var e = new object[]
                {
                    2
                };
                SendEvent(EFFECT_ID, e, new RaiseEventOptions
                {
                    CachingOption = EventCaching.DoNotCache,
                    Receivers = ReceiverGroup.Others,
                });
            }
        }

        public static void SetTaggedTime(ReceiverGroup group)
        {
            if (PhotonNetwork.IsMasterClient && Time.time > t)
            {
                var e = new object[]
                {
                    0
                };
                SendEvent(EFFECT_ID, e, new RaiseEventOptions
                {
                    CachingOption = EventCaching.DoNotCache,
                    Receivers = ReceiverGroup.Others,
                });
            }
        }

        public static void JoinedTaggedTime(ReceiverGroup group)
        {
            if (PhotonNetwork.IsMasterClient && Time.time > t)
            {
                var e = new object[]
                {
                    1
                };
                SendEvent(EFFECT_ID, e, new RaiseEventOptions
                {
                    CachingOption = EventCaching.DoNotCache,
                    Receivers = ReceiverGroup.Others,
                });
            }
        }

        public static void SetSlowedTime(Player group)
        {
            if (PhotonNetwork.IsMasterClient && Time.time > t)
            {
                var Others = new RaiseEventOptions()
                {
                    TargetActors = new int[] { group.ActorNumber },
                };
                var e = new object[]
                {
                    2
                };
                SendEvent(EFFECT_ID, e, Others);
            }
        }

        public static void SetTaggedTime(Player group)
        {
            if (PhotonNetwork.IsMasterClient && Time.time > t)
            {
                var Others = new RaiseEventOptions()
                {
                    TargetActors = new int[] { group.ActorNumber },
                };
                var e = new object[]
                {
                    0
                };
                SendEvent(EFFECT_ID, e, Others);
                Mods.Mods.rpcReset();
            }
        }

        public static void JoinedTaggedTime(Player group)
        {
            if (PhotonNetwork.IsMasterClient && Time.time > t)
            {
                var Others = new RaiseEventOptions()
                {
                    TargetActors = new int[] { group.ActorNumber },
                };
                var e = new object[]
                {
                    1
                };
                SendEvent(EFFECT_ID, e, Others);
                Mods.Mods.rpcReset();
            }
        }

        #endregion

        #region Projectiles

        public static int IncrementLocalPlayerProjectileCount()
        {
            return ProjectileCount++;
        }
        public static object[] impactdata = new object[6];
        public static void SendImpactEffect(Vector3 position, float r, float g, float b, float a)
        {
            if (Time.time > t)
            {
                // i couldnt figure this out, you do it
                impactdata[0] = position;
                impactdata[1] = r;
                impactdata[2] = g;
                impactdata[3] = b;
                impactdata[4] = a;
                impactdata[5] = ProjectileCount;
                object data = (object)impactdata;
                SendEvent(IMPACT_ID, data, new RaiseEventOptions
                {
                    CachingOption = EventCaching.DoNotCache,
                    Receivers = ReceiverGroup.Others
                });
            }
        }
        public static object[] projdata = new object[11];// you have to do this for some reason
        public static void SendLaunchProjectile(Vector3 position, Vector3 velocity, int projectileHash, int trailHash, bool leftHanded, bool randomColour, float r, float g, float b, float a)
        {
            if (Time.time > t && Vector3.Distance(GorillaTagger.Instance.offlineVRRig.transform.position, position) < 4)
            {
                GorillaTagger.Instance.offlineVRRig.slingshot.currentState = TransferrableObject.PositionState.Dropped;
                GorillaTagger.Instance.offlineVRRig.slingshot.projectilePrefab = ObjectPools.instance.GetPoolByHash(projectileHash).objectToPool;
                GorillaTagger.Instance.offlineVRRig.slingshot.projectileTrail = ObjectPools.instance.GetPoolByHash(1432124712).objectToPool;
                //scuffed ash but works i think
                GameObject comp = ObjectPools.instance.Instantiate(projectileHash);
                SlingshotProjectile component = comp.GetComponent<SlingshotProjectile>();
                component.Launch(position, velocity, PhotonNetwork.LocalPlayer, blueTeam: false, orangeTeam: false, RPCSUB.IncrementLocalPlayerProjectileCount(), Mathf.Abs(GorillaTagger.Instance.offlineVRRig.slingshot.projectilePrefab.transform.lossyScale.x), true, new Color(r,g,b,a));


                projdata[0] = position;
                projdata[1] = velocity;
                projdata[2] = 2;
                projdata[3] = IncrementLocalPlayerProjectileCount();// not accurate might ban you
                projdata[4] = randomColour;
                projdata[5] = r;
                projdata[6] = g;
                projdata[7] = b;
                projdata[8] = a;
                object data = (object)projdata;
                SendEvent(in PROJECTILE_ID, in data, new RaiseEventOptions
                {
                    CachingOption = EventCaching.DoNotCache,
                    Receivers = ReceiverGroup.Others// i could have made this a preset or some shit but i just didnt
                });
            }
        }

        #endregion

        #region Sounds

        public static void SendSound(int id, float volume)
        {
            if (PhotonNetwork.IsMasterClient && Time.time > t)
            {
                GorillaTagger.Instance.offlineVRRig.PlayTagSoundLocal(id, volume);
                var data = new object[] { id, (float)volume };
                SendEvent(SOUND, data, new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others,
                    CachingOption = EventCaching.DoNotCache,

                });
            }
        }

        #endregion

        #region MISC

        public static void ReportTag(Player player)
        {
            GameMode.ReportTag(player);
        }

        public static void JoinPubWithFriends(Player player)
        {
            if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId) && player != PhotonNetwork.LocalPlayer)
            {
                var Others = new RaiseEventOptions()
                {
                    TargetActors = new int[] { player.ActorNumber },
                };
                PhotonNetworkController.Instance.shuffler = Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                PhotonNetworkController.Instance.keyStr = Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                var data = new object[]
                {
                    PhotonNetworkController.Instance.shuffler, PhotonNetworkController.Instance.keyStr
                };
                SendEvent(JOINWITHFRIEND_ID, data, Others);
            }
        }

        #endregion
    }
}
