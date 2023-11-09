using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class Shark
    {
        public int SharkId { get; set; }
        public int SessionId { get; set; }
        public int PlayerId { get; set; }
        public int? MemberId { get; set; }
        public DateTime SharkDate { get; set; }
        public SharkType SharkType { get; set; }
        public int TeamId { get; set; }
        public int Points { get; set; }

        public virtual Player Player { get; set; } = null!;
        public virtual Session Session { get; set; } = null!;
    }
}
