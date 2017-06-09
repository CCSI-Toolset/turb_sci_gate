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
using Turbine.Consumer;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.SimSinter;


namespace ExcelSinterConsumerWindowsService
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
        private int timeOut = 1000 * 30; // Sleep 30 seconds if no jobs on queue
        private bool stop = false;
        private bool shutdown = false;
        private bool doUnregister = true;
        private IConsumerRegistrationContract contract;
        private Process shutdownProcess = null;
        //private Turbine.Consumer.Excel.ExcelSinterConsumer consumer = null;
        private Turbine.Consumer.Contract.Behaviors.IConsumerRun consumerRun = null;
        private Turbine.Consumer.Contract.Behaviors.IConsumerMonitor consumerMonitor = null;
        private bool taskMonitorFinished = true;
        private Task<int> taskMonitor = null;

        public SinterService()
        {
            ServiceName = "TurbineExcel";
            AutoLog = true;
            InitializeComponent();
            CanShutdown = true;
            //consumer = new Turbine.Consumer.Excel.ExcelSinterConsumer();
        }

        protected override void OnStart(string[] args)
        {
            Debug.WriteLine("OnStart Called", "TurbineExcel");
            background = new Thread(ScheduledRun);
            background.Name = "TurbineExcel";
            background.IsBackground = false;
            background.Start();
        }

        protected override void OnStop()
        {
            Debug.WriteLine("OnStop Called", "TurbineExcel");
            stop = true;
            background.Join();
            Debug.WriteLine("OnStop Join Complete", "TurbineExcel");
        }

        protected override void OnShutdown()
        {
            Debug.WriteLine("OnShutdown Called", "TurbineExcel");
            stop = true;
            background.Join();
            Debug.WriteLine("OnShutdown Join Complete", "TurbineExcel");
        }

        void ScheduledRun()
        {
            //contract = new Turbine.Consumer.Data.Contract.ConsumerRegistrationContract();
            contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            consumerRun = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            consumerMonitor = Turbine.Consumer.AppUtility.GetConsumerMonitorContract();

            consumerMonitor.setConsumerRun(consumerRun);

            try
            {
                contract.Register(consumerRun);
            }
            catch (System.Data.UpdateException ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed Register UpdateException: " + ex, "TurbineExcel");
                Debug.WriteLine("InnerException:  " + ex.InnerException, "TurbineExcel");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed to Register: " + ex.Message, "TurbineExcel");
                Debug.WriteLine(ex.StackTrace, "TurbineExcel");
                Environment.Exit(1);
            }

            // Run KeepAlive task
            var ktask = new Task<bool>(() => KeepAlive());
            ktask.Start();

            try
            {
                _ScheduledRun();
            }
            catch (System.Data.UpdateException ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed ScheduledRun UpdateException: " + ex, "TurbineExcel");
                Debug.WriteLine("InnerException:  " + ex.InnerException, "TurbineExcel");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed ScheduledRun: " + ex.Message, "TurbineExcel");
                Debug.WriteLine(ex.StackTrace, "TurbineExcel");
                contract.Error();
                Debug.WriteLine("SERVICE GOING TO EXIT", "TurbineExcel");
                Environment.Exit(1);
            }

            try
            {
                contract.UnRegister();
            }
            catch (System.Data.UpdateException ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed UnRegister UpdateException: " + ex.Message, "TurbineExcel");
                Debug.WriteLine("InnerException:  " + ex.InnerException, "TurbineExcel");
            }
            catch (Exception ex)
            {
                // TODO: Ask to be shutdown 
                Debug.WriteLine("Failed UnRegister: " + ex.Message, "TurbineExcel");
                Debug.WriteLine(ex.StackTrace, "TurbineExcel");
            }

            if (shutdown)
            {
                Debug.WriteLine("CONTRACT SHUTDOWN", "TurbineExcel");
                shutdownProcess = System.Diagnostics.Process.Start("shutdown", "/s /t 10");
            }
            else if (!stop)
            {
                Debug.WriteLine("SERVICE GOING TO EXIT", "TurbineExcel");
                Environment.Exit(1);
            }
        }

        void _ScheduledRun()
        {
            Debug.WriteLine("Start ScheduledRun", "TurbineExcel");
            bool ran = false;
            // taskMonitor = new Task<int>(() => consumerMonitor.Monitor(false));
            //taskMonitor.Start();
            while (!stop)
            {
                try
                {
                    ran = OneRun();
                }
                catch (OptimisticConcurrencyException)
                {
                    Debug.WriteLine("Data Concurrency Exception", "TurbineExcel");
                    continue;
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("Concurrent access to job", "TurbineExcel");
                    continue;
                }
                catch (InvalidStateChangeException)
                {
                    Debug.WriteLine("Invalid state change, concurrent access to job", "TurbineExcel");
                    continue;
                }

                if (!ran)
                    Thread.Sleep(timeOut);
            }

            consumerRun.CleanUp();
            consumerMonitor.CleanUp();
            Debug.WriteLine("Finished ScheduledRun", "TurbineExcel");
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
                    Debug.WriteLine(String.Format("KeepAlive Exception {0}: {1}", failures, ex.Message), "TurbineExcel");
                    Debug.WriteLine(ex.StackTrace, "TurbineExcel");
                    minutes = 1;
                }
                System.Threading.Thread.Sleep(minutes * 60 * 1000);
            }
            Debug.WriteLine("KeepAlive Failed 10 consecutive times", "TurbineExcel");
            //stop = true;
            return false;
        }

        bool OneRun()
        {
            string dir = Turbine.Consumer.AppUtility.GetAppContext().BaseWorkingDirectory;
            if (!Directory.Exists(dir))
            {
                var msg = String.Format("EXIT: Base Directory does not exist: {0}", dir);
                Debug.WriteLine(msg);
                throw new FileNotFoundException(msg);
            }
            try
            {
                var acl = Directory.GetAccessControl(dir);
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine(String.Format("EXIT: Base Directory is not writable: {0}", dir));
                throw;
            }

            int timeSleepInterval = 1000;
            int iterations = 60 * 30;
            int setupIterations = 60 * 5;
            int postInitIterations = 60 * 8;
            bool finish = false;
            var taskRun = new Task<Boolean>(() => consumerRun.Run());

            if (taskMonitorFinished)
            {
                Debug.WriteLine("> New Monitor Task", "ExcelSinter.OneRun");
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
                    Debug.WriteLine("taskMonitor.IsCompleted", "ExcelSinter.OneRun");
                    taskMonitorFinished = true;
                    System.Threading.Thread.Sleep(2000);
                    break;
                }

                if (contract.GetStatus() == "down")
                {
                    Debug.WriteLine("ConsumerContract: Consumer is Down", "ExcelSinter.OneRun");
                    stop = true;
                    Debug.WriteLine("Exit Unregister Consumer ", "ExcelSinter.OneRun");
                    try
                    {
                        contract.UnRegister();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception caught from UnRegister: " + ex.Message, "ExcelSinter.OneRun");
                        Debug.WriteLine(ex.StackTrace, "ExcelSinter.OneRun");
                    }
                    // lock will block if "SinterConsumer.DoConfigure --> sim.openSim()" is being called
                    Debug.WriteLine("lock consumer", "ExcelSinter.OneRun");
                    int wait_seconds = 30;

                    if (Monitor.TryEnter(consumerRun, new TimeSpan(0, 0, wait_seconds)))
                    {
                        consumerMonitor.Monitor(true);
                        Debug.WriteLine("Obtained ConsumerRun Lock: Monitor Terminate", "ExcelSinter.OneRun");
                    }
                    else
                    {
                        consumerMonitor.Monitor(true);
                        Debug.WriteLine("Failed to Obtain ConsumerRun Lock: Monitor Terminate", "ExcelSinter.OneRun");
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
                    Debug.WriteLine("Exception caught from Run: " + ex.Message, "TurbineExcel");
                    Debug.WriteLine(ex.StackTrace, "TurbineExcel");
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine("InnerException: " + ex.InnerException.Message, "TurbineExcel");
                        Debug.WriteLine(ex.InnerException.StackTrace, "TurbineExcel");
                    }

                    System.Threading.Thread.Sleep(2000);

                    //System.Threading.Thread.Sleep(4000);
                    Debug.WriteLine("Environment.Exit", "TurbineExcel");
                    Environment.Exit(1);
                    break;
                }

                Debug.WriteLine(">> Waiting " + i, "ExcelWindowsServer");
                if (finish)
                {
                    if (taskRun.IsFaulted)
                    {
                        Debug.WriteLine("Faulted: " + taskRun.Exception, "ExcelWindowsServer");
                    }
                    else if (taskRun.IsCompleted)
                        Debug.WriteLine("Completed", "ExcelWindowsServer");
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
                    if (i >= setupIterations)
                    {
                        // Allows SinterConsumer to attempt to reopen the simulation
                        // IF attempts max out in SinterConsumer, throw Exception
                        // and task.IsFaulted and job will move to error
                        Debug.WriteLine("HANG SETUP, TRY AGAIN", "TurbineExcel");
                        consumerMonitor.MonitorTerminate();
                        Debug.WriteLine("Killed Aspen instances", "TurbineExcel");
                        i = 0;
                        continue;
                    }
                }
                else if (consumerMonitor.IsEngineRunning)
                {
                    hasRun = true;
                    if (i >= iterations)
                    {
                        // Assume Simulation is done Initializing
                        Debug.WriteLine("HANG RUNNING EXIT", "TurbineExcel");
                        //consumer.MonitorTerminateAPWN();
                        try
                        {
                            //consumer.MonitorTerminateAspenEngine();
                            //consumer.MonitorTerminateRunning();
                            consumerMonitor.MonitorTerminate();
                        }
                        catch (TerminateException ex)
                        {
                            Debug.WriteLine(ex.Message, "TurbineExcel");
                        }
                    }
                }
                else if (hasInitialized && !hasRun)
                {
                    // Stuck 
                    if (i >= postInitIterations)
                    {
                        // Assume Simulation is done Initializing
                        Debug.WriteLine(String.Format("HANG POST INIT: {0}", i), "TurbineExcel");
                        consumerMonitor.MonitorTerminate();
                        Debug.WriteLine("Killed Aspen instances", "TurbineExcel");
                        i = 0;
                        continue;
                    }
                }

                if (i % 60 == 0)
                {
                    Debug.WriteLine(String.Format("Waiting {0}", i), "TurbineExcel");
                }
            }

            bool didRun = false;
            if (finish)
            {
                if (taskRun.IsFaulted)
                {
                    Debug.WriteLine("Faulted: " + taskRun.Exception, "TurbineExcel");
                    didRun = true;
                }
                else if (taskRun.IsCompleted)
                {
                    Debug.WriteLine("Completed", "TurbineExcel");
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
