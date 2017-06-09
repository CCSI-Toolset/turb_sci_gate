using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Producer.AWS
{
    public interface IAWSProducerContext
    {

        string Region { get; set; }
        string Bucket { get; set; }
        string RequestTopicArn { get; set; }
        string AccessKey { get; set; }
        string SecretKey { get; set; }

        string SQSServiceURL { get; set; }
        string SNSServiceURL { get; set; }

    }
}
