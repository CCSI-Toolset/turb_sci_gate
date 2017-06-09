using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Turbine.Consumer.AWS.Data.Contract.Messages;
using Turbine.Orchestrator.AWS.Data.Contract.Messages;
using System.Diagnostics;

namespace AWSConsumerConsole
{
    class ConsoleContext : Turbine.Consumer.Contract.Behaviors.IContext
    {
        public string UserName
        {
            get { return "consoleuser"; }
        }
        public string BaseWorkingDirectory
        {
            get { return System.IO.Directory.GetCurrentDirectory(); }
        }
    }

    class ConsumerContext : Turbine.Consumer.Contract.Behaviors.IConsumerContext
    {
        private static Guid id = Guid.NewGuid();
        public Guid Id
        {
            get { return id;}
        }
        private static string hostname = System.Net.Dns.GetHostName();
        public string Hostname
        {
            get { return hostname; }
        }
    }

    class Program
    {

        public static void Main(string[] args)
        {
            var program = new Program();
            var regInfo = program.GetRegistration();
            program.Run(regInfo);
            
      
            /*
            string line = null;
            while (line != "quit")
            {
                Console.WriteLine("==============================");
                Console.WriteLine("  Turbine Consumer Console");
                Console.WriteLine("      register --");
                Console.WriteLine("==============================");
                Console.Write(">> ");
                line = Console.ReadLine();
                if (line == "register") contract.GetRunningInstances();

            }
             * */

        }

        private void Run(RegistrationInfo regInfo)
        {
            var jobQueue = new Turbine.Consumer.AWS.Data.Contract.JobQueue(regInfo);
            Turbine.Consumer.Data.Contract.Behaviors.IJobConsumerContract contract;
            do
            {
                Debug.WriteLine("Find Next Job", GetType().Name);
                contract = jobQueue.GetNext();
            }
            while (contract == null);

            contract.Setup();
        }

      

        private RegistrationInfo GetRegistration()
        {
            Register msg = new Register();
            msg.ConsumerId = Guid.NewGuid();
            msg.Id = Guid.NewGuid();
            msg.AMI = "ami-test";
            RegistrationInfo regInfo;
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

            if (regInfo == null)
            {
                Debug.WriteLine("No Registration Information Returned", "AWSConsumerConsole");
                return null;
            }
            Console.WriteLine("RegistrationInfo: " + regInfo.Id);
            Console.WriteLine("  JobQueueUrl:      " + regInfo.JobQueueUrl);
            Console.WriteLine("  RequestId:        " + regInfo.RequestId);
            Console.WriteLine("  ResponseQueueUrl: " + regInfo.ResponseQueueUrl);
            Console.WriteLine(regInfo.Timestamp);

            // Try again should work, response queue already exists
            //regInfo = Turbine.Consumer.AWS.Data.Contract.SimpleMessageConnect.SendReceive(msg);
            return regInfo;
        }
    }
}