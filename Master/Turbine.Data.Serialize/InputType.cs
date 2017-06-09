using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [DataContract]
    public class InputType
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public bool Required { get; set; }
    }
}