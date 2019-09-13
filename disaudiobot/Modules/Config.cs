using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Discord;

namespace disaudiobot.Modules
{
    [DataContract]
    class Config
    {
        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public char Prefix { get; set; }

        [DataMember]
        public int StarsCount { get; set; }       

        [DataMember]
        public int GetPlaylistCount { get; set; }

        
        [DataMember]
        public uint ColorValue { get; set; }


        [IgnoreDataMember]
        public Color Color;

    }
}
