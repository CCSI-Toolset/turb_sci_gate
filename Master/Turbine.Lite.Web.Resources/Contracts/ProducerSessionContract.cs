using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Turbine.Producer;
using System.Diagnostics;
using Turbine.Producer.Data.Contract.Behaviors;
//using Turbine.Data.Contract;
using Turbine.DataEF6;
using Turbine.Data.Entities;


namespace Turbine.Lite.Web.Resources.Contracts
{
    public class ProducerSessionContract : ISessionProducerContract
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Guid Create()
        {
            Guid session = Guid.NewGuid();
            string owner = Container.GetAppContext().UserName;
            using (var db = new ProducerContext())
            {
                User user = db.Users.Single<User>(s => s.Name == owner);
                db.Sessions.Add(new Session() { 
                    Id = session, User = user, Create = DateTime.UtcNow });
                db.SaveChanges();
            }
            return session;
        }
        
        /// <summary>
        /// Delete moves all jobs to cancel or terminate, then safely deletes them.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int Delete(Guid session)
        {
            int num = 0;
            int count = 0;

            num += Cancel(session);
            num += Terminate(session);

            using (var db = new ProducerContext())
            {
                Session obj = db.Sessions.Single<Session>(s => s.Id == session);
                foreach (Job j in obj.Jobs)
                {
                    db.Processes.Remove(j.Process);
                    db.Messages.RemoveRange(j.Messages);
                    db.StagedInputFiles.RemoveRange(j.StagedInputFiles);
                    db.StagedOutputFiles.RemoveRange(j.StagedOutputFiles);
                    db.GeneratorJobs.RemoveRange(db.GeneratorJobs.Where(g => g.JobId == j.Id));
                }
                db.Generators.RemoveRange(db.Generators.Where(g => g.SessionId == obj.Id));
                db.Jobs.RemoveRange(obj.Jobs);
                db.Sessions.Remove(obj);
                db.SaveChanges();
            }
            return count;
        }
        
        public int Submit(Guid session)
        {
            string user = Container.GetAppContext().UserName;
            int num = 0;
            using (ProducerContext db = new ProducerContext())
            {
                Session obj = db.Sessions.Single<Session>(s => s.Id == session);
                Debug.WriteLine(String.Format("start session {0}, jobs {1}", session, obj.Jobs.Count()), this.GetType().Name);
                foreach (var job in obj.Jobs.Where(s => s.State == "create" & s.User.Name == user)) {
                    num += 1;
                    job.State = "submit";
                    job.Submit = DateTime.UtcNow;
                }
                db.SaveChanges();
            }
            return num;
        }

        /// <summary>
        /// Ignore concurrency exception
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int Cancel(Guid session)
        {
            int num = 0;
            using (ProducerContext db = new ProducerContext())
            {
                Session obj = db.Sessions.First<Session>(s => s.Id == session);
                /*
                foreach (var job in obj.Jobs.Where(s => s.State == "create" || s.State == "submit"))
                {
                    IJobProducerContract contract = new ProducerAspenJobContract(job.Id);
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
                 */
                foreach (Job job in obj.Jobs.OrderByDescending(s => s.Submit).Where(s => s.State == "create" || s.State == "submit"))
                {
                    job.State = "cancel";
                    job.Finished = DateTime.UtcNow;
                }
                db.SaveChanges();
            }
            return num;
        }

