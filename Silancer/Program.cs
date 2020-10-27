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
            lancers[idNameMap["testf"]].WriteLine("[MA]2");
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