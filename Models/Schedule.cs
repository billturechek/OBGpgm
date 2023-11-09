using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class Schedule
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int Teams { get; set; }
        public int Week { get; set; }
        public int TimeSlot { get; set; }
        public int TableGroup { get; set; }
        public int HomeTeam { get; set; }
        public int VisitingTeam { get; set; }
        public string? Note { get; set; }
    }
}
