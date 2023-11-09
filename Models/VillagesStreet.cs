using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class VillagesStreet
    {
        public string? Prefix { get; set; }
        public string? StreetName { get; set; }
        public string? Location { get; set; }
        public double? District { get; set; }
        public string? County { get; set; }
        public string? Type { get; set; }
    }
}
