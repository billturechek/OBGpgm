using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Interfaces
{
    public interface IPhotoRepository
    {
        List<Photo> SelectAll();
        List<Photo> SelectAll(int id);
        List<Photo> SelectAllByGroup(int id);
        List<Photo> SelectFirstByGroup();
        Photo SelectByID(int id);
        int SelectHighGroup();
        int Insert(Photo photo);
        void Update(Photo photo);
        void Delete(int id);
    }
}
