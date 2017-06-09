using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Newtonsoft.Json;
using Turbine.Data.POCO.Domain;
using NHibernate.Criterion;


namespace Turbine.NHData.Contract
{
    public class SessionFactory
    {
        private static NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        private static NHibernate.ISessionFactory sessionFactory;

        static SessionFactory()
        {
            config.Configure();
            config.AddAssembly(typeof(Job).Assembly);
            sessionFactory = config.BuildSessionFactory();
        }

        public static NHibernate.ISession OpenSession() 
        {
            return sessionFactory.OpenSession();
        }
    }

    public class SinterProcessContract : IProcess
    {
        private int id;

        public static IProcess Get(int id) 
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var process = session
                                       .CreateCriteria<SinterProcess>()
                                       .Add(Restrictions.Eq("Id", id))
                                       .UniqueResult<SinterProcess>();
                    return new SinterProcessContract() { id = id };
                }
            }
        }

        public static IProcess New(int jobid)
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var job = session
                        .CreateCriteria<Job>().Add(Restrictions.Eq("Id", jobid))
                        .UniqueResult<Job>();

                    var process = new SinterProcess() { job = job };
                    session.Save(process);
                    trans.Commit();
                    return new SinterProcessContract() { id = process.Id };
                }
            }
        }

        private SinterProcessContract() { }

        public void AddStdout(string data)
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var process = session
                                       .CreateCriteria<SinterProcess>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<SinterProcess>();

                    if (process.stdout == null)
                        process.stdout = data;
                    else
                        process.stdout += data;
                    
                    session.Save(process);
                    trans.Commit();
                }
            }
        }

        public void AddStderr(string data)
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var process = session
                                       .CreateCriteria<SinterProcess>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<SinterProcess>();


                    if (process.stderr == null)
                        process.stderr = data;
                    else
                        process.stderr += data;

                    session.Save(process);
                    trans.Commit();
                }
            }
        }

        void SetInput(string data)
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var process = session
                                       .CreateCriteria<SinterProcess>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<SinterProcess>();


                    process.input = data;
                    session.Save(process);
                    trans.Commit();
                }
            }
        }

        void SetOutput(string data)
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var process = session
                                       .CreateCriteria<SinterProcess>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<SinterProcess>();


                    process.output = data;
                    session.Save(process);
                    trans.Commit();
                }
            }
        }

        public void SetStatus(int status)
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var process = session
                                       .CreateCriteria<SinterProcess>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<SinterProcess>();


                    process.status = status;
                    session.Save(process);
                    trans.Commit();
                }
            }
        }

        //Sets the dir in the function to MyDocs, but doesn't actually chdir
        void SetWorkingDirectory(string path)
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var process = session
                                       .CreateCriteria<SinterProcess>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<SinterProcess>();
                    process.workingdir = path;
                    session.Save(process);
                    trans.Commit();
                }
            }
        }

        public IDictionary<String, Object> Input
        {
            get
            {
                Dictionary<String, Object> dict = null;
                using (NHibernate.ISession session = SessionFactory.OpenSession())
                {
                    using (NHibernate.ITransaction trans = session.BeginTransaction())
                    {
                        var process = session
                                           .CreateCriteria<SinterProcess>()
                                           .Add(Restrictions.Eq("Id", this.id))
                                           .UniqueResult<SinterProcess>();
                        dict = JsonConvert.DeserializeObject<Dictionary<string, Object>>(process.input);
                        return dict;
                    }
                }
            }
            set
            {
                String data = JsonConvert.SerializeObject(value);
                //SetInput(System.Text.Encoding.ASCII.GetBytes(data));
                SetInput(data);
            }
        }

        public IDictionary<string, object> Output
        {
            get
            {
                Dictionary<String, Object> dict = null;
                using (NHibernate.ISession session = SessionFactory.OpenSession())
                {
                    using (NHibernate.ITransaction trans = session.BeginTransaction())
                    {
                        var process = session
                                           .CreateCriteria<SinterProcess>()
                                           .Add(Restrictions.Eq("Id", this.id))
                                           .UniqueResult<SinterProcess>();
                        dict = JsonConvert.DeserializeObject<Dictionary<string, Object>>(process.output);
                        return dict;
                    }
                }
            }
            set
            {
                String data = JsonConvert.SerializeObject(value);
                //SetOutput(System.Text.Encoding.ASCII.GetBytes(data));
                SetOutput(data);
            }
        }

        public string WorkingDirectory
        {
            get
            {
                using (NHibernate.ISession session = SessionFactory.OpenSession())
                {
                    using (NHibernate.ITransaction trans = session.BeginTransaction())
                    {
                        var process = session
                                           .CreateCriteria<SinterProcess>()
                                           .Add(Restrictions.Eq("Id", this.id))
                                           .UniqueResult<SinterProcess>();
                        return process.workingdir;
                    }
                }
            }
            set
            {
                SetWorkingDirectory(value);
            }
        }


        private void AddError(string error_type, Dictionary<string, List<string>> dictionary)
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var process = session
                                       .CreateCriteria<SinterProcess>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<SinterProcess>();
                    //ProcessError error;
                    foreach (KeyValuePair<String, List<String>> entry in dictionary)
                    {
                        foreach (String msg in entry.Value)
                        {
                            var error = new ProcessError();
                            error.type = "convergence";
                            error.name = entry.Key;
                            error.msg = msg;
                            process.errors.Add(error);
                        }
                    }
                }
            }
        }

        public void AddConvergenceError(Dictionary<string, List<string>> dictionary)
        {
            AddError("convergence", dictionary);
        }

        public void AddBlockError(Dictionary<string, List<string>> dictionary)
        {
            AddError("block", dictionary);
        }

        public void AddStreamError(Dictionary<string, List<string>> dictionary)
        {
            AddError("stream", dictionary);
        }

        public void AddConvergenceWarning(Dictionary<string, List<string>> dictionary)
        {
            AddConvergenceError(dictionary);
        }

        public void AddBlockWarning(Dictionary<string, List<string>> dictionary)
        {
            AddBlockError(dictionary);
        }

        public void AddStreamWarning(Dictionary<string, List<string>> dictionary)
        {
            AddStreamError(dictionary);
        }

    }
}