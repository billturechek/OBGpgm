using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Models
{
    public class CreateSessionViewModel
    {
        public Session Session { get; set; }
        public Session csession { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime sDate { get; set; }
        public int aCount { get; set; }
        public int teamCount { get; set; }
    }
}
