using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.AWS
{
    public interface IAWSContext
    {
        string Region { get; set; }
        string Bucket { get; set; }
        string RequestTopicArn { get; set; }
        string AccessKey { get; set; }
        string SecretKey { get; set; }
        string InstanceId { get; }
        string AmiId { get; }
        string ResponseQueuePrefix { get; set; }
        string SQSServiceURL { get; set; }
        string SNSServiceURL { get; set; }
    }
}
