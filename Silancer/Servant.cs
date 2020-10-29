using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Silancer
{
    public class Servant
    {
        public Dictionary<string, List<Ammo>> AmmosDictionary { get; private set; } = new Dictionary<string, List<Ammo>>();
        public List<Ammo> AllAmmos { get; private set; } = new List<Ammo>() { new Ammo("TEST INFO") };
        private readonly Random random = new Random();
        public Ammo RandomAmmo { get => AllAmmos[random.Next(0, AllAmmos.Count)]; }
        public Ammo GetAmmo(string listName, int index) => AmmosDictionary[listName][index];
        public int LoadAmmos(string filePath,string listName, AmmoType type = AmmoType.Plain)
        {
            if (AmmosDictionary.ContainsKey(listName)) return -2;
            if (!File.Exists(filePath)) return -1;
            FileStream fileStream;
            try
            {
                fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch
            {
                return -1;
            }
            List<Ammo> tempList = new List<Ammo>();
            if (fileStream == null) return -1;
            using StreamReader sr = new StreamReader(fileStream);
            switch (type)
            {
                case AmmoType.Plain:
                    while (!sr.EndOfStream)
                    {
                        Ammo re = new Ammo(sr.ReadLine());
                        tempList.Add(re);
                        AllAmmos.Add(re);
                    }
                    break;
            }
            AmmosDictionary[listName] = tempList;
            return tempList.Count;
        }
    }
    public enum AmmoType
    {
        Plain
    }
    public class Ammo
    {
        public string Content { get; set; }
        public Ammo(string content)
        {
            Content = content;
        }
    }
}
