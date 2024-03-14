using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.Components
{
    internal class FadingColors : MonoBehaviour
    {
        Renderer renderer;
        public Color color;


        void Start()
        {
            renderer = GetComponent<Renderer>();
            renderer.material.shader = Shader.Find("UI/Default"); 
        }

        void LateUpdate()
        {
            color = Color32.Lerp(Color.black, new Color32(146, 99, 255, 155), Mathf.PingPong(Time.time, 1f));
            renderer.material.color = color;
        }
    }
}
