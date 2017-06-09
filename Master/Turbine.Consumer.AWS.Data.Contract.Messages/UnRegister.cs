using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.AWS.Data.Contract.Messages
{
    public class UnRegister : Turbine.AWS.Messages.Message
    {
        public Guid ConsumerId { get; set; }
        public string ResponseQueueUrl { get; set; }
    }
}
