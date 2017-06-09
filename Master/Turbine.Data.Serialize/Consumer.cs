using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [DataContract]
    public class Consumer
    {        
        [DataMember]
        public System.Guid Id { get; set; }

        [DataMember]
        public string hostname { get; set; }

        [DataMember]
        public int processID { get; set; }

        [DataMember]
        public string status { get; set; }

        [DataMember]
        public string keepalive { get; set; }

        [DataMember]
        public string Application_Name { get; set; }
    }
}
