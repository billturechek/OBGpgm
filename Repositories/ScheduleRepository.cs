
using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OBGpgm.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ObgDbContext db = null;
        public ScheduleRepository(ObgDbContext db)
        {
            this.db = db;
        }
        public void Delete(int id)
        {
            Schedule sched = db.Schedules.Find(id);
            db.Schedules.Remove(sched);
            db.SaveChanges();
        }

        public int Insert(Schedule s)
        {
            db.Schedules.Add(s);
            db.SaveChanges();
            int id = s.Id; // Yes it's here
            return id;
        }

        public List<Schedule> SelectAll()
        {
            List<Schedule> data = db.Schedules
                .OrderBy(s => s.Teams)
                .ThenBy(s => s.Week)
                .ThenBy(s => s.TimeSlot)
                .ToList();
            return data;
        }

        public List<Schedule> SelectAllBySessionId(int sessionId)
        {
            List<Schedule> data = db.Schedules
                .Where(s => s.SessionId == sessionId)
                .OrderBy(s => s.Week)
                .ThenBy(s => s.TimeSlot)
                .ThenBy(s => s.TableGroup)
                .ToList();
            return data;
        }

        public List<Schedule> SelectAllByTeams(int teams, int sessionId)
        {
            List<Schedule> data = db.Schedules
                .Where(s => s.Teams == teams)
                .Where(s => s.SessionId == sessionId)
                .OrderBy(s => s.Teams)
                .ThenBy(s => s.Week)
                .ThenBy(s => s.TimeSlot)
                .ThenBy(s => s.TableGroup)
                .ToList();
            return data;
        }

        public List<Schedule> SelectAllWeeksHigherBySessionId(int sessionId, int week)
        {
            List<Schedule> data = db.Schedules
                .Where(s => s.SessionId == sessionId)
                .Where(s => s.Week > week)
                .OrderBy(s => s.Week)
                .ThenBy(s => s.TimeSlot)
                .ThenBy(s => s.TableGroup)
                .ToList();
            return data;
        }

        public List<Schedule> SelectAllByWeek(int sessionId, int week)
        {
            List<Schedule> data = db.Schedules
                .Where(s => s.SessionId == sessionId)
                .Where(s => s.Week == week)
                .OrderBy(s => s.TimeSlot)
                .ThenBy(s => s.TableGroup)
                .ToList();
            return data;

        }

        public List<Schedule> SelectAllSessions()
        {
            List<Schedule> data = db.Schedules
                .Where(s => s.SessionId > 0)
                .Where(s => s.Week == 1)
                .Where(s => s.TimeSlot == 1)
                .Where(s => s.TableGroup == 1)
                .OrderByDescending(s => s.SessionId)
                .ToList();
            return(data);
        }

        public Schedule SelectByID(int id)
        {
            Schedule sched = db.Schedules
                .Where(s => s.Id == id)
                .FirstOrDefault();
            return sched;
        }

        public void Update(Schedule s)
        {
            db.Entry(s).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
