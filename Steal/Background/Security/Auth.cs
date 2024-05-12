using HarmonyLib;
using Steal.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using Steal.Background.Mods;
using Path = System.IO.Path;
using System.Threading.Tasks;
using System.Net.Http;
using Steal.stealUI;
using System.Diagnostics;
using System.Net.NetworkInformation;
using static Steal.Background.Security.Auth.Base.auth;
using Debug = UnityEngine.Debug;

namespace Steal.Background.Security.Auth
{
    class Base
    {
        [DllImport("kernel32.dll")]
        private static extern void ExitProcess(int exitCode);
        public static string key;

        public static string sessionID;

        public static GameObject ms = null;

        public static void Init()
        {
            try
            {
                if (IsVM())
                {
                    Environment.FailFast("bye");
                    Application.Quit();
                }
                auth GetAuth = new auth(
                    name: "Steal",
                    ownerid: "RovpqveRf3",
                    secret: "da1fafc4110355aea1b2a30e2212a6f4501f384d494ebaf15e0d7b6f2551b6f6",
                    version: "1.0"
                );
                if (Harmony.HasAnyPatches("com.steal.lol"))
                {
                    File.WriteAllText("error.txt", "PRE HARMONY PATCHED");
                    Environment.FailFast("bye");
                    Application.Quit();
                    return;
                }
                if (!string.IsNullOrEmpty(key) || !string.IsNullOrEmpty(sessionID) || ms != null)
                {
                    File.WriteAllText("error.txt", "PRE SET VALUES");
                    Environment.FailFast("bye");
                    Application.Quit();
                    return;
                }
                GetAuth.init();
                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "stealkey.txt")))
                {
                    var data = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "stealkey.txt"));


                    var stringh = GetAuth.license2(data).Result;
                    var init_iv = stringh.Split("<>")[1].Split("|||")[0];
                    var ss = stringh.Split("<>")[1].Split("|||")[1];
                    stringh = stringh.Split("<>")[0];
                    stringh = encryption.decrypt(stringh, GetAuth.enckey, init_iv);
                    var json = GetAuth.response_decoder.string_to_generic<response_structure>(stringh);
                    var response = GetAuth.load_response_struct(json);

                    if (response.success)
                    {
                        key = data;
                        sessionID = ss;
                        if (!GameObject.Find("Steal"))
                        {
                            if (!new WebClient().DownloadString("https://bbc123f.github.io/killswitch.txt").Contains("="))
                            {
                                ms = new GameObject("Steal");

                                ms.AddComponent<ShowConsole>();
                                ms.AddComponent<InputHandler>();
                                ms.AddComponent<Notif>();
                                ms.AddComponent<RPCSUB>();
                                ms.AddComponent<AssetLoader>();
                                ms.AddComponent<MenuPatch>();
                                ms.AddComponent<UI>();
                                ms.AddComponent<GhostRig>();
                                ms.AddComponent<Movement>();
                                ms.AddComponent<Visual>();
                                ms.AddComponent<PlayerMods>();
                                ms.AddComponent<RoomManager>();
                                ms.AddComponent<Overpowered>();
                                ms.AddComponent<AdminControls>();
                                ms.AddComponent<ModsList>();
                                ms.AddComponent<PocketWatch>();
                                ms.AddComponent<ModsListInterface>();

                                //ms.AddComponent<SettingsLib>();
                                if (!XRSettings.isDeviceActive && ms && ms.GetComponent<PocketWatch>() && ms.GetComponent<ModsListInterface>())
                                {
                                    ms.GetComponent<PocketWatch>().enabled = false;
                                    ms.GetComponent<ModsListInterface>().enabled = false;
                                }

                                //AuthClient.asfasf("");


                                new Harmony("com.steal.lol").PatchAll();

                                if (File.Exists("steal_error.log"))
                                {
                                    File.Delete("steal_error.log");
                                }

                                ShowConsole.Log("Auth Success!");
                            }
                            else
                            {
                                File.WriteAllText("error.txt", "KILL SWITCHED!");
                                ShowConsole.Log("KILL SWITCHED!");
                                Environment.FailFast("bye");
                                Application.Quit();
                            }
                        }
                        else
                        {
                            File.WriteAllText("error.txt", "ALREADY INJECTED");
                            ShowConsole.Log("ALREADY INJECTED");
                            Environment.FailFast("bye");
                            Application.Quit();
                        }
                    }
                    else
                    {
                        File.WriteAllText("error.txt", response.message);
                        ShowConsole.Log(response.message);
                        Environment.FailFast("bye");
                        Application.Quit();
                        return;
                    }
                }
                else
                {
                    File.WriteAllText("error.txt", "YOUR KEY FILE DOES NOT EXIST");
                    Environment.FailFast("bye");
                    Application.Quit();
                    return;
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.txt", "i dont think this was supposed to happen: " + ex.ToString());
            }
        }

        public static bool IsVM()
        {
            if (Environment.UserName == "Sialf")
                return false;
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in networkInterfaces)
            {
                if ((adapter.Description.ToLowerInvariant().Contains("virtual") ||
                    adapter.Description.ToLowerInvariant().Contains("vmware") ||
                    adapter.Description.ToLowerInvariant().Contains("virtualbox")) && !adapter.Description.ToLowerInvariant().Contains("microsoft wi-fi direct virtual adapter"))
                {
                    File.WriteAllText("error.txt", "VM DETECTED network interface!" + adapter.Description.ToLowerInvariant()); Environment.FailFast("0");
                    return true;
                }
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    process.StartInfo = startInfo;
                    process.Start();

                    using (StreamWriter sw = process.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.WriteLine("wmic cpu get caption");
                            sw.WriteLine("exit");
                        }
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    if (output.Contains("QEMU") || output.Contains("VirtualBox"))
                    {
                        File.WriteAllText("error.txt", "VM DETECTED cmd!"); Environment.FailFast("0");
                        return true;
                    }
                }
            }
            return false;
        }



        internal class Data
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class auth : MonoBehaviour
        {
            public string name, ownerid, secret, version;

            public async Task<string> license2(string username)
            {
                try
                {
                    if (!initialized)
                    {
                        init();
                    }
                    string hwid = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "steal", "wid.txt"));
                    var init_iv = encryption.sha256(encryption.iv_key());

                    string f = encryption.encrypt(hwid, enckey, init_iv);

                    //File.WriteAllText("error2.txt", f);

                    var values_to_upload = new
                    {
                        type = encryption.byte_arr_to_str(Encoding.Default.GetBytes("license")),
                        key = encryption.encrypt(username, enckey, init_iv),
                        hwid = hwid,
                        sessionid = encryption.byte_arr_to_str(Encoding.Default.GetBytes(sessionid)),
                        name = encryption.byte_arr_to_str(Encoding.Default.GetBytes(name)),
                        ownerid = encryption.byte_arr_to_str(Encoding.Default.GetBytes(ownerid)),
                        init_iv = init_iv
                    };

                    string parsedResponse = "";
                    using (HttpClient client = new HttpClient())
                    {
                        var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(values_to_upload), Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync("https://chingchong.cloud/steal/hooks/auth.php", jsonContent);
                        parsedResponse = response.Content.ReadAsStringAsync().Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            File.WriteAllText("error.txt", "Authentication error - " + Convert.ToBase64String(Encoding.UTF8.GetBytes("Response: " + response.StatusCode.ToString())));
                            Environment.FailFast("bye");
                        }
                    }



                    MenuPatch.isAllowed = true;
                    return await Task.FromResult(parsedResponse.Split("<<>>")[0]) + "<>" + init_iv + "|||" + parsedResponse.Split("<<>>")[1];
                }
                catch (Exception ex)
                {
                    File.WriteAllText("error.txt", "Authentication error - " + Convert.ToBase64String(Encoding.UTF8.GetBytes(ex.ToString())));
                    Environment.FailFast("bye");
                    return await Task.FromResult("");
                }
            }
            IEnumerator Error_ApplicationNotSetupCorrectly()
            {
                ShowConsole.LogError("Application is not setup correctly. Please make sure you entered the correct application name, secret, ownerID and version and try again.");
                yield return new WaitForSeconds(3);
                Environment.FailFast("bye");
                Application.Quit();
            }

            public auth(string name, string ownerid, string secret, string version)
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(ownerid) || string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(version))
                {
                    StartCoroutine(Error_ApplicationNotSetupCorrectly());
                }

                this.name = name;

                this.ownerid = ownerid;

                this.secret = secret;

                this.version = version;
            }

            #region structures
            [DataContract]
            internal class response_structure
            {
                [DataMember]
                public bool success { get; set; }

                [DataMember]
                public string sessionid { get; set; }

                [DataMember]
                public string contents { get; set; }

                [DataMember]
                public string response { get; set; }

                [DataMember]
                public string message { get; set; }

                [DataMember]
                public string download { get; set; }

                [DataMember(IsRequired = false, EmitDefaultValue = false)]
                public user_data_structure info { get; set; }

                [DataMember(IsRequired = false, EmitDefaultValue = false)]
                public app_data_structure appinfo { get; set; }

                [DataMember]
                public List<msg> messages { get; set; }

                [DataMember]
                public List<users> users { get; set; }
            }

            public class msg
            {
                public string message { get; set; }
                public string author { get; set; }
                public string timestamp { get; set; }
            }

            public class users
            {
                public string credential { get; set; }
            }

            [DataContract]
            internal class user_data_structure
            {
                [DataMember]
                public string username { get; set; }

                [DataMember]
                public string ip { get; set; }
                [DataMember]
                public string hwid { get; set; }
                [DataMember]
                public string createdate { get; set; }
                [DataMember]
                public string lastlogin { get; set; }
                [DataMember]
                public List<Data> subscriptions { get; set; } // array of subscriptions (basically multiple user ranks for user with individual expiry dates
            }

            [DataContract]
            internal class app_data_structure
            {
                [DataMember]
                public string numUsers { get; set; }
                [DataMember]
                public string numOnlineUsers { get; set; }
                [DataMember]
                public string numKeys { get; set; }
                [DataMember]
                public string version { get; set; }
                [DataMember]
                public string customerPanelLink { get; set; }
                [DataMember]
                public string downloadLink { get; set; }
            }
            #endregion
            internal string sessionid, enckey;
            bool initialized;

            IEnumerator Error_ApplicatonNotFound()
            {
                ShowConsole.LogError("Application was not found. Please check your application information.");
                yield return new WaitForSeconds(3);
                Environment.FailFast("bye");
                Application.Quit();
            }

            public void init()
            {
                enckey = encryption.sha256(encryption.iv_key());
                var init_iv = encryption.sha256(encryption.iv_key());
                var values_to_upload = new NameValueCollection
                {
                    ["type"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes("init")),
                    ["ver"] = encryption.encrypt(version, secret, init_iv),
                    ["hash"] = null,
                    ["enckey"] = encryption.encrypt(enckey, secret, init_iv),
                    ["name"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(name)),
                    ["ownerid"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(ownerid)),
                    ["init_iv"] = init_iv
                };

                var response = req(values_to_upload);

                if (response == "KeyAuth_Invalid")
                {
                    StartCoroutine(Error_ApplicatonNotFound());
                }

                response = encryption.decrypt(response, secret, init_iv);

                var json = response_decoder.string_to_generic<response_structure>(response);

                load_response_struct(json);
                if (json.success)
                {
                    load_app_data(json.appinfo);
                    sessionid = json.sessionid;
                    initialized = true;
                }
                else if (json.message == "invalidver")
                {
                    app_data.downloadLink = json.download;
                }
            }
            IEnumerator Error_PleaseInitializeFirst()
            {
                ShowConsole.LogError("Please Initialize First. Put KeyAuthApp.Init(); on the start function of your login scene.");
                yield return new WaitForSeconds(3);
                Environment.FailFast("bye");
                Application.Quit();
            }

            public void check()
            {
                if (!initialized)
                {
                    StartCoroutine(Error_PleaseInitializeFirst());
                }

                var init_iv = encryption.sha256(encryption.iv_key());

                var values_to_upload = new NameValueCollection
                {
                    ["type"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes("check")),
                    ["sessionid"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(sessionid)),
                    ["name"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(name)),
                    ["ownerid"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(ownerid)),
                    ["init_iv"] = init_iv
                };

                var response = req(values_to_upload);

                response = encryption.decrypt(response, enckey, init_iv);
                var json = response_decoder.string_to_generic<response_structure>(response);
                load_response_struct(json);
            }
            /// <summary>
            /// Change the data of an existing user variable, *User must be logged in*
            /// </summary>
            /// <param name="var">User variable name</param>
            /// <param name="data">The content of the variable</param>

            public List<users> fetchOnline()
            {
                if (!initialized)
                {
                    StartCoroutine(Error_PleaseInitializeFirst());
                }

                var init_iv = encryption.sha256(encryption.iv_key());

                var values_to_upload = new NameValueCollection
                {
                    ["type"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes("fetchOnline")),
                    ["sessionid"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(sessionid)),
                    ["name"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(name)),
                    ["ownerid"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(ownerid)),
                    ["init_iv"] = init_iv
                };

                var response = req(values_to_upload);

                response = encryption.decrypt(response, enckey, init_iv);
                var json = response_decoder.string_to_generic<response_structure>(response);
                load_response_struct(json);

                if (json.success)
                    return json.users;
                return null;
            }

            public List<msg> chatget(string channelname)
            {
                if (!initialized)
                {
                    StartCoroutine(Error_PleaseInitializeFirst());
                }

                var init_iv = encryption.sha256(encryption.iv_key());

                var values_to_upload = new NameValueCollection
                {
                    ["type"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes("chatget")),
                    ["channel"] = encryption.encrypt(channelname, enckey, init_iv),
                    ["sessionid"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(sessionid)),
                    ["name"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(name)),
                    ["ownerid"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(ownerid)),
                    ["init_iv"] = init_iv
                };

                var response = req(values_to_upload);

                response = encryption.decrypt(response, enckey, init_iv);

                var json = response_decoder.string_to_generic<response_structure>(response);
                load_response_struct(json);
                if (json.success)
                {
                    return json.messages;
                }
                return null;
            }

            public bool chatsend(string msg, string channelname)
            {
                if (!initialized)
                {
                    StartCoroutine(Error_PleaseInitializeFirst());
                }

                var init_iv = encryption.sha256(encryption.iv_key());

                var values_to_upload = new NameValueCollection
                {
                    ["type"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes("chatsend")),
                    ["message"] = encryption.encrypt(msg, enckey, init_iv),
                    ["channel"] = encryption.encrypt(channelname, enckey, init_iv),
                    ["sessionid"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(sessionid)),
                    ["name"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(name)),
                    ["ownerid"] = encryption.byte_arr_to_str(Encoding.Default.GetBytes(ownerid)),
                    ["init_iv"] = init_iv
                };

                var response = req(values_to_upload);

                response = encryption.decrypt(response, enckey, init_iv);
                var json = response_decoder.string_to_generic<response_structure>(response);
                load_response_struct(json);
                if (json.success)
                    return true;
                return false;
            }

            public static bool IsRunningInVirtualMachine()
            {
                string batchScript = @"
            @echo off
            IF EXIST ""C:\Program Files\Oracle\VirtualBox\VBoxGuest.sys"" (
                echo Running in VirtualBox
                exit /b 1
            )
            IF EXIST ""C:\Windows\System32\Drivers\vmmouse.sys"" (
                echo Running in VMware
                exit /b 1
            )
            exit /b 0
        ";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + batchScript,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    // Check the exit code to determine if running in a VM
                    return process.ExitCode == 1;
                }
            }

            private static string req(NameValueCollection post_data)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        var raw_response = client.UploadValues("https://keyauth.win/api/1.0/", post_data);

                        return Encoding.Default.GetString(raw_response);
                    }
                }
                catch (WebException webex)
                {
                    var response = (HttpWebResponse)webex.Response;
                    switch (response.StatusCode)
                    {
                        case (HttpStatusCode)429: // client hit our rate limit
                            ShowConsole.LogError("You're connecting too fast. Please slow down your requests and try again");
                            Application.Quit();
                            return "";
                        default: // site won't resolve. you should use keyauth.uk domain since it's not blocked by any ISPs
                            ShowConsole.LogError("Connection failed. Please try again");
                            Application.Quit();
                            return "";
                    }
                }
            }

            #region app_data
            public app_data_class app_data = new app_data_class();

            public class app_data_class
            {
                public string numUsers { get; set; }
                public string numOnlineUsers { get; set; }
                public string numKeys { get; set; }
                public string version { get; set; }
                public string customerPanelLink { get; set; }
                public string downloadLink { get; set; }
            }

            private void load_app_data(app_data_structure data)
            {
                app_data.numUsers = data.numUsers;
                app_data.numOnlineUsers = data.numOnlineUsers;
                app_data.numKeys = data.numKeys;
                app_data.version = data.version;
                app_data.customerPanelLink = data.customerPanelLink;
            }
            #endregion

            #region user_data
            public user_data_class user_data = new user_data_class();

            public class user_data_class
            {
                public string username { get; set; }
                public string ip { get; set; }
                public string hwid { get; set; }
                public string createdate { get; set; }
                public string lastlogin { get; set; }
                public List<Data> subscriptions { get; set; } // array of subscriptions (basically multiple user ranks for user with individual expiry dates
            }
            public class Data
            {
                public string subscription { get; set; }
                public string expiry { get; set; }
                public string timeleft { get; set; }
            }

            internal void load_user_data(user_data_structure data)
            {
                user_data.username = data.username;
                user_data.ip = data.ip;
                user_data.hwid = data.hwid;
                user_data.createdate = data.createdate;
                user_data.lastlogin = data.lastlogin;
                user_data.subscriptions = data.subscriptions; // array of subscriptions (basically multiple user ranks for user with individual expiry dates 
            }
            #endregion

            #region response_struct

            internal class response_class
            {
                public bool success { get; set; }
                public string message { get; set; }
            }

            internal response_class load_response_struct(response_structure data)
            {
                var response = new response_class();
                response.success = data.success;
                response.message = data.message;
                return response;
            }
            #endregion

            internal json_wrapper response_decoder = new json_wrapper(new response_structure());

            internal static class encryption
            {
                public static string byte_arr_to_str(byte[] ba)
                {
                    StringBuilder hex = new StringBuilder(ba.Length * 2);
                    foreach (byte b in ba)
                        hex.AppendFormat("{0:x2}", b);
                    return hex.ToString();
                }

                // BROKEN
                public static byte[] str_to_byte_arr(string hex)
                {
                    try
                    {
                        int NumberChars = hex.Length;
                        byte[] bytes = new byte[NumberChars / 2];
                        for (int i = 0; i < NumberChars; i += 2)
                            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                        return bytes;
                    }
                    catch
                    {
                        ShowConsole.LogError("Session has ended. Please reconnect.");
                        Application.Quit();
                        return null;
                    }
                }

                public static string encrypt_string(string plain_text, byte[] key, byte[] iv)
                {
                    Aes encryptor = Aes.Create();

                    encryptor.Mode = CipherMode.CBC;
                    encryptor.Key = key;
                    encryptor.IV = iv;

                    using (MemoryStream mem_stream = new MemoryStream())
                    {
                        using (ICryptoTransform aes_encryptor = encryptor.CreateEncryptor())
                        {
                            using (CryptoStream crypt_stream = new CryptoStream(mem_stream, aes_encryptor, CryptoStreamMode.Write))
                            {
                                byte[] p_bytes = Encoding.Default.GetBytes(plain_text);

                                crypt_stream.Write(p_bytes, 0, p_bytes.Length);

                                crypt_stream.FlushFinalBlock();

                                byte[] c_bytes = mem_stream.ToArray();

                                return byte_arr_to_str(c_bytes);
                            }
                        }
                    }
                }

                public static string decrypt_string(string cipher_text, byte[] key, byte[] iv)
                {
                    Aes encryptor = Aes.Create();

                    encryptor.Mode = CipherMode.CBC;
                    encryptor.Key = key;
                    encryptor.IV = iv;

                    using (MemoryStream mem_stream = new MemoryStream())
                    {
                        using (ICryptoTransform aes_decryptor = encryptor.CreateDecryptor())
                        {
                            using (CryptoStream crypt_stream = new CryptoStream(mem_stream, aes_decryptor, CryptoStreamMode.Write))
                            {
                                byte[] c_bytes = str_to_byte_arr(cipher_text);

                                crypt_stream.Write(c_bytes, 0, c_bytes.Length);

                                crypt_stream.FlushFinalBlock();

                                byte[] p_bytes = mem_stream.ToArray();

                                return Encoding.Default.GetString(p_bytes, 0, p_bytes.Length);
                            }
                        }
                    }
                }

                public static string iv_key() =>
                    Guid.NewGuid().ToString().Substring(0, Guid.NewGuid().ToString().IndexOf("-", StringComparison.Ordinal));

                public static string sha256(string r) =>
                    byte_arr_to_str(new SHA256Managed().ComputeHash(Encoding.Default.GetBytes(r)));

                public static string encrypt(string message, string enc_key, string iv)
                {
                    byte[] _key = Encoding.Default.GetBytes(sha256(enc_key).Substring(0, 32));

                    byte[] _iv = Encoding.Default.GetBytes(sha256(iv).Substring(0, 16));

                    return encrypt_string(message, _key, _iv);
                }

                public static string decrypt(string message, string enc_key, string iv)
                {
                    byte[] _key = Encoding.Default.GetBytes(sha256(enc_key).Substring(0, 32));

                    byte[] _iv = Encoding.Default.GetBytes(sha256(iv).Substring(0, 16));

                    return decrypt_string(message, _key, _iv);
                }
            }

        }
        internal class json_wrapper
        {
            public static bool is_serializable(Type to_check) =>
                to_check.IsSerializable || to_check.IsDefined(typeof(DataContractAttribute), true);

            public json_wrapper(object obj_to_work_with)
            {
                current_object = obj_to_work_with;

                var object_type = current_object.GetType();

                serializer = new DataContractJsonSerializer(object_type);

                if (!is_serializable(object_type))
                    throw new Exception($"the object {current_object} isn't a serializable");
            }

            public object string_to_object(string json)
            {
                var buffer = Encoding.Default.GetBytes(json);

                //SerializationException = session expired

                using (var mem_stream = new MemoryStream(buffer))
                    return serializer.ReadObject(mem_stream);
            }

            public T string_to_generic<T>(string json) =>
                (T)string_to_object(json);

            private DataContractJsonSerializer serializer;

            private object current_object;
        }
    }
}