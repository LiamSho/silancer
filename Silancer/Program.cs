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
        static void Main(string[] args)
        {
            Dictionary<string, Lancer> lancers = new Dictionary<string, Lancer>();
            Dictionary<string, string> idNameMap = new Dictionary<string, string>();
            (lancers,idNameMap) = LoadLancers();
            float interval = 3;
            while (true)
            {
                if (interval > 0)
                {
                    Thread.Sleep(100);
                    interval -= 0.1f;
                    continue;
                }
                Console.WriteLine(lancers[idNameMap["testf"]].Command (AttackMode.Normal,DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"))); 
                interval = 3;
            }
        }
        static (Dictionary<string, Lancer>, Dictionary<string,string>) LoadLancers(string filePath = "enemies.json")
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
                newLancer.SendFailed += (s, a) => { Console.WriteLine($"[Failed]{a.MessageIndex}"); };
                newLancer.SendSucceeded += (s, a) => { Console.WriteLine($"[Successful]{a.MessageIndex}"); };
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