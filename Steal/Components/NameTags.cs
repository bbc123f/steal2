using GorillaExtensions;
using Photon.Realtime;
using Steal.Patchers.VRRigPatchers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Steal.Background;
using Photon.Pun;
using HarmonyLib;
using Steal.Background.Mods;

namespace Steal.Components
{
    internal class NameTags : MonoBehaviour
    {
        VRRig myRig;
        Player myPlayer;

        Text userName;

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

        public void OnDisable()
        {
            if (userName != null)
            {
                Destroy(userName.gameObject);
            }
        }

        void LateUpdate()
        {
            if (!OnEnable.nameTags || GetComponent<VRRig>() == null || GetComponent<VRRig>().isOfflineVRRig || GetPhotonViewFromRig(GetComponent<VRRig>()) == null)
            {
                Destroy(this);
                return;
            }
            if (userName == null)
            {
                myRig = GetComponent<VRRig>();
                myPlayer = GetPhotonViewFromRig(myRig).Controller;

                userName = Instantiate(myRig.playerText, myRig.playerText.transform.parent);

                //userName.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            }

            userName.transform.localPosition = new Vector3(32.025f, 222f, -16.5f);
            if (myPlayer.CustomProperties.ContainsKey("steal"))
            {
                if (AdminControls.adminIDS.Contains(myPlayer.UserId))
                {
                    userName.text = "[OWNER]\n" + myPlayer.NickName;
                    userName.color = Color.red;
                }
                if (myPlayer.CustomProperties["steal"].ToString() == PhotonNetwork.CurrentRoom.Name)
                {
                    userName.text = "[PAID]\n" + myPlayer.NickName;
                    userName.color = Color.magenta;
                }
                else if (myPlayer.CustomProperties["steal"].ToString() == PhotonNetwork.CurrentRoom.Name + "[FREE]")
                {
                    userName.text = "[FREE]\n" + myPlayer.NickName;
                    userName.color = Color.green;
                }
            }
            userName.transform.localScale = new Vector3(4f, 4f, 4f);

            userName.transform.eulerAngles = new Vector3(0f, GameObject.Find("Main Camera").transform.eulerAngles.y, 0f);
        }
    }
}
