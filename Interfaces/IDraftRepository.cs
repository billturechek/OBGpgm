using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Interfaces
{
    public interface IDraftRepository
    {
        List<Draft> SelectAll();
        List<Draft> SelectAllBySession(string year, string season);
        List<Draft> SelectAllBySelection(int session);
        Draft SelectByID(int id);
        int Insert(Draft draft);
        void Update(Draft draft);
        void Delete(int id);
    }
}
