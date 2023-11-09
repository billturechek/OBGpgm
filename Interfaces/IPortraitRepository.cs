using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Interfaces
{
    public interface IPortraitRepository
    {
        List<Portrait> SelectAll();
        Portrait SelectByID(int id);
        List<Portrait> SelectAllByDeceased();
        List<Portrait> SelectAllByLiving();
        int Insert(Portrait portrait);
        void Update(Portrait portrait);
        void Delete(int id);
    }
}
