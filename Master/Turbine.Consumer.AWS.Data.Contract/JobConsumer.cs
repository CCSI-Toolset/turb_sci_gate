using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;
using Amazon.SimpleNotificationService;
using Turbine.Consumer.AWS.Data.Contract.Messages;
using Newtonsoft.Json;
using Amazon.S3;
using Amazon.S3.Model;
using System.Diagnostics;
using Turbine.Orchestrator.AWS.Data.Contract.Messages;


namespace Turbine.Consumer.AWS.Data.Contract
{
    public class PublishException : Exception
    {
        public PublishException(string s) : base(s)
        {
        }
    }

    class JobConsumer : IJobConsumerContract
    {
        private SubmitJobMessage job;
        //static private Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient notification;
        //private AmazonSimpleNotificationService notification;
        private Turbine.Data.Contract.Behaviors.IProcess process = null;
        private string _requestTopicArn = null;


        public JobConsumer(SubmitJobMessage job)
        {
            // TODO: Complete member initialization
            this.job = job;
        }

        private string RequestTopicArn
        {
            get
            {
                if (_requestTopicArn == null)
                {
                    var awsCtx = Turbine.Consumer.AWS.AppUtility.GetContext();
                    _requestTopicArn = awsCtx.RequestTopicArn;
                }
                return _requestTopicArn;
            }
        }
        public int Id
        {
            get
            {
                return job.Inc;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ApplicationName
        {
            get { throw new NotImplementedException(); }
        }

        public string SimulationName
        {
            get { return job.SimulationName; }
        }

        public Guid SimulationId
        {
            get { return job.SimulationId; }
        }

        public bool Reset
        {
            get { return job.Reset; }
        }

        public delegate Amazon.SQS.Model.DeleteMessageBatchResponse SetupDeleteMessageDelegate(Amazon.SQS.Model.DeleteMessageRequest msg);

        private Func<Amazon.SQS.Model.DeleteMessageRequest, Amazon.SQS.Model.DeleteMessageResponse> setupCallback = null;
        private Amazon.SQS.Model.DeleteMessageRequest setupCBRequest;
        private Action<Amazon.SQS.Model.DeleteMessageResponse> setupCallbackCheck;

        public void AddSetupCB(Func<Amazon.SQS.Model.DeleteMessageRequest, Amazon.SQS.Model.DeleteMessageResponse> callback, 
            Amazon.SQS.Model.DeleteMessageRequest request, Action<Amazon.SQS.Model.DeleteMessageResponse> check)
        {
            setupCallbackCheck = check;
            setupCallback = callback;
            setupCBRequest = request;
        }

        public IEnumerable<SimpleFile> GetSimulationInputFiles()
        {
            // S3:URL in description
            string bucketName = "Simulations";
            string key =job.SimulationId.ToString();
            //string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "name.bin");
            IAWSContext awsCtx = Turbine.Consumer.AWS.AppUtility.GetContext();
            byte[] bytes;
                
            using (AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(awsCtx.AccessKey, awsCtx.SecretKey))
            {
                ListObjectsRequest listObjectsRequest = new ListObjectsRequest()
                    .WithBucketName(bucketName)
                    .WithDelimiter("/")
                    .WithPrefix(String.Format("/{0}/StagedInputFiles/",key));

                using (ListObjectsResponse listObjectsResponse = client.ListObjects(listObjectsRequest))
                {
                    foreach (S3Object obj in listObjectsResponse.S3Objects)
                    {
                        GetObjectRequest getObjectRequest = new GetObjectRequest()
                            .WithBucketName(bucketName)
                            .WithKey(String.Format("/{0}/StagedInputFiles/{1}", key, obj.Key));
                        using (S3Response getObjectResponse = client.GetObject(getObjectRequest))
                        {
                            using (System.IO.Stream s = getObjectResponse.ResponseStream)
                            {
                                using (var ms = new System.IO.MemoryStream())
                                {
                                    s.CopyTo(ms);
                                    bytes = ms.ToArray();
                                }
                            }
                        }
                        var f = new SimpleFile() { content = bytes, name = obj.Key };
                        yield return f;
                    }
                }
            }
        }

        public bool Initialize()
        {
            return job.Initialize;
        }

        public void Message(string s)
        {
            // Update Job: Add Message
            Turbine.Consumer.Contract.Behaviors.IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            var msg = new JobAddMessage();
            msg.Id = Guid.NewGuid();
            msg.Message = s;
            msg.Timestamp = DateTime.UtcNow;
            msg.JobId = job.Id;

            SimpleMessageConnect.Send(msg);
        }

        public Turbine.Data.Contract.Behaviors.IProcess Process
        {
            get { return process; }
        }

        public Turbine.Data.Contract.Behaviors.IProcess Setup()
        {
            // Update Job: Add Message
            Turbine.Consumer.Contract.Behaviors.IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            JobStateChange msg = new JobStateChange();
            msg.JobId = job.Id;
            msg.State = "setup";
            SimpleMessageConnect.Send(msg);
            // NOTE: Purpose is to delete message from submission queue after it's been 'claimed'
            setupCallbackCheck(setupCallback(setupCBRequest));

            return new Process(job.Id);
        }

        public void Running()
        {
            // Update Job: Add Message
            Turbine.Consumer.Contract.Behaviors.IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            JobStateChange msg = new JobStateChange();
            msg.JobId = job.Id;
            msg.State = "running";
            SimpleMessageConnect.Send(msg);
        }

        public void Error(string e)
        {
            // Update Job: Add Message
            Turbine.Consumer.Contract.Behaviors.IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            JobStateChange msg = new JobStateChange();
            msg.JobId = job.Id;
            msg.State = "error";
            msg.Message = e;
            SimpleMessageConnect.Send(msg);
        }

        public void Success()
        {
            // Update Job: Add Message
            Turbine.Consumer.Contract.Behaviors.IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            JobStateChange msg = new JobStateChange();
            msg.JobId = job.Id;
            msg.State = "success";
            SimpleMessageConnect.Send(msg);
        }

        public void Warning(string p)
        {
            // Update Job: Add Message
            Turbine.Consumer.Contract.Behaviors.IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            JobStateChange msg = new JobStateChange();
            msg.JobId = job.Id;
            msg.State = "warning";
            msg.Message = p;
            SimpleMessageConnect.Send(msg);
        }
    }
}
