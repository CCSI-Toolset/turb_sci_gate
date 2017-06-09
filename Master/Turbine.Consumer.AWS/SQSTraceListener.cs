using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace Turbine.Consumer.AWS
{
    class SQSTraceListener : System.Diagnostics.TraceListener
    {
        private string queueURL = null;
        private static Amazon.SQS.AmazonSQS _queue = null;
        static IUnityContainer container = new UnityContainer();
        static IAWSContext context;

        static SQSTraceListener()
        {
            container.LoadConfiguration("aws");
            context = container.Resolve<IAWSContext>();
        }

        public SQSTraceListener(String queueURL)
        {
            this.queueURL = queueURL;
        }

        static Amazon.SQS.AmazonSQS Queue 
        {
            get
            {
                if (_queue != null) return _queue;
                _queue = AWSClientFactory.CreateAmazonSQSClient();
                /*
                if ((context.AccessKey == null && context.SecretKey == null) || 
                    (context.AccessKey.Equals("") && context.SecretKey.Equals("")))
                {
                    if (context.Region == null || context.Region.Equals(""))
                    {
                        _queue = AWSClientFactory.CreateAmazonSQSClient();
                    }
                    else
                    {
                        _queue = AWSClientFactory.CreateAmazonSQSClient(RegionEndpoint.GetBySystemName(context.Region));
                    }
                    
                }
                else
                {
                    _queue = AWSClientFactory.CreateAmazonSQSClient(
                        context.AccessKey,
                        context.SecretKey,
                        new Amazon.SQS.AmazonSQSConfig().WithServiceURL(context.Region)
                        );
                }
                 */ 
                return _queue;

            }
        }

        public override void Write(string message)
        {
            var queue = Queue;
            if (queue == null) return;
            var sendMessageRequest = new Amazon.SQS.Model.SendMessageRequest();
            sendMessageRequest.WithQueueUrl(queueURL);
            //sendMessageRequest.WithMessageBody(String.Format("{\"instanceID\":\"{0}\",\"message\":\"{1}\"}", SQSContext.InstanceID, message));
            sendMessageRequest.WithMessageBody("{\"instanceID\":\"" + SQSTraceListener.context.InstanceId + "\",\"message\":\"" + message + "\"}");
            try
            {
                var sendMessageReply = queue.SendMessage(sendMessageRequest);
            }
            catch (Exception ex)
            {
            }
        }

        public override void WriteLine(string message)
        {
            var queue = Queue;
            if (queue == null) return;
            var sendMessageRequest = new Amazon.SQS.Model.SendMessageRequest();
            sendMessageRequest.WithQueueUrl(queueURL);
            var builder = new StringBuilder();
            builder.Append("{\"instanceID\":\"");
            builder.Append(SQSTraceListener.context.InstanceId);
            builder.Append("\",\"timestamp\":\"");
            builder.Append(DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
            builder.Append("\",\"message\":\"");
            if (message.Contains('"'))
            {
                var sb = new StringBuilder();
                sb.Append(message);
                sb.Replace("\"", "\\\"");
                builder.Append(sb);
            }
            else
            {
                builder.Append(message);
            }

            builder.Append("\"}");
            sendMessageRequest.WithMessageBody(builder.ToString());
            try
            {
                var sendMessageReply = queue.SendMessage(sendMessageRequest);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
