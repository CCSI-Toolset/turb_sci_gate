using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.Contract.Behaviors
{
    public interface IContext
    {
        string BaseWorkingDirectory { get; }
    }
}
