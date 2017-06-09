using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.SimpleNotificationService;
using Turbine.Consumer.AWS.Data.Contract.Messages;
using Newtonsoft.Json;
using System.Diagnostics;
using Turbine.Orchestrator.AWS.Data.Contract.Messages;
using Amazon.Auth.AccessControlPolicy;
using Newtonsoft.Json.Linq;
using Turbine.AWS.Messages;
using Amazon.SQS.Model;

namespace Turbine.Orchestrator.AWS.Data.Contract
{
    public class SimpleMessageListener
    {
        private AmazonSimpleNotificationService notification;
        private Amazon.SQS.AmazonSQS queue;
        private string topicArn;
        static private string requestQueueUrl;
        static private string requestQueue;
        private string subscriptionArn;
        private string submitQueue;
        private string submitQueueUrl;
        private Guid queueId;

        public string SubmitQueueUrl { get { return submitQueueUrl; }}

        public SimpleMessageListener()
        {
            IAWSOrchestratorContext awsCtx = Turbine.Orchestrator.AWS.AppUtility.GetContext();
            topicArn = awsCtx.RequestTopicArn;

            notification = Amazon.AWSClientFactory
                .CreateAmazonSNSClient(awsCtx.AccessKey, awsCtx.SecretKey,
                new AmazonSimpleNotificationServiceConfig().WithServiceURL(awsCtx.SNSServiceURL));
            queue = Amazon.AWSClientFactory
                .CreateAmazonSQSClient(
                awsCtx.AccessKey, awsCtx.SecretKey,
                new Amazon.SQS.AmazonSQSConfig().WithServiceURL(awsCtx.SQSServiceURL));
          
            requestQueue = awsCtx.RequestQueue;// REQUIRED
            submitQueue = awsCtx.SubmitQueue;
            queueId = Guid.NewGuid();
        }

        /// <summary>
        /// CreateQueueTopicPolicy:  Create Request Queue that subscribes to Topic,
        /// add permission/policy so topic can publish to the Queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private string CreateQueueTopicPolicy(string queueName)
        {
            Debug.WriteLine("Queuename: " + queueName);
            var msg1 = new Amazon.SQS.Model.CreateQueueRequest()
                .WithQueueName(queueName)
                .WithDefaultVisibilityTimeout(60)
                .WithDelaySeconds(20);
            
            Amazon.SQS.Model.CreateQueueResponse rsp = queue.CreateQueue(msg1);
            if (rsp.CreateQueueResult.IsSetQueueUrl())
            {
                var attr = queue.GetQueueAttributes(new Amazon.SQS.Model.GetQueueAttributesRequest()
                    .WithQueueUrl(rsp.CreateQueueResult.QueueUrl)
                    .WithAttributeName(new string[]{"QueueArn"}));

                if (attr.IsSetGetQueueAttributesResult() && attr.GetQueueAttributesResult.IsSetAttribute()) 
                {
                    var policy = new Policy("QueuePolicy"+queueName);
                    policy.WithStatements(new Statement(Statement.StatementEffect.Allow)
                        .WithPrincipals(new Principal("*"))
                        .WithActionIdentifiers(Amazon.Auth.AccessControlPolicy.ActionIdentifiers.SQSActionIdentifiers.SendMessage)
                        .WithResources(new Resource(attr.GetQueueAttributesResult.QueueARN))
                        .WithConditions(Amazon.Auth.AccessControlPolicy.ConditionFactory.NewSourceArnCondition(topicArn)));

                    var setAttrsRequest = new Amazon.SQS.Model.SetQueueAttributesRequest()
                       .WithQueueUrl(rsp.CreateQueueResult.QueueUrl);
                     setAttrsRequest.Attribute.Add(new Amazon.SQS.Model.Attribute()
                        .WithName("Policy")
                        .WithValue(policy.ToJson()));

                     var setAttrsResponse = queue.SetQueueAttributes(setAttrsRequest); 
                }

                return rsp.CreateQueueResult.QueueUrl;
            }
            return null;
        }

        /// <summary>
        /// CreateSubmitQueue:  IAM Account on consumer must have permission/policy to Receive and Delete
        ///    messages from this queue.
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private string CreateSubmitQueue(string queueName)
        {
            Debug.WriteLine("Queuename: " + queueName);
            var msg1 = new Amazon.SQS.Model.CreateQueueRequest()
                .WithQueueName(queueName)
                .WithDefaultVisibilityTimeout(60)
                .WithDelaySeconds(20);

            Amazon.SQS.Model.CreateQueueResponse rsp = queue.CreateQueue(msg1);
            if (rsp.CreateQueueResult.IsSetQueueUrl())
            {
                return rsp.CreateQueueResult.QueueUrl;
            }
            return null;
        }

