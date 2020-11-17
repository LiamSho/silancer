using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Silancer
{
    public static class GlobalSettings
    {
        public static string Google_Key
        {
            get { return _google_key; }
            set { _google_key = value; Host = $"https://www.youtube.com/youtubei/v1/live_chat/send_message?key={_google_key}"; }
        }
        private static string _google_key = "";
        public static string Settings_Folder_Name { get; set; } = "settings";
        public static string Ammos_Folder_Name { get; set; } = "ammos";
        public static string Lancers_Json_File_Name { get; set; } = "lancers.json";
        public static string Lancer_Tokens_Json_File_Name { get; set; } = "lancertokens.json";
        public static string Enemies_Json_File_Name { get; set; } = "enemies.json";

        public static string Host { get; set; }
        public static string Ammos_Folder_Path
        {
            get
            {
                return Path.Combine(Settings_Folder_Name, Ammos_Folder_Path);
            }
        }
        public static string Lancers_Json_Path
        {
            get
            {
                return Path.Combine(Settings_Folder_Name, Lancers_Json_File_Name);
            }
        }
        public static string Lancer_Tokens_Json_Path
        {
            get
            {
                return Path.Combine(Settings_Folder_Name, Lancer_Tokens_Json_File_Name);
            }
        }
        public static string Enemies_Json_Path
        {
            get
            {
                return Path.Combine(Settings_Folder_Name, Enemies_Json_File_Name);
            }
        }

        public static void LoadSettings(string path)
        {
            if (!File.Exists(path)) return;
            using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);
            Dictionary<string, string> tempDic = null;
            try
            {
                tempDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
            }
            catch
            {
                return;
            }
            foreach (var p in typeof(GlobalSettings).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                try
                {
                    string key = p.Name;
                    if (tempDic.ContainsKey(key))
                        if (!string.IsNullOrEmpty(tempDic[key]))
                            p.SetValue(null, tempDic[key]);
                }
                catch
                {
                    continue;
                }
            }
        }
        public static bool SaveSettings(string path)
        {
            try
            {
                FileStream fs = null;
                if (!File.Exists(path))
                    fs = File.Create(path);
                else
                    fs = File.OpenWrite(path);
                using var sw = new StreamWriter(fs);
                sw.Write(GlobalSettings.ToString());
                fs.Flush();
                fs.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public new static string ToString()
        {
            return $"{{\"Google_Key\":\"{Google_Key}\",\"Settings_Folder_Name\":\"{Settings_Folder_Name}\",\"Ammos_Folder_Name\":\"{Ammos_Folder_Name}\"," +
                    $"\"Lancers_Json_File_Name\":\"{Lancers_Json_File_Name}\",\"Enemies_Json_File_Name\":\"{Enemies_Json_File_Name}\",\"Lancer_Tokens_Json_File_Name\":\"{Lancer_Tokens_Json_File_Name}\"}}";
        }
    }
}
