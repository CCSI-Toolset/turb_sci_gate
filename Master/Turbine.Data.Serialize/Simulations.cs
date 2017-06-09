using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [CollectionDataContract]
    public class Simulations : List<Simulation>
    {
        public Simulations() { }
        public Simulations(IEnumerable<Simulation> source) : base(source) { }
    }
}