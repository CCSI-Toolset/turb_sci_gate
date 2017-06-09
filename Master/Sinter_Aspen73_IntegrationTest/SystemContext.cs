using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinter_Aspen73_IntegrationTest
{
    class SystemContext : Turbine.Consumer.Contract.Behaviors.IContext
    {
        public string BaseWorkingDirectory
        {
            get { return System.IO.Directory.GetCurrentDirectory(); }
        }
    }
}
