using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using OBGpgm.Repositories;
using OBGpgm.ViewModels;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using Range = Microsoft.Office.Interop.Excel.Range;
using System.Numerics;
using System.Diagnostics.Metrics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System.Reflection.PortableExecutable;

namespace OBGpgm.Controllers
{
    public class DraftsController : Controller
    {
        private readonly ObgDbContext _context;
        private readonly HttpClient client = null;
        private readonly IDraftRepository draftRepository;
        private readonly IMemberRepository memberRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly ILogger logger;

        public DraftsController(ObgDbContext context,
                                HttpClient client,
                                IDraftRepository draftRepository,
                                IMemberRepository memberRepository,
                                IPlayerRepository playerRepository,
                                ISessionRepository sessionRepository,
                                ITeamRepository teamRepository,
                                IWebHostEnvironment hostEnvironment,
                                IConfiguration config,
                                ILogger<DraftsController> logger)
        {
            _context = context;
            this.client = client;
            this.draftRepository = draftRepository;
            this.memberRepository = memberRepository;
            this.playerRepository = playerRepository;
            this.sessionRepository = sessionRepository;
            this.teamRepository = teamRepository;
            this.hostEnvironment = hostEnvironment;
            this.logger = logger;
        }

        // GET: Drafts
        public IActionResult Index(int pg = 1)
        {
            List<Draft> drafts = _context.Drafts
                .OrderByDescending(d => d.DraftId)
                .ToList();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = drafts.Count();
            var pager = new Pager("Drafts", recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = drafts.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            this.ViewBag.returnPage = pg;
            return View(data);
            /*
            var oBGcoreContext = _context.Drafts.Include(d => d.DraftSession);
            return View(await oBGcoreContext.ToListAsync()); */
        }

        public async Task<IActionResult> ListAsync(string year, string season)
        {
            await FillMembersAsync();
            await FillYearsAsync();

            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            if (String.IsNullOrEmpty(year))
            {
                if (HttpContext.Session.GetString("Year") == null)
                {
                    year = csession.Year;
                    season = csession.Season.ToString();
                }
                else
                {
                    year = HttpContext.Session.GetString("Year");
                    season = HttpContext.Session.GetString("Season");
                }
            }

            await FillSeasonsAsync(year);
            if (year != HttpContext.Session.GetString("Year"))
            {
                SelectListItem temp = ViewBag.Seasons[0];
                season = temp.Value;
            }
            ViewData["Year"] = year;
            ViewData["Season"] = season;
            HttpContext.Session.SetString("Year", year);
            HttpContext.Session.SetString("Season", season);
            await FillPlayersAsync(year, season);
            List<Draft> data = draftRepository.SelectAllBySession(year, season);

            return View(data);
        }


        // GET: Drafts/Details/5
        public async Task<IActionResult> Details(int? id, int pg = 1)
        {
            if (id == null || _context.Drafts == null)
            {
                return NotFound();
            }

            var draft = await _context.Drafts
                .Include(d => d.DraftSession)
                .FirstOrDefaultAsync(m => m.DraftId == id);
            if (draft == null)
            {
                return NotFound();
            }

            ViewBag.returnPage = pg;
            return View(draft);
        }

        // GET: Drafts/Create
        public IActionResult Create()
        {
            ViewData["DraftSessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId");
            return View();
        }

        // POST: Drafts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DraftId,DraftType,DraftSessionId,DraftTeamId,DraftPlayerId,DraftRound,DraftPosition,DraftSelection,DraftDivision,DraftPreDraft")] Draft draft)
        {
            if (ModelState.IsValid)
            {
                _context.Add(draft);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DraftSessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", draft.DraftSessionId);
            return View(draft);
        }

        public async Task<IActionResult> AvailableList()
        {
            return View();
        }

            // GET: Drafts/Edit/5
            public async Task<IActionResult> Edit(int? id, int pg = 1)
        {
            if (id == null || _context.Drafts == null)
            {
                return NotFound();
            }

            var draft = await _context.Drafts.FindAsync(id);
            if (draft == null)
            {
                return NotFound();
            }
            ViewData["DraftSessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", draft.DraftSessionId);
            ViewBag.returnPage = pg;
            return View(draft);
        }

        // POST: Drafts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DraftId,DraftType,DraftSessionId,DraftTeamId,DraftPlayerId,DraftRound,DraftPosition,DraftSelection,DraftDivision,DraftPreDraft")] Draft draft)
        {
            if (id != draft.DraftId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(draft);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DraftExists(draft.DraftId))
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
            ViewData["DraftSessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", draft.DraftSessionId);
            return View(draft);
        }

        // GET: Drafts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Drafts == null)
            {
                return NotFound();
            }

            var draft = await _context.Drafts
                .Include(d => d.DraftSession)
                .FirstOrDefaultAsync(m => m.DraftId == id);
            if (draft == null)
            {
                return NotFound();
            }

            return View(draft);
        }

        // POST: Drafts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Drafts == null)
            {
                return Problem("Entity set 'OBGcoreContext.Drafts'  is null.");
            }
            var draft = await _context.Drafts.FindAsync(id);
            if (draft != null)
            {
                _context.Drafts.Remove(draft);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DraftExists(int id)
        {
            return (_context.Drafts?.Any(e => e.DraftId == id)).GetValueOrDefault();
        }
        //[Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> EnterAsync()
        {
            logger.LogInformation("Info Logging");
            await FillSessionsAsync();
            await FillTeamsAsync();
            await FillAvailableAsync();
            ViewBag.SetReturn = "false";
            ViewBag.SelectedPlayer = "Please select";
            return View();
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> EnterAsync(EnterDraftView model, string Command)
        {
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();
            if (Command == "Select")
            {

                if (model.Player != null && model.Team != null)
                {
                    Draft oldDraft = draftRepository.SelectByID(model.Draft.DraftId);

                    Player oldPlayer = playerRepository.SelectByID(model.Player.PlayerId);

                    oldDraft.DraftPlayerId = model.Player.PlayerId;
                    //oldDraft.DraftPreDraft = model.pre;
                    oldDraft.DraftPreDraft = model.Draft.DraftPreDraft;
                    oldPlayer.DraftId = model.Draft.DraftId;
                    oldPlayer.TeamId = model.Team.TeamId;

                    // Update draft entry
                    draftRepository.Update(oldDraft);
                    TempData["Message"] = "Player successfully added to team!";
                }
            }
            else if (Command == "Return to Available")
            {
                if (model.Player != null && model.Team != null)
                {
                    Player player = playerRepository.SelectByID(model.Player.PlayerId);
                    Draft draft = draftRepository.SelectByID(player.DraftId);
                    Session session = sessionRepository.SelectById(player.SessionId);

                    player.DraftId = 0;
                    player.TeamId = 0;
                    draft.DraftPlayerId = 0;
                    draft.DraftPreDraft = false;
                    playerRepository.Update(player);
                    draftRepository.Update(draft);
                    TempData["Message"] = "Player successfully returned to available!";
                }
            }
            return RedirectToAction("GetSelections", new { id = csession.SessionId.ToString() });
        }

        public async Task<IActionResult> SetPreDraft(int id)
        {
            string PreDraft = "false";
            if (id != 0)
            {
                PreDraft = "true";
                HttpContext.Session.SetString("PreDraft", PreDraft);
            }
            else
            {
                HttpContext.Session.Remove("PreDraft");
                PreDraft = null;
            }

            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            return RedirectToAction("GetSelections", new { id = csession.SessionId.ToString() });
        }
        public async Task<IActionResult> SetSelect(int id)
        {
            await FillTeamsAsync();
            await FillSelectionsAsync();
            await FillAvailableAsync();
            await FillSelectedAsync();

            ViewBag.SetReturn = "false";
            ViewBag.PlayerList = ViewBag.Available;
            Session csession = sessionRepository.SelectByCurrent();
            return RedirectToAction("GetSelections", new { id = csession.SessionId.ToString() });
        }
        public async Task<IActionResult> SetReturn(int id)
        {
            await FillTeamsAsync();
            await FillSelectionsAsync();
            await FillAvailableAsync();
            await FillSelectedAsync();

            if (HttpContext.Session.GetString("PreDraft") == null)
            {
                ViewBag.PreDraft = null;
            }
            else
            {
                ViewBag.PreDraft = HttpContext.Session.GetString("PreDraft");
            }

            ViewBag.SetReturn = "true";
            ViewBag.PlayerList = ViewBag.Selected;

            return View("Enter");
        }
        public async Task<IActionResult> GetSelections(int id, [FromQuery] string pid)
        {
            string session = id.ToString();
            await FillSessionsAsync(session);
            await FillTeamsAsync();
            await FillSelectionsAsync();
            await FillAvailableAsync();
            await FillSelectedAsync();

            if (HttpContext.Session.GetString("PreDraft") == null)
            {
                ViewBag.PreDraft = null;
            }
            else
            {
                ViewBag.PreDraft = HttpContext.Session.GetString("PreDraft");
            }


            if (pid != null)
            {
                List<SelectListItem> list = ViewBag.Available;
                foreach (SelectListItem item in list)
                {
                    if (pid == item.Value)
                    {
                        ViewBag.SelectedPlayer = item.Text;
                        ViewBag.SelectedValue = item.Value;
                        item.Selected = true;
                        break;
                    }
                }
                ViewBag.Available = list;
            }
            else
            {
                ViewBag.SelectedPlayer = "Please select";
            }

            if (ViewBag.Selections.Count > 0)
            {
                Draft thisEntry = new Draft();
                List<SelectListItem> selections = ViewBag.Selections;
                SelectListItem selectedItem = selections[0];
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                    int draftid = Convert.ToInt32(selectedItem.Value);

                    thisEntry = draftRepository.SelectByID(draftid);

                    ViewBag.DraftPosition = thisEntry.DraftPosition;
                    ViewBag.DraftRound = thisEntry.DraftRound;
                    ViewBag.Division = 1;
                    ViewBag.DraftID = thisEntry.DraftId;

                    var drTypes = from DraftTypes s in Enum.GetValues(typeof(DraftTypes))
                                  select new { ID = s, Name = s.ToString() };
                    ViewBag.DraftType = new SelectList(drTypes, "ID", "Name", thisEntry.DraftType);

                    Team thisTeam = teamRepository.SelectByID(thisEntry.DraftTeamId);

                    ViewBag.teamName = thisTeam.TeamName;
                    ViewBag.teamNumber = thisTeam.TeamNumber;
                    ViewBag.teamID = thisTeam.TeamId;

                    Player thisCaptain = playerRepository.SelectByTeamCaptain(thisEntry.DraftTeamId);

                    Member mem = memberRepository.SelectById(thisCaptain.MemberId);

                    ViewBag.captain = mem.FullName;
                }
                ViewBag.Selections = selections;
                ViewBag.PlayerList = ViewBag.Available;
            }
            return View("Enter");
        }


        public async Task<IActionResult> ReturnSelection(int id, [FromQuery] string pid)
        {            
            await FillSessionsAsync();
            await FillTeamsAsync();
            await FillSelectionsAsync();
            await FillAvailableAsync();
            await FillSelectedAsync();
            
            if (HttpContext.Session.GetString("PreDraft") == null)
            {
                ViewBag.PreDraft = null;
            }
            else
            {
                ViewBag.PreDraft = HttpContext.Session.GetString("PreDraft");
            }

            int playerid;
            Player player = new Player();

            if (pid != null)
            {
                List<SelectListItem> list = ViewBag.Selected;
                foreach (SelectListItem item in list)
                {
                    if (pid == item.Value)
                    {
                        ViewBag.SelectedPlayer = item.Text;
                        ViewBag.SelectedValue = item.Value;
                        item.Selected = true;

                        /*
                        playerid = Convert.ToInt32(ViewBag.SelectedValue);
                        player = playerRepository.SelectByID(playerid);
                        */
                        break;
                    }
                }
                ViewBag.Available = list;
            }
            else
            {
                ViewBag.SelectedPlayer = "Please select";
            }
            /*
            if(player != null && player.PlayerId != 0)
            {
                Draft draft = draftRepository.SelectByID(player.DraftId);
                Session session = sessionRepository.SelectById(player.SessionId);

                player.DraftId = 0;
                player.TeamId = 0;
                draft.DraftPlayerId = 0;
                playerRepository.Update(player);
                draftRepository.Update(draft);
            }
            */
            ViewBag.SetReturn = "true";

            return View("Enter");
        }

        /*
        public async Task<IActionResult> ReturnPlayer(string pid)
        {
            await FillSessionsAsync();
            await FillTeamsAsync();
            await FillSelectionsAsync();
            await FillAvailableAsync();
            await FillSelectedAsync();

            int playerid = Convert.ToInt32(ViewBag.SelectedValue);
            Player player = playerRepository.SelectByID(playerid);

            if (player != null && player.PlayerId != 0)
            {
                Draft draft = draftRepository.SelectByID(player.DraftId);
                Session session = sessionRepository.SelectById(player.SessionId);

                player.DraftId = 0;
                player.TeamId = 0;
                draft.DraftPlayerId = 0;
                playerRepository.Update(player);
                draftRepository.Update(draft);
            }

            Session csession = sessionRepository.SelectByCurrent();
            return RedirectToAction("GetSelections", new { id = csession.SessionId.ToString() });
        }

        */


            public async Task<bool> FillSelectionsAsync()
        {
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            List<Draft> listDrafts = draftRepository
                .SelectAllBySelection(csession.SessionId);
            List<SelectListItem> selections = (from d in listDrafts
                                               select new SelectListItem()
                                               { Text = d.DraftSelection.ToString(), Value = d.DraftId.ToString() })
                .ToList();
            ViewBag.Selections = selections;
            return true;
        }
        public async Task<bool> FillMembersAsync()
        {
            List<Member> listMembers = memberRepository.SelectAll();
            List<SelectListItem> members = (from m in listMembers
                                            select new SelectListItem()
                                            { Text = m.FullName, Value = m.MemberId.ToString() }).ToList();
            ViewBag.Members = listMembers;
            ViewBag.MemberId = members;
            return true;
        }

        public async Task<bool> FillPlayersAsync(string year, string season)
        {
            List<Player> listPlayers = playerRepository.SelectAllBySession(year, season);
            List<SelectListItem> players = (from p in listPlayers
                                            select new SelectListItem()
                                            { Text = p.PlayerId.ToString(), Value = p.PlayerId.ToString() }).ToList();
            ViewBag.PlayerId = players;
            ViewBag.Players = listPlayers;
            return true;
        }

        public async Task<bool> FillSessionsAsync(string session = "")
        {
            IEnumerable<Session> listSessions = sessionRepository.SelectAll();
            List<SelectListItem> sessions = (from s in listSessions
                                             select new SelectListItem()
                                             {
                                                 Text = s.SessionId.ToString(),
                                                 Value = s.SessionId.ToString()
                                             }).ToList();
            SelectListItem selectedItem = (from i in sessions where i.Value == session select i).SingleOrDefault();
            if (selectedItem != null)
            {
                selectedItem.Selected = true;
            }
            ViewBag.Sessions = sessions;
            return true;
        }

        public async Task<bool> FillAvailableAsync()
        {
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            List<Player> listPlayers = playerRepository.SelectAllByAvailable(csession.SessionId);
            List<Player> availPlayers = new List<Player>();
            foreach (Player p in listPlayers)
            {
                if (p.IsInDraft)
                {
                    if (p.MemberId > 0)
                    {
                        if (p.TeamId == 0)
                        {
                            Member mem = memberRepository.SelectById(p.MemberId);
                            if (mem.IsActive)
                            {
                                p.Member = mem;
                                availPlayers.Add(p);
                            }
                        }
                    }
                }
            }
            List<SelectListItem> list = (from p in availPlayers
                                         select new SelectListItem()
                                         {
                                             Text = p.Member.ReverseName,
                                             Value = p.PlayerId.ToString()
                                         }).ToList();
            ViewBag.Available = list;
            return true;
        }

        public async Task<bool> FillSelectedAsync()
        {
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            List<Player> listPlayers = playerRepository.SelectAllBySelected(csession.SessionId);
            List<Player> selectedPlayers = new List<Player>();
            foreach (Player p in listPlayers)
            {
                if (p.IsInDraft)
                {
                    if (p.MemberId > 0)
                    {
                        if (p.TeamId != 0)
                        {
                            if (p.IsCaptain != true)
                            {
                                Member mem = memberRepository.SelectById(p.MemberId);
                                if (mem.IsActive)
                                {
                                    p.Member = mem;
                                    selectedPlayers.Add(p);
                                }
                            }
                        }
                    }
                }
            }
            List<SelectListItem> list = (from p in selectedPlayers
                                         select new SelectListItem()
                                         {
                                             Text = p.Member.ReverseName,
                                             Value = p.PlayerId.ToString()
                                         }).ToList();
            ViewBag.Selected = list;
            return true;
        }


        public async Task<bool> FillTeamsAsync()
        {
            List<Team> listTeams = teamRepository.SelectAll();
            List<SelectListItem> teams = (from t in listTeams
                                          select new SelectListItem()
                                          {
                                              Text = (t.SessionId.ToString() + " - " + t.TeamNumber.ToString()),
                                              Value = t.TeamId.ToString()
                                          }).ToList();
            ViewBag.TeamId = teams;
            return true;
        }

        public async Task<bool> FillYearsAsync()
        {
            List<string> listSessions = sessionRepository.SelectByYears();
            List<SelectListItem> sessions = (from s in listSessions
                                             select new SelectListItem()
                                             { Text = s, Value = s }).ToList();
            ViewBag.Years = sessions;
            return true;
        }

        public async Task<bool> FillSeasonsAsync(string year)
        {
            List<string> listSeasons = sessionRepository.SelectAllSeasons(year);
            List<SelectListItem> seasons = (from s in listSeasons
                                            select new SelectListItem()
                                            {
                                                Text = Enum.GetName(typeof(snType), int.Parse(s)),
                                                Value = s
                                            }).ToList();
            ViewBag.Seasons = seasons;
            return true;
        }


        public IActionResult MakeSheet(int session)
        {
            Session theSession = sessionRepository.SelectById(session);
            var theSeason = theSession.Season.ToString();
            //    (snType)int.Parse(theSession.Season);
            DateTime sDate = DateTime.Parse(theSession.StartDate);

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "DraftResults" + schedDate + "0" + theSession.Season;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/DraftOrder/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/DraftOrder/pdf/" + fileName;

            List<Team> teams = teamRepository.SelectAllBySession(session.ToString());
            List<Draft> drafts = draftRepository.SelectAllBySession(theSession.Year, theSession.Season.ToString());
            List<Player> captains = playerRepository.SelectAllByNewCaptain(session);


            // Initialize Excel workbook
            Excel.Application xla = new Excel.Application();
            Excel.Workbook xlb = xla.Workbooks.Add();
            Excel.Worksheet xls = (Excel.Worksheet)xlb.Worksheets.get_Item(1);
            Excel.Range xlr;

            // Now set up the page
            xls.PageSetup.Orientation = XlPageOrientation.xlLandscape;
            xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.TopMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.BottomMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.HeaderMargin = xla.InchesToPoints(0);
            xls.PageSetup.FooterMargin = xla.InchesToPoints(0);
            xls.PageSetup.CenterHorizontally = true;
            xls.PageSetup.CenterVertically = true;


            xlr = xls.get_Range(xls.Columns[1], xls.Columns[16]);
            xlr.NumberFormat = "@";
            xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            xlr.Font.Size = 10;


            int r = 1;
            int c = 1;

            int i;
            int j;
            int k;

            int n = teams.Count();
            int hRow = 1;
            int startRow = 2;

            int cTeam = 1;
            int cFirst = 1;
            int cRound1 = 2;
            int cRound2 = 3;
            int cRound3 = 4;
            int cLast = 4;
            int numCol;
            int nameCol;


            xlr = xls.get_Range(xls.Columns[cTeam], xls.Columns[cTeam]);
            xlr.ColumnWidth = 6;
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            xlr.VerticalAlignment = XlVAlign.xlVAlignCenter;

            xls.get_Range(xls.Columns[cRound1], xls.Columns[cRound1]).ColumnWidth = 30;
            xls.get_Range(xls.Columns[cRound2], xls.Columns[cRound2]).ColumnWidth = 30;
            xls.get_Range(xls.Columns[cRound3], xls.Columns[cRound3]).ColumnWidth = 30;


            //  This is the header line
            xls.Cells[hRow, cTeam] = "Team";
            xls.Cells[hRow, cRound1] = "Round 1";
            xls.Cells[hRow, cRound2] = "Round 2";
            xls.Cells[hRow, cRound3] = "Round 3";

            xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[1, cLast]);
            xlr.Font.Bold= true;    
            xlr.Font.Italic= true; 
            xlr.Font.Size = 12;
            xlr.HorizontalAlignment=XlHAlign.xlHAlignCenter;


            //  Now make the lines for the draft selections
            for (i = 0; i < drafts.Count; i++)
            {
                Draft d = drafts[i];       //  Get next draft entry

                if (d.DraftSelection == 0)
                {
                    //  DraftSelection == 0  Indicates this is a captain
                    //  initialize his selections in all three rounds

                    Player p = new Player();
                    p = playerRepository.SelectByID(d.DraftPlayerId);
                    Team t = new Team();
                    t = teamRepository.SelectByID(d.DraftTeamId);
                    // Make cell for team number
                    xls.Cells[startRow + (i * 2), cTeam] = t.TeamNumber.ToString();
                    xlr = xls.get_Range(xls.Cells[startRow + (i * 2), cTeam], xls.Cells[startRow + (i * 2) + 1, cTeam]);
                    xlr.Merge();
                    xlr.VerticalAlignment = XlVAlign.xlVAlignCenter;
                    xlr.Font.Bold = true;

                    xls.Cells[startRow + (i * 2), cRound1] = 
                        (i + 1).ToString() + ". " + p.Member.FullName;
                    xls.Cells[startRow + (i * 2), cRound2] = 
                        (i + n + 1).ToString() + ".  " + p.Member.FullName;
                    xls.Cells[startRow + (n - i - 1) * 2, cRound3] = 
                        ((3 * n) - i).ToString() + ".  " + p.Member.FullName;
                }
                else
                {
                    //  It's not a captain, so determine in which column the entry belongs
                    if(d.DraftRound == 1)
                    {
                        numCol= cRound1;
                    }
                    else if (d.DraftRound==2)
                    {
                        numCol= cRound2;
                    }
                    else
                    {
                        numCol= cRound3;
                    }
                    // Has a player been selected for this slot?
                    if (d.DraftPlayerId != 0)
                    {
                        Player p = playerRepository.SelectByID(d.DraftPlayerId);
                        if (p != null)
                        {
                            xls.Cells[startRow + ((d.DraftPosition - 1) * 2) + 1, numCol] =
                                p.Member.FullName.Trim();
                        }
                        else
                        {
                            xls.Cells[startRow + ((d.DraftPosition - 1) * 2) + 1, numCol] = "Player quit";
                        }
                        xlr = xls.get_Range(xls.Cells[startRow + ((d.DraftPosition - 1) * 2) + 1, numCol],
                            xls.Cells[startRow + ((d.DraftPosition - 1) * 2) + 1, numCol]);
                        xlr.Font.Bold = true;
                        xlr.Font.Italic = true;
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                        if (d.DraftPreDraft)
                        {
                            xlr = xls.get_Range(xls.Cells[startRow + (d.DraftPosition - 1) * 2, numCol],
                                xls.Cells[startRow + ((d.DraftPosition - 1) * 2) + 1, numCol]);
                            xlr.Interior.Color = XlRgbColor.rgbLightGray;
                        }
                    }
                }

                // Split output into two sheets each with half the teams if we are half way through
                if (i == (n / 2))
                {
                    xlr = xls.get_Range(xls.Cells[startRow + (i * 2), cFirst], 
                                        xls.Cells[startRow + (i * 2), cFirst]);
                    //xlr.PageBreak = XlPageBreak.xlPageBreakManual;
                    xls.HPageBreaks.Add(xls.get_Range(xls.Rows[startRow + (i * 2)], xls.Rows[startRow + (i * 2)]));
                }
            }

            // Now format the boxes around the entries
            xlr = xls.get_Range(xls.Cells[1, cFirst], xls.Cells[hRow + captains.Count() * 2, cLast]);
            xlr.Borders[XlBordersIndex.xlDiagonalDown].LineStyle = XlLineStyle.xlLineStyleNone;
            xlr.Borders[XlBordersIndex.xlDiagonalUp].LineStyle = XlLineStyle.xlLineStyleNone;


            for (i = 0; i < n; i++)
            {
                for (j = 1; j < 5; j++)
                {
                    xlr = xls.get_Range(xls.Cells[startRow + (i * 2), j],
                            xls.Cells[startRow + (i * 2) + 1, j]);
                    boxSlot(xlr);
                }
            }

            //  Outline header row with thick box
            xlr = xls.get_Range(xls.Cells[hRow, cFirst],
                        xls.Cells[hRow, cRound3]);
            boxOutline(xlr);

            //  Outline the top of the box with thick border
            xlr = xls.get_Range(xls.Cells[1, cFirst],
                    xls.Cells[hRow + n, cRound3]);
            boxOutline(xlr);

            //  Outline the bottom of box with thick border so both pages are boxed
            xlr = xls.get_Range(xls.Cells[startRow + n, cFirst],
                    xls.Cells[hRow + (n * 2), cRound3]);
            boxOutline(xlr);

            // Outline the team column and fill with yellow background
            xlr = xls.get_Range(xls.Cells[1, cFirst],
                    xls.Cells[hRow + (n * 2), cFirst]);
            boxOutline(xlr);
            xlr.Interior.ColorIndex = 6;
            xlr.Font.Size = 12;
            xlr.Font.Bold= true;    



            //  Save the worksheet and close the workbook          
            xlb.SaveAs(SaveFileName + ".xlsx");
            xlb.ExportAsFixedFormat(
                Excel.XlFixedFormatType.xlTypePDF,
                SavePdfName,
                Excel.XlFixedFormatQuality.xlQualityStandard,
                true,
                true,
                1,
                10,
                false);
            xlb.Close();



            return RedirectToAction("GetSelections", new { id = session });
        }
        public IActionResult MakeAvailable(int session)
        {
            Session theSession = sessionRepository.SelectByCurrent();
            var theSeason = theSession.Season.ToString();
                //(snType)int.Parse(theSession.Season);
            DateTime sDate = DateTime.Parse(theSession.StartDate);

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "DraftPool" + schedDate + "0" + theSession.Season;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/DraftPool/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/DraftPool/pdf/" + fileName;


            int r = 1;
            int c = 1;
            int i;
            int j;
            int k;
            Player p = new Player();
            int hRow;
            int startRow = 2;
            int endRow = 2;
            bool captains = false;
            bool notFound = false;
            int numCaptains = 0;
            int sht = 0;
            string sheet = "AllPlayers";
            Session s = sessionRepository.SelectByCurrent();

            // Initialize Excel workbook
            Excel.Application xla = new Excel.Application();
            Excel.Workbook xlb = xla.Workbooks.Add();
            Excel.Worksheet xls = (Excel.Worksheet)xlb.Worksheets.get_Item(1);
            Excel.Range xlr;
            Excel.Range xlr2;

            xlb = xla.Workbooks.Add();

            for (sht = 0; sht < 2; sht++)
            {
                if (sht == 0)
                {
                    captains = true;
                    sheet = "AllPlayers";
                }
                else
                {
                    captains = false;
                    sheet = "Players";
                }

                List<Player> playerList = new List<Player>();
                playerList = playerRepository.SelectAllByAvailable(captains, s.SessionId);
                if (playerList.Count > 0)
                {
                    xls = (Excel.Worksheet)xlb.Worksheets.Add();
                    xls.Activate();
                    xls.Name = sheet;


                    // Now set up the page
                    string topHeader = "&24 " + theSeason + " " + s.Year;
                    topHeader = topHeader + " - Player Draft Pool";
                    string botFooter = " Rnd=Round Drafted        Selct #=Overall selection number";
                    xls.PageSetup.Orientation = XlPageOrientation.xlPortrait;
                    xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.TopMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.BottomMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.HeaderMargin = xla.InchesToPoints(0);
                    xls.PageSetup.FooterMargin = xla.InchesToPoints(0);
                    xls.PageSetup.CenterHorizontally = true;
                    xls.PageSetup.CenterVertically = true;

                    xls.PageSetup.PrintTitleRows = "$1:$1";

                    xls.PageSetup.TopMargin = xla.InchesToPoints(0.85);
                    xls.PageSetup.BottomMargin = xla.InchesToPoints(0.61);
                    xls.PageSetup.HeaderMargin = xla.InchesToPoints(0.17);
                    xls.PageSetup.FooterMargin = xla.InchesToPoints(0.39);

                    xls.PageSetup.LeftHeader = "";
                    xls.PageSetup.CenterHeader = topHeader;
                    xls.PageSetup.RightHeader = "";
                    xls.PageSetup.LeftFooter = "";
                    xls.PageSetup.CenterFooter = botFooter;
                    xls.PageSetup.RightFooter = "Page &P of  &N";

                    xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    xlr = xls.get_Range(xls.Columns[2], xls.Columns[2]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;

                    xlr = xls.get_Range(xls.Columns[3], xls.Columns[10]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 6;  //Number for selection call
                    xls.get_Range(xls.Columns[2], xls.Columns[2]).ColumnWidth = 20; //Name
                    xls.get_Range(xls.Columns[3], xls.Columns[3]).ColumnWidth = 16; //Telephone
                    xls.get_Range(xls.Columns[4], xls.Columns[4]).ColumnWidth = 6;  //Last Draft Round
                    xls.get_Range(xls.Columns[5], xls.Columns[5]).ColumnWidth = 6;  //Last Draft Number
                    xls.get_Range(xls.Columns[6], xls.Columns[6]).ColumnWidth = 6;  //Previous Draft Round
                    xls.get_Range(xls.Columns[7], xls.Columns[7]).ColumnWidth = 6;  //Previous Draft Number
                    xls.get_Range(xls.Columns[8], xls.Columns[8]).ColumnWidth = 6;  //Previous Draft Round
                    xls.get_Range(xls.Columns[9], xls.Columns[9]).ColumnWidth = 6;  //Previous Draft Number

                    hRow = 1;
                    xls.Cells[hRow, 1] = "#";
                    xls.Cells[hRow, 2] = "Name";
                    xls.Cells[hRow, 3] = "Telephone";
                    xls.Cells[hRow, 4] = "Rnd";
                    xls.Cells[hRow, 5] = "Selct #";
                    xls.Cells[hRow, 6] = "Rnd";
                    xls.Cells[hRow, 7] = "Selct #";
                    xls.Cells[hRow, 8] = "Rnd";
                    xls.Cells[hRow, 9] = "Selct #";

                    xlr = xls.get_Range(xls.Rows[1], xls.Rows[1]);
                    xlr.Font.Bold= true;    

                    startRow = hRow + 1;
                    for (i = 0; i < playerList.Count(); i++)
                    {
                        p = playerList[i];

                        xls.Cells[startRow + i, 1] = (i + 1).ToString();

                        if (p.IsCaptain)
                        {
                            xls.Cells[startRow + i, 2] = "*" + p.Member.ReverseName;
                        }
                        else
                        {
                            xls.Cells[startRow + i, 2] = p.Member.ReverseName;
                        }
                        xls.Cells[startRow + i, 3] = p.Member.Telephone;

                        notFound = true;

                        List<Player> draftList = new List<Player>();
                        draftList = playerRepository.SelectAllByMember(p.MemberId);

                        if(draftList.Count > 0)
                        {
                            int max = 3;
                            if (draftList.Count < max)
                                max = draftList.Count;

                            for (k = 0; k < max; k++)
                            {
                                Player px = draftList[k];
                                if (px.SessionId != s.SessionId)
                                {
                                    if (px.DraftId != 0)
                                    {
                                        Draft dr = new Draft();
                                        dr = draftRepository.SelectByID(px.DraftId);
                                        if (dr.DraftRound == 0)
                                        {
                                            xls.Cells[startRow + i, 4 + (2 * k)] = "Captain";
                                        }
                                        else
                                        {
                                            xls.Cells[startRow + i, 4 + (2 * k)] = 
                                                dr.DraftRound.ToString();
                                        }  
                                        if (px.IsCaptain)
                                            xls.Cells[startRow + i, 4 + (2 * k)] = "Captain";

                                        xls.Cells[startRow + i, 5 + (2 * k)] = 
                                            dr.DraftSelection.ToString();
                                        dr = null;
                                    }
                                }
                            }
                        }

                        //  Put fill pattern in empty cells
                        for (k = 0; k < 3; k++)
                        {
                            xlr = xls.get_Range(xls.Cells[startRow + i, 4 + (2 * k)], 
                                    xls.Cells[startRow + i, 4 + (2 * k)]);

                            xlr2 = xls.get_Range(xls.Cells[startRow + i, 5 + (2 * k)],
                                    xls.Cells[startRow + i, 5 + (2 * k)]);

                            if (xlr.Value2 == null  && xlr2.Value2 == null)
                            {
                                xlr.Interior.ColorIndex = 0;
                                xlr.Interior.Pattern = XlPattern.xlPatternGray8;
                                xlr.Interior.PatternColorIndex = XlColorIndex.xlColorIndexAutomatic;

                                xlr2.Interior.ColorIndex = 0;
                                xlr2.Interior.Pattern = XlPattern.xlPatternGray8;
                                xlr2.Interior.PatternColorIndex = XlColorIndex.xlColorIndexAutomatic;
                            }
                        }
                        endRow = startRow + i;
                    }   
                }
                xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[endRow, 9]);
                boxInterior(xlr);
                boxOutline(xlr);

                xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[1, 9]);
                boxOutline(xlr);

                xlr = xls.get_Range(xls.Cells[1, 8], xls.Cells[endRow, 9]);
                boxOutline(xlr);

                xlr = xls.get_Range(xls.Cells[1, 6], xls.Cells[endRow, 7]);
                boxOutline(xlr);

                xlr = xls.get_Range(xls.Cells[1, 4], xls.Cells[endRow, 5]);
                boxOutline(xlr);
            }



            //  Save the worksheet and close the workbook          
            xlb.SaveAs(SaveFileName + ".xlsx");
            xlb.ExportAsFixedFormat(
                Excel.XlFixedFormatType.xlTypePDF,
                SavePdfName,
                Excel.XlFixedFormatQuality.xlQualityStandard,
                true,
                true,
                1,
                10,
                false);
            xlb.Close();
            TempData["Message"] = "Available player list successfully built!";

            return View("AvailableList");
        }

            private void boxSlot(Range xlr)
        {
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            return;
        }

        private void boxInterior(Range xlr)
        {
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlInsideVertical].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlInsideVertical].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlInsideHorizontal].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlInsideHorizontal].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            return;
        }
        private void boxOutline(Range xlr)
        {
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlThick;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlThick;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlThick;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlThick;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].ColorIndex = XlColorIndex.xlColorIndexAutomatic;
            return;
        }
    }
}

