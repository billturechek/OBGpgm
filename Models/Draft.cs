using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class Draft
    {
        public Draft()
        {
            Players = new HashSet<Player>();
        }

        public int DraftId { get; set; }
        public DraftTypes DraftType { get; set; }
        public int DraftSessionId { get; set; }
        public int DraftTeamId { get; set; }
        public int DraftPlayerId { get; set; }
        public int DraftRound { get; set; }
        public int DraftPosition { get; set; }
        public int DraftSelection { get; set; }
        public int DraftDivision { get; set; }
        public bool DraftPreDraft { get; set; }

        public virtual Session DraftSession { get; set; } = null!;
        public virtual ICollection<Player> Players { get; set; }
    }
}
