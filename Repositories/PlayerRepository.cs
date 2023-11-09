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
    public class PlayerRepository : IPlayerRepository
    {
        private readonly OBGpgm.Interfaces.ISessionRepository isession;
        private readonly ObgDbContext db = null;
        public PlayerRepository(ObgDbContext db)
        {
            this.db = db;
            this.isession = new SessionRepository(new ObgDbContext());
        }
        public void Delete(int id)
        {
            Player player = db.Players.Find(id);
            db.Players.Remove(player);
            db.SaveChanges();
        }

        public int Insert(Player player)
        {
            db.Players.Add(player);
            db.SaveChanges();
            int id = player.PlayerId; // Yes it's here
            return id;
        }

        public List<Player> SelectAll()
        {
            List<Player> data = db.Players
                .OrderByDescending(p => p.SessionId)
                .OrderBy(p => p.PlayerId)
                .ToList();
            return data;
        }

        public List<Player> SelectAllByYear(string year)
        {

            List<Player> data = db.Players
                .Where(p => p.Session.Year == year)
                .Include(p => p.Member)
                .Include(p => p.Team)
                .Include(p => p.Session)
                .OrderBy(p => p.Member.LastName)
                .ThenBy(p => p.Member.FirstName)
                .ToList();
            return data;
        }

        public List<Player> SelectAllBySession(string year, string season, string sort = "Name")
        {
            Session sess = isession.SelectBySeason(year, season);

            List<Player> data = new List<Player>();
            if (sort == "Name")
            {
                data = db.Players
                    .Where(p => p.SessionId == sess.SessionId)
                    .Include(p => p.Member)
                    .Include(p => p.Team)
                    .Include(p => p.Session)
                    .OrderBy(p => p.Member.LastName)
                    .ToList();
            }
            else
            {
                data = db.Players
                    .Where(p => p.SessionId == sess.SessionId)
                    .Include(p => p.Member)
                    .Include(p => p.Team)
                    .Include(p => p.Session)
                    .OrderBy(p => p.Member.Bday)
                    .ToList();
            }
            
            return data;
        }

        public List<Player> SelectAllByBday(string year, string season, string sort)
        {
            Session sess = isession.SelectBySeason(year, season);
            DateTime DefaultDate = new DateTime(1905, 1, 1);
            List<Player> data = new List<Player>();
            if (sort == "Name")
            {
                data = db.Players.Where(x =>
                            x.SessionId == sess.SessionId &&
                            x.Member.Bday > DefaultDate)
                    .Include(p => p.Member)
                    .Include(p => p.Team)
                    .Include(p => p.Session)
                    .OrderBy(p => p.Member.LastName)
                    .ToList();
            }
            else if (sort == "Chron")
            {
                data = db.Players
                    .Where(x => 
                            x.SessionId == sess.SessionId &&
                            x.Member.Bday > DefaultDate)
                    .Include(p => p.Member)
                    .Include(p => p.Team)
                    .Include(p => p.Session)
                    .OrderBy(p => p.Member.Bday)
                    .ToList();
            }

            return data;
        }

        public List<Player> SelectAllByTeamsInSession(string year, string season)
        {
            Session sess = isession.SelectBySeason(year, season);

            List<Player> data = db.Players
                .Where(p => p.SessionId == sess.SessionId)
                .Include(p => p.Team)
                .Include(p => p.Member)
                .OrderBy(p => p.Team.TeamNumber)
                .ThenByDescending(p => p.IsCaptain)
                .ThenBy(p => p.Member.LastName)
                .ToList();
            return data;
        }

        public List<Player> SelectAllByCaptain(int session)
        {
            //SqlParameter p = new SqlParameter("@lastSessionID", session);
            List<Player> data = db.Players
                .Where(p => p.SessionId == session && p.IsCaptain && p.IsPlaying)
                .Include(p => p.Member)
                .Include(p => p.Team)
                .OrderBy(p => p.Team.TeamPoints)
                .ThenBy(p => p.Team.TeamNumber)
                .ToList();
            return data;
        }

        public List<Player> SelectAllMembers()
        {
            List<Player> data = db.Players
                    .Include(p => p.Member)
                    .OrderBy(p => p.Member.LastName)
                    .ThenBy(p=> p.Member.FirstName)
                    .ThenByDescending(p=> p.PlayerId)
                    .GroupBy(p => p.MemberId)
                    .Select(g => g.First())
                    .ToList();
            return data;
        }

        public List<Player> SelectAllByMember(int id)
        {
            List<Player> data = db.Players
                .Where(p => p.MemberId == id)
                .Include(p => p.Member)
                .Include(p => p.Draft)
                .OrderByDescending(p => p.SessionId)
                .ToList();
            return data;
        }

        public List<Player> SelectAllByNonCaptain(int session)
        {
            //SqlParameter p = new SqlParameter("@sessionID", session);
            List<Player> data = db.Players
                .Where(p => p.Session.SessionId == session && p.IsPlaying && !p.IsCaptain)
                .Include(p => p.Member)
                .ToList();
            return data;
        }

        public List<Player> SelectAllByNewCaptain(int session)
        {
            //SqlParameter p = new SqlParameter("@SessionID", session);
            List<Player> data = db.Players
                .Where(p => p.IsCaptain && p.Session.SessionId == session)
                .Include(p => p.Member)
                .OrderBy(p => p.Member.LastName)
                .ToList();
            return data;
        }

        public List<Player> SelectAllByAvailable(int session)
        {            
            List<Player> data = db.Players
                .Where(p=>p.Session.SessionId == session && p.Member.IsActive && p.IsInDraft && p.TeamId == 0)
                .Include(p => p.Member)
                .OrderBy(p=>p.Member.LastName)
                .ToList();
            return data;  
        }

        public List<Player> SelectAllByAvailable(bool captains, int session)
        {
            List<Player> data = new List<Player>();
            if (captains)
            {
                 data = db.Players
                    .Where(p => p.Session.SessionId == session && 
                                    p.Member.IsActive && 
                                    p.IsInDraft) 
                    .Include(p => p.Member)
                    .OrderBy(p => p.Member.LastName)
                    .ToList();
            }
            else
            {
                data = db.Players
                    .Where(p => p.Session.SessionId == session &&
                                    p.Member.IsActive && 
                                    p.IsInDraft && 
                                    !(p.IsCaptain) &&
                                    p.TeamId == 0)
                    .Include(p => p.Member)
                    .OrderBy(p => p.Member.LastName)
                    .ToList();
            }
            return data;
        }

        public List<Player> SelectAllBySelected(int session)
        {
            List<Player> data = db.Players
                .Where(p => p.Session.SessionId == session && p.Member.IsActive && p.IsInDraft && p.TeamId != 0 && p.IsCaptain != true)
                .Include(p => p.Member)
                .OrderBy(p => p.Member.LastName)
                .ToList();
            return data;
        }

        public List<Player> SelectAllByTeam(int teamId)
        {
            List<Player> data = db.Players
                .Where(p => p.TeamId == teamId)
                .Include(p => p.Member)
                .OrderByDescending(p=>p.IsCaptain)
                .ToList();
            return data;
        }

        public Player SelectByID(int id)
        {
            Player player = db.Players
                .Where(p => p.PlayerId == id)
                .Include(p => p.Member)
                .SingleOrDefault();
            return player;
        }

        public Player SelectByMemberId(int id, int sid)
        {
            Player player = db.Players
                .Where(p => p.SessionId == sid && p.MemberId == id)
                .Include(p => p.Member)
                .Include(p => p.Team)
                .SingleOrDefault();
            return player;
        }


        public Player SelectByTeamCaptain(int team)
        {
            Player player = db.Players
                .Where(p => p.TeamId == team && p.IsCaptain)
                .Include(p => p.Member)
                .SingleOrDefault();

            return player;
        }

        public Player SelectByTeamCaptain(int team, int session)
        {
            Player player = (Player)db.Players
                .Where(p => p.SessionId == session && p.TeamId == team && p.IsCaptain)
                .Include(p => p.Member)
                .SingleOrDefault();
            return player;
        }

        public void Update(Player player)
        {
            db.Entry(player).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
