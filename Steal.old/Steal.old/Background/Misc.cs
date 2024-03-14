using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Steal.Background
{
    static class Misc
    {
        public static string name;
        public static string id;
        public static string created;
        public static string lastlogin;
        public static string banned;
        public static string location;
        static public void SearchID(string ID)
        {
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
            {
                PlayFabId = ID,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowDisplayName = true,
                    ShowCreated = true,
                    ShowLastLogin = true,
                    ShowBannedUntil = true,
                    ShowLocations = true,
                }
            }, new Action<GetPlayerProfileResult>(OnGetPlayerProfile), new Action<PlayFabError>(OnGetPlayerProfileError), null, null);
        }
        static void OnGetPlayerProfile(GetPlayerProfileResult result)
        {
            name = result.PlayerProfile.DisplayName;
            id = result.PlayerProfile.PlayerId;
            created = result.PlayerProfile.Created.ToString();
            lastlogin = result.PlayerProfile.LastLogin.ToString();
            banned = result.PlayerProfile.BannedUntil.ToString();
            location = result.PlayerProfile.Locations[0].City;

            ShowConsole.Log($"Got player profile: {result.PlayerProfile.DisplayName} {result.PlayerProfile.PlayerId} {result.PlayerProfile.Created} {result.PlayerProfile.LastLogin} {result.PlayerProfile.BannedUntil}");
        }
        static void OnGetPlayerProfileError(PlayFabError error)
        {
            ShowConsole.Log($"Got error retrieving player profile: {error.ErrorMessage}");
        }
    }
}
