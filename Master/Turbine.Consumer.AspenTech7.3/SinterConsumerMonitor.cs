using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Turbine.Consumer.Data.Contract.Behaviors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Turbine.Consumer.Contract.Behaviors;


namespace Turbine.Consumer.AspenTech73
{
    /// <summary>
    /// SinterConsumer
    /// </summary>
    public class SinterConsumerMonitor : IConsumerMonitor
    {
        protected bool isTerminated = false;
        protected Guid consumerId;
        protected IConsumerRun consumerRun;
        private IJobConsumerContract job;

        public SinterConsumerMonitor()
        {
            consumerId = Guid.NewGuid();
        }

        Guid IConsumerMonitor.ConsumerId
        {
            get { return consumerId; }
        }

        /// <summary>
        /// IsSetup returns true if the simulation has been opened or the COM process server "apwn" was
        /// created and the RCW returned.  This should be monitored and if it doesn't change over some
        /// reasonable time interval, call DoTerminate to destroy.
        /// </summary>
        ///
        public bool IsEngineRunning
        {
            get { return consumerRun.IsEngineRunning; }
        }
        public bool IsSimulationInitializing
        {
            get { return consumerRun.IsSimulationInitializing; }
        }
        public bool IsSimulationOpened
        {
            get { return consumerRun.IsSimulationOpened; }
        }

        public void CleanUp()
        {
            Debug.WriteLine("CleanUp not IMPLEMENTED");
        }

        /// <summary>
        /// MonitorTerminate:  KIlls the underlying process and children
        /// </summary>
        /// <returns></returns>
        public void setConsumerRun(IConsumerRun iconsumerRun)
        {
            consumerRun = iconsumerRun;
        }

        /// <summary>
        /// MonitorTerminate:  KIlls the underlying process and children
        /// </summary>
        /// <returns></returns>
        public virtual void MonitorTerminate()
        {
            Debug.WriteLine("MonitorTerminate", GetType().Name);
            bool stopped = false;
            stopped = consumerRun.Stop();
            if (!stopped)
            {
                try
                {
                    Debug.WriteLine("Wait for terminate to release lock on consumerRun", "SinterConsumerMonitor.MonitorTerminate");
                    isTerminated = consumerRun.terminateSimAndJob();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Job Termination Failed", "SinterConsumerMonitor.MonitorTerminate");
                    Debug.WriteLine(e.Message, "MonitorTerminate");
                    Debug.WriteLine(e.StackTrace, "MonitorTerminate");
                }
            }
        }


        /// <summary>
        /// Monitor the jobs and manages to terminate any process if needed
        /// </summary>
        /// <returns></returns>
        public int Monitor(bool cancelKeyPressed)
        {
            int jobTerminateState = 0;

            // Checks if the cancel key is pressed
            if (cancelKeyPressed == true)
            {
                if (IsSimulationInitializing)
                {
                    Debug.WriteLine("Exit Simulation Initializing", "SinterConsumerMonitor.Monitor");
                    MonitorTerminate();
                }
                else if (IsEngineRunning)
                {
                    Debug.WriteLine("Exit Simulation Running", "SinterConsumerMonitor.Monitor");
                    MonitorTerminate();
                }
                else if (IsSimulationOpened)
                {
                    Debug.WriteLine("Exit Simulation Opened", "SinterConsumerMonitor.Monitor");
                    MonitorTerminate();
                }
            }
            else
            {
                Debug.WriteLine("Aspen 7.3 Checking Terminate starts", "SinterConsumerMonitor.Monitor");

                isTerminated = false;
                while (isTerminated == false)
                {
                    System.Threading.Thread.Sleep(500);
                    jobTerminateState = CheckTerminate();
                }
                Debug.WriteLine("Checking Terminate ends", "SinterConsumerMonitor.Monitor");
            }

            return jobTerminateState;
        }


        /// <summary>
        /// CheckTerminate:  lock around this, wait for sim.openSim to complete so the process can be terminated
        /// if that was requested.
        /// </summary>
        /// <returns></returns>
        public int CheckTerminate()
        {
            if (isTerminated)
            {
                return -1;
            }

            try
            {
                if (consumerRun.isJobTerminated())
                {
                    Debug.WriteLine("Job is Terminated", "SinterConsumerMonitor.CheckTerminate");
                    //Debug.WriteLine(String.Format("CheckTerminate: job {0} has been terminated by producer", job.Id), GetType().Name);
                    MonitorTerminate();
                    return -1;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Job termination process failed", "SinterConsumerMonitor.CheckTerminate");
                Debug.WriteLine(e.Message, "Error CheckTerminate");
                Debug.WriteLine(e.StackTrace, "ErrorStack CheckTerminate");
            }

            return 0;
        }
    }
}