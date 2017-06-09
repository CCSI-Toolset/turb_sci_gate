using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class SimulationStagedInput
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        [MaxLength]
        public byte[] Content { get; set; }
        public string Hash { get; set; }

        [Required, ForeignKey("Simulation")]
        public Guid SimulationId { get; set; }
        public virtual Simulation Simulation { get; set; }
        /*
        [ForeignKey("InputFileType")]
        public Guid InputFileTypeId { get; set; }
        public virtual InputFileType InputFileType { get; set; }
         */
    }
}