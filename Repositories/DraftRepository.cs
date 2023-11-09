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
    public class DraftRepository : IDraftRepository
    {
        private readonly OBGpgm.Interfaces.ISessionRepository isession;
        private readonly ObgDbContext db = null;
        public DraftRepository(ObgDbContext db)
        {
            this.db = db;
            this.isession = new SessionRepository(new ObgDbContext());
        }
        public void Delete(int id)
        {
            int count = db.Database.ExecuteSqlRaw("DELETE FROM Draft WHERE DraftID = {0}", id);
        }

        public int Insert(Draft draft)
        {
            db.Drafts.Add(draft);
            db.SaveChanges();
            int id = draft.DraftId; // Yes it's here
            return id;
        }

        public List<Draft> SelectAll()
        {
            List<Draft> data = db.Drafts
                .OrderByDescending(d => d.DraftSessionId)
                .ThenBy(d => d.DraftRound)
                .ThenBy(d => d.DraftPosition)
                .ToList();
            return data;
        }

        public List<Draft> SelectAllBySession(string year, string season)
        {
            Session sess = isession.SelectBySeason(year, season);
            List<Draft> data = db.Drafts
                .Where(d => d.DraftSessionId == sess.SessionId)
                .ToList();
            return data;
        }

        public List<Draft> SelectAllBySelection(int session)
        {
            List<Draft> data = db.Drafts
                .Where(d => d.DraftSessionId == session && d.DraftSelection != 0 && d.DraftPlayerId == 0)
                .OrderBy(d => d.DraftSelection)
                .ToList();
            return data;
        }
        public Draft SelectByID(int id)
        {
            
            Draft draft = db.Drafts
                .Where(d => d.DraftId == id)
                .SingleOrDefault();
            return draft;
        }

        public void Update(Draft draft)
        {
            db.Entry(draft).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
