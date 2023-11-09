using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OBGpgm.Interfaces
{
    public interface IPayoutRepository
    {
        List<Payout> SelectAll();
        List<Payout> SelectAllBySession(string year, string season);
        List<Payout> SelectAllByPayout(string session);
        Payout SelectByID(int id);
        Payout SelectByTeamID(int id);
        int Insert(Payout payout);
        void Update(Payout payout);
        void Delete(int id);
    }
}
