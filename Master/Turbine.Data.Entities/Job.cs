using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class Job
    {

        public Job()
        {
            Messages = new List<Message>();
            StagedInputFiles = new List<StagedInputFile>();
            StagedOutputFiles = new List<StagedOutputFile>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Count { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string State { get; set; }
        [Required]
        public DateTime Create { get; set; }
        public DateTime? Submit { get; set; }
        public DateTime? Setup { get; set; }
        public DateTime? Running { get; set; }
        public DateTime? Finished { get; set; }

        [Required]
        public bool Initialize { get; set; }
        [Required]
        public bool Reset { get; set; }
        [Required]
        public bool Visible { get; set; }

        // 1 to 0 or 1
        public virtual Process Process { get; set; }
        public virtual ICollection<Message> Messages { get; set; }

        [ForeignKey("Session"), Required]
        public Guid SessionId { get; set; }
        public virtual Session Session { get; set; }

        [ForeignKey("User"), Required]
        public string UserName { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("Consumer")]
        public Guid? ConsumerId { get; set; }
        public virtual JobConsumer Consumer { get; set; }

        public virtual ICollection<StagedInputFile> StagedInputFiles { get; set; }
        public virtual ICollection<StagedOutputFile> StagedOutputFiles { get; set; }

        //public virtual List<GeneratorJob> GeneratorJobs { get; set; }

        [ForeignKey("Simulation"), Required]
        public Guid SimulationId { get; set; }
        public virtual Simulation Simulation { get; set; }

    }
}
