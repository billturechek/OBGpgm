using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OBGpgm.Models;

namespace OBGpgm.Models
{
    public class DraftViewModel
    {
        public Draft draft { get; set; }  
        public Player player { get; set; }
        public Team team { get; set; }
        public Member member { get; set; }
        public bool pre { get; set; }
        public bool retn { get; set; }

    }
}


    