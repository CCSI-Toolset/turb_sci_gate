using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Turbine.Data.Contract
{
    public static class JobSweeper
    {
        // Moves old jobs to expired
        public static int SweepSubmitQueue(int expiredMS, int max = 0)
        {
            DateTime expired = DateTime.UtcNow.AddMilliseconds(- expiredMS);
            int swept = 0;
            Job job = null;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                while (true) 
                {
                    //job = container.Jobs.FirstOrDefault<Job>(s => s.State == "submit" && s.Submit <= expired);
                    job = container.Jobs.OrderBy(t => t.Submit).FirstOrDefault(s => s.State == "submit" && s.Submit <= expired);
                    if (job == null) break;
                    job.State = "expired";
                    job.Finished = DateTime.UtcNow;
                    var msg = new Message();
                    msg.Value = "Job has expired in submit state after waiting on run queue for " + expiredMS + " milliseconds";
                    job.Messages.Add(msg);
                    Debug.WriteLine(String.Format("Job({0}) moved to expired state", job.Id));
                    swept += 1;
                    container.SaveChanges();
                    if (max > 0 && swept >= max) break;
                }
            }
            return swept;
        }

        // Moves old jobs to expired
        public static int SweepCreate(int expiredMS, int max = 0)
        {
            DateTime expired = DateTime.UtcNow.AddMilliseconds(-expiredMS);
            int swept = 0;
            Job job = null;
            Message msg = null;

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                while (true)
                {
                    //job = container.Jobs.FirstOrDefault<Job>(s => s.State == "submit" && s.Submit <= expired);
                    job = container.Jobs.OrderBy(t => t.Submit).FirstOrDefault(s => s.State == "create" && s.Create <= expired);
                    if (job == null) break;
                    job.State = "expired";
                    job.Finished = DateTime.UtcNow;
                    msg = new Message();
                    msg.Value = "Job has expired in create state after waiting for " + expiredMS + " milliseconds";
                    Debug.WriteLine(String.Format("Job({0}) moved to expired state", job.Id));
                    swept += 1;
                    container.SaveChanges();
                    if (max > 0 && swept >= max) break;
                }
            }
            return swept;
        }
        // Moves old jobs to expired
        public static int SweepSetup(int expiredMS, int max = 0)
        {
            DateTime expired = DateTime.UtcNow.AddMilliseconds(-expiredMS);
            int swept = 0;
            Job job = null;
            Message msg = null;

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                while (true)
                {
                    job = container.Jobs.OrderBy(t => t.Submit).FirstOrDefault(s => s.State == "setup" && s.Create <= expired);
                    if (job == null) break;
                    job.State = "expired";
                    job.Finished = DateTime.UtcNow;
                    msg = new Message();
                    msg.Value = "Job has expired in setup state after waiting for " + expiredMS + " milliseconds";
                    job.Messages.Add(msg);
                    Debug.WriteLine(String.Format("Job({0}) moved to expired state", job.Id));
                    swept += 1;
                    container.SaveChanges();
                    if (max > 0 && swept >= max) break;
                }
            }
            return swept;
        }
        // Moves old jobs to expired
        public static int SweepRunning(int expiredMS, int max = 0)
        {
            DateTime expired = DateTime.UtcNow.AddMilliseconds(-expiredMS);
            int swept = 0;
            Job job = null;
            Message msg = null;

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                while (true)
                {
                    //job = container.Jobs.FirstOrDefault<Job>(s => s.State == "submit" && s.Submit <= expired);
                    job = container.Jobs.OrderBy(t => t.Submit).FirstOrDefault(s => s.State == "running" && s.Running <= expired);
                    if (job == null) break;
                    job.State = "error";
                    job.Finished = DateTime.UtcNow;
                    msg = new Message();
                    msg.Value = "Job moved to error after being in running state for " + expiredMS + " milliseconds";
                    job.Messages.Add(msg);
                    Debug.WriteLine(String.Format("Job({0}) moved to expired state", job.Id));
                    swept += 1;
                    container.SaveChanges();
                    if (max > 0 && swept >= max) break;
                }
            }
            return swept;
        }
    }
}
