using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

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
    }
}
