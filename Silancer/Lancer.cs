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
    public class LancerSendEventArgs : EventArgs
    {
        public ulong MessageIndex { get; set; }
        public ulong SendCounter { get; set; }
        public ulong SuccessCounter { get; set; }
        public ulong ContinueFailedCounter { get; set; }
        public bool IsSuccessful { get; set; }
        public bool MayBeBanned { get; set; } = false;
    }
    public class Lancer
    {
        // 本地实例身份
        public string ID { get; set; }
        public string Name { get; set; }

        // WEB参数
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

        // 私有计数器
        private ulong messageIndex = 0;
        private ulong successCounter = 0;
        private ulong sendCounter = 0;
        private ulong continueFailedCounter = 0;

        /// <summary>
        /// 发送单条消息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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
        #region 命令行
        public ConcurrentQueue<string> CmdsQueue { get; private set; } = new ConcurrentQueue<string>();
        public bool WriteLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return true;
            try
            {
                CmdsQueue.Enqueue(line);
            }
            catch
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 进程
        public bool ReadyToStop { get; set; } = false;
        private void Thread_ReadingIn()
        {
            while (!ReadyToStop)
            {
                while (CmdsQueue.Count < 1) Thread.Sleep(40);
                string msg = "";
                while (string.IsNullOrEmpty(msg))
                {
                    if (CmdsQueue.Count < 1) continue;
                    if (CmdsQueue.TryDequeue(out msg)) break;
                }
                Match match = Regex.Match(msg, @"^\[(NM|MA{1})\](.+)");
                if (match.Success)
                {
                    if (match.Groups[1].Value == "NM")
                    {
                        messageIndex += 1;
                        (bool f, HttpWebResponse r) = SendMessage(match.Groups[2].Value);
                        if (!f)
                        {
                            continueFailedCounter += 1UL;
                            SendFailed?.Invoke(this, new LancerSendEventArgs
                            {
                                MessageIndex = messageIndex,
                                SendCounter = sendCounter,
                                IsSuccessful = false,
                                SuccessCounter = successCounter,
                                ContinueFailedCounter = continueFailedCounter
                            });
                        }
                        else
                        {
                            string resContent = "";
                            using (StreamReader sr = new StreamReader(r.GetResponseStream()))
                            {
                                resContent = sr.ReadToEnd();
                            }
                            bool flag5 = !resContent.Contains("addChatItemAction");
                            if (flag5)
                            {
                                SendFailed?.Invoke(this, new LancerSendEventArgs
                                {
                                    MessageIndex = messageIndex,
                                    SendCounter = sendCounter,
                                    IsSuccessful = false,
                                    SuccessCounter = successCounter,
                                    MayBeBanned = true
                                });
                            }
                            else
                            {
                                continueFailedCounter = 0;
                                successCounter += 1;
                                SendSucceeded?.Invoke(this, new LancerSendEventArgs
                                {
                                    MessageIndex = messageIndex,
                                    SendCounter = sendCounter,
                                    IsSuccessful = true,
                                    SuccessCounter = successCounter
                                });
                            }
                        }
                    }
                }
            }
        }
        public Thread MyThread { get; private set; }
        #endregion

        #region 委托与事件
        public delegate void LancerSendEventHandler(Lancer sender, object args);
        public event LancerSendEventHandler SendSucceeded;
        public event LancerSendEventHandler SendFailed;
        #endregion

        #region 结构与析构
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
        ~Lancer()
        {
            ReadyToStop = true;
            MyThread.Join();
            MyThread = null;
        }
        #endregion
    }
}
