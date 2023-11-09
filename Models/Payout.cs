using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class Payout
    {
        public int PayoutId { get; set; }
        public int SessionId { get; set; }
        public int TeamId { get; set; }
        public int Players { get; set; }
        public int CaptainId { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public int Player3Id { get; set; }
        public int Player4Id { get; set; }
        public decimal TotalPayout { get; set; }
        public decimal Individual { get; set; }

        public virtual Session Session { get; set; } = null!;
        public virtual Team Team { get; set; } = null!;
    }
}
