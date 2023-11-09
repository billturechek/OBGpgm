using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class Player
    {
        public Player()
        {
            Ptlogs = new HashSet<Ptlog>();
            Sharks = new HashSet<Shark>();
        }

        public int PlayerId { get; set; }
        public int SessionId { get; set; }
        public int TeamId { get; set; }
        public int MemberId { get; set; }
        public int DraftId { get; set; }
        public string StartWeek { get; set; } = null!;
        public string EndWeek { get; set; } = null!;
        public bool IsPlaying { get; set; }
        public bool IsCaptain { get; set; }
        public string SkillLevel { get; set; } = null!;
        public string? DraftRound { get; set; }
        public bool IsInDraft { get; set; }
        public bool? IsBeingTraded { get; set; }

        public virtual Draft Draft { get; set; } = null!;
        public virtual Member Member { get; set; } = null!;
        public virtual Session Session { get; set; } = null!;
        public virtual Team Team { get; set; } = null!;
        public virtual ICollection<Ptlog> Ptlogs { get; set; }
        public virtual ICollection<Shark> Sharks { get; set; }
    }
}
