using Oculus.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Steal.Background
{
    internal class DiscordRPC
    {
        static Assembly rpcAssembly;

        public static void Init()
        {
            byte[] dllBytes = LoadEmbeddedResource("Steal.Resources.DiscordRPC.dll");
            rpcAssembly = Assembly.Load(dllBytes);

            var rpcClientType = rpcAssembly.GetType("DiscordRPC.DiscordRpcClient");
            var rpcClient = Activator.CreateInstance(rpcClientType, "1226755656346767380");
            var initializeMethod = rpcClientType.GetMethod("Initialize");
            initializeMethod.Invoke(rpcClient, null);

            var richPresenceType = rpcAssembly.GetType("DiscordRPC.RichPresence");
            var presence = Activator.CreateInstance(richPresenceType);

            richPresenceType.GetProperty("Details").SetValue(presence, "Using Steal.lol Cheat in Gorilla Tag!");
            richPresenceType.GetProperty("State").SetValue(presence, "discord.gg/paste");

            var assetsType = rpcAssembly.GetType("DiscordRPC.Assets");
            var assets = Activator.CreateInstance(assetsType);

            assetsType.GetProperty("LargeImageKey").SetValue(assets, "dimondresized");
            assetsType.GetProperty("LargeImageText").SetValue(assets, "Steal.lol");

            richPresenceType.GetProperty("Assets").SetValue(presence, assets);

            var setPresenceMethod = rpcClientType.GetMethod("SetPresence");
            setPresenceMethod.Invoke(rpcClient, new object[] { presence });

            /*
            client.SetPresence(new RichPresence()
            {
                Details = "Example Project",
                State = "csharp example",
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = "Lachee's Discord IPC Library",
                    SmallImageKey = "image_small"
                }
            });*/
        }

        private static byte[] LoadEmbeddedResource(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found.");
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
