using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    /// <summary>
    /// 1 Job to 0 or 1 Process
    /// </summary>
    public class Process
    {
        public Process()
        {
            Id = Guid.NewGuid();
            Error = new List<ProcessError>();
        }
        [Key,ForeignKey("Job")]
        public Guid Id { get; set; }
        public virtual Job Job { get; set; }

        public int Status { get; set; }
        public string Stdout { get; set; }
        public string Stdin { get; set; }
        public string Stderr { get; set; }
        public string WorkingDir { get; set; }
        [MaxLength]
        public string Input { get; set; }
        [MaxLength]
        public string Output { get; set; }

        public virtual ICollection<ProcessError> Error { get; set; }
    }
}
