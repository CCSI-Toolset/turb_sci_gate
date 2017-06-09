using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Turbine.Data.Entities
{
    public class Session
    {
        public Guid Id { get; set; }
        public DateTime Create { get; set; }

        public DateTime? Submit { get; set; }
        public DateTime? Finished { get; set; }
        public string Description { get; set; }

        public virtual User User { get; set; }
        public virtual List<Job> Jobs { get; set; }
        public virtual List<Generator> Generators { get; set; }
    }
}
