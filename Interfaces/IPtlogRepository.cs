using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Interfaces
{
    public interface IPtlogRepository
    {
        List<Ptlog> SelectAll();
        List<Ptlog> SelectAllBySession(string year, string season);
        Ptlog SelectByID(int id);
        int Insert(Ptlog ptlog);
        void Update(Ptlog ptlog);
        void Delete(int id);
    }
}
