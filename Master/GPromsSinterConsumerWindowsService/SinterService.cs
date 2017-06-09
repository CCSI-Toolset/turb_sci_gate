using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Turbine.Data.Contract;
using Turbine.Data.Contract.Behaviors;
using Turbine.Consumer.Contract.Behaviors;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer;


namespace GPromsSinterConsumerWindowsService
{
    /*
     *  WARNING: This service will shutdown the machine
     *  if it receives a shutdown signal from the IConsumerRegistrationContract
     * 
     * 
     * 
     */
    public partial class SinterService : ServiceBase
    {
        private Thread background;
        private int timeOut = 1000 * Properties.Settings.Default.CheckForNewJobsInterval;
        private bool stop = false;
        private bool shutdown = false;
        private IConsumerRegistrationContract contract;
        private Process shutdownProcess = null;
        private Turbine.Consumer.Contract.Behaviors.IConsumerRun consumerRun = null;
        private Turbine.Consumer.Contract.Behaviors.IConsumerMonitor consumerMonitor = null;
        private bool taskMonitorFinished = true;
        private Task<int> taskMonitor = null;

        public SinterService()
        {
            ServiceName = "TurbineGPromsEngineering";
            AutoLog = true;
            InitializeComponent();
            CanShutdown = true;
        }

        protected override void OnStart(string[] args)
        {
            Debug.WriteLine("OnStart Called", "GPromsSinter.OnStart");
            background = new Thread(ScheduledRun);
            background.Name = "TurbineGProms";
            background.IsBackground = false;
            try
            {
                background.Start();
            }
            catch (Exception ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed to start background: " + ex.Message, "GPromsSinter.OnStart");
                Debug.WriteLine("InnerException:  " + ex.InnerException, "GPromsSinter.OnStart");
                Debug.WriteLine("StackTrace:  " + ex.StackTrace, "GPromsSinter.OnStart");
            }
        }

        protected override void OnStop()
        {
            Debug.WriteLine("OnStop Called", "GPromsSinter");
            stop = true;
            background.Join();
            Debug.WriteLine("OnStop Join Complete", "GPromsSinter");
        }

        protected override void OnShutdown()
        {
            Debug.WriteLine("OnShutdown Called", "GPromsSinter");
            stop = true;
            background.Join();
            Debug.WriteLine("OnShutdown Join Complete", "GPromsSinter");
        }

