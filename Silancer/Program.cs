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
        private static List<LancerSendResult> Results { get; set; } = new List<LancerSendResult>();
        static void Main(string[] args)
        {
            Servant servant = new Servant();
            servant.LoadAmmos(Path.Combine("settings", "test.txt"), "test");
            Dictionary<string, Lancer> lancers =
                LoadFromJson<Lancer>(Path.Combine("settings",  "lancers.json"),
                (newLancer) =>
                {
                    newLancer.SendFailed += (s, a) => { Results.Add(a); FailedCounter += 1; };
                    newLancer.SendSucceeded += (s, a) => { Results.Add(a); SuccessCounter += 1; };
                });
            Dictionary<string, Enemy> enemies =
                LoadFromJson<Enemy>(Path.Combine("settings", "enemies.json"));

            float interval = 3;
            while (true)
            {
                if (interval > 0)
                {
                    Thread.Sleep(100);
                    interval -= 0.1f;
                    continue;
                }
                lancers["Survivol10"].Command(AttackMode.Normal, servant.RandomAmmo, enemies["testf"]);
                interval = 3;
                //string cmdString = Console.ReadLine();

                //string[] commandFormat = cmdString.Split(" ");
                //switch (commandFormat[0].ToUpper())
                //{
                //    case "LISTLOGS":
                //        if (commandFormat.Length == 3)
                //        {
                //            int l;
                //            int h;
                //            try
                //            {
                //                l = Convert.ToInt32(commandFormat[1]);
                //                h = Math.Min(Convert.ToInt32(commandFormat[2]), Logs.Count - 1);
                //            }
                //            catch
                //            {
                //                Console.WriteLine("[CORE]解析LISTLOGS命令结构失败");
                //                continue;
                //            }
                //            for (int i = l; i <= h; i++)
                //            {
                //                Console.WriteLine(Logs[i]);
                //            }
                //        }
                //        else
                //        {
                //            for (int i = 0; i < Logs.Count; i++)
                //            {
                //                Console.WriteLine(Logs[i]);
                //            }
                //        }
                //        break;
                //    case "SEND":
                //        if (commandFormat.Length == 3)
                //        {
                //            int l;
                //            int h;
                //            try
                //            {
                //                l = Convert.ToInt32(commandFormat[1]);
                //                h = Math.Min(Convert.ToInt32(commandFormat[2]), Logs.Count - 1);
                //            }
                //            catch
                //            {
                //                Console.WriteLine("[CORE]解析LISTLOGS命令结构失败");
                //                continue;
                //            }
                //            for (int i = l; i <= h; i++)
                //            {
                //                Console.WriteLine(Logs[i]);
                //            }
                //        }
                //        else
                //        {
                //            Console.WriteLine("[CORE]解析SEND命令结构失败");
                //        }
                //        break;
                //}
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