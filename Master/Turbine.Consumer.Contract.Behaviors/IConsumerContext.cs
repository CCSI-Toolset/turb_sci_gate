using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.Contract.Behaviors
{
    public interface IConsumerContext
    {
        /* Id: consumer identity 
         * 
         */
        Guid Id { get; }

        String Hostname { get; }

        String BindSimulationName { get; set; }
    }
}
