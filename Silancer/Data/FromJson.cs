using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Silancer.Data
{
    //public class FromJson
    //{
    //    public static Dictionary<string, TEntity> LoadFromJson<TEntity>(string filePath, Action<TEntity> bindingFunc = null) where TEntity : IFromJson, new()
    //    {
    //        List<Dictionary<string, string>> iniDicts = null;
    //        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Write))
    //        using (StreamReader sr = new(fs))
    //        {
    //            iniDicts = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(sr.ReadToEnd());
    //        }
    //        var entities = new Dictionary<string, TEntity>();
    //        foreach (var e in iniDicts)
    //        {
    //            var newEntity = new TEntity();
    //            newEntity.InitializeFromDictionary(e);
    //            bindingFunc?.Invoke(newEntity);
    //            var tempName = e["Name"];
    //            int counter = 1;
    //            if (entities.ContainsKey(tempName))
    //            {
    //                while (entities.ContainsKey($"{tempName}-{counter}"))
    //                    counter++;
    //                tempName = $"{tempName}-{counter}";
    //            }
    //            newEntity.Name = tempName;
    //            entities[tempName] = newEntity;
    //        }
    //        return entities;
    //    }
    //}
    public interface IFromJson
    {
        public string Name { get; set; }
        public bool InitializeFromDictionary(Dictionary<string, string> iniDict);
    }
}
