using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;
using Amazon.EC2.Model;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Turbine.Data.Contract.Behaviors;
using System.IO;
using Turbine.Data;


namespace Turbine.Orchestrator.AWS
{
    public class EC2ScaleContract : Turbine.Data.Contract.Behaviors.IScaleContract
    {
        private Amazon.EC2.AmazonEC2 client;
        private Amazon.SQS.AmazonSQS queue;
        int reserved_floor = 0;

        public EC2ScaleContract()
        {
            IAWSContext ctx;
            ctx = AppUtility.GetContext();
            client = ctx.GetEC2Client();
            queue = ctx.GetQueueClient();
        }

        private Dictionary<string, Object> GetConfigOptions()
        {
            Amazon.S3.Model.GetObjectResponse rsp;
            IAWSContext ctx = AppUtility.GetContext();
            var name = AppUtility.GetContext().Bucket;
            var key = "config";
            string content;
            using (var client = ctx.GetS3Client())
            {
                var req = new Amazon.S3.Model.GetObjectRequest()
                    .WithBucketName(name).WithKey(key);
                try
                {
                    rsp = client.GetObject(req);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("Bucket {0} key {1}: {2}", name, key, ex.Message), GetType());
                    throw;
                }
                using (StreamReader r = new StreamReader(rsp.ResponseStream))
                {
                    content = r.ReadToEnd();
                }
            }
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Object>>(content);
            return dict;
        }


        /// <summary>
        /// GetRunningInstances clears the terminate queue and returns number of currently
        /// running instances.
        /// </summary>
        /// <returns></returns>
        public int GetRunningInstances()
        {
            ClearShutdownQueue();

            var ctx = AppUtility.GetContext();
            var req = new DescribeInstancesRequest();
            var filter = new Filter();
            filter.Name = "image-id";
            filter.Value = new System.Collections.Generic.List<String>() { ctx.AMI };
            req.Filter.Add(filter);

            filter = new Filter();
            filter.Name = "instance-state-name";
            filter.Value = new System.Collections.Generic.List<String>() { "running" };
            req.Filter.Add(filter);

            DescribeInstancesResponse rsp = client.DescribeInstances(req);
            var listRunningInstance = new List<string>();
            foreach (var i in rsp.DescribeInstancesResult.Reservation)
            {
                Debug.WriteLine("== Reservation");
                foreach (var j in i.RunningInstance)
                {
                    Debug.WriteLine(String.Format("{0}) {1} -- {2}", listRunningInstance.Count, j.InstanceId, j.LaunchTime));
                    listRunningInstance.Add(j.InstanceId);

                }
            }
            UpdateInstances(listRunningInstance);
            return listRunningInstance.Count;
        }

        private void UpdateInstances(List<string> instances)
        {
            Amazon.S3.Model.PutObjectResponse rsp;
            IAWSContext ctx = AppUtility.GetContext();
            var name = AppUtility.GetContext().Bucket;
            var key = "floor";
            var request = new Amazon.S3.Model.PutObjectRequest();
            request.WithBucketName(name);
            request.WithKey("instances");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(instances);
            request.WithContentBody(json);
            using (var client = ctx.GetS3Client())
            {
                rsp = client.PutObject(request);
            }
        }

        /// <summary>
        /// StartInstances utilizes EC2 interface to launch specified number of instances.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public int StartInstances(int count)
        {
            var configOptions = GetConfigOptions();
            var ctx = AppUtility.GetContext();
            Debug.WriteLine(String.Format("StartInstances: {0}", count), this.GetType());
            var runRequest = new RunInstancesRequest();
            runRequest.ImageId = ctx.AMI;
            runRequest.MinCount = count;
            runRequest.MaxCount = count;
            runRequest.InstanceType = ctx.InstanceType;
            Object instanceType;

            if (configOptions.TryGetValue("instance", out instanceType))
            {
                Debug.WriteLine("InstanceType Requested: " + instanceType, GetType());
                var instanceList = new List<string>(){
                    "t1.micro","m1.small","c1.medium"
                };
                if (!instanceList.Contains(instanceType))
                {
                    var msg = String.Format("Requested instanceType {0} not in {1}",
                        instanceType, string.Join(",", instanceList.ToArray()));
                    Debug.WriteLine(msg);
                }
                else
                {
                    runRequest.InstanceType = (string)instanceType;
                }
            }

            runRequest.SecurityGroup = new System.Collections.Generic.List<String>() { ctx.SecurityGroup };
            runRequest.WithInstanceInitiatedShutdownBehavior("terminate");
            if (!String.IsNullOrEmpty(ctx.AvailabilityZone))
            {
                runRequest.Placement = new Placement();
                runRequest.Placement.AvailabilityZone = ctx.AvailabilityZone;
            }
            RunInstancesResponse response = client.RunInstances(runRequest);

            return response.RunInstancesResult.Reservation.RunningInstance.Count();
        }

