using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Steal.Background
{
    internal class AssetLoader : MonoBehaviour
    {
        #region Assets
        static Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("Steal.Resources.click");
        static AssetBundle bundle = AssetBundle.LoadFromStream(str);
        public static AudioClip click = bundle.LoadAsset("click") as AudioClip;
        #endregion
        #region Audio Source
        public AudioSource source;
        #endregion
        #region Instance
        public static AssetLoader Instance
        {
            get
            {
                return assetLoader;
            }
        }
        static AssetLoader assetLoader;
        #endregion
        void Awake()
        {
            assetLoader = this;
        }
        public void PlayClick()
        {
            if (source != null)
            {
                Destroy(source);
            }
            source = gameObject.AddComponent<AudioSource>();
            source.clip = click;
            source.loop = false;
            source.Play();
        }

        public static Texture2D DownloadBackround(string imagestring) 
        {
            byte[] imageBytes;
            using (WebClient webClient = new WebClient())
            {
                imageBytes = webClient.DownloadData(imagestring);
            }
            Texture2D ImageTexture = new Texture2D(200, 200);
            ImageConversion.LoadImage(ImageTexture, imageBytes);
            return ImageTexture;
        }


    }
}
