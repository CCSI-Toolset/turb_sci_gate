using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [CollectionDataContract]
    public class ApplicationList : List<Application>
    {
        public ApplicationList() { }
        public ApplicationList(IEnumerable<Application> source) : base(source) { }
    }
}