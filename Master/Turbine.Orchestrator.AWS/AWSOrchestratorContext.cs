using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Orchestrator.AWS
{
    class AWSOrchestratorContext : IAWSOrchestratorContext
    {
        private string region;
        private string accessKey;
        private string secretKey;
        private string requestTopicArn;
        private string bucket;
        private string responseQueuePrefix = "";
        private string sqsServiceURL;
        private string snsServiceURL;
        private string requestQueue;
        private string submitQueue;

        public string AccessKey
        {
            get { return accessKey; }
            set { accessKey = value; }
        }

        public string SecretKey
        {
            get { return secretKey; }
            set { secretKey = value; }
        }

        public string Bucket
        {
            get { return bucket; }
            set { bucket = value; }
        }

        public string ResponseQueuePrefix
        {
            get { return responseQueuePrefix; }
            set { responseQueuePrefix = value; }
        }

        public string SQSServiceURL
        {
            get { return sqsServiceURL; }
            set { sqsServiceURL = value; }
        }

        public string SNSServiceURL
        {
            get { return snsServiceURL; }
            set { snsServiceURL = value; }
        }
        public string Region
        {
            get { return region; }
            set { region = value; }
        }

        public string RequestTopicArn
        {
            get { return requestTopicArn; }
            set { requestTopicArn = value; }
        }


        public string RequestQueue
        {
            get { return requestQueue; }
            set { requestQueue = value; }
        }

        public string SubmitQueue
        {
            get { return submitQueue; }
            set { submitQueue = value; }
        }
    }
}
