using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OBGpgm.Data;
using OBGpgm.Models;

namespace OBGpgm.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ObgDbContext db = null;
        public CommentRepository(ObgDbContext db)
        {
            this.db = db;
        }

        public Comment SelectByID(int id)
        {
            Comment comment = db.Comments.FromSqlRaw("SELECT * FROM Comment WHERE commentId ={0}", id).SingleOrDefault();
            return comment;
        }

        public List<Comment> SelectAll()
        {
            List<Comment> data = db.Comments.ToList();
            return data;
        }

        public List<Comment> SelectAllByArticle(int id)
        {
            List<Comment> data = db.Comments
                .Where(a => a.articleId==id)
                .OrderByDescending(a => a.pubDate)
                .ToList();
            return data;
        }

        public void Insert(Comment comment)
        {
            int count = db.Database.ExecuteSqlRaw("INSERT INTO Comment(" +
                 "authorId, articleId, email, itemBody, pubDate) VALUES(" +
                 "{0},{1},{2},{3},{4})",
                 comment.authorId, comment.articleId, comment.email, comment.itemBody, comment.pubDate);
        }
        public void Update(Comment comment)
        {
            int count = db.Database.ExecuteSqlRaw("UPDATE Comment SET " +
                 "authorId = {0}, articleId = {1}, email = {2}, itemBody = {3}, " +
                 "pubDate = {4} WHERE commentId = {5}",
                 comment.authorId, comment.articleId, comment.email, comment.itemBody, comment.pubDate, comment.commentId);
        }
        public void Delete(int id)
        {
            int count = db.Database.ExecuteSqlRaw("DELETE FROM Comment WHERE commentId = {0}", id);
        }
    }
}
