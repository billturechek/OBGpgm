using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OBGpgm.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly ObgDbContext db = null;
        public PhotoRepository(ObgDbContext db)
        {
            this.db = db;
        }

        public Photo SelectByID(int id)
        {
            Photo photo = db.Photos.FromSqlRaw("SELECT * FROM Photo WHERE id ={0}", id).SingleOrDefault();
            return photo;
        }

        public List<Photo> SelectAll()
        {
            List<Photo> data = db.Photos.ToList();
            return data;
        }

        public List<Photo> SelectAll(int id)
        {
            List<Photo> data = db.Photos
                .Where(p => (p.articleId==id))
                .ToList();
            return data;
        }

        public List<Photo> SelectAllByGroup(int id)
        {
            List<Photo> data = db.Photos
                .Where(p => (p.groupId == id))
                .ToList();
            return data;
        }

        public int SelectHighGroup()
        {
            int? highGroup = 0;
            highGroup = db.Photos
                .Max(p => p.groupId);
            int hGroup = highGroup ?? 0;
            return hGroup;
        }

        public List<Photo> SelectFirstByGroup()
        {
            List<Photo> photoList = db.Photos
                .OrderBy(p => p.groupId)
                .ToList();
            int i = 0;
            List<Photo> results = new List<Photo>();
            foreach(Photo p in photoList)
            {
                if (p.groupId != null)
                {
                    if (p.groupId > i)
                    {
                        results.Add(p);
                        i = p.groupId;
                    }
                }
            }

            return results;
        }

        public List<Photo> SelectFirstByGroupxx()
        {
            List<Photo> results = db.Photos
                .GroupBy(x => x.groupId)
                .Select(g => g.OrderBy(x => x.id)
                .FirstOrDefault()).ToList();
            return results;
        }

        public int Insert(Photo photo)
        {
            db.Photos.Add(photo);
            db.SaveChanges();
            int id = photo.id; // Yes it's here
            return id;
        }

        public void Update(Photo photo)
        {
            int count = db.Database.ExecuteSqlRaw("UPDATE Photo SET " +
                 "articleId = {0}, memberId = {1}, caption = {2}, notes = {3}, " +
                 "thumbImage = {4}, largeImage = {5}, groupId = {6}, groupName = {7} WHERE id = {8}",
                 photo.articleId, photo.memberId, photo.caption, photo.notes, photo.thumbImage, 
                 photo.largeImage, photo.groupId, photo.groupName, photo.id);
            return;
        }

        public void Delete(int id)
        {
            int count = db.Database.ExecuteSqlRaw("DELETE FROM Photo WHERE id = {0}", id);
            return;
        }
    }
}
