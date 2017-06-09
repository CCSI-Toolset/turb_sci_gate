using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public DateTime Create { get; set; }

        [Required, ForeignKey("Job")]
        public Guid JobId { get; set; }
        public virtual Job Job { get; set; }
    }
}
