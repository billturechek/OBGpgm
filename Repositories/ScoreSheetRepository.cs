using OBGpgm.Data;
using OBGpgm.Models;
using OBGpgm.Interfaces;
using OBGpgm.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OBGpgm.Repositories
{
    public class ScoreSheetRepository : IScoreSheetRepository
    {
        private readonly OBGpgm.Interfaces.ISessionRepository isession;
        private readonly ObgDbContext db = null;
        public ScoreSheetRepository(ObgDbContext db)
        {
            this.db = db;
            this.isession = new SessionRepository(new ObgDbContext());
        }
        public void Delete(int id, int week, int team)
        {
            ScoreSheet ss = (ScoreSheet)db.ScoreSheets
                .Where(ss => ss.SsSessionId == id && ss.SsWeek == week && ss.SsHteam == team); 
            db.ScoreSheets.Remove(ss);
            db.SaveChanges();

        }

        public void Insert(ScoreSheet scoreSheet)
        {
            db.ScoreSheets.Add(scoreSheet);
            db.SaveChanges();
            return;
        }

        public List<ScoreSheet> SelectAll()
        {
            List<ScoreSheet> data = db.ScoreSheets
                .OrderByDescending(s => s.SsSessionId)
                .ThenByDescending(s => s.SsWeek)
                .ThenBy(s => s.SsHteam)
                .ToList();
            return data;
        }

        public List<ScoreSheet> SelectAllBySession(string year, string season)
        {
            Session sess = isession.SelectBySeason(year, season);
            List<ScoreSheet> data = db.ScoreSheets
                .Where(s => s.SsSessionId == sess.SessionId)
                .OrderByDescending(s => s.SsWeek)
                .OrderBy(s => s.SsHteam)
                .ToList();
            return data;
        }


        public List<ScoreSheet> SelectAllByWeek(int sid, int wid)
        {
            List<ScoreSheet> data = db.ScoreSheets
                .Where(s => s.SsSessionId == sid && s.SsWeek == wid)
                .OrderBy(s => s.SsHteam)
                .ToList();
            return data;
        }

        public List<ScoreSheet> SelectFirstByWeek(int sid)
        {
            List<ScoreSheet> tdata = db.ScoreSheets
                    .Where(s => s.SsSessionId == sid)
                    .OrderBy(s => s.SsWeek)
                    .ToList();

            int low = 0;
            List<ScoreSheet> data = new List<ScoreSheet>();   
            foreach (ScoreSheet item in tdata)
            {
                if(item.SsWeek > low)
                {
                    data.Add(item);
                    low = item.SsWeek;
                }
            }

            return data;
        }



        public ScoreSheet SelectByID(int id, int week, int team)
        {
            ScoreSheet scoreSheet = db.ScoreSheets
                .Where(s => s.SsSessionId == id && s.SsWeek == week && s.SsHteam == team)
                .SingleOrDefault();
            return scoreSheet;
        }

        public void Update(ScoreSheet scoreSheet)
        {
            db.Entry(scoreSheet).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
