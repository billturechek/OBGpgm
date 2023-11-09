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
    public class PtlogRepository : IPtlogRepository
    {
        private readonly OBGpgm.Interfaces.ISessionRepository isession;
        private readonly ObgDbContext db = null;
        public PtlogRepository(ObgDbContext db)
        {
            this.db = db;
            this.isession = new SessionRepository(new ObgDbContext());
        }
        public void Delete(int id)
        {
            int count = db.Database.ExecuteSqlRaw("DELETE FROM Ptlog WHERE Ptlid = {0}", id);
        }

        public int Insert(Ptlog ptlog)
        {
            db.Ptlogs.Add(ptlog);
            db.SaveChanges();
            int id = ptlog.Ptlid; // Yes it's here
            return id;
        }

        public List<Ptlog> SelectAll()
        {
            List<Ptlog> data = db.Ptlogs
                .OrderByDescending(p => p.PtlDate)
                .ToList();                
            return data;
        }

        public List<Ptlog> SelectAllBySession(string year, string season)
        {
            Session sess = isession.SelectBySeason(year, season);
            List<Ptlog> data = db.Ptlogs
                .Where(p => p.Ptlsession == sess.SessionId)
                .OrderByDescending(p => p.PtlDate)
                .ToList();
            return data;
        }

        public Ptlog SelectByID(int id)
        {
            Ptlog ptlog = db.Ptlogs
                .Where(p => p.Ptlid == id)
                .SingleOrDefault();
            return ptlog;
        }

        public void Update(Ptlog ptlog)
        {
            db.Entry(ptlog).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
