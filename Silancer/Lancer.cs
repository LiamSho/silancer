using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        public bool IsMegaAttack { get; set; } = false;
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
        public int SendMessage(string str)
        {
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
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                using (StreamWriter sr = new StreamWriter(requestStream))
                    sr.Write(s);
                using HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                using Stream resStream = response.GetResponseStream();
                string resContent = "";
                using (StreamReader sr = new StreamReader(resStream))
                    resContent = sr.ReadToEnd();
                if (!resContent.Contains("addChatItemAction"))
                    return 0;
            }
            catch (WebException)
            {
                return -2;
            }
            catch (Exception)
            {
                return -1;
            }
            return 1;
        }

        public (bool f, List<HttpWebResponse> rs) MegaAttack(string str)
        {
            List<HttpWebResponse> retResponses = new List<HttpWebResponse>();
            List<Task> tasks = new List<Task>();
            bool end = false;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            while (!end)
            {
                Task t = new Task(() =>
                {
                    if (SendMessage(str) >= 0)
                    {
                        end = true;
                    }
                }, cancellationTokenSource.Token);
                t.Start();
                tasks.Add(t);
                Thread.Sleep(100);
            }
            cancellationTokenSource.Cancel();
            return (true, retResponses);
        }
        #region 命令
        public bool Command(AttackMode mode, string content)
        {
            if (string.IsNullOrEmpty(content)) return true;
            try
            {
                CmdsQueue.Enqueue(new AttackCommand() { Content = content, Mode = mode });
            }
            catch
            {
                return false;
            }
            return true;
        }
        public ConcurrentQueue<AttackCommand> CmdsQueue { get; private set; } = new ConcurrentQueue<AttackCommand>();
        #endregion

        #region 进程
        public bool ReadyToStop { get; set; } = false;
        private void Thread_ReadingIn()
        {
            while (!ReadyToStop)
            {
                while (CmdsQueue.Count < 1) Thread.Sleep(40);
                string msg = "";
                if (CmdsQueue.TryDequeue(out AttackCommand cmd)) continue;
                if (string.IsNullOrEmpty(cmd.Content)) continue;
                messageIndex += 1;
                switch (cmd.Mode)
                {
                    case AttackMode.Normal:
                        {
                            int f = SendMessage(cmd.Content);
                            if (f<0)
                            {
                                continueFailedCounter += 1;
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
                                if (f==0)
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
                            break;
                        }
                    case AttackMode.MegaAttack:
                        {
                            (bool f, List<HttpWebResponse> rs) = MegaAttack(cmd.Content);
                            if (!f)
                            {
                                continueFailedCounter += 1;
                                SendFailed?.Invoke(this, new LancerSendEventArgs
                                {
                                    MessageIndex = messageIndex,
                                    SendCounter = sendCounter,
                                    IsSuccessful = false,
                                    IsMegaAttack = true,
                                    SuccessCounter = successCounter,
                                    ContinueFailedCounter = continueFailedCounter
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
                                    IsMegaAttack = true,
                                    SuccessCounter = successCounter
                                });
                            }
                            break;
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
    public class AttackCommand
    {
        public AttackMode Mode { get; set; }
        public string Content { get; set; }
    }
    public enum AttackMode
    {
        Normal, MegaAttack
    }
}