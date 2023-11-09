using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OBGpgm.Interfaces
{
    public interface IStateRepository
    {
        List<State> SelectAll();
    }
}
