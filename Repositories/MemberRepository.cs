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
    public class MemberRepository : IMemberRepository
    {
        private readonly ObgDbContext db;

        public MemberRepository(ObgDbContext db)
        {
            this.db = db;
        }

        public List<Member> SelectAll()
        {
            return db.Members
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToList();
        }

        public List<Member> SelectAllPaidMembers()
        {
            return db.Members
                .Where(m => m.HasPaidAnnualDues && !m.IsDeceased)
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToList();
        }

        public Member SelectById(int id)
        {
            return db.Members.Find(id);
        }

        public Member SelectByEmail(String Email)
        {
            Member member = db.Members.FirstOrDefault(m => m.Email == Email);
            return member;
        }

        public Member SelectByOldest()
        {
            Member member;
            DateTime highAge = DateTime.MinValue;
            highAge = (DateTime)db.Members
                .Where(p => p.IsActive && p.Bday != null)
                .Min(p => p.Bday);
            if (highAge!=null)
            {
                     member = db.Members
                    .FirstOrDefault(m => m.Bday == highAge);
            }
            else
            {
                member = null;
            }
            return member;
        }
        public Member SelectByYoungest()
        {
            Member member;
            DateTime lowAge = DateTime.Now;
            lowAge = (DateTime)db.Members
                .Where(p => p.IsActive && p.Bday != null)
                .Max(p => p.Bday);
            if (lowAge != null)
            {
                member = db.Members
               .FirstOrDefault(m => m.Bday == lowAge);
            }
            else
            {
                member = null;
            }

            return member;
        }

        public void Insert(Member mem)
        {
            db.Members.Add(mem);
            db.SaveChanges();
        }
        public List<Member> SelectAlive()
        {
            List<Member> data = db.Members
                .Where(m => m.IsDeceased == false)
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName).ToList();
            return data;
        }
        
        public List<Member> SelectAllPhoto(bool isDeceased)
        {
            /*
                List<Member> data = db.Members
                           .Where(m => m.imageID != null && m.IsDeceased == isDeceased)
                           .OrderBy(m => m.LastName).ToList();
                return data;
            */
            return null;
        }
        
        public void Update(Member mem)
        {
            db.Entry(mem).State = EntityState.Modified;
            db.SaveChanges();
        }
        public void Delete(int id)
        {
            Member mem = db.Members.Find(id);
            db.Members.Remove(mem);
            db.SaveChanges();
        }

    }
}