        /// <summary>
        /// TerminateInstances sends number of terminate messages to specified queue.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public int TerminateInstances(int num)
        {
            Debug.WriteLine(String.Format("TerminateInstnaces: {0}", num), this.GetType());
            int count = 0;
            for (int i = 1; i <= num; i++)
            {
                var msg = new Amazon.SQS.Model.SendMessageRequest();
                msg.WithQueueUrl(AppUtility.GetContext().ShutdownQueue);
                msg.WithMessageBody("shutdown");

                var rsp = queue.SendMessage(msg);
                if (rsp.IsSetSendMessageResult())
                {
                    if (rsp.SendMessageResult.IsSetMessageId())
                        count += 1;
                }
            }

            return count;
        }

        public int GetNotifications()
        {
            int count = 0;
            // Check Queue For Shutdown Message
            var request = new Amazon.SQS.Model.ReceiveMessageRequest();
            request.WithQueueUrl(AppUtility.GetContext().ShutdownNotificationQueue);

            var rsp = queue.ReceiveMessage(request);
            if (rsp.IsSetReceiveMessageResult())
            {
                foreach (var msg in rsp.ReceiveMessageResult.Message)
                {
                    if (msg.IsSetMessageId())
                    {
                        Debug.WriteLine("Notification Instance shutdown: " + msg.Body);
                        var delMsg = new Amazon.SQS.Model.DeleteMessageRequest();
                        delMsg.WithQueueUrl(AppUtility.GetContext().ShutdownQueue);
                        delMsg.WithReceiptHandle(msg.ReceiptHandle);
                        var delRsp = queue.DeleteMessage(delMsg);
                        count += 1;
                    }
                }
            }

            return count;
        }

        /*
         * Remove all messages from shutdown queue
         */
        private void ClearShutdownQueue()
        {
            var QueueURL = AppUtility.GetContext().ShutdownQueue;
            Amazon.SQS.Model.ReceiveMessageRequest request = new Amazon.SQS.Model.ReceiveMessageRequest();
            request.WithMaxNumberOfMessages(10);
            request.WithQueueUrl(QueueURL);
            var rsp = queue.ReceiveMessage(request);

            while (rsp.IsSetReceiveMessageResult() && rsp.ReceiveMessageResult.Message.Count > 0)
            {
                Debug.WriteLine("Remove Shutdown Messages: " + rsp.ReceiveMessageResult.Message.Count);
                var result = rsp.ReceiveMessageResult.Message;
                foreach (var msg in rsp.ReceiveMessageResult.Message)
                {
                    //Console.WriteLine("Message: " + msg);
                    if (msg.IsSetMessageId())
                    {
                        //Console.WriteLine("Body: " + msg.Body);
                        //if (msg.Body == "shutdown")
                        //{
                        //}
                        var delMsg = new Amazon.SQS.Model.DeleteMessageRequest();
                        delMsg.WithQueueUrl(QueueURL);
                        delMsg.WithReceiptHandle(msg.ReceiptHandle);
                        var delRsp = queue.DeleteMessage(delMsg);
                    }
                }
                rsp = queue.ReceiveMessage(request);
            }
        }

        public int GetReservedFloor()
        {
            int reserved_floor = 0;
            var bucketDict = GetConfigOptions();
            var key = "expire";
            DateTime stamp;
            Object obj;
            if (bucketDict.TryGetValue(key, out obj))
            {
                string content = (string)obj;
                if (!DateTime.TryParse(content, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
                        System.Globalization.DateTimeStyles.RoundtripKind, out stamp))
                {
                    throw new ArgumentException(
                        String.Format("Failed to parse {1}: {2}", key, content)
                    );
                }
                var now = DateTime.UtcNow;
                if (now >= stamp)
                {
                    throw new ArgumentException(
                        String.Format("Reservation Expired {0} >= {1}", now, stamp)
                    );
                }

                Debug.WriteLine("Retrieve Floor {0} < {1}", now, stamp); 

                key = "floor";
                if (bucketDict.TryGetValue(key, out obj))
                {
                    reserved_floor = Convert.ToInt32((Int64)obj);
                }
            }

            return reserved_floor;
        }

        public int GetQueueLength()
        {
            int length = 0;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                length = container.Jobs.Count<Job>(s => s.State == "submit");
            }
            return length;
        }
    }
}
