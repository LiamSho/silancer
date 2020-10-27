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
            List<Dictionary<string, string>> enemies = null;
            using (FileStream fs = File.Open("enemies.json", FileMode.Open, FileAccess.Read, FileShare.Write))
            using (StreamReader sr = new StreamReader(fs))
            {
                enemies = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(sr.ReadToEnd());
            }
            Dictionary<string, Lancer> lancers = new Dictionary<string, Lancer>();
            foreach (var e in enemies)
            {
                var newLancer = new Lancer(e) { ID = Guid.NewGuid().ToString("X") };
                lancers[e["Name"]] = newLancer;
                //Console.WriteLine(JsonConvert.SerializeObject(newLancer));
            }
            lancers["测试114514 - 靶场请不要开启穿甲弹"].WriteLine("[NM]TEST");
            //(bool f, HttpWebResponse r) = lancers["测试114514 - 靶场请不要开启穿甲弹"].SendMessage("JustTest");
            //if (f)
            //Console.WriteLine(r.StatusCode);
        }
    }
    
}