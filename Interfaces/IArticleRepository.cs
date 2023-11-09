using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OBGpgm.Models;

namespace OBGpgm.Repositories
{
    public interface IArticleRepository 
    {
        Article SelectByID(int id);
        List<Article> SelectAll();
        List<Article> SelectAllByCategory(int cat);
        List<Article> SelectAllById(int id);
        List<Article> SelectAllByTopic(int top);
        List<Article> SelectAllByLost();
        List<Article> SelectTop3();
        int Insert(Article article);
        void Update(Article article);
        void Delete(int id);
    }
}
