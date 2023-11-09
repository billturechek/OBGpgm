using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class ScoreSheet
    {
        public int SsSessionId { get; set; }
        public int SsDivision { get; set; }
        /// <summary>
        /// Week Number
        /// </summary>
        public int SsWeek { get; set; }
        /// <summary>
        /// Team Number
        /// </summary>
        public int SsHteam { get; set; }
        /// <summary>
        /// Team Number
        /// </summary>
        public int SsVteam { get; set; }
        public int SsHpoints { get; set; }
        public int SsVpoints { get; set; }
        /// <summary>
        /// Date of match
        /// </summary>
        public DateTime? SsDate { get; set; }

        public virtual Session SsSession { get; set; } = null!;
    }
}