        public void Setup()
        {
            if (requestQueueUrl != null)
            {
                Debug.WriteLine("no setup: RequestQueueUrl : " + requestQueueUrl);
                return;
            }
            IAWSOrchestratorContext awsCtx = Turbine.Orchestrator.AWS.AppUtility.GetContext();
            if (awsCtx.RequestQueue == null)
            {
                requestQueue = String.Format("orchestrator_request_queue_{0}", queueId);
                // create one & attach to topic
                requestQueueUrl = CreateQueueTopicPolicy(requestQueue);
                if (requestQueueUrl == null)
                    throw new Exception(String.Format("Failed to Create RequestQueue {0}", requestQueue));
            }
            else
            {
                var rsp = queue.GetQueueUrl(new Amazon.SQS.Model.GetQueueUrlRequest().WithQueueName(requestQueue));
                if (rsp.IsSetGetQueueUrlResult())
                {
                    if (rsp.GetQueueUrlResult.IsSetQueueUrl())
                    {
                        requestQueueUrl = rsp.GetQueueUrlResult.QueueUrl;
                    }
                }
                if (requestQueueUrl == null)
                    throw new Exception(String.Format("Failed to Get RequestQueue {0}", requestQueue));
            }

            if (awsCtx.SubmitQueue == null)
            {
                // Create Queue, Allow Consumers to pop messages
                submitQueue = "orchestrator_submit_queue_" + queueId;
                submitQueueUrl = CreateSubmitQueue(submitQueue);
                if (submitQueueUrl == null)
                    throw new Exception(String.Format("Failed to Create submitQueue {0}", submitQueue));
            }
            else
            {
                var rsp = queue.GetQueueUrl(new Amazon.SQS.Model.GetQueueUrlRequest().WithQueueName(submitQueue));
                if (rsp.IsSetGetQueueUrlResult())
                {
                    if (rsp.GetQueueUrlResult.IsSetQueueUrl())
                    {
                        submitQueueUrl = rsp.GetQueueUrlResult.QueueUrl;
                    }
                }
                if (submitQueueUrl == null)
                    throw new Exception(String.Format("Failed to Get submitQueue {0}", submitQueue));
            }

            var queueAttributesResponse = queue.GetQueueAttributes(
                new Amazon.SQS.Model.GetQueueAttributesRequest()
                .WithQueueUrl(requestQueueUrl)
                .WithAttributeName(new string[] {"QueueArn"}));

            if (!queueAttributesResponse.IsSetGetQueueAttributesResult())
                throw new Exception(String.Format("Failed to Get RequestQueue Arn {0}", requestQueue));

            if (!queueAttributesResponse.GetQueueAttributesResult.IsSetAttribute())
                throw new Exception(String.Format("Failed RequestQueue {0} QueueArn attribute is not set", requestQueue));

            var subResponse = notification.Subscribe(new Amazon.SimpleNotificationService.Model.SubscribeRequest()
                .WithTopicArn(topicArn)
                .WithProtocol("SQS")
                .WithEndpoint(queueAttributesResponse.GetQueueAttributesResult.QueueARN));

            if (!subResponse.IsSetSubscribeResult())
                throw new Exception(String.Format("Failed RequestQueue {0} QueueArn attribute is not set", requestQueue));
            if (!subResponse.SubscribeResult.IsSetSubscriptionArn())
                throw new Exception(String.Format("Failed RequestQueue {0} QueueArn attribute is not set", requestQueue));

            subscriptionArn = subResponse.SubscribeResult.SubscriptionArn;
        }

