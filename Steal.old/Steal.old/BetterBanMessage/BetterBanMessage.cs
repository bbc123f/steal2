using BepInEx;
using GorillaNetworking;
using HarmonyLib;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.BetterBanMessage
{
    [HarmonyPatch(typeof(PlayFabAuthenticator), "OnPlayFabError", MethodType.Normal)]
    public class OnPlayFabError : MonoBehaviour
    {
        public static string getMinutesLeft(int totalminutes, int totalhours)
        {
            return (totalminutes - (totalhours * 60)).ToString();
        }

        static bool Prefix(PlayFabError obj)
        {
            GorillaLevelScreen[] array = GorillaComputer.instance.levelScreens;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].badMaterial.color = Color.red * 0.6f;
            }
            PlayFabAuthenticator.instance.LogMessage(obj.ErrorMessage);
            Debug.Log("OnPlayFabError(): " + obj.ErrorMessage);
            PlayFabAuthenticator.instance.loginFailed = true;
            if (obj.ErrorMessage == "The account making this request is currently banned")
            {
                using (Dictionary<string, List<string>>.Enumerator enumerator = obj.ErrorDetails.GetEnumerator())
                {
                    if (!enumerator.MoveNext())
                    {
                        return false;
                    }
                    KeyValuePair<string, List<string>> keyValuePair = enumerator.Current;
                    if (keyValuePair.Value[0] != "Indefinite")
                    {
                        int totalmin = (int)(DateTime.Parse(keyValuePair.Value[0]) - DateTime.UtcNow).TotalMinutes;
                        int totalhour = (int)(DateTime.Parse(keyValuePair.Value[0]) - DateTime.UtcNow).TotalHours;
                        PlayFabAuthenticator.instance.gorillaComputer.GeneralFailureMessage(string.Concat(new string[]
                        {
                            "YOUR ACCOUNT HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: <b>",
                            keyValuePair.Key,
                            "</b>\nHOURS LEFT: <b>",
                            ((int)((DateTime.Parse(keyValuePair.Value[0]) - DateTime.UtcNow).TotalHours + 1.0)).ToString(),
                            "</b>\nMINUTES: <b>",
                            getMinutesLeft(totalmin, totalhour),
                            "</b>"
                        }));
                        return false;
                    }
                    PlayFabAuthenticator.instance.gorillaComputer.GeneralFailureMessage("YOUR ACCOUNT HAS BEEN BANNED INDEFINITELY.\nREASON: <b>" + keyValuePair.Key + "</b>");
                    return false;
                }
            }
            if (obj.ErrorMessage == "The IP making this request is currently banned")
            {
                using (Dictionary<string, List<string>>.Enumerator enumerator2 = obj.ErrorDetails.GetEnumerator())
                {
                    if (!enumerator2.MoveNext())
                    {
                        return false;
                    }
                    KeyValuePair<string, List<string>> keyValuePair2 = enumerator2.Current;
                    if (keyValuePair2.Value[0] != "Indefinite")
                    {
                        int totalmin = (int)(DateTime.Parse(keyValuePair2.Value[0]) - DateTime.UtcNow).TotalMinutes;
                        int totalhour = (int)(DateTime.Parse(keyValuePair2.Value[0]) - DateTime.UtcNow).TotalHours;
                        PlayFabAuthenticator.instance.gorillaComputer.GeneralFailureMessage("THIS IP HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: <b>" + keyValuePair2.Key + "</b>\nHOURS LEFT: <b>" + ((int)((DateTime.Parse(keyValuePair2.Value[0]) - DateTime.UtcNow).TotalHours + 1.0)).ToString() + "</b>\nMINUTES: <b>" + getMinutesLeft(totalmin, totalhour) + "</b>");
                        return false;
                    }
                    PlayFabAuthenticator.instance.gorillaComputer.GeneralFailureMessage("THIS IP HAS BEEN BANNED INDEFINITELY.\nREASON: <b>" + keyValuePair2.Key + "</b>");
                    return false;
                }
            }
            if (PlayFabAuthenticator.instance.gorillaComputer != null)
            {
                PlayFabAuthenticator.instance.gorillaComputer.GeneralFailureMessage(PlayFabAuthenticator.instance.gorillaComputer.unableToConnect);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayFabAuthenticator), "ShowBanMessage", MethodType.Normal)]
    public class ShowBanMessage
    {
        static bool Prefix(PlayFabAuthenticator.BanInfo banInfo)
        {
            try
            {
                if (banInfo.BanExpirationTime != null && banInfo.BanMessage != null)
                {
                    if (banInfo.BanExpirationTime != "Indefinite")
                    {
                        int totalmin = (int)(DateTime.Parse(banInfo.BanExpirationTime) - DateTime.UtcNow).TotalMinutes;
                        int totalhour = (int)(DateTime.Parse(banInfo.BanExpirationTime) - DateTime.UtcNow).TotalHours;
                        PlayFabAuthenticator.instance.gorillaComputer.GeneralFailureMessage("YOUR ACCOUNT HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: <b>" + banInfo.BanMessage + "</b>\nHOURS LEFT: <b>" + ((int)((DateTime.Parse(banInfo.BanExpirationTime) - DateTime.UtcNow).TotalHours + 1.0)).ToString() + "</b>\nMINUTES: <b>" + OnPlayFabError.getMinutesLeft(totalmin, totalhour) + "</b>");
                    }
                    else
                    {
                        PlayFabAuthenticator.instance.gorillaComputer.GeneralFailureMessage("YOUR ACCOUNT HAS BEEN BANNED INDEFINITELY.\nREASON: " + banInfo.BanMessage);
                    }
                }
                return false;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
