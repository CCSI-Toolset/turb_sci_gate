using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Turbine.Data;


namespace EC2AutoScalingWindowsService
{
    public partial class ScalingService : ServiceBase
    {
        private static int jobs_per_instance_per_hour = 10;
        private static int max_instances = 100;
        private Thread background;
        private int timeOut = 1000 * 30; // Sleep 30 seconds if no jobs on queue
        private bool stop = false;

        public ScalingService()
        {
            ServiceName = "TurbineOrchestrator";
            AutoLog = true;
            InitializeComponent();
            CanShutdown = true;
        }

        protected override void OnStart(string[] args)
        {
            background = new Thread(ScheduledRun);
            background.Name = "turbineorchestratorrun";
            background.IsBackground = false;
            background.Start();
        }

        protected override void OnStop()
        {
            Debug.WriteLine("OnStop Called", this.GetType());
            stop = true;
            background.Join();
            Debug.WriteLine("OnStop Join Complete", this.GetType());
        }

        protected override void OnShutdown()
        {
            Debug.WriteLine("OnShutdown Called", this.GetType());
            stop = true;
            background.Join();
            Debug.WriteLine("OnShutdown Join Complete", this.GetType());
        }

        void ScheduledRun()
        {
            try
            {
                _ScheduledRun();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed ScheduledRun: " + ex.Message, this.GetType());
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void _ScheduledRun()
        {
            int count = 0;
            int ceiling = 0;
            var contract = Turbine.Orchestrator.AWS.AppUtility.GetScaleContract();
            int reserved_floor = 0;
            int sleep_seconds = 0;

            while (!stop)
            {
                if (sleep_seconds > 0)
                {
                    Thread.Sleep(1000);
                    sleep_seconds -= 1;
                    continue;
                }

                try
                {
                    count = contract.GetRunningInstances();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception GetRunningInstances: " + ex.Message, GetType());
                    Debug.WriteLine(ex.StackTrace, GetType());
                    sleep_seconds = 5;
                    continue;
                }

                int length = 0;
                try 
                {
                    length = contract.GetQueueLength(); 
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception GetQueueLength: " + ex.Message, GetType());
                    Debug.WriteLine(ex.StackTrace, GetType());
                    sleep_seconds = 30;
                    Debug.WriteLine(String.Format("queue: sleep {0} seconds", sleep_seconds), GetType());
                    continue;
                }

                Debug.WriteLine(String.Format("{0} >>> Queued Tasks: {1}, Running Instances {2}",
                    DateTime.UtcNow, length, count));

                try
                {
                    reserved_floor = contract.GetReservedFloor();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Reserved Floor: " + ex.Message, GetType());
                    Debug.WriteLine(ex.StackTrace, GetType());
                    reserved_floor = 0;
                }

                Debug.WriteLine(String.Format("{0} >>> Queued Tasks: {1}, Running Instances {2}, Reserved Floor {3}",
                    DateTime.UtcNow, length, count, reserved_floor));

                int optimum = 0;
                if (length > 0)
                    optimum = (length / jobs_per_instance_per_hour) + 1;
                if (optimum < reserved_floor) optimum = reserved_floor;
                if (optimum > max_instances) optimum = max_instances;

                if (count < optimum)
                {
                    int launch = optimum - count;
                    if (ceiling > 0 && launch > ceiling)
                    {
                        Debug.WriteLine(String.Format("Backoff: Ceiling set at {0}", ceiling), this.GetType());
                        launch = ceiling;
                    }
                    try
                    {
                        contract.StartInstances(launch);
                        ceiling = 0;
                    }
                    catch (Amazon.EC2.AmazonEC2Exception ex)
                    {
                        Debug.WriteLine("Failed StartInstances: " + ex.Message, this.GetType());
                        Debug.WriteLine(ex.StackTrace);
                        ceiling = launch / 2;
                        if (ceiling == 0) ceiling = 1;
                        sleep_seconds = 1 * 60;
                        continue;
                    }
                    // Noticed Thrashing at 1 minute (ie. EC2 not reporting all instances as Running)
                    // TODO: Rather than sleep Wait here until reach expected # instances
                    sleep_seconds = 2 * 60;
                    Debug.WriteLine(String.Format("start: sleep {0} seconds", sleep_seconds), GetType());
                }
                else if (count > optimum)
                {
                    int terminate = count - optimum;
                    try
                    {
                        contract.TerminateInstances(terminate);
                    }
                    catch (Amazon.EC2.AmazonEC2Exception ex)
                    {
                        Debug.WriteLine("Failed TerminateInstances: " + ex.Message, this.GetType());
                        Debug.WriteLine(ex.StackTrace);
                        sleep_seconds = 5;
                        continue;
                    }
                    sleep_seconds = 2 * 60;
                    Debug.WriteLine(String.Format("terminate: sleep {0} seconds", sleep_seconds), GetType());
                }
                else
                {
                    int mins = 2;
                    sleep_seconds = mins * 60;
                }

            }
        }
    }
}
