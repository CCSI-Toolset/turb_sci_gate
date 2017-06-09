using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Turbine.Data.Entities
{
    public class JobConsumer
    {
        public JobConsumer()
        {
            Jobs = new List<Job>();
        }

        public Guid Id { get; set; }
        public string hostname { get; set; }
        public int processId { get; set; }

        [Required]
        public string status { get; set; }

        [Required]
        public DateTime keepalive { get; set; }

        /**
         * http://stackoverflow.com/questions/6038541/ef-validation-failing-on-update-when-using-lazy-loaded-required-properties
         * To require a virtual need to specify the FK and make it Required. 
         */
        [Required]
        public string Application_Name { get; set; }
        [ForeignKey("Application_Name")]
        public virtual Application Application { get; set; }

        public virtual List<Job> Jobs { get; set; }
    }
}
