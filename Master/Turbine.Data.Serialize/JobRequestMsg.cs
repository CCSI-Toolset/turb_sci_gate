 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    //[CollectionDataContract]
    //public class Input : List<String>
    //{
    //    public Input() { }
    //    public Input(IEnumerable<String> source) : base(source) { }
    //}
    [DataContract]
    public class JobRequestMsg
    {
        [DataMember]
        public String Simulation { get; set; }

        [DataMember]
        public List<String> Inputs { get; set; }
    }
}
