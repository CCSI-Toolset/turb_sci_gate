using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.AWS;
using Turbine.Consumer.AWS.Data.Contract.Messages;
using Newtonsoft.Json;
using Turbine.Orchestrator.AWS.Data.Contract.Messages;



namespace Turbine.Consumer.AWS.Data.Contract
{
    public class NotificationException : Exception
    {
        public NotificationException(string msg) :
            base(msg)
        {
        }
    }

    public class ConsumerRegistrationContract : IConsumerRegistrationContract
    {
        private JobQueue jobQueue;
        private RegistrationInfo regInfo = null;

        //static private Amazon.SQS.AmazonSQS queue;
        //static private string requestQueueURL = "";
        //static private string responseQueueURL = "";
        //static private string snsTopic = "";
        //static private IAWSContext context;

        /*
        static ConsumerRegistrationContract()
        {
            IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            IAWSContext awsCtx = Turbine.Consumer.AWS.AppUtility.GetContext();
            // many (consumer) -to-1 (orchestrator) Request Queue
            string requestTopicArn = awsCtx.RequestTopicArn;
            // 1 (orchestrator)-to-1 (consumer) Response Queue
            string responseQueueName = String.Format("consumer_notification_{0}", ctx.Id);

            //notification = new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient();

            queue = Amazon.AWSClientFactory.CreateAmazonSQSClient(
                context.AccessKey, context.SecretKey,
                new Amazon.SQS.AmazonSQSConfig().WithServiceURL(context.Region)
                );

            // 1-to-many Request Queue
            var msg = new Amazon.SQS.Model.GetQueueUrlRequest().WithQueueName(requestQueueName);
            try
            {
                requestQueueURL = queue.GetQueueUrl(msg).GetQueueUrlResult.QueueUrl;
            }
            catch (Exception ex)
            {
                var s = ex.StackTrace;
                Debug.Fail(String.Format("Failed to Find RequestQueue {0}", requestQueueName), 
                    String.Format("{0} -- {1}", ex.Message, ex.StackTrace));
                throw;
            }
            // 1-to-1 Response Queue ( long poll )
            var msg1 = new Amazon.SQS.Model.CreateQueueRequest()
                .WithQueueName(responseQueueName).WithDefaultVisibilityTimeout(600).WithDelaySeconds(20);
            responseQueueURL = queue.CreateQueue(msg1).CreateQueueResult.QueueUrl;
        }

         */
        public IJobQueue Queue
        {
            get { return jobQueue; }
        }

        /// <summary>
        /// Register the consumer by sending a formatted request to a SQS.  This request contains a 
        /// URL to the queue this method will poll for a response.  The response will be checked
        /// via a GUID provided in the request and returned in the reply.  The response will also
        /// contain a SNS Topic that will be used to send updates to.  
        /// 
        /// If no reply on long poll return null.
        /// </summary>
        public IJobQueue Register()
        {
            IAWSContext awsCtx = Turbine.Consumer.AWS.AppUtility.GetContext();
            var registerMsg = new Register();
            registerMsg.Id = Guid.NewGuid();
            registerMsg.InstanceID = awsCtx.InstanceId;
            registerMsg.AMI = awsCtx.AmiId;
            regInfo = SimpleMessageConnect.SendReceive(registerMsg);
            this.jobQueue = new JobQueue(regInfo);
            return jobQueue;
        }

        public void UnRegister()
        {
            IAWSContext awsCtx = Turbine.Consumer.AWS.AppUtility.GetContext();
            var unreg = new UnRegister();
            unreg.Id = Guid.NewGuid();
            unreg.ConsumerId = this.regInfo.Id;
            this.jobQueue = null;
            SimpleMessageConnect.Send(unreg);
        }

        public void Error()
        {
            throw new NotImplementedException();
        }
    }
}
