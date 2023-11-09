using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class Photo
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public byte[] PhotoData { get; set; } = null!;
        public int Owner { get; set; }

        public virtual Member OwnerNavigation { get; set; } = null!;
    }
}
