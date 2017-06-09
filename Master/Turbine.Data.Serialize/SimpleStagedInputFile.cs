using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.Serialize
{
    public class SimpleStagedInputFile
    {
        public string Name { get; set; }
        public System.Guid Id { get; set; }
        public string MD5Sum { get; set; }
    }
}
