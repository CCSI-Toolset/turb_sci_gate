using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.POCO.Domain
{
    /*
     *Users
[('id', 'integer', None, 32, 2, 0, 'NO'),
 ('name', 'character varying', None, None, None, None, 'NO'),
 ('token', 'character varying', None, None, None, None, 'NO')]
     *
     */
    public class User
    {
        public virtual int Id { get; set; }
        public virtual string name { get; set; }
        public virtual string token { get; set; }
    }
}
