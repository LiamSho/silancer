﻿using Silancer.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Silancer
{
    public enum AmmoMode
    {
        Random,
        Loop
    }
    public class LancerSendResult : EventArgs
    {
        public ulong MessageIndex { get; set; }
        public ulong SendCounter { get; set; }
        public ulong SuccessCounter { get; set; }
        public ulong ContinueFailedCounter { get; set; }
        public Ammo MyAmmo { get; set; }
        public bool IsSendSuccessful { get; set; }
        public bool IsNetSuccessful { get; set; }
        public bool IsMegaAttack { get; set; } = false;
        public bool IsInnerException { get; set; } = false;
    }
    public class Lancer : IFromJson
    {
        public bool IsAlive { get => MyThread.IsAlive; }
        /// <summary>
        /// 本地实例身份
        /// </summary>
        public string Name { get; set; }
        public Servant MyServant { get; set; } = null;
        public AmmoMode ShootMode { get; set; } = AmmoMode.Random;
        public int LoopAmmoPointer { get; set; } = 0;
        public string LoopAmmoList { get; set; }
        public Enemy MyEnemy { get; set; } = null;
        public int MaxInterval { get; set; } = 3000;


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
        public string Host { get; private set; }
        public string Cookie { get; set; }
        public string Authorization
        {
            get
            {
                if (string.IsNullOrEmpty(_authorization))
                    _authorization = GetAuth(Cookie);
                return _authorization;
            }
        }
        public string GetAuth(string cookie)
        {
            string time = ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();
            StringBuilder sub = new StringBuilder();
            foreach (var t in SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes($"{time} {Regex.Match(cookie, @"SAPISID=(.*?);").Groups[1]} https://www.youtube.com")))
            {
                sub.Append(t.ToString("X2"));
            }
            return $"SAPISIDHASH {time}_{sub.ToString().ToLower()}";
        }
        public string _authorization = "";
        public string Onbehalfofuser { get; set; }

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
        public int SendMessage(string str, Enemy emeny)
        {
            sendCounter++;
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Host);
                httpWebRequest.KeepAlive = true;
                httpWebRequest.Headers.Add("X-Origin", "https://www.youtube.com");
                httpWebRequest.Headers.Add("Origin", "https://www.youtube.com");
                httpWebRequest.Headers.Set(HttpRequestHeader.Authorization, Authorization);
                httpWebRequest.Headers.Set(HttpRequestHeader.Cookie, Cookie);
                httpWebRequest.Headers.Set(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 Safari/537.36");
                httpWebRequest.Method = "POST";
                httpWebRequest.ServicePoint.Expect100Continue = false;
                string s = string.Concat(new string[]
                {
                    "{\"context\":{\"client\":{ \"clientName\":\"WEB\",\"clientVersion\":\"2.20201023.02.00\"},\"request\":{ },\"user\":{ \"onBehalfOfUser\":\"",
                    Onbehalfofuser,
                    "\"}},\"params\":\"",
                    emeny.Params,
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

        public int MegaAttack(string str, Enemy enemy)
        {
            List<Task> tasks = new List<Task>();
            bool end = false;
            int counter = 0;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            while (!end)
            {
                Task t = new Task(() =>
                {
                    if (counter >= 400) { end = true; return; }
                    if (SendMessage(str, enemy) > 0)
                    {
                        end = true;
                    }
                }, cancellationTokenSource.Token);
                t.Start();
                tasks.Add(t);
                Interlocked.Increment(ref counter);
                Thread.Sleep(100);
            }
            cancellationTokenSource.Cancel();
            return counter;
        }

        #region 线程
        public bool ReadyToStop { get; set; } = false;
        private long coolDown = 0;
        private void Thread_ReadingIn()
        {
            while (!ReadyToStop)
            {
                try
                {
                    while (coolDown > 0 || MyServant == null || MyEnemy == null)
                    {
                        Thread.Sleep(40);
                        coolDown -= 40;
                    }
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    messageIndex += 1;
                    bool isNetSuccessful = false;
                    bool isSendSuccessful = false;
                    bool isInnerException = false;
                    Ammo thisAmmo = null;
                    switch (ShootMode)
                    {
                        case AmmoMode.Random:
                            thisAmmo = MyServant.RandomAmmo;
                            break;
                        case AmmoMode.Loop:
                            try
                            {
                                (thisAmmo, LoopAmmoPointer) = MyServant.LoopAmmo(LoopAmmoPointer, LoopAmmoList);
                            }
                            catch
                            {
                                thisAmmo = MyServant.RandomAmmo;
                            }
                            break;
                    }
                    try
                    {
                        int f = -1;
                        if (ShootMode == AmmoMode.Random)
                            f = SendMessage(thisAmmo.Content, MyEnemy);
                        if (f < 0)
                            continueFailedCounter += 1;
                        else if (f == 0)
                            isNetSuccessful = true;
                        else
                        {
                            continueFailedCounter = 0;
                            successCounter += 1;
                            isNetSuccessful = true;
                            isSendSuccessful = true;
                        }
                    }
                    catch
                    {
                        isInnerException = true;
                    }
                    LancerSendResult args = new LancerSendResult()
                    {
                        MyAmmo = thisAmmo,
                        MessageIndex = messageIndex,
                        SendCounter = sendCounter,
                        SuccessCounter = successCounter,
                        IsNetSuccessful = isNetSuccessful,
                        IsSendSuccessful = isSendSuccessful,
                        ContinueFailedCounter = continueFailedCounter,
                        IsInnerException = isInnerException
                    };
                    if (args.IsNetSuccessful) coolDown = MaxInterval;
                    try
                    {
                        SendComplete?.Invoke(this, args);
                    }
                    catch
                    {

                    }
                    coolDown -= sw.ElapsedMilliseconds;
                }
                catch
                {
                    coolDown = MaxInterval;
                }

            }
        }
        public Thread MyThread { get; private set; }
        #endregion

        #region 委托与事件
        public delegate void LancerSendEventHandler(Lancer sender, LancerSendResult args);
        public event LancerSendEventHandler SendComplete;
        #endregion

        #region 结构与析构
        public bool InitializeFromDictionary(Dictionary<string, string> iniDict)
        {
            try
            {
                Name = iniDict["Name"];
                Key = iniDict["Key"];
                Cookie = iniDict["Cookie"];
                Onbehalfofuser = iniDict["Onbehalfofuser"];
            }
            catch
            {
                return false;
            }
            if (string.IsNullOrEmpty(Cookie) || string.IsNullOrEmpty("Key") || string.IsNullOrEmpty("Name"))
                return false;
            MyThread = new Thread(new ThreadStart(Thread_ReadingIn));
            MyThread.Start();
            return true;
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
        public Ammo MyAmmo { get; set; }
        public Enemy Enemy { get; set; }
    }
    public enum AttackMode
    {
        Normal, MegaAttack
    }
}