using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.Components
{
    internal class GhostRig : MonoBehaviour
    {
        private VRRig ghostRig;

        public static GhostRig instance;

        public static bool hasinstance = false;

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
                hasinstance = true;
            }
        }

        public void Update()
        {
            if (ghostRig == null && GorillaTagger.Instance.offlineVRRig != null)
            {
                var rigOB = Instantiate<GameObject>(GorillaTagger.Instance.offlineVRRig.gameObject);
                ghostRig = rigOB.GetComponent<VRRig>();
                Destroy(rigOB.GetComponent<Rigidbody>());
                ghostRig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                ghostRig.mainSkin.material.color = new Color32(255, 255, 255, 40);
                ghostRig.transform.position = Vector3.zero;
                ghostRig.enabled = false;
            }
            if (ghostRig == null) { return; }
            if (ghostRig.enabled && GorillaTagger.Instance.offlineVRRig.enabled)
            {
                ghostRig.transform.position = Vector3.zero;
                ghostRig.enabled = false;

            }
            if (!GorillaTagger.Instance.offlineVRRig.enabled)
            {
                ghostRig.enabled = true;
                ghostRig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                ghostRig.mainSkin.material.color = new Color32(255, 255, 255, 40);

                ghostRig.leftHandPlayer.gameObject.transform.position = Player.Instance.leftControllerTransform.position;
                ghostRig.rightHandPlayer.gameObject.transform.position = Player.Instance.rightControllerTransform.position;
                ghostRig.leftHandPlayer.gameObject.transform.rotation = Player.Instance.leftControllerTransform.rotation;
                ghostRig.rightHandPlayer.gameObject.transform.rotation = Player.Instance.rightControllerTransform.rotation;

                ghostRig.leftHand.rigTarget.gameObject.transform.position = Player.Instance.leftControllerTransform.position;
                ghostRig.rightHand.rigTarget.gameObject.transform.position = Player.Instance.rightControllerTransform.position;
                ghostRig.leftHand.rigTarget.gameObject.transform.rotation = Player.Instance.leftControllerTransform.rotation;
                ghostRig.rightHand.rigTarget.gameObject.transform.rotation = Player.Instance.rightControllerTransform.rotation;


                ghostRig.rightHandTransform.gameObject.transform.position = Player.Instance.leftControllerTransform.position;
                ghostRig.leftHandTransform.gameObject.transform.position = Player.Instance.rightControllerTransform.position;
                ghostRig.leftHandTransform.gameObject.transform.rotation = Player.Instance.leftControllerTransform.rotation;
                ghostRig.rightHandTransform.gameObject.transform.rotation = Player.Instance.rightControllerTransform.rotation;

                //ghostRig.transform.position = Player.Instance.transform.position;
                //ghostRig.transform.rotation = Player.Instance.transform.rotation;
                ghostRig.headConstraint.transform.rotation = Player.Instance.headCollider.transform.rotation;
            }
        }
    }
}
