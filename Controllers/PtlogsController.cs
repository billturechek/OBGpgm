using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBGpgm.Data;
using OBGpgm.Models;

namespace OBGpgm.Controllers
{
    public class PtlogsController : Controller
    {
        private readonly ObgDbContext _context;

        public PtlogsController(ObgDbContext context)
        {
            _context = context;
        }

        // GET: Ptlogs
        public IActionResult Index(int pg=1)
        {
            List<Ptlog> ptlogs = _context.Ptlogs
                .Include(p => p.PtlsessionNavigation)
                .Include(p => p.PtlplayerNavigation)
                .Include(s => s.PtlplayerNavigation.Member)
                .OrderByDescending(p => p.PtlDate)
                .ToList();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = ptlogs.Count();
            var pager = new Pager("Ptlogs", recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = ptlogs.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            this.ViewBag.returnPage = pg;
            return View(data);
            /*
            var oBGcoreContext = _context.Ptlogs.Include(p => p.PtlplayerNavigation).Include(p => p.PtlsessionNavigation);
            return View(await oBGcoreContext.ToListAsync());  */
        }

        // GET: Ptlogs/Details/5
        public async Task<IActionResult> Details(int? id, int pg=1)
        {
            if (id == null || _context.Ptlogs == null)
            {
                return NotFound();
            }

            var ptlog = await _context.Ptlogs
                .Include(p => p.PtlplayerNavigation)
                .Include(p => p.PtlsessionNavigation)
                .FirstOrDefaultAsync(m => m.Ptlid == id);
            if (ptlog == null)
            {
                return NotFound();
            }

            ViewBag.returnPage = pg;
            return View(ptlog);
        }

        // GET: Ptlogs/Create
        public IActionResult Create()
        {
            ViewData["Ptlplayer"] = new SelectList(_context.Players, "PlayerId", "PlayerId");
            ViewData["Ptlsession"] = new SelectList(_context.Sessions, "SessionId", "SessionId");
            return View();
        }

        // POST: Ptlogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ptlid,Ptltype,PtlDate,Ptlsession,Ptlmember,Ptlplayer,Ptlteam")] Ptlog ptlog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ptlog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Ptlplayer"] = new SelectList(_context.Players, "PlayerId", "PlayerId", ptlog.Ptlplayer);
            ViewData["Ptlsession"] = new SelectList(_context.Sessions, "SessionId", "SessionId", ptlog.Ptlsession);
            return View(ptlog);
        }

        // GET: Ptlogs/Edit/5
        public async Task<IActionResult> Edit(int? id, int pg=1)
        {
            if (id == null || _context.Ptlogs == null)
            {
                return NotFound();
            }

            var ptlog = await _context.Ptlogs.FindAsync(id);
            if (ptlog == null)
            {
                return NotFound();
            }
            ViewData["Ptlplayer"] = new SelectList(_context.Players, "PlayerId", "PlayerId", ptlog.Ptlplayer);
            ViewData["Ptlsession"] = new SelectList(_context.Sessions, "SessionId", "SessionId", ptlog.Ptlsession);
            ViewBag.returnPage = pg;
            return View(ptlog);
        }

        // POST: Ptlogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Ptlid,Ptltype,PtlDate,Ptlsession,Ptlmember,Ptlplayer,Ptlteam")] Ptlog ptlog)
        {
            if (id != ptlog.Ptlid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ptlog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PtlogExists(ptlog.Ptlid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Ptlplayer"] = new SelectList(_context.Players, "PlayerId", "PlayerId", ptlog.Ptlplayer);
            ViewData["Ptlsession"] = new SelectList(_context.Sessions, "SessionId", "SessionId", ptlog.Ptlsession);
            return View(ptlog);
        }

        // GET: Ptlogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Ptlogs == null)
            {
                return NotFound();
            }

            var ptlog = await _context.Ptlogs
                .Include(p => p.PtlplayerNavigation)
                .Include(p => p.PtlsessionNavigation)
                .FirstOrDefaultAsync(m => m.Ptlid == id);
            if (ptlog == null)
            {
                return NotFound();
            }

            return View(ptlog);
        }

        // POST: Ptlogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Ptlogs == null)
            {
                return Problem("Entity set 'OBGcoreContext.Ptlogs'  is null.");
            }
            var ptlog = await _context.Ptlogs.FindAsync(id);
            if (ptlog != null)
            {
                _context.Ptlogs.Remove(ptlog);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PtlogExists(int id)
        {
          return (_context.Ptlogs?.Any(e => e.Ptlid == id)).GetValueOrDefault();
        }
    }
}
