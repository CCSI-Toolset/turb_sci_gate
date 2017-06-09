using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.AWS.Data.Contract.Messages
{
    public class JobStateChange : Turbine.AWS.Messages.Message
    {
        public Guid JobId { get; set; }
        public string State { get; set; }
        public string Message { get; set; }
    }
}