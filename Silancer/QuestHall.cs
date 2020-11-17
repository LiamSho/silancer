using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Silancer
{
    public static class QuestHall
    {
        public static Dictionary<string, LancerToken> LancerTokens { get; private set; } = new();
        public static Dictionary<string, Lancer> Lancers { get; private set; } = new();
        public static Dictionary<string, Enemy> Enemies { get; private set ; } = new();
        public static Dictionary<string, Servant> Servants { get; private set; } = new();

        public static void Initiate()
        {
            if (File.Exists(GlobalSettings.Lancer_Tokens_Json_Path))
            {
                using var fs = File.Open(GlobalSettings.Lancer_Tokens_Json_Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader sr = new(fs);
                try
                {
                    LancerTokens = JsonSerializer.Deserialize<Dictionary<string, LancerToken>>(sr.ReadToEnd());
                }
                catch
                {

                }
            }
        }
        private static void LoadDictFromJson<TValue>(string filePath, Dictionary<string,TValue> targetDict)
        {

        }
    }
}
