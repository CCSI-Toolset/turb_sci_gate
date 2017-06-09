using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    public class ProcessError
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public string Type { get; set; }
    }

    [CollectionDataContract]
    public class ProcessErrorList : List<ProcessError>
    {
        public ProcessErrorList() { }
        public ProcessErrorList(IEnumerable<ProcessError> source) : base(source) { }
    }
}
