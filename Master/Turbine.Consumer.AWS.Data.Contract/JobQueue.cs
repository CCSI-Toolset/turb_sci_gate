using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.Consumer.Data.Contract.Behaviors;
using System.Diagnostics;
using Newtonsoft.Json;
using Turbine.Consumer.AWS.Data.Contract.Messages;
using Turbine.Orchestrator.AWS.Data.Contract.Messages;

namespace Turbine.Consumer.AWS.Data.Contract
{
    public class JobQueue : IJobQueue
    {
        private string jobQueueUrl;
        private Amazon.SQS.AmazonSQS _queue;
        private Amazon.SQS.AmazonSQS Queue 
        {
            get
            {
                if (_queue == null)
                {
                    Turbine.Consumer.Contract.Behaviors.IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
                    IAWSContext awsCtx = Turbine.Consumer.AWS.AppUtility.GetContext();
                    _queue = Amazon.AWSClientFactory.CreateAmazonSQSClient(
                        awsCtx.AccessKey, awsCtx.SecretKey,
                        new Amazon.SQS.AmazonSQSConfig().WithServiceURL(awsCtx.SQSServiceURL)
                    );
                }
                return _queue;
            }
        }

        public JobQueue(RegistrationInfo reg)
        {
            this.jobQueueUrl = reg.JobQueueUrl;
        }

        /// <summary>
        /// GetNext:  If no job available on queue return null
        /// </summary>
        /// <returns></returns>
        public IJobConsumerContract GetNext()
        {
            Amazon.SQS.Model.ReceiveMessageResponse receiveMsgResponse;
            Debug.WriteLine(String.Format("Register: Poll Queue {0} for response", jobQueueUrl), GetType().Name);
            receiveMsgResponse = Queue.ReceiveMessage(new Amazon.SQS.Model.ReceiveMessageRequest()
                    .WithMaxNumberOfMessages(1)
                    .WithWaitTimeSeconds(5)
                    .WithQueueUrl(jobQueueUrl));

            if (receiveMsgResponse.IsSetReceiveMessageResult() == false || receiveMsgResponse.ReceiveMessageResult.IsSetMessage() == false)
            {
                return null;
            }

            // Response Message Should Have ResponseQueue & SNS Topic
            List<Amazon.SQS.Model.Message> msgResultMsg = receiveMsgResponse.ReceiveMessageResult.Message;
            if (msgResultMsg.Count != 1)
            {
                var exStr = String.Format("Protocol Error: Response should have 1 Message found: {0}", msgResultMsg.Count);
                Debug.WriteLine(exStr, GetType().Name);
                throw new Exception(exStr);
            }

            Amazon.SQS.Model.Message msg = msgResultMsg[0];

            Turbine.Orchestrator.AWS.Data.Contract.Messages.SubmitJobMessage rsp = null;

            try
            {
                rsp = JsonConvert.DeserializeObject<Turbine.Orchestrator.AWS.Data.Contract.Messages.SubmitJobMessage>(msg.Body);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception deserializing SubmitJobMessage: {0}", ex), GetType().Name);
                throw;
            }

            //Amazon.SQS.Model.DeleteMessageResponse delMsgRsp = Queue.DeleteMessage(

            var jobConsumer = new JobConsumer(rsp);
            jobConsumer.AddSetupCB(Queue.DeleteMessage, new Amazon.SQS.Model.DeleteMessageRequest()
                .WithReceiptHandle(msg.ReceiptHandle)
                .WithQueueUrl(jobQueueUrl),
                CheckDeleteMessageResponse);

            return jobConsumer;
        }

        static void CheckDeleteMessageResponse(Amazon.SQS.Model.DeleteMessageResponse delMsgRsp)
        {
            if (delMsgRsp == null)
                throw new Exception("Failed to Delete Message");
        }
    }
}
