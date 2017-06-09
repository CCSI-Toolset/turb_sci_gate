using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace Turbine.Consumer.AWS
{
    class AWSConsumerContext : IAWSContext
    {
        private string region;
        private string accessKey;
        private string secretKey;
        private string requestTopicArn;
        private string bucket;
        private string responseQueuePrefix = "";
        private static string instanceID = null;
        private static string amiID = null;
        private static string META_DATA_URL = "http://169.254.169.254/latest/meta-data/";
        private string sqsServiceURL;
        private string snsServiceURL;
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

        public String InstanceId
        {
            get
            {
                if (instanceID != null) return instanceID;
                instanceID = GetMetaData("instance-id");
                return instanceID;
            }
            set
            {
                // for testing
                instanceID = value;
            }
        }

        public String AmiId
        {
            get
            {
                if (amiID != null) return amiID;
                return GetMetaData("ami-id");
            }
            set
            {
                // for testing
                amiID = value;
            }
        }

        private String GetMetaData(string key)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                System.IO.Path.Combine(META_DATA_URL, key));
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (System.Net.WebException ex)
            {
                Debug.WriteLine(String.Format("WebException Failed to GET {0}: {1}", key, ex.Message), GetType());
                throw;
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Debug.WriteLine(String.Format("GetMetaData Failed to GET {0}: HTTP Code {1}", key, response.StatusCode), GetType());
                return null;
            }
            using (System.IO.Stream dataStream = response.GetResponseStream())
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
                return reader.ReadToEnd();
            }
        }
    }
}
