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

namespace Steal.Components
{
    internal class NameTags : MonoBehaviour
    {
        VRRig myRig;
        Player myPlayer;

        Text userName;

        void LateUpdate()
        {
            if (!OnEnable.nameTags || GetComponent<VRRig>().IsNull() || GetComponent<VRRig>().isOfflineVRRig || ModHandler.GetPhotonViewFromRig(GetComponent<VRRig>()) == null || ModHandler.GetPhotonViewFromRig(GetComponent<VRRig>()).Owner == null)
            {
                Destroy(this);
                return;
            }
            if (userName != null && userName.text != ModHandler.GetPhotonViewFromRig(myRig).Controller.NickName)
            {
                Destroy(this);
                return;
            }
            if (userName == null)
            {
                myRig = GetComponent<VRRig>();
                myPlayer = ModHandler.GetPhotonViewFromRig(myRig).Controller;

                userName = Instantiate(myRig.playerText, myRig.playerText.transform.parent);

                //userName.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            }

            userName.text = Mathf.CeilToInt((GorillaLocomotion.Player.Instance.headCollider.transform.position - this.myRig.transform.position).magnitude).ToString() + "M\n" + myPlayer.NickName;

            userName.transform.localPosition = new Vector3(32.025f, 222f, -16.5f);
            if (PhotonNetwork.LocalPlayer.CustomProperties["steal"] == "real")
            {
                userName.text = "<color=blue>" + userName.text + "</color>";
            }
            else //STOP YOU FUCKING NIGGER WE NEED TO WORK ON THE MENU AND YOUR BEING ANNOYING AS SHIT
                userName.text = "<color=red>" + userName.text + "</color>";
            //userName.gameObject.transform.eulerAngles = new Vector3(0f, GameObject.Find("Main Camera").transform.eulerAngles.y, 0f);
            userName.transform.localScale = new Vector3(4f, 4f, 4f);

            userName.transform.eulerAngles = new Vector3(0f, GameObject.Find("Main Camera").transform.eulerAngles.y, 0f);
        }
    }
}
