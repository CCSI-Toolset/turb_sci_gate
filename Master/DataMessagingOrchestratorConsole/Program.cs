using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.AWS.Messages;
using Turbine.Orchestrator.AWS.Data.Contract.Messages;
//using Turbine.Orchestrator.AWS;
//using Turbine.Orchestrator.AWS.Data.Contract;
//using Turbine.Orchestrator.AWS.Data.Contract.Messages;
//using Turbine.Consumer.AWS.Data.Contract.Messages;


namespace DataMessagingOrchestratorConsole
{
    class Program
    {
        private static void Run()
        {
            // Check Request Queue
            //Turbine.Consumer.AWS.Data.Contract.Messages.Message msg = null;
            var context = Turbine.Orchestrator.AWS.AppUtility.GetContext();
            // Update Job Queue

            var ms = new Turbine.Orchestrator.AWS.Data.Contract.SimpleMessageListener();
            ms.Setup();

            while (true)
            {
                // Check for New Jobs put on Queue
                var jobs = GetJobsToSchedule();
                var count = ms.PutOnQueue(jobs);
                Debug.WriteLine("Put Queue Jobs: " + count);

                // Check For Requests
                RPCEnvelope[] messages = ms.Listen();
                if (messages == null) continue;

                Debug.WriteLine("Listen returned messages: " + messages.Count());
                foreach (var msg in messages)
                {
                    Debug.WriteLine("Message: " + msg.Operation);

                    if (msg.Operation == "Register")
                    {
                        // Register and send RegistrationInfo back to Response Queue
                        var registration = (Turbine.Consumer.AWS.Data.Contract.Messages.Register)msg.Message;
                        var consumerId = registration.ConsumerId;
                        var instanceId = registration.InstanceID;
                        // successfully registered
                        Debug.WriteLine("SendRegistrationInfo");
                        ms.SendRegistrationInfo(msg);
                    }
                    else
                    {
                        Debug.WriteLine(String.Format("Unknown Operation: {0}", msg.Operation));
                    }
                }
            }
        }
        private static int inc = 0;

        private static SubmitJobMessage[] GetJobsToSchedule()
        {
            inc += 1;
            var job = new SubmitJobMessage();
            job.Id = Guid.NewGuid();
            job.Inc = inc;
            job.Initialize = false;
            job.Reset = false;
            job.SimulationId = Guid.NewGuid();
            job.SimulationName = "test";


            return new SubmitJobMessage[] { job };
        }

        static void Main(string[] args)
        {
            Run();
        }
    }
}
