using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data.Contract.Behaviors;
using Turbine.Data.Contract;
using Turbine.Common;
using Turbine.Data.POCO.Domain;
using NHibernate.Criterion;
using Turbine.NHData.Contract;


namespace Turbine.Consumer.NHData.Contract
{
    public class JobConsumerContract : IJobConsumerContract
    {
        private int id;

        public JobConsumerContract(int id)
        {
            this.id = id;
        }

        public int Id
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public byte[] GetSimulationConfiguration()
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var job = session
                                       .CreateCriteria<Job>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<Job>();
                    return job.simulation.configuration;
                }
            }
        }

        public byte[] GetSimulationBackup()
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var job = session
                                       .CreateCriteria<Job>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<Job>();
                    return job.simulation.backup;
                }
            }
        }

        public Dictionary<String, Object> GetSimulationDefaults()
        {
            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var job = session
                                       .CreateCriteria<Job>()
                                       .Add(Restrictions.Eq("Id", this.id))
                                       .UniqueResult<Job>();
                    String data = System.Text.Encoding.ASCII.GetString(job.simulation.defaults);
                    return JsonConvert.DeserializeObject<Dictionary<string, Object>>(data);
                }
            }
        }


        public void Message(string msg)
        {
            throw new NotImplementedException();
        }

        public IProcess Setup()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public void Running()
        {
            throw new NotImplementedException();
        }

        public void Error(string msg)
        {
            throw new NotImplementedException();
        }

        public void Success()
        {
            throw new NotImplementedException();
        }

        public void Warning(string p)
        {
            throw new NotImplementedException();
        }

        public IProcess Process
        {
            get { throw new NotImplementedException(); }
        }
    }
}
