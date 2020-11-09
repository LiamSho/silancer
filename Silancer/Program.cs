using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;

namespace Silancer
{
    class Program
    {
        private class GlobalSettings
        {
            public string Ammos_Folder { get; set; }
            public string Lancers_Json_Path { get; set; }
            public string Enemies_Json_Path { get; set; }
            public static GlobalSettings LoadSettings(string path)
            {
                if (!File.Exists(path))
                    return null;
                GlobalSettings ret = new GlobalSettings();
                try
                {
                    using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var sr = new StreamReader(fs);
                    Dictionary<string, string> tempDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                    if (tempDic.ContainsKey("Ammos_Folder")) ret.Ammos_Folder = tempDic["Ammos_Folder"];
                    if (tempDic.ContainsKey("Lancers_Json_Path")) ret.Lancers_Json_Path = tempDic["Lancers_Json_Path"];
                    if (tempDic.ContainsKey("Enemies_Json_Path")) ret.Enemies_Json_Path = tempDic["Enemies_Json_Path"];
                }
                catch
                {
                    return null;
                }
                return ret;
            }
            public static bool SaveSettings(string path, GlobalSettings settings)
            {
                try
                {
                    FileStream fs = null;
                    if (!File.Exists(path))
                        fs = File.Create(path);
                    else
                        fs = File.OpenWrite(path);
                    using var sw = new StreamWriter(fs);
                    sw.Write(JsonConvert.SerializeObject(settings));
                    fs.Flush();
                    fs.Close();
                    return true;
                }
                catch
                {
                    return false;
                }

            }
        }
        static void Main(string[] args)
        {
            if (System.Globalization.CultureInfo.CurrentCulture.LCID != 2052)
                return;
            string mainSettingsFilePath = "";
            if (args.Length<1)
            {
                Console.WriteLine("未输入主配置文件路径，将使用缺省值settings.json代替");
                mainSettingsFilePath = "settings.json";
            }
            else
            {
                if (!File.Exists(args[0]))
                {
                    Console.WriteLine($"输入的主配置文件路径({args[0]})不可用，程序已退出");
                    Environment.Exit(-1);
                }
                mainSettingsFilePath = args[0];
            }

            
            GlobalSettings settings = GlobalSettings.LoadSettings(mainSettingsFilePath);
            if (settings == null)
            {
                Console.WriteLine("主配置文件settings.json解析失败，程序已退出");
                Environment.Exit(-1);
            }

            if (!Directory.Exists(settings.Ammos_Folder))
            {
                Console.WriteLine($"弹药库路径({settings.Ammos_Folder})不可用，程序已退出");
                Environment.Exit(-1);
            }
            Servant servant = new Servant();
            foreach (var f in Directory.GetFiles(settings.Ammos_Folder))
            {
                int tcounter = servant.LoadAmmos(f, Path.GetFileNameWithoutExtension(f));
                if (tcounter > 0)
                {
                    Console.WriteLine($"弹药库-{Path.GetFileNameWithoutExtension(f)}-装载成功，共读入{tcounter}条记录");
                }
                else
                {
                    switch (tcounter)
                    {
                        case -1:
                            Console.WriteLine($"弹药库-{Path.GetFileNameWithoutExtension(f)}-文件不可用，已跳过");
                            continue;
                    }
                }
            }
            
            Dictionary<string, Lancer> lancers =
                LoadFromJson<Lancer>(settings.Lancers_Json_Path,
                (newLancer) =>
                {
                    newLancer.MyServant = servant;
                    newLancer.SendComplete += (s, a) => 
                    { 
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{s.Name}][{(a.IsNetSuccessful ? (a.IsSendSuccessful ? "SU" : "SE") : "NE")}|{a.MessageIndex}]{a.MyAmmo.Content}"); 
                    };
                });
            Dictionary<string, Enemy> enemies =
                LoadFromJson<Enemy>(settings.Enemies_Json_Path);

            while (true)
            {
                string cmdString = Console.ReadLine();
                string[] commandFormat = cmdString.Split(" ");
                switch (commandFormat[0].ToUpper())
                {
                    case "CREATE":
                        if (commandFormat.Length >= 5)
                        {
                            if (!lancers.ContainsKey(commandFormat[2]))
                            {
                                Console.WriteLine($"[CORE]不存在名为{commandFormat[2]}的Lancer");
                                continue;
                            }
                            var curLancer = lancers[commandFormat[2]];
                            if (!enemies.ContainsKey(commandFormat[3]))
                            {
                                Console.WriteLine($"[CORE]不存在名为{commandFormat[2]}的Enemy");
                                continue;
                            }
                            curLancer.MyEnemy = enemies[commandFormat[3]];
                            try
                            {
                                curLancer.MaxInterval = Math.Max(100, Math.Abs(Convert.ToInt32(commandFormat[1])));
                            }
                            catch
                            {
                                Console.WriteLine($"[CORE]Maxinterval必须是一个正整数");
                                continue;
                            }
                            switch (commandFormat[4].ToUpper())
                            {
                                default:
                                    curLancer.ShootMode = AmmoMode.Random;
                                    break;
                                case "LOOP":
                                    curLancer.LoopAmmoPointer = 0;
                                    curLancer.ShootMode = AmmoMode.Loop;
                                    break;
                            }
                            if (commandFormat.Length >= 6)
                            {
                                curLancer.LoopAmmoList = commandFormat[5];
                            }
                            else
                            {
                                curLancer.LoopAmmoList = null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("[CORE]解析CREATE命令结构失败");
                        }
                        break;
                    case "PAUSE":
                        if (commandFormat.Length >= 2)
                        {
                            if (!lancers.ContainsKey(commandFormat[1]))
                            {
                                Console.WriteLine($"[CORE]不存在名为{commandFormat[1]}的Lancer");
                                continue;
                            }
                            lancers[commandFormat[1]].MyEnemy = null;
                        }
                        break;
                }
            }
        }
        static Dictionary<string, TEntity> LoadFromJson<TEntity>(string filePath, Action<TEntity> bindingFunc = null) where TEntity : IFromJson, new()
        {
            List<Dictionary<string, string>> iniDicts = null;
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Write))
            using (StreamReader sr = new StreamReader(fs))
            {
                iniDicts = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(sr.ReadToEnd());
            }
            var entities = new Dictionary<string, TEntity>();
            foreach (var e in iniDicts)
            {
                var newEntity = new TEntity();
                newEntity.InitializeFromDictionary(e);
                bindingFunc?.Invoke(newEntity);
                var tempName = e["Name"];
                int counter = 1;
                if (entities.ContainsKey(tempName))
                {
                    while (entities.ContainsKey($"{tempName}-{counter}"))
                        counter++;
                    tempName = $"{tempName}-{counter}";
                }
                newEntity.Name = tempName;
                entities[tempName] = newEntity;
            }
            return entities;
        }
    }

    public interface IFromJson
    {
        public string Name { get; set; }
        public bool InitializeFromDictionary(Dictionary<string, string> iniDict);
    }
}