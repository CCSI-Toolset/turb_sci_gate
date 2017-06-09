using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class StagedInputFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte[] Content { get; set; }

        [Required, ForeignKey("Job")]
        public Guid jobId { get; set; }
        public virtual Job Job { get; set; }

        [Required, ForeignKey("InputFileType")]
        public Guid InputFileTypeId { get; set; }
        public virtual InputFileType InputFileType { get; set; }

    }
}
