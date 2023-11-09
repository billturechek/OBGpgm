using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OBGpgm.Data;
using OBGpgm.Models;

namespace OBGpgm.Views.ScoreSheets
{
    public class IndexModel : PageModel
    {
        private readonly OBGpgm.Data.ObgDbContext _context;

        public IndexModel(OBGpgm.Data.ObgDbContext context)
        {
            _context = context;
        }

        public IList<ScoreSheet> ScoreSheet { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.ScoreSheets != null)
            {
                ScoreSheet = await _context.ScoreSheets
                .Include(s => s.SsSession).ToListAsync();
            }
        }
    }
}
