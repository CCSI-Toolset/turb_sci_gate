using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [CollectionDataContract]
    public class Jobs : List<Job>
    {
        public Jobs() { }
        public Jobs(IEnumerable<Job> source) : base(source) { }
    }
}
