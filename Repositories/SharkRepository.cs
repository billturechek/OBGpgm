using OBGpgm.Data;
using OBGpgm.Models;
using OBGpgm.Interfaces;
using OBGpgm.Repositories;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Repositories
{
    public class SharkRepository : ISharkRepository
    {
        private readonly ObgDbContext db;
        public SharkRepository(ObgDbContext db)
        {
            this.db = db;
        }

        public List<Shark> SelectAll()
        {
            List<Shark> data = db.Sharks
                    .Include(s => s.Player)
                    .Include(s => s.Player.Team)
                    .Include(s => s.Player.Member)
                    .OrderByDescending(s => s.SharkId)
                    .ToList();
            return data;
        }

        public List<Shark> SelectAllBySession(string year, string season)
        {
            Session? sess = db.Sessions.FirstOrDefault(m => m.Year == year && m.Season == season);
            if (sess != null)
            {
                List<Shark> data = db.Sharks
                    .Where(s => s.SessionId == sess.SessionId)
                    .Include(s => s.Player)
                    .Include(s => s.Player.Team)
                    .Include(s => s.Player.Member)
                    .ToList();
                return data;
            }

            return null;
        }

        public int SelectHighBySession(int sessionid)
        {
            int highScore=0;
            highScore = db.Sharks
                .Where(p => p.SessionId == sessionid)
                .Max(p => p.Points);
            return highScore;
        }

        public List<Shark> SelectAllHighBySession(int sessionid)
        {
            int max = db.Sharks
                .Where(s => s.SessionId == sessionid && s.SharkType == 0)
                .Max(p => p.Points);                
            
            List<Shark> data = db.Sharks
                .Where(s => s.SessionId == sessionid && s.SharkType == 0 && s.Points == max)
                .OrderBy(s=>s.SharkDate)
                .ToList();
            return data;
        }

        public Shark SelectByID(int id)
        {
            Shark shark = db.Sharks.Where(s=>s.MemberId == id).FirstOrDefault();
            return shark;
        }
        public int Insert(Shark shark)
        {
            db.Sharks.Add(shark);
            db.SaveChanges();
            int id = shark.SharkId; // Yes it's here
            return id;
        }

        public void Update(Shark shark)
        {
            db.Entry(shark).State = EntityState.Modified;
            db.SaveChanges();
        }
        public void Delete(int id)
        {
            Shark shark = db.Sharks.Find(id);
            db.Sharks.Remove(shark);
            db.SaveChanges();
        }
    }
}

