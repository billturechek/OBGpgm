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
    public class StateRepository : IStateRepository
    {
        private readonly ObgDbContext db = null;
        public StateRepository(ObgDbContext db)
        {
            this.db = db;
        }
        public List<State> SelectAll()
        {
            List<State> data = db.States
                .OrderBy(s => s.StateName)
                .ToList();
            return data;
        }
    }
}
