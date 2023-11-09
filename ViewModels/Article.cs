using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBGpgm.Models
{
    public partial class Article
    {
        [Key]
        [Column("articleId")]
        public int articleId { get; set; }
        [Column("authId")]
        public int authId { get; set; }
        [Column("category")]
        public int category { get; set; }
        [Column("topic")]
        public int topic { get; set; }
        [Required]
        [Column("title")]
        [StringLength(50)]
        public string title { get; set; }
        [Required]
        [Column("slug")]
        [StringLength(500)]
        public string slug { get; set; }
        [Required]
        [Column("itemBody")]
        public string itemBody { get; set; }
        [Column("topItem")]
        public bool topItem { get; set; }
        [Column("pubDate")]
        public DateTime pubDate { get; set; }
        [Column("lastModified")]
        public DateTime lastModified { get; set; }
        [Column("isPublished")]
        public bool isPublished { get; set; }
        [NotMapped]
        public bool hasbeenFound { get; set; }
    }
    public enum articleCategory
    {
        League = 1,
        Member = 2
    }
    public enum articleTopic
    {
        News = 1,
        Editorial = 2,
        Question = 3,
        LostAndFound = 4
    }
}
