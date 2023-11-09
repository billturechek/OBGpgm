using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class Team
    {
        public Team()
        {
            Payouts = new HashSet<Payout>();
            Players = new HashSet<Player>();
        }

        public int TeamId { get; set; }
        public int SessionId { get; set; }
        public int Division { get; set; }
        public int TeamNumber { get; set; }
        public string? TeamName { get; set; }
        public int TeamPoints { get; set; }
        public bool IsChampion { get; set; }
        public bool? IsRunnerUp { get; set; }

        public virtual Session Session { get; set; } = null!;
        public virtual ICollection<Payout> Payouts { get; set; }
        public virtual ICollection<Player> Players { get; set; }
    }
}
