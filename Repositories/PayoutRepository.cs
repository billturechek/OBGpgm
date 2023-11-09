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

    public class PayoutRepository : IPayoutRepository
    {
        private readonly OBGpgm.Interfaces.ISessionRepository isession;
        private readonly ObgDbContext db = null;
        public PayoutRepository(ObgDbContext db)
        {
            this.db = db;
            this.isession = new SessionRepository(new ObgDbContext());
        }
        public void Delete(int id)
        {
            int count = db.Database.ExecuteSqlRaw("DELETE FROM Payout WHERE PayoutId = {0}", id);
        }

        public int Insert(Payout payout)
        {
            db.Payouts.Add(payout);
            db.SaveChanges();
            int id = payout.PayoutId; // Yes it's here
            return id;
        }

        public List<Payout> SelectAll()
        {
            List<Payout> data = db.Payouts
                .OrderByDescending(p => p.PayoutId)
                .ToList();
            return data;
        }

        public List<Payout> SelectAllBySession(string year, string season)
        {
            Session sess = isession.SelectBySeason(year, season);
            List<Payout> data = db.Payouts
                .Where(p => p.SessionId == sess.SessionId)
                .OrderBy(p => p.TeamId)
                .ToList();
            return data;
        }

        public List<Payout> SelectAllByPayout(string session)
        {
            if (Int32.TryParse(session, out int sid))
            {
                List<Payout> data = db.Payouts
                .Where(p => p.SessionId == sid)
                .OrderByDescending(p => p.TeamId)
                .ToList();
                return data;
            }
            else
            {
                return null;
            }
        }

        public Payout SelectByID(int id)
        {
            Payout payout = db.Payouts
                .Where(p => p.PayoutId == id)
                .SingleOrDefault();
            return payout;
        }

        public Payout SelectByTeamID(int id)
        {
            Payout payout = db.Payouts
                .Where(p => p.TeamId == id)
                .SingleOrDefault();
            return payout;
        }

        public void Update(Payout payout)
        {
            db.Entry(payout).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
