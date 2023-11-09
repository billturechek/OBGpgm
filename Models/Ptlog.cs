using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class Ptlog
    {
        public int Ptlid { get; set; }
        public int? Ptltype { get; set; }
        public DateTime? PtlDate { get; set; }
        public int? Ptlsession { get; set; }
        public int? Ptlmember { get; set; }
        public int? Ptlplayer { get; set; }
        public int? Ptlteam { get; set; }

        public virtual Player? PtlplayerNavigation { get; set; }
        public virtual Session? PtlsessionNavigation { get; set; }
    }
}
