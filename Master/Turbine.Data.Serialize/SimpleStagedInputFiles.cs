using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [CollectionDataContract]
    public class SimpleStagedInputFiles : List<SimpleStagedInputFile>
    {
        public SimpleStagedInputFiles() { }
        public SimpleStagedInputFiles(IEnumerable<SimpleStagedInputFile> source) : base(source) { }
    }
}
