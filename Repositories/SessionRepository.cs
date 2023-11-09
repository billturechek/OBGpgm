using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.EntityFrameworkCore;
using OBGpgm.Data;
using OBGpgm.Models;
using OBGpgm.Interfaces;
using OBGpgm.Repositories;

namespace OBGpgm.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private ObgDbContext db;

        public SessionRepository(ObgDbContext db)
        {
            this.db = db;
        }

        public IEnumerable<Session> SelectAll()
        {
            return db.Sessions.OrderByDescending(s => s.SessionId).ToList();
        }

        public List<string> SelectByYears()
        {
            List<string> results = (from ta in db.Sessions
                                    select ta.Year)
                           .Distinct()
                           .OrderByDescending(x => x)
                           .ToList();
            return results;
        }

        public List<string> SelectAllSeasons(string year)
        {
            List<Session> sList = db.Sessions
                .Where(s => s.Year == year)
                .OrderBy(s => s.Season)
                .ToList();
            List<string> results = (from ta in sList
                                    select ta.Season)
                .ToList();
            return results;
        }

        public Session SelectByCurrent()
        {
            Session session = db.Sessions
                .Where(s => s.CurrentSeason)
                .SingleOrDefault();
            return session;
        }

        public bool ResetCurrent()
        {
            Session session = db.Sessions
                .Where(s => s.CurrentSeason == true)
                .SingleOrDefault();
            session.CurrentSeason = false;
            db.Entry(session).State = EntityState.Modified;
            db.SaveChanges();

            return true;
        }
        public bool SetCurrent(int id)
        {
            Session session = db.Sessions.Find(id);
            session.CurrentSeason = true;
            db.Entry(session).State = EntityState.Modified;
            db.SaveChanges();
            return true;
        }


        public Session SelectBySeason(string year, string season)
        {
            Session session = db.Sessions
                .Where(s => s.Year == year)
                .Where(s =>  s.Season == season)
                .SingleOrDefault();
            return session;
        }

        public Session SelectById(int id)
        {
            return db.Sessions.Find(id);
        }

        public int Insert(Session session)
        {
            db.Sessions.Add(session);
            db.SaveChanges();
            int id = session.SessionId; // Yes it's here
            return id;
        }

        public void Update(Session session)
        {
            db.Entry(session).State = EntityState.Modified;
            db.SaveChanges();
        }
        public void Delete(int id)
        {
            Session session = db.Sessions.Find(id);
            db.Sessions.Remove(session);
            db.SaveChanges();
        }
    }
}