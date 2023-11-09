using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBGpgm.Models;


namespace OBGpgm.Interfaces

{
    public interface ISessionRepository
    {
        IEnumerable<Session> SelectAll();
        List<string> SelectByYears();
        List<string> SelectAllSeasons(string year);
        Session SelectById(int id);
        Session SelectByCurrent();
        Session SelectBySeason(string year, string season);
        int Insert(Session model);
        void Update(Session model);
        bool ResetCurrent();
        bool SetCurrent(int id);
        void Delete(int id);
    }
}
