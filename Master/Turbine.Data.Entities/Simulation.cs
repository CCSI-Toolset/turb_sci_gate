using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class Simulation
    {
        //public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Count { get; set; }

        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public DateTime Update { get; set; }
        public DateTime Create { get; set; }

        [Required, ForeignKey("User")]
        public string UserName { get; set; }
        public virtual User User { get; set; }
        
        public virtual List<SimulationStagedInput> SimulationStagedInputs { get; set; }
        /*
        public virtual List<Job> Jobs { get; set; }
         */
        [Required, ForeignKey("Application")]
        public string ApplicationName { get; set; }
        public virtual Application Application { get; set; }

    }
}
