using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.AWS.Data.Contract.Messages
{
    public class Register : Turbine.AWS.Messages.Message
    {
        public Guid ConsumerId { get; set; }
        public string InstanceID { get; set; }
        public string AMI { get; set; }
    }
}
