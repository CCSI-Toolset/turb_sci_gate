using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Orchestrator.AWS.Data.Contract.Messages
{
    /// <summary>
    /// Acknowledgment: echos back Id of Message
    /// 
    /// </summary>
    public class Acknowledgment : Turbine.AWS.Messages.Message
    {
        public Guid RequestId { get; set; }
    }
}