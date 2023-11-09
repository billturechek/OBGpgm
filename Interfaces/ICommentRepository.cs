using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OBGpgm.Models;

namespace OBGpgm.Repositories
{
    public interface ICommentRepository
    {
        Comment SelectByID(int id);
        List<Comment> SelectAll();
        List<Comment> SelectAllByArticle(int aid);
        void Insert(Comment comment);
        void Update(Comment comment);
        void Delete(int id);
    }
}
