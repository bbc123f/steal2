using Steal.Attributes;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.Components
{
    internal class BtnCollider : MonoBehaviour
    {
        public Button button;

        public void OnTriggerEnter(Collider other)
        {
            if (other != null && Time.frameCount >= Main.Instance.framePressCooldown && other.name.Contains("MenuClicker"))
            {
                AssetLoader.Instance.PlayClick();
                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength / 2, GorillaTagger.Instance.tagHapticDuration / 2);
                Main.Instance.Toggle(button);
                Main.Instance.framePressCooldown = Time.frameCount + 15;
                Destroy(Main.Instance.menu);
                Main.Instance.menu = null;
            }
        }
    }
}
