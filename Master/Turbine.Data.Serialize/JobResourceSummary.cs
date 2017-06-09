using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Turbine.Data.Serialize
{
    [DataContract]
    public class JobResourceSummary
    {
        [DataMember]
        public List<int> Id { get; set; }
    }
}
