using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Interfaces
{
    public interface IScoreSheetRepository
    {
        List<ScoreSheet> SelectAll();
        List<ScoreSheet> SelectAllBySession(string year, string season);
        List<ScoreSheet> SelectAllByWeek(int sid, int wid);
        List<ScoreSheet> SelectFirstByWeek(int sid);
        ScoreSheet SelectByID(int id, int week, int team);
        void Insert(ScoreSheet scoreSheet);
        void Update(ScoreSheet scoreSheet);
        void Delete(int id, int week, int team);
    }
}
