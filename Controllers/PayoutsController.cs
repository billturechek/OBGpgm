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
    public class PayoutsController : Controller
    {
        private readonly ObgDbContext _context;

        public PayoutsController(ObgDbContext context)
        {
            _context = context;
        }

        // GET: Payouts
        public IActionResult Index(int pg=1)
        {
            List<Payout> payouts = _context.Payouts
                .OrderByDescending(p => p.PayoutId)
                .ToList();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = payouts.Count();
            var pager = new Pager("Payouts", recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = payouts.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            this.ViewBag.returnPage = pg;
            return View(data);
            /*
            var oBGcoreContext = _context.Payouts.Include(p => p.Session).Include(p => p.Team);
            return View(await oBGcoreContext.ToListAsync());  */
        }

        // GET: Payouts/Details/5
        public async Task<IActionResult> Details(int? id, int pg=1)
        {
            if (id == null || _context.Payouts == null)
            {
                return NotFound();
            }

            var payout = await _context.Payouts
                .Include(p => p.Session)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.PayoutId == id);
            if (payout == null)
            {
                return NotFound();
            }

            ViewBag.returnPage = pg;
            return View(payout);
        }

        // GET: Payouts/Create
        public IActionResult Create()
        {
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId");
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamId");
            return View();
        }

        // POST: Payouts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PayoutId,SessionId,TeamId,Players,CaptainId,Player1Id,Player2Id,Player3Id,Player4Id,TotalPayout,Individual")] Payout payout)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payout);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", payout.SessionId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamId", payout.TeamId);
            return View(payout);
        }

        // GET: Payouts/Edit/5
        public async Task<IActionResult> Edit(int? id, int pg=1)
        {
            if (id == null || _context.Payouts == null)
            {
                return NotFound();
            }

            var payout = await _context.Payouts.FindAsync(id);
            if (payout == null)
            {
                return NotFound();
            }
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", payout.SessionId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamId", payout.TeamId);
            ViewBag.returnPage = pg;
            return View(payout);
        }

        // POST: Payouts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PayoutId,SessionId,TeamId,Players,CaptainId,Player1Id,Player2Id,Player3Id,Player4Id,TotalPayout,Individual")] Payout payout)
        {
            if (id != payout.PayoutId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payout);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PayoutExists(payout.PayoutId))
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
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", payout.SessionId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamId", payout.TeamId);
            return View(payout);
        }

        // GET: Payouts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Payouts == null)
            {
                return NotFound();
            }

            var payout = await _context.Payouts
                .Include(p => p.Session)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.PayoutId == id);
            if (payout == null)
            {
                return NotFound();
            }

            return View(payout);
        }

        // POST: Payouts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Payouts == null)
            {
                return Problem("Entity set 'OBGcoreContext.Payouts'  is null.");
            }
            var payout = await _context.Payouts.FindAsync(id);
            if (payout != null)
            {
                _context.Payouts.Remove(payout);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PayoutExists(int id)
        {
          return (_context.Payouts?.Any(e => e.PayoutId == id)).GetValueOrDefault();
        }
    }
}
