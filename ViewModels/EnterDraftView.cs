using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Models
{
    public class EnterDraftView
    {
        public Draft Draft { get; set; }
        public Team Team { get; set; }
        public Player Player { get; set; }
        public Member Member { get; set; }
        public bool pre { get; set; }
        public bool retn { get; set; }
    }
}
