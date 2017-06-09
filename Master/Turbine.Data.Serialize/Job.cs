using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Turbine.Data.Serialize
{
    [DataContract]
    public class Job
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string State { get; set; }

        [DataMember]
        public string[] Messages { get; set; }

        [DataMember]
        public string Simulation { get; set; }

        [DataMember]
        public List<string> Input { get; set; }

        [DataMember]
        public string Output { get; set; }

        [DataMember]
        public string Create { get; set; }

        [DataMember]
        public string Submit { get; set; }

        [DataMember]
        public string Setup { get; set; }

        [DataMember]
        public string Running { get; set; }

        [DataMember]
        public string Finished { get; set; }

        [DataMember]
        public bool Initialize { get; set; }

        // Aspen Process Information
        [DataMember]
        public ProcessErrorList Errors { get; set; }

        [DataMember]
        public int Status { get; set; }

        [DataMember]
        public System.Guid Session { get; set; }

        [DataMember]
        public System.Guid Consumer { get; set; }
    }
}
