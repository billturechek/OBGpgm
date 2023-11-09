using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OBGpgm.Models;

namespace OBGpgm.Models
{
    public class MemberViewModel
    {
        public Member Member { get; set; }
        public string fState { get; set; }  
        public string hState { get; set; }
        public string streetName { get; set; }
    }
}
