using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.POCO.Domain
{
    public class SinterProcess
    {
        public virtual int Id { get; set; }
        public virtual Int32 status { get; set; }
        public virtual string stdout { get; set; }
        public virtual string stderr { get; set; }
        public virtual string workingdir { get; set; }
        public virtual string configuration { get; set; }
        public virtual string backup { get; set; }
        public virtual string input { get; set; }
        public virtual string output { get; set; }
        public virtual Job job { get; set; }
        public virtual List<ProcessError> errors { get; set; }
    }
}
