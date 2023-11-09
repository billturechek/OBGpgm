using System;
using System.Collections.Generic;
using OBGpgm.Models;

namespace OBGpgm.Models
{
    public partial class Session
    {
        public Session()
        {
            Drafts = new HashSet<Draft>();
            Payouts = new HashSet<Payout>();
            Players = new HashSet<Player>();
            Ptlogs = new HashSet<Ptlog>();
            ScoreSheets = new HashSet<ScoreSheet>();
            Sharks = new HashSet<Shark>();
            Teams = new HashSet<Team>();
        }

        public int SessionId { get; set; }
        public string Year { get; set; } = null!;
        public string Season { get; set; } = null!;
        
        //public Seasons Season { get; set; } = null!; 
        public int TeamsD1 { get; set; }
        public int TeamsD2 { get; set; }
        public string? StartDate { get; set; }
        public string CurrentWeek { get; set; } = null!;
        public bool CurrentSeason { get; set; }
        public int President { get; set; }
        public int VicePresident { get; set; }
        public int Secretary { get; set; }
        public int Treasurer { get; set; }
        public int SecondVp1 { get; set; }
        public int SecondVp2 { get; set; }
        public int? SecondVp3 { get; set; }
        public int? SecondVp4 { get; set; }
        public int DraftType { get; set; }

        public virtual ICollection<Draft> Drafts { get; set; }
        public virtual ICollection<Payout> Payouts { get; set; }
        public virtual ICollection<Player> Players { get; set; }
        public virtual ICollection<Ptlog> Ptlogs { get; set; }
        public virtual ICollection<ScoreSheet> ScoreSheets { get; set; }
        public virtual ICollection<Shark> Sharks { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
    }
    
    public enum snType
    {
        Spring = 1,
        Summer = 2,
        Fall = 3,
        Winter = 4
    }
}
