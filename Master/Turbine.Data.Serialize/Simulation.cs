using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [DataContract]
    public class Simulation
    {
        [DataMember]
        public System.Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public SimpleStagedInputFiles StagedInputs { get; set; }

        [DataMember]
        public string Application { get; set; }
    }
}