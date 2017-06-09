using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.POCO.Domain
{
    public class ProcessError
    {
        public virtual int Id { get; set; }
        public virtual string type { get; set; }
        public virtual string name { get; set; }
        public virtual string msg { get; set; }
        public virtual SinterProcess process { get; set; }
    }
}
