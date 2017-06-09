using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Producer.AWS.Contracts
{
    public interface IAWSContext
    {
        Amazon.SQS.AmazonSQS GetQueueClient();
        Amazon.EC2.AmazonEC2 GetEC2Client();
        Amazon.S3.AmazonS3Client GetS3Client();

        string AvailabilityZone { get; set; }
        string Region { get; set; }
        string SubmitQueue { get; set; }
        string Bucket { get; set; }
        string NotificationQueue { get; set; }
        string AWSAccessKey { get; set; }
        string AWSSecretKey { get; set; }
    }
}
