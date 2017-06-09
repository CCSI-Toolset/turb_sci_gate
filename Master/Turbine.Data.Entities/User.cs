using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class User
    {
        [Key]
        public string Name { get; set; }
        public string Token { get; set; }

        public virtual List<Application> Applications { get; set; }
        public virtual List<Job> Jobs { get; set; }
        public virtual List<Session> Sessions { get; set; }
        public virtual List<Simulation> Simulations { get; set; }
    }
}
