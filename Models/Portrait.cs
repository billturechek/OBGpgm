using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing;

namespace OBGpgm.Models
{
    public partial class Portrait
    {
        public Portrait()
        {
            Members = new HashSet<Member>();
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public byte[] LargeImage { get; set; } = null!;
        public byte[]? ThumbImage { get; set; }
        public string? Notes { get; set; }
        public int Memberid { get; set; }

        public virtual Member Member { get; set; } = null!;
        public virtual ICollection<Member> Members { get; set; }
        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile ImageFile { get; set; }

        internal static Image FromStream(Stream stream, bool v1, bool v2)
        {
            throw new NotImplementedException();
        }
    }
}
