using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Silancer.Data;
using Fclp;
using System.Linq;
using Terminal.Gui;

namespace Silancer.Cli
{
    public sealed class Program
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
        private static void PrintMessage(string msg)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{DateTime.Now:MM-dd HH:mm:ss}] ");
            Console.ResetColor();
            Console.WriteLine($"{msg}");
            Console.Write("> ");
        }
        static void Main(string[] args)
        {
            if (System.Globalization.CultureInfo.CurrentCulture.LCID != 2052)
                return;
            Application.Init();
            var top = Application.Top;

            #region Lancers面板
            var lancersWindow = new Window("Lancers")
            {
                X = 0,
                Y = 0,
                Width = Dim.Percent(10),
                Height = Dim.Percent(100)-3
            };
            top.Add(lancersWindow);
            var lancersList = new RadioGroup(0, 0, new NStack.ustring[] { "Survivol10" });
            lancersWindow.Add(lancersList);
            lancersWindow.Remove(lancersList);
            //lancersWindow.Add(new RadioGroup(0, 0, new NStack.ustring[] { "Survivol101"}));
            #endregion

            #region Enemies面板
            var enemiesWindow = new Window("Enemies")
            {
                X = Pos.Percent(10),
                Y = 0,
                Width = Dim.Percent(10),
                Height = Dim.Percent(100) - 3
            };
            top.Add(enemiesWindow);
            #endregion

            #region 监视面板
            var inspectorWindow = new Window("监视面板")
            {
                X = Pos.Percent(20),
                Y = 0,
                Width = Dim.Percent(80),
                Height = Dim.Percent(50)
            };
            top.Add(inspectorWindow);
            #endregion

            #region 
            #endregion

            #region 输入区域
            var inputWindow = new Window("发布命令")
            {
                X = 0,
                Y = Pos.Bottom(top)-3,
                Width = Dim.Fill(),
                Height = 3
            };
            var inputField = new TextField("")
            {
                X = 1,
                Width = Dim.Fill()
            };
            inputWindow.Add(inputField);
            top.Add(inputWindow);
            #endregion
            
            Application.Run();

            string mainSettingsFilePath = "";
            if (args.Length < 1)
            {
                PrintMessage("未输入主配置文件路径，将使用缺省值settings.json代替");
                mainSettingsFilePath = "settings.json";
            }
            else
            {
                if (!File.Exists(args[0]))
                {
                    PrintMessage($"输入的主配置文件路径({args[0]})不可用，程序已退出");
                    Environment.Exit(-1);
                }
                mainSettingsFilePath = args[0];
            }

            GlobalSettings settings = GlobalSettings.LoadSettings(mainSettingsFilePath);
            if (settings == null)
            {
                PrintMessage("主配置文件settings.json解析失败，程序已退出");
                Environment.Exit(-1);
            }

            if (!Directory.Exists(settings.Ammos_Folder))
            {
                PrintMessage($"弹药库路径({settings.Ammos_Folder})不可用，程序已退出");
                Environment.Exit(-1);
            }
            Servant servant = new Servant();
            foreach (var f in Directory.GetFiles(settings.Ammos_Folder))
            {
                int tcounter = servant.LoadAmmos(f, Path.GetFileNameWithoutExtension(f));
                if (tcounter > 0)
                {
                    PrintMessage($"弹药库-{Path.GetFileNameWithoutExtension(f)}-装载成功，共读入{tcounter}条记录");
                }
                else
                {
                    switch (tcounter)
                    {
                        case -1:
                            PrintMessage($"弹药库-{Path.GetFileNameWithoutExtension(f)}-文件不可用，已跳过");
                            continue;
                    }
                }
            }

            Dictionary<string, Lancer> lancers =
                FromJson.LoadFromJson<Lancer>(settings.Lancers_Json_Path,
                (newLancer) =>
                {
                    newLancer.MyServant = servant;
                    newLancer.SendComplete += (s, a) =>
                    {
                        PrintMessage($"[{s.Name}][{(a.IsNetSuccessful ? (a.IsSendSuccessful ? "SU" : "SE") : "NE")}|{a.MessageIndex}]{a.MyAmmo.Content}");
                    };
                });
            Dictionary<string, Enemy> enemies =
                FromJson.LoadFromJson<Enemy>(settings.Enemies_Json_Path);

            while (true)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("> ");
                string cmdString = Console.ReadLine();

                string[] commandFormat = cmdString.Split(" ");
                string commandKey = commandFormat[0];
                commandFormat = commandFormat.Skip(1).ToArray();
                var commandStructure = new FluentCommandLineParser();
                switch (commandKey.ToUpper())
                {
                    case "CREATE":
                        {
                            string tLancer = "", tEnemy = "", tAmmoMode = "", tMagazine = null;
                            int tInterval = 15000;
                            commandStructure.Setup<string>('l', "lancer").Callback(lancer => tLancer = lancer).Required();
                            commandStructure.Setup<string>('e', "enemy").Callback(enemy => tEnemy = enemy).Required();
                            commandStructure.Setup<string>('m', "mode").Callback(ammoMode => tAmmoMode = ammoMode).SetDefault("LOOP");
                            commandStructure.Setup<string>('z', "magazine").Callback(magazine => tMagazine = magazine);
                            commandStructure.Setup<int>('i', "interval").Callback(interval => tInterval = interval).SetDefault(15000);
                            commandStructure.Parse(commandFormat);

                            if (!lancers.ContainsKey(tLancer))
                            {
                                PrintMessage($"[CORE]不存在名为{tLancer}的Lancer");
                                continue;
                            }
                            var curLancer = lancers[tLancer];
                            if (!enemies.ContainsKey(tEnemy))
                            {
                                PrintMessage($"[CORE]不存在名为{tEnemy}的Enemy");
                                continue;
                            }
                            curLancer.MyEnemy = enemies[tEnemy];
                            curLancer.MaxInterval = Math.Max(100, tInterval);

                            switch (tAmmoMode)
                            {
                                default:
                                    curLancer.ShootMode = AmmoMode.Random;
                                    break;
                                case "LOOP":
                                    curLancer.LoopAmmoPointer = 0;
                                    curLancer.ShootMode = AmmoMode.Loop;
                                    if (!string.IsNullOrEmpty(tMagazine))
                                    {
                                        curLancer.LoopAmmoList = tMagazine;
                                    }
                                    else
                                    {
                                        curLancer.LoopAmmoList = null;
                                    }
                                    break;
                            }
                            break;
                        }
                    case "PAUSE":
                        {
                            if (commandFormat.Length >= 2)
                            {
                                if (!lancers.ContainsKey(commandFormat[1]))
                                {
                                    PrintMessage($"[CORE]不存在名为{commandFormat[1]}的Lancer");
                                    continue;
                                }
                                lancers[commandFormat[1]].MyEnemy = null;
                            }
                            break;
                        }
                    case "LANCERS":
                        {
                            foreach (var lancer in lancers)
                            {

                            }
                            break;
                        }
                    default:
                        PrintMessage($"[CORE]{commandFormat[0].ToUpper()}-未知的命令");
                        break;
                }
            }
        }

    }
}