        void ScheduledRun()
        {
            Debug.WriteLine("Starting", "GPromsSinter.ScheduledRun");
            //contract = new Turbine.Consumer.Data.Contract.ConsumerRegistrationContract();
            contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            consumerRun = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            consumerMonitor = Turbine.Consumer.AppUtility.GetConsumerMonitorContract();

            consumerMonitor.setConsumerRun(consumerRun);

            try
            {
                contract.Register(consumerRun);
                Debug.WriteLine("contract registered consumer Run", "GPromsSinter.ScheduledRun");
            }
            catch (System.Data.UpdateException ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed Register UpdateException: " + ex, "GPromsSinter");
                Debug.WriteLine("InnerException:  " + ex.InnerException, "GPromsSinter");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed to Register: " + ex.Message, "GPromsSinter");
                Debug.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }

            // Run KeepAlive task
            var ktask = new Task<bool>(() => KeepAlive());
            ktask.Start();

            try
            {
                _ScheduledRun();
                Debug.WriteLine("Calling _ScheduledRun", "GPromsSinter.ScheduledRun");
            }
            catch (System.Data.UpdateException ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed ScheduledRun UpdateException: " + ex, "GPromsSinter");
                Debug.WriteLine("InnerException:  " + ex.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed ScheduledRun: " + ex.Message, "GPromsSinter");
                Debug.WriteLine(ex.StackTrace);
                contract.Error();
                Debug.WriteLine("SERVICE GOING TO EXIT", "GPromsSinter");
                Environment.Exit(1);
            }

            try
            {
                contract.UnRegister();
                Debug.WriteLine("contract unregistering", "GPromsSinter.ScheduledRun");
            }
            catch (System.Data.UpdateException ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed UnRegister UpdateException: " + ex.Message, "GPromsSinter");
                Debug.WriteLine("InnerException:  " + ex.InnerException, "GPromsSinter");
            }
            catch (Exception ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed UnRegister: " + ex.Message, "GPromsSinter");
                Debug.WriteLine(ex.StackTrace, "GPromsSinter");
            }

            if (shutdown)
            {
                Debug.WriteLine("CONTRACT SHUTDOWN", "GPromsSinter");
                shutdownProcess = System.Diagnostics.Process.Start("shutdown", "/s /t 10");
            }
            else if (!stop)
            {
                Debug.WriteLine("SERVICE GOING TO EXIT", "GPromsSinter");
                Environment.Exit(1);
            }
        }

        void _ScheduledRun()
        {
            Debug.WriteLine("Start ScheduledRun", "GPromsSinter");
            bool ran = false;
            //Task<int> taskMonitor = new Task<int>(() => consumerMonitor.Monitor(false));
            //taskMonitor.Start();
            while (!stop)
            {
                try
                {
                    ran = OneRun();
                }
                catch (OptimisticConcurrencyException)
                {
                    Debug.WriteLine("Data Concurrency Exception", "GPromsSinter");
                    continue;
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("Concurrent access to job", "GPromsSinter");
                    continue;
                }
                catch (InvalidStateChangeException)
                {
                    Debug.WriteLine("Invalid state change, concurrent access to job", "GPromsSinter");
                    continue;
                }

                if (!ran)
                    Thread.Sleep(timeOut);
            }

            consumerRun.CleanUp();
            consumerMonitor.CleanUp();
            Debug.WriteLine("Finished ScheduledRun", "GPromsSinter");
        }

        bool KeepAlive()
        {
            int minutes = 0;
            int failures = 0;
            IConsumerRegistrationContract cc = contract;
            while (failures < 5)
            {
                try
                {
                    cc.KeepAlive();
                    minutes = 5;
                    failures = 0;
                }
                catch (Exception ex)
                {

                    failures += 1;
                    Debug.WriteLine(String.Format("KeepAlive Exception {0}: {1}", failures, ex.Message), "GPromsSinter");
                    Debug.WriteLine(ex.StackTrace, "GPromsSinter");
                    minutes = 1;
                }
                System.Threading.Thread.Sleep(minutes * 60 * 1000);
            }
            Debug.WriteLine("KeepAlive Failed 10 consecutive times", "GPromsSinter");
            //stop = true;
            return false;
        }

        bool OneRun()
        {
            string dir = Turbine.Consumer.AppUtility.GetAppContext().BaseWorkingDirectory;
            if (!Directory.Exists(dir))
            {
                var msg = String.Format("EXIT: Base Directory does not exist: {0}", dir);
                Debug.WriteLine(msg, "GPromsSinter.OneRun");
                throw new FileNotFoundException(msg);
            }
            try
            {
                var acl = Directory.GetAccessControl(dir);
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine(String.Format("EXIT: Base Directory is not writable: {0}", dir), "GPromsSinter.OneRun");
                throw;
            }

            int timeSleepInterval = 1000;
            //int iterations = 60 * 30;
            //int setupIterations = 60 * 5;
            //int postInitIterations = 60 * 8;
            int iterations = 60 * Properties.Settings.Default.TimeOutIterations;
            int setupIterations = 60 * Properties.Settings.Default.TimeOutSetupIterations;
            int postInitIterations = 60 * Properties.Settings.Default.TimePostInitIterations;

            bool finish = false;
            var taskRun = new Task<Boolean>(() => consumerRun.Run());

            if (taskMonitorFinished)
            {
                Debug.WriteLine("> New Monitor Task", "GPromsSinter.oneRun");
                taskMonitorFinished = false;
                taskMonitor = new Task<int>(() => consumerMonitor.Monitor(false));
                taskMonitor.Start();
            }

            finish = false;
            int i = 0;

            taskRun.Start();
            bool hasInitialized = false;
            bool hasRun = false;

            while (!finish)
            {
                if (taskMonitor.IsCompleted)
                {
                    Debug.WriteLine("taskMonitor.IsCompleted", "GPromsSinter.oneRun");
                    taskMonitorFinished = true;
                    System.Threading.Thread.Sleep(2000);
                    break;
                }

                if (contract.GetStatus() == "down")
                {
                    Debug.WriteLine("ConsumerContract: Consumer is Down", "GPromsSinter.oneRun");
                    stop = true;
                    Debug.WriteLine("Exit Unregister Consumer ", "GPromsSinter.oneRun");
                    try
                    {
                        contract.UnRegister();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception caught from UnRegister: " + ex.Message, "GPromsSinter.oneRun");
                        Debug.WriteLine(ex.StackTrace, "GPromsSinter.oneRun");
                    }
                    // lock will block if "SinterConsumer.DoConfigure --> sim.openSim()" is being called
                    Debug.WriteLine("lock consumer", "GPromsSinter.oneRun");
                    int wait_seconds = 30;

                    if (Monitor.TryEnter(consumerRun, new TimeSpan(0, 0, wait_seconds)))
                    {
                        consumerMonitor.Monitor(true);
                        Debug.WriteLine("Obtained ConsumerRun Lock: Monitor Terminate", "GPromsSinter.oneRun");
                    }
                    else
                    {
                        consumerMonitor.Monitor(true);
                        Debug.WriteLine("Failed to Obtain ConsumerRun Lock: Monitor Terminate", "GPromsSinter.oneRun");
                    }
                    break;
                }

                i++;
                try
                {
                    finish = taskRun.Wait(timeSleepInterval);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception caught from Run: " + ex.Message, "GPromsSinter.oneRun");
                    Debug.WriteLine(ex.StackTrace, "GPromsSinter.oneRun");
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine("InnerException: " + ex.InnerException.Message, "GPromsSinter.oneRun");
                        Debug.WriteLine(ex.InnerException.StackTrace, "GPromsSinter.oneRun");
                    }

                    System.Threading.Thread.Sleep(2000);

                    //System.Threading.Thread.Sleep(4000);
                    Debug.WriteLine("Environment.Exit", "GPromsSinter.oneRun");
                    Environment.Exit(1);
                    break;
                }

                Debug.WriteLine(">> Waiting " + i, "GPromsSinterWindowsService");
                if (finish)
                {
                    if (taskRun.IsFaulted)
                    {
                        Debug.WriteLine("Faulted: " + taskRun.Exception, "GPromsSinterWindowsService");
                    }
                    else if (taskRun.IsCompleted)
                        Debug.WriteLine("Completed", "GPromsSinterWindowsService");
                    else
                        throw new Exception("Task Unexpected status?");

                    if (!taskRun.Result)
                    {
                        // Nothing on Job Queue
                        System.Threading.Thread.Sleep(4000);
                    }
                    break;
                }

                // DETECT HANGS
                if (consumerMonitor.IsSimulationInitializing)
                {
                    hasInitialized = true;
                    Debug.WriteLine("> Simulation Initializing: " + setupIterations);
                    if (i >= setupIterations)
                    {
                        // Allows SinterConsumer to attempt to reopen the simulation
                        // IF attempts max out in SinterConsumer, throw Exception
                        // and task.IsFaulted and job will move to error
                        Debug.WriteLine("HANG SETUP, TRY AGAIN", "GPromsSinterWindowsService");
                        consumerMonitor.MonitorTerminate();
                        Debug.WriteLine("Killed GProms instances", "GPromsSinterWindowsService");
                        i = 0;
                        continue;
                    }
                }
                else if (consumerMonitor.IsEngineRunning)
                {
                    hasRun = true;
                    Debug.WriteLine("> Engine Running: " + iterations);
                    if (i >= iterations)
                    {
                        // Assume Simulation is done Initializing
                        Debug.WriteLine("HANG RUNNING EXIT", "GPromsSinter.oneRun");
                        //consumer.MonitorTerminateAPWN();
                        try
                        {
                            consumerMonitor.MonitorTerminate();
                        }
                        catch (TerminateException ex)
                        {
                            Debug.WriteLine(ex.Message, "GPromsSinter.oneRun");
                        }
                    }
                }
                else if (hasInitialized && !hasRun)
                {
                    // Stuck 
                    if (i >= postInitIterations)
                    {
                        // Assume Simulation is done Initializing
                        Debug.WriteLine(String.Format("HANG POST INIT: {0}", i), "GPromsSinter.oneRun");
                        consumerMonitor.MonitorTerminate();
                        Debug.WriteLine("Killed GProms instances", "GPromsSinter.oneRun");
                        i = 0;
                        continue;
                    }
                }

                if (i % 60 == 0)
                {
                    Debug.WriteLine(String.Format("Waiting {0}", i), "GPromsSinter.oneRun");
                }
            }

            bool didRun = false;
            if (finish)
            {
                if (taskRun.IsFaulted)
                {
                    Debug.WriteLine("Faulted: " + taskRun.Exception, "GPromsSinter.oneRun");
                    didRun = true;
                }
                else if (taskRun.IsCompleted)
                {
                    Debug.WriteLine("Completed", "GPromsSinter.oneRun");
                    didRun = taskRun.Result;
                }
                else
                {
                    throw new Exception("Task Unexpected status: " + taskRun.Status);
                }
            }

            return didRun;
        }
    }
}
