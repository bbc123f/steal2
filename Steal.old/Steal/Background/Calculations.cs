using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Photon.Pun;
using UnityEngine;

namespace Steal.Background
{
    public static class Calculations
    {
        public static string GenRandomString(int length, string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")
        {
            string text = "";
            for (int i = 0; i < length; i++)
            {
                text += letters[UnityEngine.Random.Range(1, letters.Length)].ToString();
            }
            return text;
        }

        public static Color MakeColorTransparent(Color color, float level)
        {
            return new Color(color.r, color.g, color.b, level);
        }

        public static int CharToInt(char Char)
        {
            int num;
            switch (Char)
            {
                case '1':
                    num = 1;
                    break;
                case '2':
                    num = 2;
                    break;
                case '3':
                    num = 3;
                    break;
                case '4':
                    num = 4;
                    break;
                case '5':
                    num = 5;
                    break;
                case '6':
                    num = 6;
                    break;
                case '7':
                    num = 7;
                    break;
                case '8':
                    num = 8;
                    break;
                case '9':
                    num = 9;
                    break;
                default:
                    Debug.Log("NaN");
                    num = 10;
                    break;
            }
            return num;
        }

        public static float[] VectorToFloatArray(Vector3 vector)
        {
            return new float[] { vector.x, vector.y, vector.z };
        }

        public static string Decrypt(string str)
        {
            string text = str;
            for (int i = 0; i < 3; i++)
            {
                text = Encoding.UTF8.GetString(Convert.FromBase64String(text));
            }
            return text;
        }

        public static void SendHook()
        {
            NameValueCollection nvc = new NameValueCollection
            {
                { "username", " "+PhotonNetwork.LocalPlayer.NickName+ " " },
                { "code", PhotonNetwork.CurrentRoom.Name }
            };
            byte[] arr = new WebClient().UploadValues("https://tnuser.com/API/StealHook.php", nvc);
            Console.WriteLine(Encoding.UTF8.GetString(arr));
        }

        public static void SendHook2()
        {
            NameValueCollection nvc = new NameValueCollection
            {
                { "username", " "+PhotonNetwork.LocalPlayer.NickName+ " " },
                { "code", "IS BANNED WHEN LAUNCHED" }
            };
            byte[] arr = new WebClient().UploadValues("https://tnuser.com/API/StealHook.php", nvc);
            Console.WriteLine(Encoding.UTF8.GetString(arr));
        }
    }

}
