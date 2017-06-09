using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turbine.Data.Entities
{
    public class Generator
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime Create { get; set; }

        public int Page { get; set; }

        public virtual DateTime? lastFinished { get; set; }

        //[ForeignKey("Session"), Required]
        public Guid SessionId { get; set; }
        public virtual Session Session { get; set; }

        //public virtual List<GeneratorJob> GeneratorJobs { get; set; }
    }
}
