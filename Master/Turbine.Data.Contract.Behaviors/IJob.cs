using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.Contract.Behaviors
{
    public interface IJob
    {
        // utility 
        IProcess Process { get; }
        int Id { get; set; }
    }
}
