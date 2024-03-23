using System;
using System.Collections.Generic;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace SteamTicketGrabber
{
	public class AuthClient : MonoBehaviour
	{
		public static void ReAuth()
		{
			string Ticket = PlayFabAuthenticator.instance.GetSteamAuthTicket();
			Auth(Ticket);
		}
		
		public static void Auth(string Ticket)
		{
			AuthClient.AuthenticateSession(Ticket);
			AuthClient.AuthTicket = Ticket;
		}
		
		private static void AuthenticateSession(string SessionTicket)
		{
			LoginWithSteamRequest loginWithSteamRequest = new LoginWithSteamRequest
			{
				TitleId = PlayFabSettings.TitleId,
				SteamTicket = SessionTicket
			};
			PlayFabClientAPI.LoginWithSteam(loginWithSteamRequest, RequestPhotonToken, SessionLoginFailed, null, null);
		}
		
		private static void RequestPhotonToken(LoginResult log)
		{
			PlayFabTitleDataCache.Instance.LoadDataFromFile();
			PlayFabTitleDataCache.Instance.UpdateData();
			PlayFabAuthenticator.instance._playFabPlayerIdCache = log.PlayFabId;
			GetPhotonAuthenticationTokenRequest getPhotonAuthenticationTokenRequest = new GetPhotonAuthenticationTokenRequest
			{
				PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime
			};
			AuthClient.ID = log.PlayFabId;
			AuthClient.Session = log.SessionTicket;
			PlayFabClientAPI.GetPhotonAuthenticationToken(getPhotonAuthenticationTokenRequest, new Action<GetPhotonAuthenticationTokenResult>(AuthClient.AuthenticateWithPhoton), new Action<PlayFabError>(AuthClient.SessionLoginFailed), null, null);
		}
		
		private static void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult photonTokenResult)
		{
			AuthenticationValues authenticationValues = new AuthenticationValues(PlayFabSettings.DeviceUniqueIdentifier)
			{
				AuthType = CustomAuthenticationType.Custom
			};
			authenticationValues.AddAuthParameter("username", AuthClient.ID);
			authenticationValues.AddAuthParameter("token", photonTokenResult.PhotonCustomAuthenticationToken);
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"UserId",
					AuthClient.ID
				},
				{
					"AppId",
					PlayFabSettings.TitleId
				},
				{
					"AppVersion",
					PhotonNetwork.AppVersion ?? "-1"
				},
				{
					"Ticket",
					AuthClient.Session
				},
				{ "Token", photonTokenResult.PhotonCustomAuthenticationToken },
				{
					"Nonce",
					AuthClient.nonce
				},
				{
					"SteamTicket",
					AuthClient.AuthTicket
				},
				{
					"Platform",
					AuthClient.platform
				}
			};
			authenticationValues.SetAuthPostData(dictionary);
			PhotonNetwork.AuthValues = authenticationValues;
			ExecuteCloudScriptRequest executeCloudScriptRequest = new ExecuteCloudScriptRequest
			{
				FunctionName = "AddOrRemoveDLCOwnership",
				FunctionParameter = new { }
			};
			PlayFabClientAPI.ExecuteCloudScript(executeCloudScriptRequest, delegate(ExecuteCloudScriptResult result)
			{
				Debug.Log("NIG");
				bool flag3 = GorillaTagger.Instance != null;
				if (flag3)
				{
					GorillaTagger.Instance.offlineVRRig.GetUserCosmeticsAllowed();
				}
			}, delegate(PlayFabError error)
			{
				Debug.Log("Got error retrieving user data:");
				Debug.Log(error.GenerateErrorReport());
				bool flag4 = GorillaTagger.Instance != null;
				if (flag4)
				{
					GorillaTagger.Instance.offlineVRRig.GetUserCosmeticsAllowed();
				}
			}, null, null);
			bool flag = CosmeticsController.instance != null;
			if (flag)
			{
				CosmeticsController.instance.Initialize();
			}
			GorillaComputer.instance.OnConnectedToMasterStuff();
			bool flag2 = PhotonNetworkController.Instance != null;
			if (flag2)
			{
				NetworkSystem.Instance.SetAuthenticationValues(null);
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002675 File Offset: 0x00000875
		private static void SessionLoginFailed(PlayFabError log)
		{
			Debug.Log(log.ErrorMessage);
		}

		// Token: 0x04000009 RID: 9
		private static string ID;

		// Token: 0x0400000A RID: 10
		private static string Session;

		// Token: 0x0400000B RID: 11
		private static string AuthTicket;

		// Token: 0x0400000C RID: 12
		private static string nonce;

		// Token: 0x0400000D RID: 13
		private static string platform;
	}
}