        public int Terminate(Guid session)
        {
            int num = 0;
            /*
            using (TurbineCompactDatabase container = new TurbineCompactDatabase())
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
             */
            using (ProducerContext db = new ProducerContext())
            {
                Session obj = db.Sessions.Single<Session>(s => s.Id == session);
                foreach (Job job in obj.Jobs.OrderByDescending(s => s.Submit).Where(s => s.State == "setup" || s.State == "running" || s.State == "locked"))
                {
                    try
                    {
                        job.State = "terminate";
                        job.Finished = DateTime.UtcNow;
                        db.SaveChanges();
                        num += 1;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(String.Format("Failed to terminate session {0} job {1} ", session, job.Id),
                            this.GetType());
                    }
                    
                }

                foreach (Job job in obj.Jobs.OrderByDescending(s => s.Submit).Where(s => s.State == "submit" || s.State == "create" || s.State == "pause"))
                {
                    try
                    {
                        job.State = "cancel";
                        db.SaveChanges();
                        num += 1;
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

        /// <summary>
        /// Concurrency errors ignore, but must handle.  Pause in descending order by Id,  to avoid competition with job consumers.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int Pause(Guid session)
        {
            int num = 0;
            using (ProducerContext db = new ProducerContext())
            {
                Session obj = db.Sessions.Single<Session>(s => s.Id == session);
                foreach (var job in obj.Jobs.Where(s => s.State == "submit").OrderByDescending(t => t.Id))
                {
                    try
                    {
                        job.State = "pause";
                        db.SaveChanges();
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

        /// <summary>
        /// Assume that state of jobs cannot be changed during this call.  Therefore
        /// if one of them has the behavior is undefined.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int Unpause(Guid session)
        {
            int num = 0;
            using (ProducerContext db = new ProducerContext())
            {
                Session obj = db.Sessions.Single<Session>(s => s.Id == session);
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
                db.SaveChanges();
            }
            return num;
        }

        public Dictionary<string, int> Status(Guid session)
        {
            var d = new Dictionary<string, int>();
            using (ProducerContext db = new ProducerContext())
            {
                Session obj = db.Sessions.First<Session>(s => s.Id == session);
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


        /// <summary>
        /// Copy Session to a new session with the desired job state.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public Dictionary<string, string> Copy(Guid sessionId, string state)
        {
            ISimulationProducerContract contract = null;

            List<int> jobList = new List<int>();

            Guid sessionIdNew = Guid.NewGuid();
            string owner = Container.GetAppContext().UserName;
            using (var db = new ProducerContext())
            {
                User user = db.Users.Single<User>(s => s.Name == owner);
                var session = new Session()
                {
                    Id = sessionIdNew,
                    User = user,
                    Create = DateTime.UtcNow
                };
                db.Sessions.Add(session);               

                db.SaveChanges();
            }


            using (var db = new ProducerContext())
            {
                var sessionJobs = db.Jobs.Where(j => j.SessionId == sessionId).Where(j => j.State == state);

                Debug.WriteLine("Simulation Name: " + sessionJobs.First().Simulation.Name, "ProducerSessionContract.Copy");

                contract = ProducerSimulationContract.Get(sessionJobs.First().Simulation.Name);


                foreach (var job in sessionJobs)
                {
                    Debug.WriteLine("job: " + job.Count.ToString(), "ProducerSessionContract.Copy");
                    var input = Newtonsoft.Json.Linq.JObject.Parse(job.Process.Input);

                    Dictionary<String, Object> dd = new Dictionary<string, object>();
                    foreach (var v in input)
                    {
                        Debug.WriteLine("VALUE: " + v);
                        Debug.WriteLine("Key: " + v.Key + ", Value: " + v.Value);
                        dd[v.Key] = v.Value;
                    }

                    Debug.WriteLine("Creating jobContract", "ProducerSessionContract.Copy");
                    IJobProducerContract jobContract;
                    jobContract = contract.NewJob(sessionIdNew, job.Initialize, job.Reset, job.Visible);
                    jobContract.Process.Input = dd;

                    Debug.WriteLine("Adding jobContract Count to jobList", "ProducerSessionContract.Copy");
                    jobList.Add(jobContract.Id);
                }
            }

            var d = new Dictionary<string, string>();

            d.Add("NewSessionGuid", sessionIdNew.ToString());
            d.Add("NewJobIds", string.Join(",", jobList.ToArray()));

            return d;
        }

        public Guid CreateGenerator(Guid sessionid)
        {
            Guid generator = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                Debug.WriteLine("Looking for session: " + sessionid.ToString(), "ProducerSessionContract.CreateGenerator");
                //var jobs = db.Jobs.Where<Job>(j => j.SessionId == sessionid);
                Session session = db.Sessions.Single<Session>(s => s.Id == sessionid);
                Debug.WriteLine("Result for session: " + sessionid.ToString(), "ProducerSessionContract.CreateGenerator");
                if (session != null)
                {
                    Debug.WriteLine("Found the session", "ProducerSessionContract.CreateGenerator");
                    db.Generators.Add(new Generator()
                    {
                        Id = generator,
                        SessionId = sessionid,
                        Page = 0,
                        Create = DateTime.UtcNow
                    });
                }
                else
                {
                    Debug.WriteLine(String.Format("InputError: This Session Id doesn't exist"),
                            this.GetType());
                    return Guid.Empty;
                }
                db.SaveChanges();
            }
            return generator;
        }


        public int CreateResultPage(Guid sessionid, Guid generatorid)
        {
            int page = 0;

            int limitPerResult = 1000;

            List<Turbine.Data.Entities.Job> jobsList = null;
            using (var db = new ProducerContext())
            {
                var generator = db.Generators.Single(g => g.Id == generatorid);

                var generatorJobList = db.GeneratorJobs.Where(g => g.Id == generatorid);
                
                //var jobs = db.Jobs.OrderBy(j => j.Finished).Where(j => j.SessionId == sessionid)
                  //  .Where(j => j.State == "success");

                var numOfNotActiveJobs = db.Jobs.Where(j => j.SessionId == sessionid)
                    .Where(j => j.State == "create" || j.State == "pause").Count();
                if (numOfNotActiveJobs != 0)
                {
                    return -2;
                }

                var jobs = (from j in db.Jobs
                           join g in db.GeneratorJobs.Where(y => y.GeneratorId == generatorid) on j.Id equals g.JobId into joinedtable
                           from x in joinedtable.DefaultIfEmpty()
                           where x.JobId == null && j.SessionId == sessionid && (j.State == "success" || j.State == "error" || j.State == "terminate")
                           orderby j.Finished
                           select j).Take(limitPerResult);

                page = generator.Page;

                if (generator.lastFinished == null)
                {
                    if (jobs.Count() == 0)
                    {                        
                        return -1;
                    }
                }
                else
                {
                    jobs = jobs.Where(j => j.Finished >= generator.lastFinished);
                }

                page = generator.Page + 1;

                foreach (var job in jobs)
                {
                    db.GeneratorJobs.Add(new GeneratorJob
                    {
                        Id = Guid.NewGuid(),
                        Page = page,
                        Generator = generator,
                        Job = job,
                    });

                    generator.Page = page;
                }
                
                db.SaveChanges();
            }
            return page;
        }
    }
}
