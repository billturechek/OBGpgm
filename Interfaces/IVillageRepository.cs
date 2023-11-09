using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Interfaces
{
    public interface IVillageRepository
    {
        List<Village> SelectAll();
    }
}
