using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OBGpgm.Models;

namespace OBGpgm.Repositories
{
    public interface IScheduleRepository
    {
        List<Schedule> SelectAll();
        List<Schedule> SelectAllSessions();
        List<Schedule> SelectAllBySessionId(int sessionId);
        List<Schedule> SelectAllWeeksHigherBySessionId(int sessionId, int week);
        List<Schedule> SelectAllByTeams(int teams, int sessionId);
        List<Schedule> SelectAllByWeek(int sessionId, int week);
        Schedule SelectByID(int id);
        int Insert(Schedule schedule);
        void Update(Schedule schedule);
        void Delete(int id);
    }
}
