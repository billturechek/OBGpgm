using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OBGpgm.Models
{
    public class Comment
    {
        [Required]
        public int commentId { get; set; }
        [Required]
        public int authorId { get; set; }
        [Required]
        public int articleId { get; set; }
        [Required, EmailAddress]
        public string email { get; set; }
        [Required]
        public string itemBody { get; set; }
        [Required]
        public DateTime pubDate { get; set; }
        public string renderContent()
        {
            return itemBody;
        }
        [NotMapped]
        public bool hasbeenFound { get; set; }
    }
}
