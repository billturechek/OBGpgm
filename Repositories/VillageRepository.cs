using OBGpgm.Data;
using OBGpgm.Models;
using OBGpgm.Interfaces;
using OBGpgm.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OBGpgm.Repositories
{
    public class VillageRepository : IVillageRepository
    {
        private readonly ObgDbContext db = null;
        public VillageRepository(ObgDbContext db)
        {
            this.db = db;
        }
        public List<Village> SelectAll()
        {
            List<Village> data = db.Villages
                .OrderBy(v=>v.F1)
                .ToList();
            return data;
        }
    }
}
