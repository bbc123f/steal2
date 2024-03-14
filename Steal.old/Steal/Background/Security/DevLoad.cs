// using System.Reflection;
// using HarmonyLib;
//
// using UnityEngine;
//
// namespace Steal.Background.Security
// {
//     
//     public class DevLoad : MonoBehaviour
//     {
//         public static GameObject ms = new GameObject("Steal");
//         public static void Load()
//         {
//            
//             //ms.AddComponent<AssetLoader>();
//             //ms.AddComponent<ShowConsole>();
//             ms.AddComponent<Main>();
//             //ms.AddComponent<ModsList>();
//             //ms.AddComponent<ControllerInput>();
//             ms.AddComponent<StealGUI>();
//             //ms.AddComponent<ControllerInput>();
//             //ms.AddComponent<RPCSUB>();
//             //ms.AddComponent<Notif>();
//             //ms.AddComponent<ModsListInterface>();
//             //ms.AddComponent<Mods.Mods>();
//             //ms.AddComponent<GhostRig>();
//             //ms.AddComponent<PocketWatch>();
//             //ms.AddComponent<Steal.GorillaOS.GorillaOS>();
//             SettingsLib.Init();
//             //Crasher.helpp();
//             //Main.AddButtonsToList();
//             new Harmony("com.steal.lol").PatchAll(Assembly.GetExecutingAssembly());
//             UnityEngine.Object.DontDestroyOnLoad(ms);
//         }
//         
//       
//     }
// }