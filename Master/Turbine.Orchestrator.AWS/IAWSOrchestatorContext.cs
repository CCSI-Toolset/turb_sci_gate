using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Orchestrator.AWS
{
    public interface IAWSOrchestratorContext
    {

        string Region { get; set; }
        string Bucket { get; set; }
        string RequestTopicArn { get; set; }
        string AccessKey { get; set; }
        string SecretKey { get; set; }

        string SQSServiceURL { get; set; }
        string SNSServiceURL { get; set; }

        string RequestQueue { get; set; }
        string SubmitQueue { get; set; }
    }
}
