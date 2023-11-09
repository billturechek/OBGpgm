using OBGpgm.Data;
using OBGpgm.Models;
using OBGpgm.Interfaces;
using OBGpgm.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly OBGpgm.Interfaces.ISessionRepository isession;
        private readonly ObgDbContext db = null;
        public TeamRepository(ObgDbContext db)

        {
            this.db = db;
            this.isession = new SessionRepository(new ObgDbContext());
        }

        public List<Team> SelectAll()
        {
            List<Team> data = db.Teams
                .OrderByDescending(t=>t.SessionId)
                .OrderBy(t=>t.TeamNumber)
                .ToList();
            return data;
        }

        public List<Team> SelectAllBySession(string session)
        {
            int sid = Convert.ToInt32(session);
            List<Team> data = db.Teams
                .Where(t => t.SessionId == sid)
                .OrderByDescending(t => t.TeamPoints)
                .ToList();
            return data;
        }

        public List<Team> SelectAllBySeason(string year, string season)
        {
            Session sess = isession.SelectBySeason(year, season);
            List<Team> data = db.Teams
                .Where(s => s.SessionId == sess.SessionId)
                .OrderByDescending(s => s.TeamPoints)
                .ToList();
                
            return data;
        }

        public List<Team> SelectAllByNumberSeason(string year, string season)
        {
            Session sess = isession.SelectBySeason(year, season);
            List<Team> data = db.Teams
                .Where(s => s.SessionId == sess.SessionId)
                .OrderBy(s => s.TeamNumber)
                .ToList();

            return data;
        }

        public List<Team> SelectAllByNumber(int session, int division = 1)
        {
            List<Team> data = db.Teams
                .Where(t => t.SessionId == session && t.Division == division)
                .OrderBy(t => t.TeamNumber)
                .ToList();
            return data;            
        }

        public Team SelectIdByNumber(int session, int teamnum)
        {
            Team theTeam = db.Teams
                .Where(t => t.SessionId == session && t.TeamNumber == teamnum)
                .SingleOrDefault();
            int id = theTeam.TeamId;
            return theTeam;
        }
        public Team SelectByID(int id)
        {
            Team team = db.Teams
                .Where(t => t.TeamId == id)
                .SingleOrDefault();
            return team;
        }

        public int Insert(Team team)
        {
            db.Teams.Add(team);
            db.SaveChanges();
            int id = team.TeamId; // Yes it's here
            return id;
        }

        public void Update(Team team)
        {
            db.Entry(team).State = EntityState.Modified;
            db.SaveChanges();
        }
        public void Delete(int id)
        {
            Team team = db.Teams.Find(id);
            db.Teams.Remove(team);
            db.SaveChanges();
        }
    }
}
