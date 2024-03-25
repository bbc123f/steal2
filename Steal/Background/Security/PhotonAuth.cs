using System;
using System.Collections.Generic;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Steal.Background.Security
{
	public class AuthClient : MonoBehaviour
	{
        static string _sessionTicket;
        static string _playFabId;
        static void MaybeGetNonce(LoginResult obj)
        {
            _playFabId = obj.PlayFabId;
            _sessionTicket = obj.SessionTicket;
        }
        static void playfabErrorFGASA(PlayFabError error)
        {
            UnityEngine.Debug.Log("Error my nega! " + error.ToString());
        }
        public static void asfasf(string ticket)
        {
            PlayFabClientAPI.LoginWithSteam(new LoginWithSteamRequest
            {
                SteamTicket = ticket,
                TitleId = PlayFabSettings.TitleId
            }, new System.Action<LoginResult>(MaybeGetNonce), new System.Action<PlayFabError>(playfabErrorFGASA));
        }
    }
}
