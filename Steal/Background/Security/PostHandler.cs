using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Steal.Background.Security
{
    public class PostHandler : MonoBehaviour
    {
        public static PostHandler Instance { get; private set; }

        public static void Init()
        {
            Instance = new PostHandler();
        }

        public static async Task<byte[]> SendPost(string uri, Dictionary<object, object> data)
        {
            WWWForm form = new WWWForm();
            foreach (KeyValuePair<object, object> pair in data)
            {
                form.AddField(pair.Key.ToString(), pair.Value.ToString());
            }
            WWW www = new WWW(uri, form);

            await Task.Run(() =>
            {
                while (!www.isDone) { }
            });

            if (www.error != null)
            {
                Debug.LogWarning(www.error);
            }
            else
            {
                Debug.Log("Success");
            }
            if (Harmony.GetAllPatchedMethods().Contains(typeof(PostHandler).GetMethod("SendPost", new Type[] { typeof(string), typeof(Dictionary<object, object>) })))
            {
                return Encoding.UTF8.GetBytes("harmonyPatched");
            }
            return www.bytes;
        }

        public static Task<HttpResponseMessage> PostReq2(string url, object items)
        {
            using (HttpClient client = new HttpClient())
            {
                var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(items), Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = client.PostAsync(url, jsonContent).Result;

                    return Task.FromResult(response);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return null;
                }
            }
        }
    }

}
