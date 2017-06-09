using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using System.Diagnostics;
using System.Threading;
using Turbine.Data.Contract;
using Turbine.Consumer.Contract.Behaviors;
using System.IO;
using System.Threading.Tasks;
using Turbine.Consumer.Data.Contract.Behaviors;
//using Turbine.Consumer.Data.Contract;


namespace TurbineLiteConsumerIntegrationTest
{

    class DummyAspenSinterConsumer : IConsumerRun
    {
        public bool IsEngineRunning
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSimulationInitializing
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSimulationOpened
        {
            get { throw new NotImplementedException(); }
        }

        public bool Run()
        {
            throw new NotImplementedException();
        }

        public void setIsTerminated(bool terminated)
        {
            throw new NotImplementedException();
        }

        public bool terminateSimAndJob()
        {
            throw new NotImplementedException();
        }

        public bool isJobTerminated()
        {
            throw new NotImplementedException();
        }

        public bool isJobDown()
        {
            throw new NotImplementedException();
        }

        public void CleanUp()
        {
            throw new NotImplementedException();
        }

        public string[] SupportedApplications
        {
            get { return new string[] { "ACM", "AspenPlus" }; }
        }

        private Guid consumerId = Guid.NewGuid();
        public Guid ConsumerId
        {
            get { return consumerId; }
        }

        public bool Stop()
        {
            throw new NotImplementedException();
        }
    }

    class DummyExcelSinterConsumer : IConsumerRun
    {
        public bool IsEngineRunning
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSimulationInitializing
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSimulationOpened
        {
            get { throw new NotImplementedException(); }
        }

        public bool Run()
        {
            throw new NotImplementedException();
        }

        public void CleanUp()
        {
            throw new NotImplementedException();
        }

        public void MonitorTerminate()
        {
            throw new NotImplementedException();
        }

        public string[] SupportedApplications
        {
            get { return new string[] { "Excel" }; }
        }

        private Guid consumerId = Guid.NewGuid();
        public Guid ConsumerId
        {
            get { return consumerId; }
        }

        public void setIsTerminated(bool terminated)
        {
            throw new NotImplementedException();
        }

        public bool terminateSimAndJob()
        {
            throw new NotImplementedException();
        }

        public bool isJobTerminated()
        {
            throw new NotImplementedException();
        }

        public bool isJobDown()
        {
            throw new NotImplementedException();
        }

        public bool Stop()
        {
            throw new NotImplementedException();
        }
    }

    class ConsumerContext : IConsumerContext
    {
        public Guid Id
        {
            get { throw new NotImplementedException(); }
        }

        public string Hostname
        {
            get { return "localhost"; }
        }

        public string BindSimulationName { get; set; }
    }

    class Program
    {
        static string label = "DummyConsumerConsole";
        public static string baseWorkDir;
        public static string username = "neverused";
        static private volatile bool jobs_available = true;

        static void OnExit()
        {
            Console.Write("[return]");
            Console.ReadLine();
        }

        static void Run()
        {
            IConsumerRegistrationContract cc = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            IConsumerRun run = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            IJobQueue queue = cc.Register(run);
            IJobConsumerContract contract = queue.GetNext(run);
            jobs_available = (contract != null);

            if (jobs_available == false) return;

            try
            {
                contract.Setup();
            }
            catch (System.Data.Entity.Core.OptimisticConcurrencyException ex)
            {
                var msg = String.Format(
                    "Setup Failed({0}): OptimisticConcurrencyException, {1}",
                    contract.Id, ex.Message);
                Debug.WriteLine(msg, label);
                throw;
            }
            catch (Turbine.Lite.Web.Resources.Contracts.InvalidStateChangeException ex)
            {
                var msg = String.Format(
                    "Setup Failed({0}): InvalidStateChangeException, {1}",
                    contract.Id, ex.Message);
                Debug.WriteLine(msg, label);
                throw;
            }

            Debug.WriteLine(String.Format("{0} Working Directory: {1}",
                DateTime.Now.ToString(),
                contract.Process.WorkingDirectory),
                label);

            // Execute Job
            try
            {
                contract.Running();
            }
            catch (Exception ex)
            {
                contract.Error(String.Format(
                    "threadid:{0}, machine:{1}, exception:{2}",
                    Thread.CurrentThread.ManagedThreadId, System.Environment.MachineName, ex.Message)
                    );
                throw;
            }

            IProcess process = contract.Process;
            var inputDict = process.Input;
            var outputDict = new Dictionary<string, Object>();

            foreach (var key in inputDict.Keys.ToArray<string>())
            {
                if (inputDict[key] == null)
                    continue;
                outputDict[key] = String.Format("{0} OUTPUT", inputDict[key]);
                process.AddStdout(String.Format("Add Output {0}\n", key));
            }

            process.AddStdout("Fake Job Completed");
            process.Output = outputDict;
            contract.Success();
        }

        static void Main(string[] args)
        {

            //baseWorkDir = Properties.Settings.Default.BaseDirectory;
            baseWorkDir = Directory.GetCurrentDirectory();

            //IJobConsumerContract contract = null; ;
            if (!Directory.Exists(baseWorkDir))
            {
                Debug.WriteLine(String.Format("Base Directory does not exist: {0}", baseWorkDir), label);
                OnExit();
                return;
            }
            try
            {
                var acl = Directory.GetAccessControl(baseWorkDir);
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine(String.Format("Base Directory is not writable: {0}", baseWorkDir), label);
                OnExit();
                return;
            }
            Debug.WriteLine(String.Format("Base Directory: {0}", baseWorkDir), label);
            List<Task> tasks = new List<Task>();
            Task task = null;
            bool waitNow = false;



            while (true)
            {
                if (jobs_available)
                {
                    task = Task.Factory.StartNew(() => Run());
                    tasks.Add(task);
                    waitNow = (tasks.Count == 10);
                }
                else
                {
                    Debug.WriteLine(String.Format("{0} No Jobs", DateTime.Now.ToString()), label);
                    Thread.Sleep(4000);
                    waitNow = true;
                }
                if (waitNow)
                {
                    Debug.WriteLine(String.Format("{0} Wait on All Tasks", DateTime.Now.ToString()), 
                        label);
                    try
                    {
                        Task.WaitAll(tasks.ToArray<Task>());
                    }
                    catch (AggregateException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        foreach (var e in ex.InnerExceptions)
                        {
                            Debug.WriteLine("  " + e.Message, label);
                        }
                    }

                    
                    tasks.Clear();
                    Debug.WriteLine(String.Format("{0} Finished", DateTime.Now.ToString()), label);
                    waitNow = false;
                }

            }
        }
    }
}
