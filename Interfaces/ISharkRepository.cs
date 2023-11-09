using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Interfaces
{
    public interface ISharkRepository
    {
        List<Shark> SelectAll();
        List<Shark> SelectAllBySession(string year, string season);
        Shark SelectByID(int id);
        int SelectHighBySession(int id);
        List<Shark> SelectAllHighBySession(int id);
        int Insert(Shark shark);
        void Update(Shark shark);
        void Delete(int id);
    }
}
