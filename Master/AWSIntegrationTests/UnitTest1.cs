using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Turbine.Consumer.AWS.Data.Contract.Messages;

namespace AWSIntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestRegistration()
        {
            Register msg = new Register();
            msg.ConsumerId = Guid.NewGuid();
            msg.Id = Guid.NewGuid();
            msg.AMI = "ami-test";
            Turbine.Orchestrator.AWS.Data.Contract.Messages.RegistrationInfo regInfo;
            // First time will create a response queue
            try
            {
                regInfo = Turbine.Consumer.AWS.Data.Contract.SimpleMessageConnect.SendReceive(msg);
            }
            catch (Amazon.SQS.AmazonSQSException ex)
            {
                Turbine.Consumer.AWS.Data.Contract.SimpleMessageConnect.Close();
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            catch (Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceException ex)
            {
                Turbine.Consumer.AWS.Data.Contract.SimpleMessageConnect.Close();
                Console.WriteLine(ex.StackTrace);
                throw;
            }

            Assert.IsNotNull(regInfo);
            Console.WriteLine(regInfo.Id);
            Console.WriteLine(regInfo.JobQueueUrl);
            Console.WriteLine(regInfo.RequestId);
            Console.WriteLine(regInfo.ResponseQueueUrl);
            Console.WriteLine(regInfo.Timestamp);
        }
    }
}
