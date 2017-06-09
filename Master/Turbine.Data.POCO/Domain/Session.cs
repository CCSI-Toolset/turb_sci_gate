using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.POCO.Domain
{
    /*
     * 
     * Sessions
========
[('guid', 'uuid', None, None, None, None, 'NO'),
 ('Create', 'timestamp without time zone', None, None, None, None, 'NO'),
 ('submit', 'timestamp without time zone', None, None, None, None, 'YES'),
 ('finished', 'timestamp without time zone', None, None, None, None, 'YES'),
 ('userid', 'integer', None, 32, 2, 0, 'NO')]
     */
    public class Session
    {
        public virtual Guid Id { get; set; }
        public virtual DateTime create { get; set; }
        public virtual DateTime submit { get; set; }
        public virtual DateTime finished { get; set; }
        public virtual User user { get; set; }
    }
}