        public RPCEnvelope[] Listen()
        {
            Setup();

            var rsp = queue.ReceiveMessage(new Amazon.SQS.Model.ReceiveMessageRequest()
                .WithWaitTimeSeconds(20)
                .WithMaxNumberOfMessages(10)
                .WithQueueUrl(requestQueueUrl));
            if (!rsp.IsSetReceiveMessageResult())
            {
                Debug.WriteLine("No Messages Available");
                return null;
            }

            if (!rsp.ReceiveMessageResult.IsSetMessage())
            {
                Debug.WriteLine("Message not set");
                return null;
            }
            List<RPCEnvelope> envList = new List<RPCEnvelope>();
            RPCEnvelope env = null;
            var deleteList = new List<Amazon.SQS.Model.DeleteMessageBatchRequestEntry>{};

            foreach (var msg in rsp.ReceiveMessageResult.Message)
            {
                // NOTE: These are SNS Messages so need to parse that envelope off
                // Type,MessageId,TopicArn,Subject,Message,Timestamp,SignatureVersion,Signature,
                // SigningCertURL,UnsubscribeURL
                //
                Debug.WriteLine("Message: " + msg.Body);
                JToken token = JObject.Parse(msg.Body);
                JToken jmsg = token.SelectToken("Message");
                JToken subject = token.SelectToken("Subject");

                if (msg.IsSetReceiptHandle())
                    deleteList.Add(new Amazon.SQS.Model.DeleteMessageBatchRequestEntry()
                        .WithReceiptHandle(msg.ReceiptHandle)
                        .WithId(Guid.NewGuid().ToString()));

                Dictionary<string, Object> d;
                try
                {
                    //env = serializer.Deserialize<RPCEnvelope>(reader);
                    env = JsonConvert.DeserializeObject<RPCEnvelope>(jmsg.ToString());
                    d = JsonConvert.DeserializeObject<Dictionary<string, Object>>(jmsg.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("Ignore exception deserializing: {0}", ex), GetType().Name);
                    continue;
                }

                var responseQueueUrl = env.ResponseQueueUrl;
                var operation = env.Operation;
                var serializer = new Newtonsoft.Json.JsonSerializer();
                Newtonsoft.Json.JsonReader reader = jmsg.CreateReader();
                var content = d["Message"].ToString();

                if (operation == "Register")
                {
                    Debug.WriteLine("Found: " + operation);
                    //env.Message = serializer.Deserialize<Register>(reader);    
                    env.Message = JsonConvert.DeserializeObject<Register>(content);
                }
                else
                {
                    Debug.WriteLine("Ignore: " + operation);
                    continue;
                }

                envList.Add(env);
            }
            var entries = deleteList.ToArray<Amazon.SQS.Model.DeleteMessageBatchRequestEntry>();
            // Delete Messages
            var deleteResponse = queue.DeleteMessageBatch(new Amazon.SQS.Model.DeleteMessageBatchRequest()
                .WithQueueUrl(requestQueueUrl)
                .WithEntries(entries));

            return envList.ToArray<RPCEnvelope>();
        }

        public void SendRegistrationInfo(RPCEnvelope msg)
        {
            var env = new RPCEnvelope();
            var regInfo = new RegistrationInfo();
            env.Message = regInfo;
            env.Operation = regInfo.GetType().Name;
            
            regInfo.RequestId = msg.Message.Id;
            regInfo.Id = Guid.NewGuid();
            regInfo.Timestamp = DateTime.UtcNow;
            regInfo.JobQueueUrl = submitQueueUrl;
            env.Message = regInfo;
            Debug.WriteLine("Send RegistrationInfo to " + msg.ResponseQueueUrl);
            queue.SendMessage(new Amazon.SQS.Model.SendMessageRequest()
                .WithDelaySeconds(0)
                .WithQueueUrl(msg.ResponseQueueUrl)
                .WithMessageBody(JsonConvert.SerializeObject(env)));
        }
        /// <summary>
        /// PutOnQueue:  Sends in batch all jobs, returns list of all job.id confirmed
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public List<Guid> PutOnQueue(SubmitJobMessage[] jobs)
        {
            int i = 0;
            var entries = new List<SendMessageBatchRequestEntry>();
            var d = new Dictionary<String,SubmitJobMessage>();

            for (i = 0; i < jobs.Count(); i++)
            {
                var job = jobs[i];
                var entry = new SendMessageBatchRequestEntry()
                    .WithId(Guid.NewGuid().ToString())
                    .WithMessageBody(JsonConvert.SerializeObject(job));
                d.Add(entry.Id, job);
            }

            var l = new List<Guid>();
            if (entries.Count == 0)
                return l;

            var sendResponse = queue.SendMessageBatch(new SendMessageBatchRequest()
                .WithEntries(entries.ToArray<SendMessageBatchRequestEntry>())
                .WithQueueUrl(submitQueueUrl));

            if (sendResponse.IsSetSendMessageBatchResult() == false)
            {
                Debug.WriteLine("SendResponse missing result", GetType().Name + ".PutOnQueue");
                throw new Exception("SendResponse missing result");
            }
            if (sendResponse.SendMessageBatchResult.IsSetSendMessageBatchResultEntry())
            {
                foreach (var e in sendResponse.SendMessageBatchResult.BatchResultErrorEntry)
                {
                    var job = d[e.Id];
                    l.Add(job.Id);
                }
            }
            if (sendResponse.SendMessageBatchResult.IsSetBatchResultErrorEntry())
            {
                Debug.WriteLine("SendResponse received error entry", GetType().Name + ".PutOnQueue");
                foreach (var b in sendResponse.SendMessageBatchResult.BatchResultErrorEntry)
                {
                    var job = d[b.Id];
                    string s = String.Format("BatchResultErrorEntry (Job ID {0}): {1} {2} {3} {4}",
                        job.Id, b.Id, b.Code, b.SenderFault, b.Message);
                    Debug.WriteLine(s, GetType().Name + ".PutOnQueue");
                }
            }
            return l;
        }
    }
}
