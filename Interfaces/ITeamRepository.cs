using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OBGpgm.Interfaces
{
    public interface ITeamRepository
    {
        List<Team> SelectAll();
        List<Team> SelectAllBySession(string session);
        List<Team> SelectAllBySeason(string year, string season);
        List<Team> SelectAllByNumber(int session, int division = 1);
        List<Team> SelectAllByNumberSeason(string year, string season);
        Team SelectByID(int id);
        Team SelectIdByNumber(int session, int teamnum);
        int Insert(Team team);
        void Update(Team team);
        void Delete(int id);
    }
}
