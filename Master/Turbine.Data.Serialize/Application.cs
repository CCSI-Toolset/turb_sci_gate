using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [DataContract]
    public class Application
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public InputTypeList Inputs { get; set; }
    }
}