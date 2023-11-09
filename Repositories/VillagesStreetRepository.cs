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
    public class VillagesStreetRepository : IVillagesStreetRepository
    {
        private readonly ObgDbContext db = null;
        public VillagesStreetRepository(ObgDbContext db)
        {
            this.db = db;
        }
        public List<VillagesStreet> SelectAll()
        {
            List<VillagesStreet> data = db.VillagesStreets
                .OrderBy(v=>v.StreetName)                
                .ToList();
            return data;
        }
    }
}
