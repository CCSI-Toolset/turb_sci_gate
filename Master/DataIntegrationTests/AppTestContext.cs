using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Producer.Contracts;


namespace Turbine.Data.Test
{
    class AppTestContext : IContext
    {
        public string BaseWorkingDirectory
        {
            get { return @"\turbine\test\AppTestContext"; }
        }
    }

    class FakeHTTPContext : IProducerContext
    {
        public string UserName
        {
            get { return "test1"; }
        }
    }
}
