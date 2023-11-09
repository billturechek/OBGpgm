using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OBGpgm.Data;
using OBGpgm.Models;

namespace OBGpgm.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly ObgDbContext db = null;
        public ArticleRepository(ObgDbContext db)
        {
            this.db = db;
        }

        public Article SelectByID(int id)
        {
            Article article = db.Articles.FromSqlRaw("SELECT * FROM Article WHERE articleId ={0}", id).SingleOrDefault();
            return article;
        }

        public List<Article> SelectAll()
        {
            List<Article> data = db.Articles.OrderByDescending(a => a.pubDate).ToList();
            return data;
        }

        public List<Article> SelectAllById(int id)
        {
            List<Article> data = db.Articles
                .Where(a => (a.authId==id))
                .OrderByDescending(a => a.pubDate).ToList();
            return data;
        }

        public List<Article> SelectAllByCategory(int cat)
        {
            List<Article> data = db.Articles
                .Where(a => a.category == cat && a.topic == 1)
                .OrderByDescending(a => a.pubDate).ToList();
            return data;
        }

        public List<Article> SelectAllByTopic(int top)
        {
            List<Article> data = db.Articles
                .Where(a => a.topic == top)
                .OrderByDescending(a => a.pubDate).ToList();
            return data;
        }
        public List<Article> SelectAllByLost()
        {
            List<Article> data = db.Articles
                .Where(a => a.topic == 4)
                .OrderByDescending(a => a.topItem)
                .ThenByDescending(a => a.pubDate).ToList();
            return data;
        }

        public List<Article> SelectTop3()
        {
            List<Article> data = db.Articles
                .Distinct()
                .Where(a => a.category == 1 && a.topic == 1)
                .OrderByDescending(a => a.pubDate)
                .Take(3).ToList();
            return data;
        }

        public int Insert(Article article)
        {
            /*
            int count = db.Database.ExecuteSqlRaw("INSERT INTO Article(" +
                 "authId, category, topic, title, slug, itemBody, " +
                 "topItem, pubDate, lastModified, isPublished) VALUES(" +
                 "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9})",
                 article.authId, article.category, article.topic, article.title, article.slug, 
                 article.itemBody, article.topItem, article.pubDate, article.lastModified, article.isPublished);
                 */
                db.Articles.Add(article);
                db.SaveChanges();
                int id = article.articleId; // Yes it's here
            return id;
        }
        public void Update(Article article)
        {
            int count = db.Database.ExecuteSqlRaw("UPDATE Article SET " +
                 "authId = {0}, category = {1}, topic = {2}, title = {3}, slug = {4}, itemBody = {5}, " +
                 "topItem = {6}, pubDate = {7}, lastModified = {8}, isPublished = {9} WHERE articleId = {10}",
                 article.authId, article.category, article.topic, article.title, article.slug, article.itemBody, 
                 article.topItem, article.pubDate, article.lastModified, article.isPublished, article.articleId);
        }
        public void Delete(int id)
        {
            int count = db.Database.ExecuteSqlRaw("DELETE FROM Article WHERE articleId = {0}", id);
        }
    }
}
