using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.POCO.Domain
{
    /*
     * Consumers
=========
[('guid', 'uuid', None, None, None, None, 'NO'),
 ('hostname', 'character varying', None, None, None, None, 'NO'),
 ('memory', 'integer', None, 32, 2, 0, 'YES'),
 ('cores', 'integer', None, 32, 2, 0, 'YES'),
 ('cpu', 'character varying', None, None, None, None, 'YES'),
 ('ami', 'character varying', None, None, None, None, 'YES'),
 ('image', 'character varying', None, None, None, None, 'YES'),
 ('instance', 'character varying', None, None, None, None, 'YES'),
 ('status', 'character varying', None, None, None, None, 'NO')]
     * 
     */
    public class JobConsumer
    {
        public virtual Guid Id { get; set; }
        public virtual string hostname { get; set; }
        public virtual Int32 memory { get; set; }
        public virtual Int32 cores { get; set; }
        public virtual string cpu { get; set; }
        public virtual string ami { get; set; }
        public virtual string image { get; set; }
        public virtual string instance { get; set; }
        public virtual string status { get; set; }
    }
}
