using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Turbine.Producer;
using System.Diagnostics;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Data;
using Turbine.Data.Contract;


namespace Turbine.Producer.Data.Contract
{
    public class AspenSessionProducerContract : ISessionProducerContract
    {
        public Guid Create()
        {
            Guid session = Guid.NewGuid();
            string owner = Container.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                User user = container.Users.Single<User>(s => s.Name == owner);
                container.Sessions.AddObject(new Session() { 
                    Id = session, User = user, Create = DateTime.UtcNow });
                container.SaveChanges();
            }
            return session;
        }

        public int Delete(Guid session)
        {
            int num = 0;
            int count = 0;

            num += Cancel(session);
            num += Terminate(session);

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Session obj = container.Sessions.First<Session>(s => s.Id == session);
                count = obj.Jobs.Count<Job>();
                foreach (var job in obj.Jobs)
                {
                    SinterProcess p = job.Process;
                    container.DeleteObject(p);
                    job.Messages.ToList().ForEach(m => container.DeleteObject(m));
                }
                
                obj.Jobs.ToList().ForEach(j => container.DeleteObject(j));
                container.DeleteObject(obj);
                container.SaveChanges();
            }
            return count;
        }

        public int Submit(Guid session)
        {
            string user = Container.GetAppContext().UserName;
            int num = 0;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Session obj = container.Sessions.First<Session>(s => s.Id == session);
                Debug.WriteLine(String.Format("start session {0}, jobs {1}", session, obj.Jobs.Count()), this.GetType().Name);
                foreach (var job in obj.Jobs.Where(s => s.State == "create" & s.User.Name == user)) {
                    num += 1;
                    job.State = "submit";
                    job.Submit = DateTime.UtcNow;
                }
                container.SaveChanges();
            }
            return num;
        }

        /* Cancel:
         * 
         * Ignore concurrency exception
         * 
         */
        public int Cancel(Guid session)
        {
            int num = 0;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Session obj = container.Sessions.First<Session>(s => s.Id == session);
                foreach (var job in obj.Jobs.Where(s => s.State == "create" || s.State == "submit"))
                {
                    IJobProducerContract contract = new AspenJobProducerContract(job.Id);
                    try
                    {
                        contract.Cancel();
                        num += 1;
                    }
                    catch (AuthorizationError ex)
                    {
                        Debug.WriteLine(String.Format("AuthorizationError({0}): Failed to cancel session {1} job {2} ", ex.Message, session, job.Id),
                            this.GetType());
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(String.Format("Failed to cancel session {0} job {1} ", session, job.Id),
                            this.GetType());
                    }
                }
            }
            return num;
        }

        public int Terminate(Guid session)
        {
            int num = 0;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Session obj = container.Sessions.First<Session>(s => s.Id == session);
                foreach (var job in obj.Jobs.Where(s => s.State == "setup" || s.State == "running"))
                {
                    IJobProducerContract contract = new AspenJobProducerContract(job.Id);
                    try
                    {
                        contract.Terminate();
                        num += 1;
                    }
                    catch (InvalidStateChangeException ex)
                    {
                        Debug.WriteLine(String.Format("InvalidStateChange: Failed to terminate session {0} job {1} ", session, job.Id),
                            this.GetType());
                        Debug.WriteLine(ex.Message);
                    }
                    catch (AuthorizationError ex)
                    {
                        Debug.WriteLine(String.Format("AuthorizationError({0}): Failed to terminate session {1} job {2} ", ex.Message, session, job.Id),
                            this.GetType());
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(String.Format("Failed to terminate session {0} job {1} ", session, job.Id),
                            this.GetType());
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            return num;
        }

        /* Pause:  Concurrency errors ignore, but must handle.  Pause in descending order by Id,
         *   to avoid competition with job consumers.
         * 
         */
        public int Pause(Guid session)
        {
            int num = 0;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Session obj = container.Sessions.First<Session>(s => s.Id == session);
                foreach (var job in obj.Jobs.Where(s => s.State == "submit").OrderByDescending(t => t.Id))
                {
                    IJobProducerContract contract = new AspenJobProducerContract(job.Id);
                    try
                    {
                        job.State = "pause";
                        container.SaveChanges();
                        num += 1;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(String.Format("Failed to pause session {0} job {1} ", session, job.Id),
                            this.GetType());
                    }
                }
            }
            return num;
        }

        /*  Unpause:
         *     Assume that state of jobs cannot be changed during this call.  Therefore
         *     if one of them has the behavior is undefined.
         */
        public int Unpause(Guid session)
        {
            int num = 0;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Session obj = container.Sessions.First<Session>(s => s.Id == session);
                foreach (var job in obj.Jobs.Where(s => s.State == "pause"))
                {
                    try
                    {
                        job.State = "submit";
                        job.Submit = DateTime.UtcNow;
                        num += 1;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(String.Format("Failed to unpause session {0} job {1} ", session, job.Id),
                            this.GetType());
                    }
                }
                container.SaveChanges();
            }
            return num;
        }


        public Dictionary<string, int> Status(Guid session)
        {
            var d = new Dictionary<string, int>();
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Session obj = container.Sessions.First<Session>(s => s.Id == session);
                d["create"] = obj.Jobs.Count<Job>(j => j.State == "create");
                d["running"] = obj.Jobs.Count<Job>(j => j.State == "running");
                d["setup"] = obj.Jobs.Count<Job>(j => j.State == "setup");
                d["submit"] = obj.Jobs.Count<Job>(j => j.State == "submit");
                d["pause"] = obj.Jobs.Count<Job>(j => j.State == "pause");
                d["locked"] = obj.Jobs.Count<Job>(j => j.State == "locked");
                d["error"] = obj.Jobs.Count<Job>(j => j.State == "error");
                d["cancel"] = obj.Jobs.Count<Job>(j => j.State == "cancel");
                d["success"] = obj.Jobs.Count<Job>(j => j.State == "success");
                d["terminate"] = obj.Jobs.Count<Job>(j => j.State == "terminate");
            }
            return d;
        }

    }
}
