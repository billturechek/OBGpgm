using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OBGpgm.Models;

namespace OBGpgm.ViewModels
{
    public class ScheduleViewModel
    {
        public IEnumerable<Schedule> DataList { get; set; }
        public string Note { get; set; }  
        public string Sid { get; set; }
        public string Teams { get; set; }
        public string Week { get; set; }   

    }
}


    