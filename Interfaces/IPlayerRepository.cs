using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OBGpgm.Interfaces
{
    public interface IPlayerRepository
    {
        List<Player> SelectAll();
        List<Player> SelectAllByTeamsInSession(string year, string season);
        List<Player> SelectAllByAvailable(int session);
        List<Player> SelectAllByAvailable(bool captains, int session);
        List<Player> SelectAllBySelected(int session);
        List<Player> SelectAllMembers();
        List<Player> SelectAllByMember(int id);
        List<Player> SelectAllByNonCaptain(int session);
        List<Player> SelectAllByCaptain(int session);
        List<Player> SelectAllByNewCaptain(int session);
        List<Player> SelectAllByTeam(int teamId);

        //List<Player> SelectAllBySession(string year, string season);
        List<Player> SelectAllBySession(string year, string season, string sort = "Name");
        List<Player> SelectAllByBday(string year, string season, string sort);
        List<Player> SelectAllByYear(string year);
        Player SelectByID(int id);
        Player SelectByMemberId(int id, int sid);
        Player SelectByTeamCaptain(int team);
        Player SelectByTeamCaptain(int team, int session);
        int Insert(Player player);
        void Update(Player player);
        void Delete(int id);
    }
}
