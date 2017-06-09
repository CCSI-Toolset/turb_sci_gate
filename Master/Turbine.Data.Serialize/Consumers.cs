using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [CollectionDataContract]
    public class Consumers : List<Consumer>
    {
        public Consumers() { }
        public Consumers(IEnumerable<Consumer> source) : base(source) { }
    }
}
