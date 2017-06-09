using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Producer.Contracts
{
    public interface IProducerContext
    {
        string UserName { get; }
    }
}
