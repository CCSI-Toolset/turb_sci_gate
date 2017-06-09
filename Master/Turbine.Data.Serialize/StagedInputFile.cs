using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.Serialize
{
    public class StagedInputFile
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public string Hash { get; set; }
        public string InputFileType { get; set; }
    }
}
