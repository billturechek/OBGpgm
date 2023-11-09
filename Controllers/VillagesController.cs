using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBGpgm.Data;
using OBGpgm.Models;
using OBGpgm.Interfaces;
using OBGpgm.Repositories;

namespace OBGpgm.Controllers
{
    public class VillagesController : Controller
    {
        private readonly OBGpgm.Interfaces.IVillageRepository ivillage;
        private readonly ObgDbContext db;

        public VillagesController(ObgDbContext context)
        {
            db = context;
            this.ivillage = new VillageRepository(new ObgDbContext());
        }
        public IActionResult Index(int pg=1)
        {
            List<Village> villages = ivillage.SelectAll();

            const int pageSize = 10;
            if (pg<1)
            {
                pg = 1;     
            }
            int recsCount = villages.Count();
            var pager = new Pager("Villages", recsCount,pg,pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = villages.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);


            //return View(villages);
        }
    }
}
