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
using Turbine.Consumer.Data.Contract;

namespace JobSweeperConsumer
{
    public partial class Service : ServiceBase
    {
        private Thread background;
        private int timeOut = 1000 * 60 * 5; // sweep every 5 minutes
        private int expiredMS = 1000 * 60 * 60 * 12;  // mark as expired if submitted > 24 hours ago
        private int maxMoveExpired = 100;  // Max number expired per sweep
        public Service()
        {
            ServiceName = "TurbineJobSweeper";
            AutoLog = true;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            background = new Thread(ScheduledSweep);
            background.Name = "turbinejobsweeper";
            background.IsBackground = false;
            background.Start();
        }

        protected override void OnStop()
        {
            background.Abort();
        }

        void ScheduledSweep()
        {
            int count;
            while (true)
            {
                count = JobSweeper.SweepSubmitQueue(expiredMS, maxMoveExpired);
                Debug.WriteLine("Submit Jobs moved to expired: " + count, this.GetType());
        
                count = JobSweeper.SweepCreate(expiredMS, maxMoveExpired);
                Debug.WriteLine("Create Jobs moved to expired: " + count, this.GetType());

                //
                // NOTE: Dangerous possibilities need to
                //   verify consumer has failed and possibly 
                //   kill hanging process.

                JobSweeper.SweepSetup(expiredMS);
                //JobSweeper.SweepRunning(expiredMS);

                Thread.Sleep(timeOut);
            }
        }
    }
}
