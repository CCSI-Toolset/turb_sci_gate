using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Producer.Data.Contract.Behaviors
{
    public interface IApplicationProducerContract
    {
        bool UpdateInputFileType(string name, bool required, string type);
    }
}
