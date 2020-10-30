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
        static ulong SuccessCounter = 0, FailedCounter = 0;
        private static List<LancerSendResult> Logs { get; set; } = new List<LancerSendResult>();
        static void Main(string[] args)
        {
            Servant servant = new Servant();
            servant.LoadAmmos("test.txt", "test");
            (Dictionary<string, Lancer> lancers, Dictionary<string, string> idNameMap) = LoadLancers();
            float interval = 3;
            while (true)
            {
                string cmdString = Console.ReadLine();

                string[] commandFormat = cmdString.Split(" ");
                switch (commandFormat[0].ToUpper())
                {
                    case "LISTLOGS":
                        if (commandFormat.Length == 3)
                        {
                            int l;
                            int h;
                            try
                            {
                                l = Convert.ToInt32(commandFormat[1]);
                                h = Math.Min(Convert.ToInt32(commandFormat[2]), Logs.Count - 1);
                            }
                            catch
                            {
                                Console.WriteLine("[CORE]解析LISTLOGS命令结构失败");
                                continue;
                            }
                            for (int i = l; i <= h; i++)
                            {
                                Console.WriteLine(Logs[i]);
                            }
                        }
                        else
                        {
                            for(int i = 0; i < Logs.Count; i++)
                            {
                                Console.WriteLine(Logs[i]);
                            }
                        }
                        break;
                    case "SEND":
                        if (commandFormat.Length == 3)
                        {
                            int l;
                            int h;
                            try
                            {
                                l = Convert.ToInt32(commandFormat[1]);
                                h = Math.Min(Convert.ToInt32(commandFormat[2]), Logs.Count - 1);
                            }
                            catch
                            {
                                Console.WriteLine("[CORE]解析LISTLOGS命令结构失败");
                                continue;
                            }
                            for (int i = l; i <= h; i++)
                            {
                                Console.WriteLine(Logs[i]);
                            }
                        }
                        else
                        {
                            Console.WriteLine("[CORE]解析SEND命令结构失败");
                        }
                        break;
                }
                if (interval > 0)
                {
                    Thread.Sleep(100);
                    interval -= 0.1f;
                    continue;
                }
                lancers[idNameMap["testf"]].Command(AttackMode.Normal, servant.RandomAmmo);
                interval = 3;
            }
        }

        static (Dictionary<string, Lancer>, Dictionary<string, string>) LoadLancers(string filePath = "enemies.json")
        {
            List<Dictionary<string, string>> enemies = null;
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Write))
            using (StreamReader sr = new StreamReader(fs))
            {
                enemies = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(sr.ReadToEnd());
            }
            Dictionary<string, Lancer> lancers = new Dictionary<string, Lancer>();
            Dictionary<string, string> idNameMap = new Dictionary<string, string>();
            foreach (var e in enemies)
            {
                var newLancer = new Lancer(e) { ID = Guid.NewGuid().ToString("X") };
                newLancer.SendFailed += (s, a) => { Logs.Add(a); FailedCounter += 1; };
                newLancer.SendSucceeded += (s, a) => { Logs.Add(a); SuccessCounter += 1; };
                var tempName = newLancer.Name;
                int counter = 1;
                while (idNameMap.ContainsKey(tempName))
                    tempName = $"{tempName}-{counter}";
                newLancer.Name = tempName;
                idNameMap[tempName] = newLancer.ID;
                lancers[newLancer.ID] = newLancer;
            }
            return (lancers, idNameMap);
        }
    }

}