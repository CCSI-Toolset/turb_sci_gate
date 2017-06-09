using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurbineLiteConsumerIntegrationTest
{
    class TestProducerContext : Turbine.Producer.Contracts.IProducerContext
    {
        static internal string name = null;
        public string UserName
        {
            get
            {
                return name;
            }
        }
    }
}
