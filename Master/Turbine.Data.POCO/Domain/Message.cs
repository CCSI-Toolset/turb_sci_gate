using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.POCO.Domain
{
    /*
     * Messages
========
[('id', 'integer', None, 32, 2, 0, 'NO'),
 ('value', 'character varying', None, None, None, None, 'NO'),
 ('jobid', 'integer', None, 32, 2, 0, 'NO')]
     * 
     */
    public class Message
    {
        public virtual int Id { get; set; }
        public virtual string value { get; set; }
        public virtual Job job { get; set; }
    }
}
