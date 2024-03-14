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

        public void LateUpdate()
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
            }
        }
    }
}
