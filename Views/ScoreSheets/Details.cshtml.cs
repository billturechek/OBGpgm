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
    public class DetailsModel : PageModel
    {
        private readonly OBGpgm.Data.ObgDbContext _context;

        public DetailsModel(OBGpgm.Data.ObgDbContext context)
        {
            _context = context;
        }

      public ScoreSheet ScoreSheet { get; set; } = default!; 

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.ScoreSheets == null)
            {
                return NotFound();
            }

            var scoresheet = await _context.ScoreSheets.FirstOrDefaultAsync(m => m.SsSessionId == id);
            if (scoresheet == null)
            {
                return NotFound();
            }
            else 
            {
                ScoreSheet = scoresheet;
            }
            return Page();
        }
    }
}
