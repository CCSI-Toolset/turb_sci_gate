using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.POCO.Domain
{
    /*
     * 
     * [('id', 'integer', None, 32, 2, 0, 'NO'),
 ('name', 'character varying', None, None, None, None, 'NO'),
 ('configuration', 'character varying', None, None, None, None, 'YES'),
 ('backup', 'character varying', None, None, None, None, 'YES'),
 ('defaults', 'character varying', None, None, None, None, 'YES'),
 ('userid', 'integer', None, 32, 2, 0, 'NO')]
     */
    public class Simulation
    {
        public virtual int Id { get; set; }
        public virtual string name { get; set; }
        public virtual byte[] configuration { get; set; }
        public virtual byte[] backup { get; set; }
        public virtual byte[] defaults { get; set; }
        public virtual User user { get; set; }
    }
}
