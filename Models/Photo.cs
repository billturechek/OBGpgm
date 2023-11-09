using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.IO;

namespace OBGpgm.Models
{
    public partial class Photo
    {
        public int id { get; set; }
        public int articleId { get; set; }  
        public int memberId { get; set; }   
        public string caption { get; set; } 
        public string notes { get; set; }   
        public byte[] largeImage { get; set; } = null!;
        public byte[] thumbImage { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }

        //public virtual Member OwnerNavigation { get; set; } = null!;
        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile ImageFile { get; set; }

        internal static Photo FromStream(Stream stream, bool v1, bool v2)
        {
            throw new NotImplementedException();
        }
    }
}
