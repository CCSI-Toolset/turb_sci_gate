using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class ProcessError
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Error { get; set; }
        public string Type { get; set; }

        [Required, ForeignKey("Process")]
        public Guid ProcessId { get; set; }
        public virtual Process Process { get; set; }

    }
}
