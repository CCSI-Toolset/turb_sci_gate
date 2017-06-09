using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class InputFileType
    {
        public InputFileType()
        {
            StagedInputFiles = new List<StagedInputFile>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }

        //[Required, ForeignKey("Application")]
        [ForeignKey("Application")]
        public string ApplicationName { get; set; }
        public virtual Application Application { get; set; }

        public virtual ICollection<StagedInputFile> StagedInputFiles { get; set; }
    }
}
