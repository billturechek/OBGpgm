using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;

namespace OBGpgm.Repositories
{
    public class PortraitRepository : IPortraitRepository
    {
        private readonly ObgDbContext db = null;
        public PortraitRepository(ObgDbContext db)
        {
            this.db = db;
        }
        public void Delete(int id)
        {
            Portrait portrait = db.Portraits.Find(id);
            db.Portraits.Remove(portrait);
            db.SaveChanges();
        }

        public int Insert(Portrait portrait)
        {

            db.Portraits.Add(portrait);
            db.SaveChanges();
            int id = portrait.Id; // Yes it's here
            return id;
        }

        public List<Portrait> SelectAll()
        {
            List<Portrait> data = db.Portraits
                .OrderBy(p => p.Member.LastName)
                .ThenBy(p => p.Member.FirstName)
                .ToList();
            return data;
        }

        public List<Portrait> SelectAllByDeceased()
        {
            List<Portrait> data = db.Portraits
                .Where(p=>p.Member.IsDeceased)
                .OrderBy(p => p.Member.LastName)
                .ThenBy(p => p.Member.FirstName)
                .ToList();
            return data;
        }

        public List<Portrait> SelectAllByLiving()
        {
            List<Portrait> data = db.Portraits
                .Where(p => p.Member.IsDeceased == false)
                .OrderBy(p => p.Member.LastName)
                .ThenBy(p => p.Member.FirstName)
                .ToList();
            return data;
        }

        
        public Portrait SelectByID(int id)
        {
            Portrait portrait = db.Portraits
                .Where(p => p.Id == id)
                .Include(p => p.Member)
                .SingleOrDefault();
            return portrait;
        }


        public void Update(Portrait portrait)
        {
            db.Entry(portrait).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
