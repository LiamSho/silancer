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
            //lancers["测试114514 - 靶场请不要开启穿甲弹"]
            //(bool f, HttpWebResponse r) = lancers["测试114514 - 靶场请不要开启穿甲弹"].SendMessage("JustTest");
            //if (f)
                //Console.WriteLine(r.StatusCode);
        }
    }
    public class LancerSendEventArgs : EventArgs
    {
        public int MessageIndex { get; set; }
        public int SendCounter { get; set; }
        public int SuccessCounter { get; set; }
        public bool IsSuccessful { get; set; }
        public bool MayBeBanned { get; set; } = false;
    }
    public class Lancer
    {
        private int messageIndex = 0;
        private int successCounter = 0;
        private int sendCounter = 0;
        public delegate void LancerSendEventHandler(Lancer sender, object args);
        public event LancerSendEventHandler SendSucceeded;
        public event LancerSendEventHandler SendFailed;
        public bool ReadyToStop { get; set; } = false;
        public Stream InputStream { get; private set; } = new MemoryStream();
        private void Thread_ReadingIn()
        {
            while (!ReadyToStop)
            {
                while (!InputStream.CanRead) Thread.Sleep(40);
                string msg = "";
                using (StreamReader sr = new StreamReader(InputStream))
                {
                    while (string.IsNullOrEmpty(msg) && InputStream.CanRead)
                        msg = sr.ReadLine();
                }
                var match = Regex.Match(msg, @"^\[?([NM,MA])\]");
                if (match.Success)
                {
                    if (match.Captures[0].Value == "NM")
                    {
                        messageIndex++;
                        (bool f, HttpWebResponse r) = SendMessage(msg);
                        if (!f)
                        {
                            SendFailed?.Invoke(this,
                             new LancerSendEventArgs()
                             {
                                 MessageIndex = messageIndex,
                                 SendCounter = sendCounter,
                                 IsSuccessful = false,
                                 SuccessCounter = successCounter
                             });
                        }
                        string resContent = "";
                        using (StreamReader sr = new StreamReader(r.GetResponseStream()))
                        {
                            resContent = sr.ReadToEnd();
                        }
                        if (!resContent.Contains("addChatItemAction"))
                        {
                            SendFailed?.Invoke(this,
                            new LancerSendEventArgs()
                            {
                                MessageIndex = messageIndex,
                                SendCounter = sendCounter,
                                IsSuccessful = false,
                                SuccessCounter = successCounter,
                                MayBeBanned = true
                            });
                            continue;
                        }
                        successCounter++;
                        SendSucceeded?.Invoke(this,
                            new LancerSendEventArgs()
                            {
                                MessageIndex = messageIndex,
                                SendCounter = sendCounter,
                                IsSuccessful = true,
                                SuccessCounter = successCounter,
                            });
                    }
                    else
                    {
                        //(bool f, HttpWebResponse r) = MegaAttack(msg);
                    }
                }
            }
        }
        public Thread MyThread { get; private set; }
        public ConcurrentQueue<string> LogsQueue { get; private set; }
        public double Interval { get; set; } = 0;

        public string ID { get; set; }
        public string Name { get; set; }
        public string Key
        {
            get => key;
            set
            {
                if (value == key) return;
                key = value;
                Host = $"https://www.youtube.com/youtubei/v1/live_chat/send_message?key={@key}";
            }
        }
        private string key = "";
        public string Host { get; set; }
        public string Cookie { get; set; }
        public string Authorization { get; set; }
        public string Onbehalfofuser { get; set; }
        public string Param { get; set; }
        public (bool successFlag, HttpWebResponse response) SendMessage(string str)
        {
            HttpWebResponse response = null;
            sendCounter++;
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Host);
                httpWebRequest.KeepAlive = true;
                httpWebRequest.Headers.Set(HttpRequestHeader.Authorization, Authorization);
                httpWebRequest.Headers.Add("X-Origin", "https://www.youtube.com");
                httpWebRequest.Headers.Add("Origin", "https://www.youtube.com");
                httpWebRequest.Headers.Set(HttpRequestHeader.Cookie, Cookie);
                httpWebRequest.Method = "POST";
                httpWebRequest.ServicePoint.Expect100Continue = false;
                string s = string.Concat(new string[]
                {
                    "{\"context\":{\"client\":{ \"clientName\":\"WEB\",\"clientVersion\":\"2.20201023.02.00\"},\"request\":{ },\"user\":{ \"onBehalfOfUser\":\"",
                    Onbehalfofuser,
                    "\"}},\"params\":\"",
                    Param,
                    "\",\"richMessage\":{ \"textSegments\":[{\"text\":\"",
                    str,
                    "\"}]}}"
                });
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                httpWebRequest.ContentLength = bytes.Length;
                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                response = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError)
                {
                    return (false, null);
                }
                response = (HttpWebResponse)ex.Response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (response != null)
                {
                    response.Close();
                }
                return (false, null);
            }
            return (true, response);
        }
        //public (bool f, HttpWebResponse r) MegaAttack(string str)
        //{

        //}
        public Lancer(Dictionary<string, string> setting)
        {
            Dictionary<string, string> tempDic = setting;
            Name = tempDic["Name"];
            Key = tempDic["Key"];
            Cookie = tempDic["Cookie"];
            Authorization = tempDic["Authorization"];
            Param = tempDic["Param"];
            try
            {
                Onbehalfofuser = tempDic["Onbehalfofuser"];
            }
            catch
            {

            }
            MyThread = new Thread(new ThreadStart(Thread_ReadingIn));
            MyThread.Start();
        }
    }
}