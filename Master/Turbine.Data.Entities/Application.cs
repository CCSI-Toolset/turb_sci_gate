using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class Application
    {
        [Key]
        public string Name { get; set; }
        public string Version { get; set; }

        public virtual List<OutputFileType> OutputFileTypes { get; set; }
        public virtual List<InputFileType> InputFileTypes { get; set; }
        public virtual List<Simulation> Simulations { get; set; }
    }
}
