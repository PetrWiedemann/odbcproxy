using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace net.pdynet.odbcproxy
{
    [DataContract(Namespace = "")]
    public class StatusResponse
    {
        [DataMember]
        public DateTime Now { get; set; }

        [DataMember]
        public int ActiveConnections { get; set; }
    }
}
