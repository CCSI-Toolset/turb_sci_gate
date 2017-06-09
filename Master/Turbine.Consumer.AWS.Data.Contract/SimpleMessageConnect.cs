using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.SimpleNotificationService;
using Turbine.Consumer.AWS.Data.Contract.Messages;
using Newtonsoft.Json;
using System.Diagnostics;
using Turbine.Orchestrator.AWS.Data.Contract.Messages;
using Newtonsoft.Json.Linq;
using Turbine.AWS.Messages;

namespace Turbine.Consumer.AWS.Data.Contract
{
    public static class SimpleMessageConnect
    {
        static private AmazonSimpleNotificationService notification;
        static private Amazon.SQS.AmazonSQS queue;
        static private string topicArn;
        static private string responseQueuePrefix;

        static SimpleMessageConnect()
        {
            IAWSContext awsCtx = Turbine.Consumer.AWS.AppUtility.GetContext();
            topicArn = awsCtx.RequestTopicArn;
            notification = Amazon.AWSClientFactory
                .CreateAmazonSNSClient(awsCtx.AccessKey, awsCtx.SecretKey,
                new AmazonSimpleNotificationServiceConfig().WithServiceURL(awsCtx.SNSServiceURL));
            Debug.WriteLine("topicArn: " + topicArn);
            Debug.WriteLine("SQSServiceURL: " + awsCtx.SQSServiceURL);
            Debug.WriteLine("SNSServiceURL: " + awsCtx.SNSServiceURL);
            queue = Amazon.AWSClientFactory
                .CreateAmazonSQSClient(
                awsCtx.AccessKey, awsCtx.SecretKey,
                new Amazon.SQS.AmazonSQSConfig().WithServiceURL(awsCtx.SQSServiceURL));
            responseQueuePrefix = awsCtx.ResponseQueuePrefix;
        }


        public static void Close()
        {
            if (responseQueueUrl == null) return;
            Debug.WriteLine("Delete consumer SQS", "SimpleMessageConnect.Close");
            var msg = new Amazon.SQS.Model.DeleteQueueRequest()
                .WithQueueUrl(responseQueueUrl);

            Amazon.SQS.Model.DeleteQueueResponse rsp = queue.DeleteQueue(msg);
        }

        private static string responseQueueUrl = null;
        /// <summary>
        /// Open: Creates Queue first time called.  Credentials must have permission to create an SQS queue.
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private static string Open(string queueName)
        {
            if (responseQueueUrl != null) return responseQueueUrl;
            Debug.WriteLine("Create consumer SQS: " + queueName, "SimpleMessageConnect.Open");
            var msg1 = new Amazon.SQS.Model.CreateQueueRequest()
                .WithQueueName(queueName)
                .WithDefaultVisibilityTimeout(600)
                .WithDelaySeconds(20);

            Amazon.SQS.Model.CreateQueueResponse rsp = queue.CreateQueue(msg1);
            if (rsp.CreateQueueResult.IsSetQueueUrl())
            {
                responseQueueUrl = rsp.CreateQueueResult.QueueUrl;
            }
            return responseQueueUrl;
        }


        private static void Send(string subject, string msg)
        {
            Debug.WriteLine("Publish Orchestrator Topic: " + subject, "SimpleMessageConnect.Send");
            var pubReq = new Amazon.SimpleNotificationService.Model.PublishRequest()
                .WithMessage(msg)
                .WithSubject(subject)
                .WithTopicArn(topicArn);
            Debug.WriteLine("TOPICARN: " + topicArn);
            var pubRsp = notification.Publish(pubReq);
            // CHECK PUBLISH RESULT
            if (!pubRsp.IsSetPublishResult())
            {
                var s = String.Format("Failed to publish {0}, Message:{1} ", subject, msg);
                Debug.Fail(s);
                throw new PublishException(s);
            }
        }


        internal static void Send(JobStateChange msg)
        {
            var env = new RPCEnvelope();
            env.Message = msg;
            env.Operation = "JobStateChange";
            env.ResponseQueueUrl = null;
            Send(msg.GetType().Name, JsonConvert.SerializeObject(env));
        }

        static public void Send(JobAddMessage msg)
        {
            var env = new RPCEnvelope();
            env.Message = msg;
            env.Operation = "JobAddMessage";
            env.ResponseQueueUrl = null;
            Send(msg.GetType().Name, JsonConvert.SerializeObject(env));
        }


        static public void Send(UnRegister msg)
        {
            var env = new RPCEnvelope();
            env.Message = msg;
            env.Operation = "UnRegister";
            env.ResponseQueueUrl = null;
            Send(msg.GetType().Name, JsonConvert.SerializeObject(env));
        }

