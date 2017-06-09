using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turbine.Data.Entities
{
    public class GeneratorJob
    {
        [Key]
        public Guid Id { get; set; }

        public int Page { get; set; }

        [ForeignKey("Generator")]
        public Guid? GeneratorId { get; set; }
        public virtual Generator Generator { get; set; }

        [ForeignKey("Job")]
        public Guid? JobId { get; set; }
        public virtual Job Job { get; set; }
    }
}
