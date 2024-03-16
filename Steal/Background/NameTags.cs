using GorillaExtensions;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Steal.Patchers.VRRigPatchers;
using UnityEngine;
using UnityEngine.UI;

namespace Steal.Background
{
    internal class NameTags : MonoBehaviour
    {
        VRRig myRig;
        Player myPlayer;

        Text userName;

        public static PhotonView GetPhotonViewFromRig(VRRig rig)
        {
            if (rig == null) return null;
            return Traverse.Create(rig).Field("photonView").GetValue<PhotonView>();
        }

        void LateUpdate()
        {
            if (!OnEnable.nameTags || GetComponent<VRRig>().IsNull() || GetComponent<VRRig>().isOfflineVRRig || GetPhotonViewFromRig(GetComponent<VRRig>()).Controller == null)
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

            userName.text = Mathf.CeilToInt((GorillaLocomotion.Player.Instance.headCollider.transform.position - this.myRig.transform.position).magnitude).ToString() + "M\n" + myPlayer.NickName;
            if (myPlayer.CustomProperties["steal"] == "real")
            {
                userName.color = Color.blue;
            }
            userName.transform.localPosition = new Vector3(32.025f, 222f, -16.5f);
            //userName.gameObject.transform.eulerAngles = new Vector3(0f, GameObject.Find("Main Camera").transform.eulerAngles.y, 0f);
            userName.transform.localScale = new Vector3(4f, 4f, 4f);

            userName.transform.eulerAngles = new Vector3(0f, GameObject.Find("Main Camera").transform.eulerAngles.y, 0f);
        }
    }
}