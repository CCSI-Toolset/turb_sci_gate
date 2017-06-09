using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;


namespace Turbine.DataEF6.Contract
{
    public partial class ProcessContract
    {
        public static IProcess Get(Guid id)
        {
            using (ProducerContext db = new ProducerContext())
            {
                var entity = db.Processes.Single(p => p.Id == id);
                return new ProcessContract() { id = id };
            }
        }

        public static IProcess New(int jobid)
        {
            Guid id = Guid.Empty;
            using (ProducerContext db = new ProducerContext())
            {
                var entity = new Turbine.Data.Entities.Process() { };
                entity.Job = db.Jobs.Single<Turbine.Data.Entities.Job>(s => s.Count == jobid);
                db.Processes.Add(entity);
                id = entity.Id;
                db.SaveChanges();
            }
            return new ProcessContract() { id = id };
        }
    }
}
