using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.POCO.Domain
{
    /*
     * Jobs
====
[('id', 'integer', None, 32, 2, 0, 'NO'),
 ('state', 'character varying', None, None, None, None, 'NO'),
 ('Create', 'timestamp without time zone', None, None, None, None, 'NO'),
 ('submit', 'timestamp without time zone', None, None, None, None, 'YES'),
 ('setup', 'timestamp without time zone', None, None, None, None, 'YES'),
 ('running', 'timestamp without time zone', None, None, None, None, 'YES'),
 ('finished', 'timestamp without time zone', None, None, None, None, 'YES'),
 ('simulationid', 'integer', None, 32, 2, 0, 'NO'),
 ('userid', 'integer', None, 32, 2, 0, 'NO'),
 ('sessionguid', 'uuid', None, None, None, None, 'NO'),
 ('jobconsumerguid', 'uuid', None, None, None, None, 'YES'),
 ('processid', 'integer', None, 32, 2, 0, 'NO')]
     * 
     */
    public class Job
    {
        public virtual int Id { get; set; }
        public virtual string state { get; set; }
        public virtual DateTime create { get; set; }
        public virtual DateTime submit { get; set; }
        public virtual DateTime setup { get; set; }
        public virtual DateTime running { get; set; }
        public virtual DateTime finished { get; set; }
        public virtual Simulation simulation { get; set; }
        public virtual User user { get; set; }
        public virtual Session session { get; set; }
        public virtual JobConsumer consumer { get; set; }
        public virtual SinterProcess process { get; set; }
        public virtual Boolean initialize { get; set; }
    }

}
