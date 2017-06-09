using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Turbine.Orchestrator.AWS.Data.Contract.Messages
{
    /// <summary>
    /// RegistrationInfo:  Contains 2 queues, 
    ///     ResponseQueueUrl : 2-way messsaging ( consuemr listens )
    ///     JobQueue:   SubmitJobMessage 
    /// 
    /// </summary>
    public class RegistrationInfo : Acknowledgment
    {
        public string ResponseQueueUrl { get; set; }
        public string JobQueueUrl { get; set; }
    }
}