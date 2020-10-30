using System;
using System.Collections.Generic;
using System.Text;

namespace Silancer
{

    public class Enemy:IFromJson
    {
        public string Name { get; set; }
        public string ChannelID { get; set; }
        public string LiveID { get; set; }
        public string Params
        {
            get
            {
                if (string.IsNullOrEmpty(_param))
                    _param = GetParams(ChannelID, LiveID);
                return _param;
            }
        }

        private string _param = null;
        public string GetParams(string channelID, string liveID)
        {
            var a = Encoding.ASCII.GetBytes("\x0a\x38\x0a\x0d\x0a\x0b");
            var b = Encoding.ASCII.GetBytes("*'\x0a\x18");
            var c = Encoding.ASCII.GetBytes("\x12\x0b");
            var d = Encoding.ASCII.GetBytes("\x10\x01\x18\x04");
            var channelCode = Encoding.UTF8.GetBytes(channelID);
            var liveCode = Encoding.UTF8.GetBytes(liveID);
            byte[] src = new byte[a.Length + b.Length + c.Length + d.Length + 2 * liveCode.Length + channelCode.Length];
            int pnt = 0;
            List<byte[]> list = new List<byte[]> { a, liveCode, b, channelCode, c, liveCode, d };
            foreach (var ba in list)
            {
                ba.CopyTo(src, pnt);
                pnt += ba.Length;
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(src).Trim('=') + "%3D")).Trim('=');
        }

        public bool InitializeFromDictionary(Dictionary<string, string> iniDict)
        {
            try
            {
                ChannelID = iniDict["ChannelID"];
                LiveID = iniDict["LiveID"];
            }
            catch
            {
                return false;
            }
            if (string.IsNullOrEmpty(ChannelID) || string.IsNullOrEmpty(LiveID))
                return false;
            return true;
        }
    }
}
