using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Repositories;
using OBGpgm.Models;

namespace OBGpgm.Controllers
{
    public class MembersController : Controller
    {
        private readonly IMemberRepository memberRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly IStateRepository stateRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IVillageRepository villageRepository;
        private readonly IVillagesStreetRepository villagesStreetRepository;
        private readonly ObgDbContext _context;

        public MembersController(ObgDbContext context,
                                IMemberRepository memberRepository,
                                IPlayerRepository playerRepository,
                                ISessionRepository sessionRepository,
                                IStateRepository stateRepository,
                                ITeamRepository teamRepository,
                                IVillageRepository villageRepository,
                                IVillagesStreetRepository villagesStreetRepository)
        {
            _context = context;
            this.memberRepository = memberRepository;
            this.playerRepository = playerRepository;
            this.sessionRepository = sessionRepository;
            this.stateRepository = stateRepository;
            this.teamRepository = teamRepository;
            this.villageRepository = villageRepository;
            this.villagesStreetRepository = villagesStreetRepository;
        }

        // GET: Members
        public IActionResult Index(int pg=1)              
        {
            List<Member> members = _context.Members
                .OrderBy(m=>m.LastName)
                .ThenBy(m=>m.FirstName)
                .ToList();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = members.Count();
            var pager = new Pager("Members", recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = members.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            this.ViewBag.returnPage = pg;
            return View(data);

            /*
            return _context.Members != null ? 
                          View(await _context.Members
                          .OrderBy(m=>m.LastName)
                          .ThenBy(m=>m.FirstName)
                          .ToListAsync()) :
                          Problem("Entity set 'OBGcoreContext.Members'  is null.");
            */
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id, int pg)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }
            ViewBag.returnPage = pg;
            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberId,LastName,FirstName,Address1,Address2,Zip,Telephone,Cellphone,Email,VillageId,Office,CurrentPlayerId,Evaluation,ShirtSize,Bday,IsPrivate,CurrentSignUpDate,UserId,PassWord,HasPaidAnnualDues,HasPaidSessionPrizeFund,IsActive,IsVerified,WillPlayNextSession,IsJabba,IsAdministrator,IsHonored,IsDeceased,GetsPrize,WillCaptainNextSession,WillCaptainIfNeeded,TeamNameIfCaptain,Village,Hometown,MovedFrom,Wife,YearMoved,Children,Grand,Great,Job,Interests,Military,YearsMilitary,WantsNoPicture,Snowbird")] Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id, int pg=1)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            ViewBag.returnPage = pg;
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberId,LastName,FirstName,Address1,Address2,Zip,Telephone,Cellphone,Email,VillageId,Office,CurrentPlayerId,Evaluation,ShirtSize,Bday,IsPrivate,CurrentSignUpDate,UserId,PassWord,HasPaidAnnualDues,HasPaidSessionPrizeFund,IsActive,IsVerified,WillPlayNextSession,IsJabba,IsAdministrator,IsHonored,IsDeceased,GetsPrize,WillCaptainNextSession,WillCaptainIfNeeded,TeamNameIfCaptain,Village,Hometown,MovedFrom,Wife,YearMoved,Children,Grand,Great,Job,Interests,Military,YearsMilitary,WantsNoPicture,Snowbird")] Member member)
        {
            if (id != member.MemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.MemberId))
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
            return View(member);
        }
        public async Task<IActionResult> InsertAsync()
        {
            await FillStatesAsync();
            await FillVillagesAsync();
            await FillVillagesStreetsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertAsync(MemberViewModel model)
        {
            await FillStatesAsync();
            await FillVillagesAsync();
            await FillVillagesStreetsAsync();
            if (ModelState.IsValid)
            {
                model.Member.Address1 = model.Member.Address1 + " " + model.streetName;
                memberRepository.Insert(model.Member);
                ViewBag.Message = "Member inserted successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Members == null)
            {
                return Problem("Entity set 'OBGcoreContext.Members'  is null.");
            }
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
          return (_context.Members?.Any(e => e.MemberId == id)).GetValueOrDefault();
        }
        public async Task<bool> FillStatesAsync()
        {
            List<State> listStates = stateRepository.SelectAll();
            List<SelectListItem> states = (from s in listStates
                                           select new SelectListItem()
                                           { Text = s.StateName, Value = s.StateAbbrev }).ToList();
            ViewBag.States = states;
            return true;
        }
        public async Task<bool> FillVillagesAsync()
        {
            List<Village> listVillages = villageRepository.SelectAll();
            List<SelectListItem> villages = (from v in listVillages
                                             select new SelectListItem()
                                             { Text = v.F1, Value = v.F1 }).ToList();
            ViewBag.Villages = villages;
            return true;
        }

        public async Task<bool> FillVillagesStreetsAsync()
        {
            /*  */
            List<VillagesStreet> listVillages = villagesStreetRepository.SelectAll();
            List<SelectListItem> villagesStreet = (from v in listVillages select new SelectListItem() { Text = (v.Prefix + " " + v.StreetName).Trim(), Value = (v.Prefix + " " + v.StreetName).Trim() }).ToList();
            ViewBag.VillagesStreets = villagesStreet;

            return true;
        }
    }
}
