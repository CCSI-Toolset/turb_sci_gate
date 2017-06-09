using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.EC2;

namespace Turbine.Producer.AWS
{
    class AWSContext : Turbine.Producer.AWS.Contracts.IAWSContext
    {
        private string region;
        private string accessKey;
        private string secretKey;
        private string submitQueue;
        private string notificationQueue;
        private string bucket;
        private string ami;
        private string zone;
        private string instanceType;
        private string securityGroup;

        public AWSContext()
        {
        }

        public Amazon.SQS.AmazonSQS GetQueueClient()
        {
            return Amazon.AWSClientFactory.CreateAmazonSQSClient(
                accessKey,
                secretKey,
                new Amazon.SQS.AmazonSQSConfig().WithServiceURL(region)
                );
        }
        public AmazonEC2 GetEC2Client()
        {
            return Amazon.AWSClientFactory.CreateAmazonEC2Client(
                accessKey,
                secretKey,
                new Amazon.EC2.AmazonEC2Config().WithServiceURL(region)
                );
        }

        public Amazon.S3.AmazonS3Client GetS3Client()
        {
            return new Amazon.S3.AmazonS3Client(accessKey, secretKey);
        }

        public string AMI
        {
            get{ return ami; }
            set{ ami = value; }
        }

        public string SecurityGroup
        {
            get { return securityGroup; }
            set { securityGroup = value; }
        }

        public string AvailabilityZone
        {
            get { return zone; }
            set { zone = value; }
        }

        public string InstanceType
        {
            get { return instanceType; }
            set { instanceType = value; }
        }

        public string Region
        {
            get { return region; }
            set { region = value; }
        }


        public string SubmitQueue
        {
            get { return submitQueue; }
            set { submitQueue = value; }
        }

        public string NotificationQueue
        {
            get { return notificationQueue; }
            set { notificationQueue = value; }
        }


        public string AWSAccessKey
        {
            get { return accessKey; }
            set { accessKey = value; }
        }

        public string AWSSecretKey
        {
            get { return secretKey; }
            set { secretKey = value; }
        }


        public string Bucket
        {
            get { return bucket; }
            set { bucket = value; }
        }
    }

}
