using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using OBGpgm.Repositories;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace OBGpgm.Controllers
{
    public class PlayersController : Controller
    {
        private readonly ObgDbContext _context;
        private readonly IMemberRepository memberRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly IPortraitRepository portraitRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly IStateRepository stateRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IWebHostEnvironment hostEnvironment;

        public PlayersController(ObgDbContext context,
                                IMemberRepository memberRepository,
                                IPlayerRepository playerRepository,
                                IPortraitRepository portraitRepository,
                                ISessionRepository sessionRepository,
                                IStateRepository stateRepository,
                                IWebHostEnvironment hostEnvironment,
                                ITeamRepository teamRepository)
        {
            _context = context;
            this.memberRepository = memberRepository;
            this.playerRepository = playerRepository;
            this.portraitRepository = portraitRepository;
            this.sessionRepository = sessionRepository;
            this.stateRepository = stateRepository;
            this.teamRepository = teamRepository;
            this.hostEnvironment = hostEnvironment;
        }

        // GET: Players
        public IActionResult Index(int pg=1)
        {
            List<Player> players = _context.Players
                .Include(p => p.Member)
                .Include(p => p.Session)
                .Include(p => p.Team)
                .OrderByDescending(p => p.PlayerId)
                .ToList();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = players.Count();
            var pager = new Pager("Players", recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = players.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            this.ViewBag.returnPage = pg;
            return View(data);
            /*
            var oBGcoreContext = _context.Players.Include(p => p.Draft).Include(p => p.Member).Include(p => p.Session).Include(p => p.Team);
            return View(await oBGcoreContext.ToListAsync()); */
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id, int pg=1)
        {
            if (id == null || _context.Players == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Draft)
                .Include(p => p.Member)
                .Include(p => p.Session)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.PlayerId == id);
            if (player == null)
            {
                return NotFound();
            }

            ViewBag.returnPage = pg;
            return View(player);
        }

        // GET: Players/Get/5
        public IActionResult Get(int id)
        {
            Player player = playerRepository.SelectByID(id);
            if (player == null)
            {
                return NotFound();
            }
            return View(player);
        }

        // GET: Players/Create
        public IActionResult Create()
        {
            ViewData["DraftId"] = new SelectList(_context.Drafts, "DraftId", "DraftId");
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "MemberId");
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId");
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamId");
            return View();
        }

        // POST: Players/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlayerId,SessionId,TeamId,MemberId,DraftId,StartWeek,EndWeek,IsPlaying,IsCaptain,SkillLevel,DraftRound,IsInDraft,IsBeingTraded")] Player player)
        {
            if (ModelState.IsValid)
            {
                _context.Add(player);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DraftId"] = new SelectList(_context.Drafts, "DraftId", "DraftId", player.DraftId);
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "MemberId", player.MemberId);
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", player.SessionId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamId", player.TeamId);
            return View(player);
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id, int pg=1)
        {
            if (id == null || _context.Players == null)
            {
                return NotFound();
            }

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            ViewData["DraftId"] = new SelectList(_context.Drafts, "DraftId", "DraftId", player.DraftId);
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "MemberId", player.MemberId);
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", player.SessionId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamId", player.TeamId);
            ViewBag.returnPage = pg;
            return View(player);
        }

        // POST: Players/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlayerId,SessionId,TeamId,MemberId,DraftId,StartWeek,EndWeek,IsPlaying,IsCaptain,SkillLevel,DraftRound,IsInDraft,IsBeingTraded")] Player player)
        {
            if (id != player.PlayerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(player);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.PlayerId))
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
            ViewData["DraftId"] = new SelectList(_context.Drafts, "DraftId", "DraftId", player.DraftId);
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "MemberId", player.MemberId);
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", player.SessionId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamId", player.TeamId);
            return View(player);
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Players == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Draft)
                .Include(p => p.Member)
                .Include(p => p.Session)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.PlayerId == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Players == null)
            {
                return Problem("Entity set 'OBGcoreContext.Players'  is null.");
            }
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                _context.Players.Remove(player);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> TeamRosterAsync(string year, string season, string number)
        {
            await FillMembersAsync();
            await FillYearsAsync();

            if (number == null || number == "")
            {
                number = "1";
            }
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            if (String.IsNullOrEmpty(year))
            {
                if (HttpContext.Session.GetString("Year") == null)
                {
                    year = csession.Year;
                    season = csession.Season;
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


            string seasonName = Enum.GetName(typeof(snType), Convert.ToInt32(season));
            ViewData["SeasonName"] = seasonName;
            ViewData["Year"] = year;
            ViewData["Season"] = season;
            HttpContext.Session.SetString("Year", year);
            HttpContext.Session.SetString("Season", season);


            Session s = sessionRepository.SelectBySeason(year, season);
            if (s == null)
            {
                TempData["Message"] = "No data for requested season!";
                HttpContext.Session.SetString("Year", csession.Year);
                HttpContext.Session.SetString("Season", csession.Season);
                return RedirectToAction("List2");
            }
            await FillTeams3Async(s);
            await FillCaptainsAsync(s.SessionId);
            await FillImagesAsync();
            await FillNonCaptainsAsync(s.SessionId);

            if (s.SessionId > 24) { ViewBag.Named = "True"; }
            else { ViewBag.Named = "False"; }

            int teamNumber = int.Parse(number);
            ViewBag.TeamNumber = teamNumber;
            ViewData["Number"] = teamNumber;
            Team theTeam = teamRepository.SelectIdByNumber(s.SessionId, teamNumber);
            ViewBag.TeamName = theTeam.TeamName.Trim();

            List<Team> teams = teamRepository.SelectAllByNumberSeason(year, season);
            ViewBag.Teams = teams;

            List<Player> rosterData = new List<Player>();
            for (int i = 0; i < teams.Count; i++)
            {
                if (teams[i].TeamNumber == teamNumber)
                {
                    rosterData = playerRepository.SelectAllByTeam(teams[i].TeamId);
                    break;
                }
            }

            List<Player> data = playerRepository.SelectAllBySession(year, season);

            return View(rosterData);
        }


        public async Task<IActionResult> ListAsync(string year, string season)
        {
            await FillMembersAsync();
            await FillTeams2Async();
            await FillYearsAsync();
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            if (String.IsNullOrEmpty(year))
            {
                if (HttpContext.Session.GetString("Year") == null)
                {
                    year = csession.Year;
                    season = csession.Season;
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

            List<Team> teams = teamRepository.SelectAllByNumberSeason(year, season);
            ViewBag.Teams = teams;

            List<Player> data = playerRepository.SelectAllBySession(year, season);
            
            return View(data);
        }

        public async Task<IActionResult> ListBdaysAsync(string year, string season, string sort)
        {
            await FillMembersAsync();
            await FillTeams2Async();
            await FillYearsAsync();
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            if (String.IsNullOrEmpty(year))
            {
                if (HttpContext.Session.GetString("Year") == null)
                {
                    year = csession.Year;
                    season = csession.Season;
                    if(sort == null)
                    {
                        sort = "Name";
                    }
                    await FillSeasonsAsync(year);
                }
                else
                {
                    year = HttpContext.Session.GetString("Year");
                    season = HttpContext.Session.GetString("Season");
                }
            }
            else
            {
                sort = HttpContext.Session.GetString("Sort");
                if (sort == null || sort == "Name" || sort == "")
                {
                    sort = "Name";
                }
                else
                {
                    sort = "Age";
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
            ViewData["Sort"] = sort;
            HttpContext.Session.SetString("Year", year);
            HttpContext.Session.SetString("Season", season);
            HttpContext.Session.SetString("Sort", sort);

            List<Team> teams = teamRepository.SelectAllByNumberSeason(year, season);
            ViewBag.Teams = teams;

            List <Player> data = playerRepository.SelectAllBySession(year, season, sort);

            return View(data);
        }

        public IActionResult MakeBdSheet(string year, string season)
        {
            Session theSession = sessionRepository.SelectBySeason(year, season);
            DateTime startDate;
            DateTime sDate = DateTime.Parse(theSession.StartDate);
            if (theSession.CurrentSeason)
            {
                startDate= DateTime.Now;
            }
            else 
            {
                startDate = sDate;
            }
            var theSeason = (snType)int.Parse(theSession.Season);
            //List<Schedule> schedule = scheduleRepository.SelectAllBySessionId(id);
            //int teams = schedule.ElementAt(0).Teams;

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "Birthdays" + schedDate + "0" + theSession.Season;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/Birthdays/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/Birthdays/pdf/" + fileName;

            int r = 1;
            int c = 1;
            int i, j, k, x;
            int sheet;
            int hRow = 1;
            int years = 1;

            List<Player> playerList = new List<Player>();
            Player p = new Player();


            // Initialize Excel workbook/*
            Excel.Application xla = new Excel.Application();
            Excel.Workbook xlb = xla.Workbooks.Add();
            Excel.Worksheet xln = (Excel.Worksheet)xlb.Worksheets.get_Item(1);
            Excel.Range xlr;


            for (k = 0; k < 2; k++)
            {
                xln = (Excel.Worksheet)xlb.Sheets.Add(After: xlb.Sheets[xlb.Sheets.Count]);
                xln.Name = "K" + k.ToString();
            }

            foreach (Worksheet xls in xlb.Sheets)
            {
                if (xls.Name == "Sheet1")
                {
                    xls.Delete();
                    break;
                }
            }

            for (sheet = 0; sheet < 2; sheet++)
            {
                Excel.Worksheet xls = (Excel.Worksheet)xlb.Worksheets.get_Item(sheet + 1);
                xls.Activate();
                if (sheet == 0)
                {
                    playerList = playerRepository.SelectAllByBday(year, season, "Chron");
                    xls.Name = "Chron";
                }
                else if (sheet == 1)
                {
                    playerList = playerRepository.SelectAllByBday(year, season, "Name");
                    xls.Name = "Alpha";
                }

                string topHeader = "&18 &B " + theSeason.ToString() + " " + theSession.Year;

                if (playerList.Count() > 0)
                {
                    xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    xlr = xls.get_Range(xls.Columns[2], xls.Columns[3]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;

                    xlr = xls.get_Range(xls.Columns[4], xls.Columns[5]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 4;  //'#
                    xls.get_Range(xls.Columns[2], xls.Columns[2]).ColumnWidth = 20; //'Name
                    xls.get_Range(xls.Columns[3], xls.Columns[3]).ColumnWidth = 15; //'Telephone
                    xls.get_Range(xls.Columns[4], xls.Columns[4]).ColumnWidth = 12; //'Bday
                    xls.get_Range(xls.Columns[5], xls.Columns[5]).ColumnWidth = 5; //'Age
                    hRow = 1;

                    xls.Cells[hRow, 1] = "#";
                    xls.Cells[hRow, 2] = "Name";
                    xls.Cells[hRow, 3] = "Telephone";
                    xls.Cells[hRow, 4] = "Birthday";
                    xls.Cells[hRow, 5] = "Age";
                    xlr = xls.get_Range(xls.Rows[1], xls.Rows[1]);
                    xlr.Font.Bold= true;

                    for ( j=1; j < playerList.Count() + 1; j++)
                    {
                        i = j; // This sets up single spacing
                        p = playerList[j - 1];

                        xls.Cells[hRow + i, 1] = i.ToString();
                        xls.Cells[hRow + i, 2] = p.Member.FullName.Trim();
                        xls.Cells[hRow + i, 3] = p.Member.Telephone.Trim();
                        DateTime temp = (DateTime)p.Member.Bday;
                        xls.Cells[hRow + i, 4] = temp.ToShortDateString();
                        
                        years = startDate.Year - temp.Year;
                        if ((startDate.Month < temp.Month) ||
                            ((startDate.Month == temp.Month) &&
                            (startDate.Day < temp.Day)))
                        {
                            years = years - 1;
                        }
                        xls.Cells[hRow + i, 5] = years.ToString();
                        if(p.Member.IsDeceased)
                        {
                            xlr = xls.get_Range(xls.Cells[hRow + i, 1], xls.Cells[hRow + i, 5]);
                            xlr.Interior.Color = XlRgbColor.rgbLightGray;
                        }
                        
                    }
                }
                if (sheet == 0)
                {
                    topHeader += " - Chronologic Birthdays";
                }
                else if (sheet == 1)
                {
                    topHeader += " - Alphabetic Birthdays";
                }

                xlr = xls.get_Range(xls.Cells[hRow, 1], xls.Cells[hRow + playerList.Count, 5]);
                boxInterior(xlr);
                boxOutline(xlr);

                // Now set up the page
                xls.PageSetup.Orientation = XlPageOrientation.xlPortrait;
                xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                xls.PageSetup.CenterHorizontally = true;
                xls.PageSetup.CenterVertically = true;
                xls.PageSetup.LeftHeader = "";
                xls.PageSetup.CenterHeader = topHeader;
                xls.PageSetup.RightHeader = "";
                xls.PageSetup.LeftFooter = "";
                xls.PageSetup.CenterFooter = "";
                xls.PageSetup.RightFooter = "Page &P of &N";
                xls.PageSetup.TopMargin = xla.InchesToPoints(0.85);
                xls.PageSetup.BottomMargin = xla.InchesToPoints(0.71);
                xls.PageSetup.HeaderMargin = xla.InchesToPoints(0.42);
                xls.PageSetup.FooterMargin = xla.InchesToPoints(0.39);
                xls.PageSetup.Draft = false;
                xls.PageSetup.PaperSize = XlPaperSize.xlPaperLetter;
                xls.PageSetup.Order = XlOrder.xlDownThenOver;
                xls.PageSetup.PrintErrors = XlPrintErrors.xlPrintErrorsDisplayed;
                xls.PageSetup.PrintTitleRows = "$1:$1";
                xls.PageSetup.PrintTitleColumns = "";
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
                60,
                false);
            xlb.Close();

            return RedirectToAction("ListBdays");
        }


        public IActionResult MakeSheet(string year, string season)
        {
            Session theSession = sessionRepository.SelectBySeason(year, season);
            var theSeason = (snType)int.Parse(theSession.Season);
            DateTime sDate = DateTime.Parse(theSession.StartDate);
            //List<Schedule> schedule = scheduleRepository.SelectAllBySessionId(id);
            //int teams = schedule.ElementAt(0).Teams;

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "MemberList" + schedDate + "0" + theSession.Season;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/Members/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/Members/pdf/" + fileName;


            int r = 1;
            int c = 1;
            int g, i, j, k, x;
            int maxLines = 47;
            int startSession;
            int thisSeason;
            int hRow;
            bool captains = false;
            bool notFound = true;
            string sortOrder = "Lastname, Firstname";
            string topHeader;
            List<Player> playerList = new List<Player>();
            Player p = new Player();
            List<Member> memberList = new List<Member>();
            Member m = new Member();
            List<Draft> draftList = new List<Draft>();
            Draft dr = new Draft();
            Team t = new Team();
            Session currentSession = sessionRepository.SelectByCurrent();


            // Initialize Excel workbook/*
            Excel.Application xla = new Excel.Application();
            Excel.Workbook xlb = xla.Workbooks.Add();
            Excel.Worksheet xln = (Excel.Worksheet)xlb.Worksheets.get_Item(1);
            Excel.Range xlr;


            for (k = 0; k < 6; k++)
            {
                xln = (Excel.Worksheet)xlb.Sheets.Add(After: xlb.Sheets[xlb.Sheets.Count]);
                xln.Name = "K" + k.ToString();
                xln.Activate();
            }

            foreach (Worksheet z in xlb.Sheets)
            {
                if (z.Name == "Sheet1")
                {
                    z.Delete();
                    break;
                }
            }

            foreach (Worksheet xls in xlb.Sheets)
            {
                if (xls.Name == "K0")
                {
                    xls.Activate();
                    xls.Name = "Session";
                    playerList = playerRepository.SelectAllBySession(year, season);
                    topHeader = "&24 " + theSeason + " " + theSession.Year + " - Member List";
                    if (playerList.Count > 0)
                    {
                        xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                        xlr = xls.get_Range(xls.Columns[2], xls.Columns[5]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                        xlr = xls.get_Range(xls.Columns[6], xls.Columns[7]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                        xlr = xls.get_Range(xls.Columns[4], xls.Columns[5]);
                        xlr.ShrinkToFit = true;

                        xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 4;  //#
                        xls.get_Range(xls.Columns[2], xls.Columns[2]).ColumnWidth = 20; //Name
                        xls.get_Range(xls.Columns[3], xls.Columns[3]).ColumnWidth = 13; //Telephone
                        xls.get_Range(xls.Columns[4], xls.Columns[4]).ColumnWidth = 28; //Email
                        xls.get_Range(xls.Columns[5], xls.Columns[5]).ColumnWidth = 28; //Address
                        xls.get_Range(xls.Columns[6], xls.Columns[6]).ColumnWidth = 6;  //Zip Code
                        xls.get_Range(xls.Columns[7], xls.Columns[7]).ColumnWidth = 6;  //Team

                        hRow = 1;
                        xls.Cells[hRow, 1] = "#";
                        xls.Cells[hRow, 2] = "Name";
                        xls.Cells[hRow, 3] = "Telephone";
                        xls.Cells[hRow, 4] = "Email Address";
                        xls.Cells[hRow, 5] = "Address";
                        xls.Cells[hRow, 6] = "Zip";
                        xls.Cells[hRow, 7] = "Team";

                        xlr = xls.get_Range(xls.Rows[hRow], xls.Rows[hRow]);
                        xlr.Font.Bold = true;

                        for (j = 1; j < playerList.Count() + 1; j++)
                        {

                            i = j; // This sets up single spacing
                            p = playerList[j - 1];

                            xls.Cells[hRow + i, 1] = i.ToString();
                            xls.Cells[hRow + i, 2] = p.Member.ReverseName.Trim();

                            if (p.Member.Telephone != null)
                            {
                                xls.Cells[hRow + i, 3] = p.Member.Telephone.Trim();
                            }
                            if (p.Member.Email != null)
                            {
                                xls.Cells[hRow + i, 4] = p.Member.Email.Trim();
                            }
                            if (p.Member.Address1 != null)
                            {
                                xls.Cells[hRow + i, 5] = p.Member.Address1.Trim();
                            }
                            if (p.Member.Zip != null)
                            {
                                xls.Cells[hRow + i, 6] = p.Member.Zip.Trim();
                            }


                            if (p.TeamId == 0)
                            {
                                xls.Cells[hRow + i, 7] = "sub";
                            }
                            else
                            {
                                t = teamRepository.SelectByID(p.TeamId);
                                xls.Cells[hRow + i, 7] = t.TeamNumber.ToString();
                            }
                            if (p.IsCaptain)
                            {
                                xlr = xls.get_Range(xls.Rows[hRow + i], xls.Rows[hRow + i]);
                                xlr.Font.Bold = true;
                                xlr.Font.ColorIndex = 3;
                            }


                        }

                        xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[playerList.Count() + 1, 7]);
                        boxInterior(xlr);
                        boxOutline(xlr);

                        // Now set up the page
                        xls.PageSetup.Orientation = XlPageOrientation.xlLandscape;
                        xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                        xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                        xls.PageSetup.CenterHorizontally = true;
                        xls.PageSetup.CenterVertically = true;
                        xls.PageSetup.LeftHeader = "";
                        xls.PageSetup.CenterHeader = topHeader;
                        xls.PageSetup.RightHeader = "";
                        xls.PageSetup.LeftFooter = "";
                        xls.PageSetup.CenterFooter = "";
                        xls.PageSetup.RightFooter = "Page &P of &N";
                        xls.PageSetup.TopMargin = xla.InchesToPoints(0.85);
                        xls.PageSetup.BottomMargin = xla.InchesToPoints(0.71);
                        xls.PageSetup.HeaderMargin = xla.InchesToPoints(0.42);
                        xls.PageSetup.FooterMargin = xla.InchesToPoints(0.39);
                        xls.PageSetup.Draft = false;
                        xls.PageSetup.PaperSize = XlPaperSize.xlPaperLetter;
                        xls.PageSetup.Order = XlOrder.xlDownThenOver;
                        xls.PageSetup.PrintErrors = XlPrintErrors.xlPrintErrorsDisplayed;
                        xls.PageSetup.PrintTitleRows = "$1:$1";
                        xls.PageSetup.PrintTitleColumns = "";

                    }


                }
                else if (xls.Name == "K1")
                {
                    xls.Activate();
                    xls.Name = "All";
                    playerList.Clear();
                    memberList = memberRepository.SelectAll();
                    topHeader = "&24 " + "Member List - Since Summer 2004";

                    xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    xlr = xls.get_Range(xls.Columns[2], xls.Columns[5]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                    xlr = xls.get_Range(xls.Columns[6], xls.Columns[7]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    xlr = xls.get_Range(xls.Columns[4], xls.Columns[5]);
                    xlr.ShrinkToFit = true;

                    xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 4;  //#
                    xls.get_Range(xls.Columns[2], xls.Columns[2]).ColumnWidth = 20; //Name
                    xls.get_Range(xls.Columns[3], xls.Columns[3]).ColumnWidth = 13; //Telephone
                    xls.get_Range(xls.Columns[4], xls.Columns[4]).ColumnWidth = 28; //Email
                    xls.get_Range(xls.Columns[5], xls.Columns[5]).ColumnWidth = 28; //Address
                    xls.get_Range(xls.Columns[6], xls.Columns[6]).ColumnWidth = 6;  //Zip Code
                    xls.get_Range(xls.Columns[7], xls.Columns[7]).ColumnWidth = 10;  //B-day

                    hRow = 1;
                    xls.Cells[hRow, 1] = "#";
                    xls.Cells[hRow, 2] = "Name";
                    xls.Cells[hRow, 3] = "Telephone";
                    xls.Cells[hRow, 4] = "Email Address";
                    xls.Cells[hRow, 5] = "Address";
                    xls.Cells[hRow, 6] = "Zip";
                    xls.Cells[hRow, 7] = "B-Day";

                    xlr = xls.get_Range(xls.Rows[hRow], xls.Rows[hRow]);
                    xlr.Font.Bold = true;

                    i = 0;
                    for (j = 1; j < memberList.Count() + 1; j++)
                    {
                        i += 1;
                        m = memberList[j - 1];

                        xls.Cells[hRow + i, 1] = i.ToString();
                        xls.Cells[hRow + i, 2] = m.ReverseName.Trim();

                        if (m.Telephone != null)
                        {
                            xls.Cells[hRow + i, 3] = m.Telephone.Trim();
                        }
                        if (m.Email != null)
                        {
                            xls.Cells[hRow + i, 4] = m.Email.Trim();
                        }
                        if (m.Address1 != null)
                        {
                            xls.Cells[hRow + i, 5] = m.Address1.Trim();
                        }
                        if (m.Zip != null)
                        {
                            xls.Cells[hRow + i, 6] = m.Zip.Trim();
                        }
                        if (m.Bday != null)
                        {
                            DateTime dt = Convert.ToDateTime(m.Bday);
                            string bDate = dt.ToShortDateString();
                            xls.Cells[hRow + i, 7] = bDate.Trim();
                        }
                        if (m.IsDeceased)
                        {
                            xlr = xls.get_Range(xls.Cells[hRow + i, 1], xls.Cells[hRow + i, 7]);
                            xlr.Interior.Color = XlRgbColor.rgbLightGrey;
                        }
                    }

                    xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[memberList.Count() + hRow, 7]);
                    boxInterior(xlr);
                    boxOutline(xlr);

                    // Now set up the page
                    xls.PageSetup.Orientation = XlPageOrientation.xlLandscape;
                    xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.CenterHorizontally = true;
                    xls.PageSetup.CenterVertically = true;
                    xls.PageSetup.LeftHeader = "";
                    xls.PageSetup.CenterHeader = topHeader;
                    xls.PageSetup.RightHeader = "";
                    xls.PageSetup.LeftFooter = "";
                    xls.PageSetup.CenterFooter = "";
                    xls.PageSetup.RightFooter = "Page &P of &N";
                    xls.PageSetup.TopMargin = xla.InchesToPoints(0.85);
                    xls.PageSetup.BottomMargin = xla.InchesToPoints(0.71);
                    xls.PageSetup.HeaderMargin = xla.InchesToPoints(0.42);
                    xls.PageSetup.FooterMargin = xla.InchesToPoints(0.39);
                    xls.PageSetup.Draft = false;
                    xls.PageSetup.PaperSize = XlPaperSize.xlPaperLetter;
                    xls.PageSetup.Order = XlOrder.xlDownThenOver;
                    xls.PageSetup.PrintErrors = XlPrintErrors.xlPrintErrorsDisplayed;
                    xls.PageSetup.PrintTitleRows = "$1:$1";
                    xls.PageSetup.PrintTitleColumns = "";



                }
                else if (xls.Name == "K2")
                {
                    memberList.Clear();
                    playerList.Clear();
                    memberList = memberRepository.SelectAllPaidMembers();
                    playerList = playerRepository.SelectAllByYear(year);
                    topHeader = "&24 " + "Member List - " + theSession.Year + " - All Paid Members";
                    xls.Name = "Paid Yearly";
                    if (playerList.Count > 0)
                    {
                        xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                        xlr = xls.get_Range(xls.Columns[2], xls.Columns[5]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                        xlr = xls.get_Range(xls.Columns[6], xls.Columns[7]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                        xlr = xls.get_Range(xls.Columns[4], xls.Columns[5]);
                        xlr.ShrinkToFit = true;

                        xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 4;  //#
                        xls.get_Range(xls.Columns[2], xls.Columns[2]).ColumnWidth = 20; //Name
                        xls.get_Range(xls.Columns[3], xls.Columns[3]).ColumnWidth = 13; //Telephone
                        xls.get_Range(xls.Columns[4], xls.Columns[4]).ColumnWidth = 28; //Email
                        xls.get_Range(xls.Columns[5], xls.Columns[5]).ColumnWidth = 28; //Address
                        xls.get_Range(xls.Columns[6], xls.Columns[6]).ColumnWidth = 6;  //Zip Code
                        xls.get_Range(xls.Columns[7], xls.Columns[7]).ColumnWidth = 6;  //Team

                        hRow = 1;
                        xls.Cells[hRow, 1] = "#";
                        xls.Cells[hRow, 2] = "Name";
                        xls.Cells[hRow, 3] = "Telephone";
                        xls.Cells[hRow, 4] = "Email Address";
                        xls.Cells[hRow, 5] = "Address";
                        xls.Cells[hRow, 6] = "Zip";
                        xls.Cells[hRow, 7] = "Team";

                        xlr = xls.get_Range(xls.Rows[hRow], xls.Rows[hRow]);
                        xlr.Font.Bold = true;

                        i = 0;
                        for (j = 1; j < memberList.Count() + 1; j++)
                        {
                            i += 1;
                            m = memberList[j - 1];

                            xls.Cells[hRow + i, 1] = i.ToString();
                            xls.Cells[hRow + i, 2] = m.ReverseName.Trim();

                            if (m.Telephone != null)
                            {
                                xls.Cells[hRow + i, 3] = m.Telephone.Trim();
                            }
                            if (m.Email != null)
                            {
                                xls.Cells[hRow + i, 4] = m.Email.Trim();
                            }
                            if (m.Address1 != null)
                            {
                                xls.Cells[hRow + i, 5] = m.Address1.Trim();
                            }
                            if (m.Zip != null)
                            {
                                xls.Cells[hRow + i, 6] = m.Zip.Trim();
                            }
                            p = playerRepository.SelectByMemberId(m.MemberId, theSession.SessionId);
                            if (p == null)
                            {
                                xls.Cells[hRow + i, 7] = "np";
                            }
                            else
                            {
                                if (p.Team != null)
                                {
                                    xls.Cells[hRow + i, 7] = p.Team.TeamNumber.ToString();
                                    if (p.IsCaptain)
                                    {
                                        xlr = xls.get_Range(xls.Rows[hRow + i], xls.Rows[hRow + i]);
                                        xlr.Font.Bold = true;
                                        xlr.Font.ColorIndex = 3;
                                    }
                                }
                                else
                                {
                                    xls.Cells[hRow + i, 7] = "np";
                                }
                            }
                        }
                        xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[memberList.Count() + hRow, 7]);
                        boxInterior(xlr);
                        boxOutline(xlr);

                        // Now set up the page
                        xls.PageSetup.Orientation = XlPageOrientation.xlLandscape;
                        xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                        xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                        xls.PageSetup.CenterHorizontally = true;
                        xls.PageSetup.CenterVertically = true;
                        xls.PageSetup.LeftHeader = "";
                        xls.PageSetup.CenterHeader = topHeader;
                        xls.PageSetup.RightHeader = "";
                        xls.PageSetup.LeftFooter = "";
                        xls.PageSetup.CenterFooter = "";
                        xls.PageSetup.RightFooter = "Page &P of &N";
                        xls.PageSetup.TopMargin = xla.InchesToPoints(0.85);
                        xls.PageSetup.BottomMargin = xla.InchesToPoints(0.71);
                        xls.PageSetup.HeaderMargin = xla.InchesToPoints(0.42);
                        xls.PageSetup.FooterMargin = xla.InchesToPoints(0.39);
                        xls.PageSetup.Draft = false;
                        xls.PageSetup.PaperSize = XlPaperSize.xlPaperLetter;
                        xls.PageSetup.Order = XlOrder.xlDownThenOver;
                        xls.PageSetup.PrintErrors = XlPrintErrors.xlPrintErrorsDisplayed;
                        xls.PageSetup.PrintTitleRows = "$1:$1";
                        xls.PageSetup.PrintTitleColumns = "";



                    }
                }
                else if (xls.Name == "K3")
                {
                    memberList.Clear();
                    playerList.Clear();
                    playerList = playerRepository.SelectAllBySession(year, season);
                    //memberList = memberRepository.se;
                    topHeader = "&24 " + theSession.Year + " - Members Email";
                    xls.Name = "Email";


                    xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                    xlr = xls.get_Range(xls.Columns[2], xls.Columns[5]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                    xlr = xls.get_Range(xls.Columns[6], xls.Columns[7]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    xlr = xls.get_Range(xls.Columns[4], xls.Columns[5]);
                    xlr.ShrinkToFit = true;

                    xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 20; //Name
                    xls.get_Range(xls.Columns[2], xls.Columns[2]).ColumnWidth = 15; //Telephone
                    xls.get_Range(xls.Columns[3], xls.Columns[3]).ColumnWidth = 15; //cellphone
                    xls.get_Range(xls.Columns[4], xls.Columns[4]).ColumnWidth = 28; //Email
                    xls.get_Range(xls.Columns[5], xls.Columns[5]).ColumnWidth = 28; //Address


                    hRow = 1;
                    xls.Cells[hRow, 1] = "Name";
                    xls.Cells[hRow, 2] = "Telephone";
                    xls.Cells[hRow, 3] = "Cellphone";
                    xls.Cells[hRow, 4] = "Email Address";
                    xls.Cells[hRow, 5] = "Address";

                    xlr = xls.get_Range(xls.Rows[hRow], xls.Rows[hRow]);
                    xlr.Font.Bold = true;

                    i = 0;
                    for (j = 1; j < playerList.Count() + 1; j++)
                    {
                        i += 1;
                        p = playerList[j-1];
                        m = p.Member;

                        xls.Cells[hRow + i, 1] = m.ReverseName.Trim();
                        if(m.Telephone != null)
                        {
                            xls.Cells[hRow + i, 2] = m.Telephone.Trim();
                        }
                        if (m.Cellphone != null)
                        {
                            xls.Cells[hRow + i, 3] = m.Cellphone.Trim();
                        }
                        if (m.Email != null)
                        {
                            xls.Cells[hRow + i, 4] = m.Email.Trim();
                        }
                        if (m.Address1 != null)
                        {
                            xls.Cells[hRow + i, 5] = m.Address1.Trim();
                        }
                    }

                    xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[playerList.Count() + hRow, 5]);
                    boxInterior(xlr);
                    boxOutline(xlr);

                    // Now set up the page
                    xls.PageSetup.Orientation = XlPageOrientation.xlLandscape;
                    xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.CenterHorizontally = true;
                    xls.PageSetup.CenterVertically = true;
                    xls.PageSetup.LeftHeader = "";
                    xls.PageSetup.CenterHeader = topHeader;
                    xls.PageSetup.RightHeader = "";
                    xls.PageSetup.LeftFooter = "";
                    xls.PageSetup.CenterFooter = "";
                    xls.PageSetup.RightFooter = "Page &P of &N";
                    xls.PageSetup.TopMargin = xla.InchesToPoints(0.85);
                    xls.PageSetup.BottomMargin = xla.InchesToPoints(0.71);
                    xls.PageSetup.HeaderMargin = xla.InchesToPoints(0.42);
                    xls.PageSetup.FooterMargin = xla.InchesToPoints(0.39);
                    xls.PageSetup.Draft = false;
                    xls.PageSetup.PaperSize = XlPaperSize.xlPaperLetter;
                    xls.PageSetup.Order = XlOrder.xlDownThenOver;
                    xls.PageSetup.PrintErrors = XlPrintErrors.xlPrintErrorsDisplayed;
                    xls.PageSetup.PrintTitleRows = "$1:$1";
                    xls.PageSetup.PrintTitleColumns = "";



                }
                else if (xls.Name == "K4")
                {
                    playerList.Clear();
                    playerList = playerRepository.SelectAllBySession(year, season);
                    topHeader = "&24 " + theSeason + " " + theSession.Year + " - Member List";
                    xls.Name = "One Page";

                    if (playerList.Count > 0)
                    {
                        xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;   //Name
                        xlr.ColumnWidth = 19;
                        xlr.ShrinkToFit = true;

                        xlr = xls.get_Range(xls.Columns[2], xls.Columns[3]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter; //Team
                        xlr.ColumnWidth = 5;

                        xlr = xls.get_Range(xls.Columns[4], xls.Columns[4]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;   //Name
                        xlr.ColumnWidth = 19;
                        xlr.ShrinkToFit = true;

                        xlr = xls.get_Range(xls.Columns[5], xls.Columns[6]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter; //Team
                        xlr.ColumnWidth = 5;

                        xlr = xls.get_Range(xls.Columns[7], xls.Columns[7]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;   //Name
                        xlr.ColumnWidth = 19;
                        xlr.ShrinkToFit = true;

                        xlr = xls.get_Range(xls.Columns[8], xls.Columns[9]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter; //Team
                        xlr.ColumnWidth = 5;

                        hRow = 1;

                        xls.Cells[hRow, 1] = "Name";
                        xls.Cells[hRow, 2] = "Team";
                        xls.Cells[hRow, 3] = "";

                        xlr = xls.get_Range(xls.Rows[hRow], xls.Rows[hRow]);
                        xlr.Font.Bold = true;


                        x = 0;
                        g = 0;
                        for ( j = 1; j < playerList.Count() + 1; j++)
                        {
                            i = j; // This sets up single spacing
                            g += 1;
                            p = playerList[j - 1];
                            xls.Cells[hRow + g, 1 + (x * 3)] = p.Member.ReverseName.Trim();
                            if (p.TeamId == 0)
                            {
                                xls.Cells[hRow + g, 2 + (x * 3)] = "sub";
                            }
                            else
                            {
                                t = p.Team;
                                xls.Cells[hRow + g, 2 + (x * 3)] = t.TeamNumber.ToString();
                            }
                            if (g == maxLines)
                            {
                                xlr = xls.get_Range(xls.Cells[hRow, 1 + (x * 3)], xls.Cells[hRow + g, 2 + (x * 3)]);
                                boxInterior(xlr);
                                boxOutline(xlr);
                                g = 0;
                                x += 1;
                                xls.Cells[hRow, 1 + (x * 3)] = "Name";
                                xls.Cells[hRow, 2 + (x * 3)] = "Team";
                                xls.Cells[hRow, 3 + (x * 3)] = "";
                            }
                            if (j == playerList.Count())
                            {
                                xlr = xls.get_Range(xls.Cells[hRow, 1 + (x * 3)], xls.Cells[hRow + g, 2 + (x * 3)]);
                                boxInterior(xlr);
                                boxOutline(xlr);
                            }
                        }
                    }
                    // Now set up the page
                    xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.CenterHorizontally = true;
                    xls.PageSetup.CenterVertically = true;
                    xls.PageSetup.LeftHeader = "";
                    xls.PageSetup.CenterHeader = topHeader;
                    xls.PageSetup.RightHeader = "";
                    xls.PageSetup.LeftFooter = "";
                    xls.PageSetup.CenterFooter = "";
                    xls.PageSetup.RightFooter = "Page &P of &N";
                    xls.PageSetup.TopMargin = xla.InchesToPoints(0.85);
                    xls.PageSetup.BottomMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.HeaderMargin = xla.InchesToPoints(0.42);
                    xls.PageSetup.FooterMargin = xla.InchesToPoints(0.39);
                    xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.Orientation = XlPageOrientation.xlPortrait;
                    xls.PageSetup.CenterHorizontally = true;
                    xls.PageSetup.Draft = false;
                    xls.PageSetup.PaperSize = XlPaperSize.xlPaperLetter;
                    xls.PageSetup.Order = XlOrder.xlDownThenOver;
                    xls.PageSetup.BlackAndWhite = false;
                    xls.PageSetup.Zoom = 100;
                    xls.PageSetup.PrintErrors = XlPrintErrors.xlPrintErrorsDisplayed;
                    xls.PageSetup.PrintTitleRows = "$1:$1";
                    xls.PageSetup.PrintTitleColumns = "";


                }
                else if (xls.Name == "K5")
                {
                    playerList.Clear();
                    playerList = playerRepository.SelectAllBySession(year, season);
                    topHeader = "&24 " + theSeason + " " + theSession.Year + " - Member List";
                    xls.Name = "One Landscape";

                    if (playerList.Count > 0)
                    {
                        xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;   //Name
                        xlr.ColumnWidth = 18;
                        xlr.ShrinkToFit = true;

                        xlr = xls.get_Range(xls.Columns[2], xls.Columns[3]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter; //Team
                        xlr.ColumnWidth = 5;

                        xlr = xls.get_Range(xls.Columns[4], xls.Columns[4]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;   //Name
                        xlr.ColumnWidth = 18;
                        xlr.ShrinkToFit = true;

                        xlr = xls.get_Range(xls.Columns[5], xls.Columns[6]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter; //Team
                        xlr.ColumnWidth = 5;

                        xlr = xls.get_Range(xls.Columns[7], xls.Columns[7]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;   //Name
                        xlr.ColumnWidth = 18;
                        xlr.ShrinkToFit = true;

                        xlr = xls.get_Range(xls.Columns[8], xls.Columns[9]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter; //Team
                        xlr.ColumnWidth = 5;

                        xlr = xls.get_Range(xls.Columns[10], xls.Columns[10]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;   //Name
                        xlr.ColumnWidth = 18;
                        xlr.ShrinkToFit = true;

                        xlr = xls.get_Range(xls.Columns[11], xls.Columns[12]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter; //Team
                        xlr.ColumnWidth = 5;

                        hRow = 1;

                        xls.Cells[hRow, 1] = "Name";
                        xls.Cells[hRow, 2] = "Team";
                        xls.Cells[hRow, 3] = "";

                        xlr = xls.get_Range(xls.Rows[hRow], xls.Rows[hRow]);
                        xlr.Font.Bold = true;

                        x = 0;
                        g = 0;
                        for (j = 1; j < playerList.Count +1; j++)
                        {
                            i = j; // This sets up single spacing
                            g += 1;
                            p = playerList[j - 1];
                            xls.Cells[hRow + g, 1 + (x * 3)] = p.Member.ReverseName.Trim();
                            if (p.Team.TeamId == 0)
                            {
                                xls.Cells[hRow + g, 2 + (x * 3)] = "sub";
                            }
                            else
                            {
                                t = p.Team;
                                xls.Cells[hRow + g, 2 + (x * 3)] = t.TeamNumber.ToString();
                            }
                            if (g == 35)
                            {
                                xlr = xls.get_Range(xls.Cells[hRow, 1 + (x * 3)], xls.Cells[hRow + g, 2 + (x * 3)]);
                                boxInterior(xlr);
                                boxOutline(xlr);
                                g = 0;
                                x += 1;
                                xls.Cells[hRow, 1 + (x * 3)] = "Name";
                                xls.Cells[hRow, 2 + (x * 3)] = "Team";
                                xls.Cells[hRow, 3 + (x * 3)] = "";
                            }
                            if (j == playerList.Count())
                            {
                                xlr = xls.get_Range(xls.Cells[hRow, 1 + (x * 3)], xls.Cells[hRow + g, 2 + (x * 3)]);
                                boxInterior(xlr);
                                boxOutline(xlr);
                            }
                        }

                    }
                    // Now set up the page
                    xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.CenterHorizontally = true;
                    xls.PageSetup.CenterVertically = true;
                    xls.PageSetup.LeftHeader = "";
                    xls.PageSetup.CenterHeader = topHeader;
                    xls.PageSetup.RightHeader = "";
                    xls.PageSetup.LeftFooter = "";
                    xls.PageSetup.CenterFooter = "";
                    xls.PageSetup.RightFooter = "Page &P of &N";
                    xls.PageSetup.TopMargin = xla.InchesToPoints(0.85);
                    xls.PageSetup.BottomMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.HeaderMargin = xla.InchesToPoints(0.42);
                    xls.PageSetup.FooterMargin = xla.InchesToPoints(0.39);
                    xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
                    xls.PageSetup.Orientation = XlPageOrientation.xlLandscape;
                    xls.PageSetup.CenterHorizontally = true;
                    xls.PageSetup.Draft = false;
                    xls.PageSetup.PaperSize = XlPaperSize.xlPaperLetter;
                    xls.PageSetup.Order = XlOrder.xlDownThenOver;
                    xls.PageSetup.BlackAndWhite = false;
                    xls.PageSetup.Zoom = 100;
                    xls.PageSetup.PrintErrors = XlPrintErrors.xlPrintErrorsDisplayed;
                    xls.PageSetup.PrintTitleRows = "$1:$1";
                    xls.PageSetup.PrintTitleColumns = "";

                }
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
                60,
                false);
            xlb.Close();

            return RedirectToAction("List");

        }





        private bool PlayerExists(int id)
        {
          return (_context.Players?.Any(e => e.PlayerId == id)).GetValueOrDefault();
        }

        public async Task<bool> FillCaptainsAsync(int sessionid)
        {
            List<Member> listMembers = memberRepository.SelectAll();
            // Get list of captains 
            List<Player> captainsList = playerRepository.SelectAllByCaptain(sessionid);

            List<SelectListItem> captainIds = (from p in captainsList
                                               join m in listMembers
                                               on p.MemberId equals m.MemberId
                                               select new SelectListItem()
                                               {
                                                   Text = m.FullName,
                                                   Value = p.TeamId.ToString()
                                               }).ToList();

            // List<SelectListItem> captainIds = (from c in captainsList select new SelectListItem() { Text = c.PlayerId.ToString(), Value = c.PlayerId.ToString() }).ToList();
            ViewBag.CaptainIds = null;
            ViewBag.CaptainIds = captainIds;
            ViewBag.Captains = null;
            ViewBag.Captains = captainsList;
            return true;
        }
        public async Task<bool> FillNonCaptainsAsync(int sessionid)
        {
            List<Member> listMembers = memberRepository.SelectAll();

            // Get list of non captains
            List<Player> playersList = playerRepository.SelectAllByNonCaptain(sessionid);

            List<SelectListItem> playerIds = (from p in playersList
                                              join m in listMembers
                                              on p.MemberId equals m.MemberId
                                              select new SelectListItem()
                                              {
                                                  Text = m.FullName,
                                                  Value = p.PlayerId.ToString()
                                              }).ToList();

            // List<SelectListItem> playerIds = (from p in playersList select new SelectListItem() { Text = p.PlayerId.ToString(), Value = p.PlayerId.ToString() }).ToList();
            ViewBag.PlayerIds = null;
            ViewBag.PlayerIds = playerIds;
            ViewBag.Players = null;
            ViewBag.Players = playersList;
            return true;
        }

        public async Task<bool> FillImagesAsync()
        {
            List<Portrait> listImages = portraitRepository.SelectAll();
            ViewBag.Images = listImages;
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

        public async Task<bool> FillPlayersAsync()
        {
            List<Player> listPlayers = playerRepository.SelectAll();
            List<SelectListItem> players = (from p in listPlayers
                                            select new SelectListItem()
                                            { Text = p.PlayerId.ToString(), Value = p.PlayerId.ToString() }).ToList();
            ViewBag.PlayerId = players;
            return true;
        }

        public async Task<bool> FillSessionsAsync()
        {
            IEnumerable<Session> listSessions = sessionRepository.SelectAll();
            List<SelectListItem> sessions = (from s in listSessions
                                             select new SelectListItem()
                                             { Text = s.SessionId.ToString(), Value = s.SessionId.ToString() }).ToList();
            ViewBag.SessionId = sessions;
            return true;
        }

            public async Task<bool> FillTeamsAsync()
        {
            List<Team> listTeams = teamRepository.SelectAll();
            List<SelectListItem> teams = (from t in listTeams
                                          select new SelectListItem()
                                          {
                                              Text = (
                                              t.SessionId.ToString() + " - " + t.TeamNumber.ToString()),
                                              Value = t.TeamId.ToString()
                                          }).ToList();
            ViewBag.TeamId = teams;
            return true;
        }

        public async Task<bool> FillTeams2Async()
        {
            List<Team> listTeams = teamRepository.SelectAll();
            List<SelectListItem> teams = (from t in listTeams
                                          select new SelectListItem()
                                          {
                                              Text = t.TeamName,
                                              Value = t.TeamId.ToString()
                                          }).ToList();
            ViewBag.TeamId = teams;
            return true;
        }

        public async Task<bool> FillTeams3Async(Session s)
        {
            List<Team> listTeams = teamRepository.SelectAllBySeason(s.Year, s.Season);
            listTeams = listTeams.OrderBy(t => t.TeamNumber).ToList();
            List<SelectListItem> teams = (from t in listTeams
                                          select new SelectListItem()
                                          {
                                              Text = t.TeamNumber.ToString(),
                                              Value = t.TeamNumber.ToString()
                                          }).ToList();
            ViewBag.TeamIds = teams;
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
                                             { Text = Enum.GetName(typeof(snType), int.Parse(s)), 
                                               Value = s }).ToList();
            ViewBag.Seasons = seasons;
            return true;
        }





        private void boxInterior(Excel.Range xlr)
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
        private void boxOutline(Excel.Range xlr)
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