        static public RegistrationInfo SendReceive(Register msg)
        {
            Debug.WriteLine("Send Register Message", "SimpleMessageConnect.SendReceive");
            var env = new RPCEnvelope();
            env.Message = msg;
            env.Operation = "Register";
            env.ResponseQueueUrl = Open(String.Format("{0}{1}",responseQueuePrefix, msg.ConsumerId));
            Send(msg.GetType().Name, JsonConvert.SerializeObject(env));
            var rsp = ReceiveRegistration(env);
            return rsp;
        }

        /// <summary>
        /// ReceiveRegistration:  Response occurs on a dedicated queue so all messages not 
        /// conforming to expected result are deleted and ignored.
        /// </summary>
        /// <param name="registerRequest"></param>
        /// <returns></returns>
        internal static RegistrationInfo ReceiveRegistration(RPCEnvelope registerRequest)
        {
            Turbine.Consumer.Contract.Behaviors.IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            IAWSContext awsCtx = Turbine.Consumer.AWS.AppUtility.GetContext();

            var queue = Amazon.AWSClientFactory.CreateAmazonSQSClient(
                awsCtx.AccessKey, awsCtx.SecretKey,
                new Amazon.SQS.AmazonSQSConfig().WithServiceURL(awsCtx.Region)
                );

            Amazon.SQS.Model.ReceiveMessageResponse receiveMsgResponse;
            var entries = new List<Amazon.SQS.Model.DeleteMessageBatchRequestEntry>();

            // NOTE: Should only be 1 message on queue, but grab 'all' and delete 'all'.
            do
            {
                Debug.WriteLine(String.Format("Register: Poll Queue {0} for response", registerRequest.ResponseQueueUrl),
                    registerRequest.Operation);
                receiveMsgResponse = queue.ReceiveMessage(new Amazon.SQS.Model.ReceiveMessageRequest()
                    .WithWaitTimeSeconds(20)
                    .WithMaxNumberOfMessages(10)
                    .WithQueueUrl(registerRequest.ResponseQueueUrl));

            } while (receiveMsgResponse.IsSetReceiveMessageResult() == false || receiveMsgResponse.ReceiveMessageResult.IsSetMessage() == false);

            // Response Message Should Have ResponseQueue & SNS Topic
            List<Amazon.SQS.Model.Message> msgResultMsg = receiveMsgResponse.ReceiveMessageResult.Message;
            Debug.WriteLine(String.Format("Count {0} Messages found",msgResultMsg.Count), registerRequest.Operation);

            if (msgResultMsg.Count == 0)
            {
                return null;
            }

            RegistrationInfo regInfo = null;
            //Dictionary<string, Object> d;

            // NOTE: Very inefficient, double parsing etc.  
            // Consider typing at the RPCEnvelope level
            foreach (Amazon.SQS.Model.Message smsg in msgResultMsg)
            {
                Debug.WriteLine(String.Format("Messages found {0}", smsg.Body), registerRequest.Operation);
                entries.Add(new Amazon.SQS.Model.DeleteMessageBatchRequestEntry()
                    .WithId(smsg.MessageId)
                    .WithReceiptHandle(smsg.ReceiptHandle));
                RPCEnvelope registerResponse = null;
                JToken token = JObject.Parse(smsg.Body);
                JToken jmsg = token.SelectToken("Message");

                try
                {
                    registerResponse = JsonConvert.DeserializeObject<RPCEnvelope>(smsg.Body);
                    //d = JsonConvert.DeserializeObject<Dictionary<string, Object>>(jmsg.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("RPCEnvelope ignore exception deserializing: {0}", ex), 
                        registerRequest.Operation);
                    continue;
                }

                //ack = (Turbine.Orchestrator.AWS.Data.Contract.Messages.RegistrationInfo)rsp.Message;
                var serializer = new Newtonsoft.Json.JsonSerializer();
                Newtonsoft.Json.JsonReader reader = jmsg.CreateReader();
                //var content = d["Message"].ToString();
                var content = jmsg.ToString();
                try
                {
                    regInfo = JsonConvert.DeserializeObject<RegistrationInfo>(content);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("RegistrationInfo ignore exception deserializing: {0}", ex),
                        registerRequest.Operation);
                    continue;
                }

                //registerRequest.Message = regInfo;
                if (regInfo.RequestId != registerRequest.Message.Id)
                {
                    var s = String.Format("Mismatch Request-Response Id : {0} != {1}", regInfo.Id, registerRequest.Message.Id);
                    Debug.WriteLine(s, regInfo.GetType().Name);
                    continue;
                }
                break;
            }

            if (entries.Count > 0)
            {
                // Delete all messages found
                var delMsgBatchRsp = queue.DeleteMessageBatch(
                    new Amazon.SQS.Model.DeleteMessageBatchRequest()
                    .WithQueueUrl(registerRequest.ResponseQueueUrl)
                    .WithEntries(entries.ToArray<Amazon.SQS.Model.DeleteMessageBatchRequestEntry>()));
            }
            return regInfo;
        }

    }
}
