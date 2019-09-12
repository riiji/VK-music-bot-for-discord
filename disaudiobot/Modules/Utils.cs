using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disaudiobot.Modules
{
    class Utils
    {
        public static Config _cfg;
        /// <summary>
        /// Fixing an URL if its corrupted
        /// </summary>
        /// <param name="url">Corrupted URL</param>
        /// <returns></returns>
        public static Uri FixUrl(Uri url)
        {
            string uri = url.AbsoluteUri;

            uri = uri.Replace("/index.m3u8", ".mp3");

            int fi = 0;
            int li = 0;

            int count = 0;

            for (int i = 0; i < uri.Length; ++i)
            {
                if (uri[i] == '/')
                    ++count;
                if (count == 4 && fi == 0)
                    fi = i;
                if (count == 5)
                {
                    li = i;
                    break;
                }
            }

            uri = uri.Remove(fi, li - fi);
            return new Uri(uri);
        }


    }
}
