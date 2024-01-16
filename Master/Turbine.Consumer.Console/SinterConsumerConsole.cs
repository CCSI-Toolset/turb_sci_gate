using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Unity;
using Turbine.Data.Contract.Behaviors;
using sinter;
using System.Threading;
using Turbine.Data.Contract;
using System.Threading.Tasks;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract;
using Turbine.Consumer;
using Turbine.Consumer.SimSinter;
using Turbine.Consumer.Contract.Behaviors;


namespace Turbine.Consumer.Console
{
    class ConsumerContext : IConsumerContext
    {
        public Guid Id
        {
            get
            {
                return SinterConsumerConsole.guid;
            }
        }
        private static string hostname = System.Net.Dns.GetHostName();
        public string Hostname
        {
            get
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                return String.Format("{0} - {1}", hostname, id);
            }
        }


        public string BindSimulationName { get; set; } 
    }

    public class SinterConsumerConsole
    {
        public static Guid guid = Guid.NewGuid();
        public static string username = "neverused";
        static IConsumerRegistrationContract cc = null;
        private static int iterations = 60 * 45;
        private static int setupIterations = 60 * 10;
        private static int postInitIterations = 60 + (60 * 10);

        static void OnExit(int retCode)
        {
            Debug.WriteLine("Unregister Consumer");
            cc.UnRegister();
            Environment.Exit(retCode);
        }

        static IConsumerRun consumerRun = null;
        static IConsumerMonitor consumerMonitor = null;
        static int ERROR_SUCCESS = 0;
        static int ERROR_PATH_NOT_FOUND = 3;
        static int ERROR_ACCESS_DENIED = 5;

        public static bool KeepAlive()
        {
            int minutes = 0;
            int failures = 0;
            while (failures < 10)
            {
                try
                {
                    cc.KeepAlive();
                    minutes = 5;
                    failures = 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("KeepAlive Exception: " + ex.Message, "SinterConsumerConsole");
                    Debug.WriteLine(ex.StackTrace, "SinterConsumerConsole");
                    minutes = 1;
                    failures += 1;
                }
                System.Threading.Thread.Sleep(minutes * 60 * 1000);
            }
            return false;
        }

        public static void setTimeOutParams(int iter, int setup, int postInit)
        {
            iterations = 60 * iter;
            setupIterations = 60 * setup;
            postInitIterations = 60 * postInit;
        }

        public static void Run()
        {
            Debug.WriteLine("SinterCosnumerConsole : Running");
            int timeSleepInterval = 1000;
            bool finish = false;
            String dir = AppUtility.GetAppContext().BaseWorkingDirectory;
            IConsumerContext consumerCtx = AppUtility.GetConsumerContext();

            // Register as a consumer, else can't use JobContract
            cc = AppUtility.GetConsumerRegistrationContract();
            consumerRun = AppUtility.GetConsumerRunContract();
            consumerMonitor = AppUtility.GetConsumerMonitorContract();
            consumerMonitor.setConsumerRun(consumerRun);
            cc.Register(consumerRun);

            // Run KeepAlive task
            var ktask = new Task<bool>(() => KeepAlive());
            ktask.Start();
            bool stop = false;
            bool cancelKeyPressed = false;
            var obj = new Object();
            System.Console.CancelKeyPress += delegate
            {
                lock (obj)
                {

                    stop = true;
                    cancelKeyPressed = true;
                    Debug.WriteLine("Exit Unregister Consumer ", "SinterConsumerConsole");
                    try
                    {
                        cc.UnRegister();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception caught from UnRegister: " + ex.Message, "SinterConsumerConsole");
                        Debug.WriteLine(ex.StackTrace, "SinterConsumerConsole");
                    }
                    // lock will block if "SinterConsumer.DoConfigure --> sim.openSim()" is being called
                    Debug.WriteLine("lock consumer", "SinterConsumerConsole");
                    int wait_seconds = 30;
                    if (Monitor.TryEnter(consumerRun, new TimeSpan(0, 0, wait_seconds)))
                    {
                        consumerMonitor.Monitor(cancelKeyPressed);
                        Debug.WriteLine("Obtained ConsumerRun Lock: Monitor Terminate", "SinterConsumerConsole");
                    }
                    else
                    {
                        consumerMonitor.Monitor(cancelKeyPressed);
                        Debug.WriteLine("Failed to Obtain ConsumerRun Lock: Monitor Terminate", "SinterConsumerConsole");
                    }
                    Debug.WriteLine("Consumer Exit", "SinterConsumerConsole");
                }
            };

            Task<int> taskMonitor = null;
            bool taskMonitorFinished = true; 

            while (stop == false)
            {
                if (!Directory.Exists(dir))
                {
                    Debug.WriteLine(String.Format("Base Directory does not exist: {0}", dir), "SinterConsumerConsole");
                    OnExit(ERROR_PATH_NOT_FOUND);
                }
                try
                {
                    var acl = Directory.GetAccessControl(dir);
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine(String.Format("Base Directory is not writable: {0}", dir), "SinterConsumerConsole");
                    OnExit(ERROR_ACCESS_DENIED);
                }

                Debug.WriteLine("> New Task", "SinterConsumerConsole");                
                                
                var taskRun = new Task<Boolean>(() => consumerRun.Run());

                if (taskMonitorFinished)
                {
                    Debug.WriteLine("> New Monitor Task", "SinterConsumerConsole.Run");    
                    taskMonitorFinished = false;
                    taskMonitor = new Task<int>(() => consumerMonitor.Monitor(false));
                    taskMonitor.Start();
                }

                finish = false;
                int i = 0;
                bool hasInitialized = false;
                bool hasRun = false;
                taskRun.Start();

                while (stop == false)
                {
                    if (taskMonitor.IsCompleted)
                    {
                        Debug.WriteLine("taskMonitor.IsCompleted", "SinterConsumerConsole.Run");
                        taskMonitorFinished = true;
                        System.Threading.Thread.Sleep(2000);
                        break;
                    }

                    if (cc.GetStatus() == "down")
                    {
                        Debug.WriteLine("ConsumerContract: Consumer is Down", "SinterConsumerConsole.Run");
                        stop = true;
                        Debug.WriteLine("Exit Unregister Consumer ", "SinterConsumerConsole.Run");
                        try
                        {
                            cc.UnRegister();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Exception caught from UnRegister: " + ex.Message, "SinterConsumerConsole.Run");
                            Debug.WriteLine(ex.StackTrace, "SinterConsumerConsole.Run");
                        }
                        // lock will block if "SinterConsumer.DoConfigure --> sim.openSim()" is being called
                        Debug.WriteLine("lock consumer", "SinterConsumerConsole.Run");
                        int wait_seconds = 30;

                        if (Monitor.TryEnter(consumerRun, new TimeSpan(0, 0, wait_seconds)))
                        {
                            consumerMonitor.Monitor(true);
                            Debug.WriteLine("Obtained ConsumerRun Lock: Monitor Terminate", "SinterConsumerConsole.Run");
                        }
                        else
                        {
                            consumerMonitor.Monitor(true);
                            Debug.WriteLine("Failed to Obtain ConsumerRun Lock: Monitor Terminate", "SinterConsumerConsole.Run");
                        }
                        break;
                    }

                    i++;
                    Debug.WriteLine("top");

                    try
                    {
                        finish = taskRun.Wait(timeSleepInterval);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception caught from Run: " + ex.Message, "SinterConsumerConsole");
                        Debug.WriteLine(ex.StackTrace, "SinterConsumerConsole");
                        Debug.WriteLine("InnerException: " + ex.InnerException, "SinterConsumerConsole");
                        System.Threading.Thread.Sleep(2000);
                        /*// check if job was terminated, InnerException would be InvalidStateChangeException
                        if (ex.InnerException is InvalidStateChangeException)
                        {
                            consumer.CheckTerminate();
                        }
                         */
                        break;
                    }
                    //Debug.WriteLine(">> Waiting!!!! " + i + " " + iterations + " " + setupIterations + " " + postInitIterations, "SinterConsumerConsole");
                    Debug.WriteLine(">> Waiting " + i, "SinterConsumerConsole");
                    if (finish)
                    {
                        if (taskRun.IsFaulted)
                        {
                            Debug.WriteLine("Faulted: " + taskRun.Exception, "SinterConsumerConsole");
                        }
                        else if (taskRun.IsCompleted)
                            Debug.WriteLine("Completed", "SinterConsumerConsole");
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
                            Debug.WriteLine("HANG SETUP, TRY AGAIN", "SinterConsumerConsole");

                            consumerMonitor.MonitorTerminate();

                            Debug.WriteLine("Killed Aspen Process", "SinterConsumerConsole");
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
                            Debug.WriteLine("HANG RUNNING EXIT", "SinterConsumerConsole");
                            //consumer.MonitorTerminateAPWN();
                            try
                            {
                                //consumer.MonitorTerminateAspenEngine();
                                //consumer.MonitorTerminateRunning();
                                consumerMonitor.MonitorTerminate();
                            }
                            catch (TerminateException ex)
                            {
                                Debug.WriteLine(ex.Message, "SinterConsumerConsole");
                            }
                        }
                    }
                    else if (hasInitialized && !hasRun)
                    {
                        // Stuck 
                        if (i >= postInitIterations)
                        {
                            // Assume Simulation is done Initializing
                            Debug.WriteLine("HANG POST INIT, TRY AGAIN", "SinterConsumerConsole");
                            consumerMonitor.MonitorTerminate();
                            Debug.WriteLine("Killed Aspen instances", "SinterConsumerConsole");
                            i = 0;
                            continue;
                        }
                    }
                    else if (i % 60 == 0)
                    {
                        Debug.WriteLine(String.Format("Waiting {0}", i), "SinterConsumerConsole");
                    }
                } // end of while
                Debug.WriteLine("Stop called", "SinterConsumerConsole");
            } // end of while

            lock (obj)
            {
                Debug.WriteLine("Stop and cleanup", "SinterConsumerConsole");

                consumerMonitor.MonitorTerminate();
            }
            //consumer.CleanUp();
            //OnExit(ERROR_SUCCESS);
        }
    }
}
