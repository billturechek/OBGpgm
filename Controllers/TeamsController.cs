using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using OBGpgm.Repositories;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;


namespace OBGpgm.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ObgDbContext _context;
        private readonly IMemberRepository memberRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly IPortraitRepository portraitRepository;
        private readonly IScoreSheetRepository scoresheetRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly IStateRepository stateRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IWebHostEnvironment hostEnvironment;
        //private WebApiConfig config = null;
        public TeamsController(ObgDbContext context,
                                IMemberRepository memberRepository,
                                IPlayerRepository playerRepository,
                                IPortraitRepository portraitRepository,
                                IScoreSheetRepository scoresheetRepository,
                                ISessionRepository sessionRepository,
                                IStateRepository stateRepository,
                                ITeamRepository teamRepository,
                                IWebHostEnvironment hostEnvironment,
                                IConfiguration config)
        {
            this.memberRepository = memberRepository;
            this.playerRepository = playerRepository;
            this.portraitRepository = portraitRepository;
            this.scoresheetRepository = scoresheetRepository;
            this.sessionRepository = sessionRepository;
            this.stateRepository = stateRepository;
            this.teamRepository = teamRepository;
            this.hostEnvironment = hostEnvironment;
            _context = context;
        }
        /*
        public TeamsController(ObgDbContext context)
        {
            _context = context;
        }
        */
        // GET: Teams
        public IActionResult Index(int pg=1)
        {
            List<Team> teams = _context.Teams
                .OrderByDescending(t => t.SessionId)
                .ThenBy(t => t.TeamNumber)  
                .ToList();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = teams.Count();
            var pager = new Pager("Teams", recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = teams.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            this.ViewBag.returnPage = pg;
            return View(data);
            /*
            var oBGdbContext = _context.Teams.Include(t => t.Session);
            return View(await oBGdbContext.ToListAsync());  */
        }

        public async Task<IActionResult> GetAsync(int id)
        {
            Team model = teamRepository.SelectByID(id);
            return View(model);
        }

        public async Task<IActionResult> RosterAsync(string year, string season)
        {
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
                return RedirectToAction("Roster");
            }
            await FillCaptainsAsync(s.SessionId);
            await FillMembersAsync();
            await FillNonCaptainsAsync(s.SessionId);

            List<Team> data = teamRepository.SelectAllByNumber(s.SessionId, 1);

            return View(data);
        }

        public async Task<IActionResult> RosterSaveAsync(string year, string season)
        {
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
            string seasonName = Enum.GetName(typeof(snType), Convert.ToInt32(season));
            ViewData["SeasonName"] = seasonName;
            ViewData["Year"] = year;
            ViewData["Season"] = season;
            HttpContext.Session.SetString("Year", year);
            HttpContext.Session.SetString("Season", season);

            Session s = sessionRepository.SelectBySeason(year, season);
            if(s == null)
            {
                TempData["Message"] = "No data for requested season!";
                HttpContext.Session.SetString("Year", csession.Year);
                HttpContext.Session.SetString("Season", csession.Season);
                return RedirectToAction("Roster");
            }
            await FillCaptainsAsync(s.SessionId);
            await FillMembersAsync();
            await FillNonCaptainsAsync(s.SessionId);

            List<Team> data = teamRepository.SelectAllByNumber(s.SessionId, 1);

            return View(data);
        }
        public async Task<IActionResult> ListAsync(string year, string season)
        {
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

            List<Team> data = teamRepository.SelectAllBySeason(year, season);

            return View(data);
        }



        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id, int pg=1)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.Session)
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null)
            {
                return NotFound();
            }

            ViewBag.returnPage = pg;
            return View(team);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId");
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamId,SessionId,Division,TeamNumber,TeamName,TeamPoints,IsChampion,IsRunnerUp")] Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", team.SessionId);
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id, int pg=1)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            ViewBag.returnPage = pg;
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", team.SessionId);
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamId,SessionId,Division,TeamNumber,TeamName,TeamPoints,IsChampion,IsRunnerUp")] Team team)
        {
            if (id != team.TeamId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.TeamId))
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
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", team.SessionId);
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.Session)
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Teams == null)
            {
                return Problem("Entity set 'OBGcoreContext.Teams'  is null.");
            }
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult MakeSheet(string year, string season)
        {
            //Session theSession = sessionRepository.SelectByCurrent();
            Session theSession = sessionRepository.SelectBySeason(year, season);
            var theSeason = (snType)int.Parse(theSession.Season);
            DateTime sDate = DateTime.Parse(theSession.StartDate);

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "Roster" + schedDate + theSeason;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/Rosters/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/Rosters/pdf/" + fileName;

            List<Team> teamsList = teamRepository.SelectAllByNumberSeason(year, season);

            // Initialize Excel workbook
            Excel.Application xla = new Excel.Application();
            Excel.Workbook xlb = xla.Workbooks.Add();
            Excel.Worksheet xls = (Excel.Worksheet)xlb.Worksheets.get_Item(1);
            Excel.Range xlr;

            // Now set up the page
            xls.PageSetup.Orientation = XlPageOrientation.xlPortrait;
            xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.TopMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.BottomMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.HeaderMargin = xla.InchesToPoints(0);
            xls.PageSetup.FooterMargin = xla.InchesToPoints(0);
            xls.PageSetup.CenterHorizontally = true;
            //xls.PageSetup.CenterVertically = true;


            xlr = xls.get_Range(xls.Columns[1], xls.Columns[16]);
            xlr.NumberFormat = "@";
            xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            xlr.Font.Size = 10;

            // Set the column widths
            xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 8;    //Name
            xls.get_Range(xls.Columns[2], xls.Columns[2]).ColumnWidth = 8;    //Name - merged
            xls.get_Range(xls.Columns[3], xls.Columns[3]).ColumnWidth = 2;    //'Telephone
            xls.get_Range(xls.Columns[4], xls.Columns[4]).ColumnWidth = 11;   //'Telephone
            xls.get_Range(xls.Columns[5], xls.Columns[5]).ColumnWidth = 1;    //'used for space
            xls.get_Range(xls.Columns[6], xls.Columns[6]).ColumnWidth = 8;    //'Name
            xls.get_Range(xls.Columns[7], xls.Columns[7]).ColumnWidth = 8;    //'Name - merged
            xls.get_Range(xls.Columns[8], xls.Columns[8]).ColumnWidth = 2;    //'Telephone
            xls.get_Range(xls.Columns[9], xls.Columns[9]).ColumnWidth = 11;   //'Telephone
            xls.get_Range(xls.Columns[10], xls.Columns[10]).ColumnWidth = 1;  //'used for space
            xls.get_Range(xls.Columns[11], xls.Columns[11]).ColumnWidth = 8;  //'Name
            xls.get_Range(xls.Columns[12], xls.Columns[12]).ColumnWidth = 8;  //'Name - merged
            xls.get_Range(xls.Columns[13], xls.Columns[13]).ColumnWidth = 2;  //'Telephone
            xls.get_Range(xls.Columns[14], xls.Columns[14]).ColumnWidth = 11; //'Telephone
            xls.get_Range(xls.Columns[15], xls.Columns[15]).ColumnWidth = 1;  //'used for space


            //  Make the sheet header lines
            xls.Cells[1, 1] = "OBG Men's Billiard Club";
            xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[1, 15]);
            xlr.Merge();
            xlr.Font.Size = 24;
            xlr.Font.Bold = true;
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xls.Cells[2, 1] = theSeason + " " + theSession.Year.ToString() + " Roster";
            xlr = xls.get_Range(xls.Cells[2, 1], xls.Cells[2, 15]);
            xlr.Merge();
            xlr.Font.Size = 24;
            xlr.Font.Bold = true;
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xlr = xls.get_Range(xls.Cells[3, 1], xls.Cells[3, 15]);
            xlr.Merge();
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xls.Cells[3, 1] = "";

 
            int startRow = 5;
            int nGroups = teamsList.Count / 3;
            if (teamsList.Count()%3 != 0)
            {
                nGroups += 1;
            };
            int lRow = startRow + (nGroups * 6) + startRow;

            xlr = xls.get_Range(xls.Cells[startRow, 1], xls.Cells[lRow, 14]);
            xlr.Font.Name = "Arial";
            xlr.Font.Size = 10;
            xlr.ShrinkToFit = true;
            xls.get_Range(xls.Rows[startRow], xls.Rows[lRow]).RowHeight = 12.75;



            int c = 1;
            int r = -1;
            int groups = 0;
            int extra = 0;
            int maxExtra = 0;
            for (int i = 0; i < teamsList.Count(); i++)
            {
                Team t = teamsList[i];
                //  the variable c is for column and r is for row
                if (i % 3 == 0)
                {
                    c = 1;
                    r = r + 6 + maxExtra;
                    maxExtra = 0;
                    extra = 0;
                    groups += 1;
                    if ((groups % 9) == 0)
                    {
                        xlr = xls.get_Range(xls.Cells[r, c], xls.Cells[r, c + 3]);
                        xlr.PageBreak = (int)XlPageBreak.xlPageBreakManual;
                    }
                }
                else if (i % 3 == 1)
                {
                    c = 6;
                }
                else
                {
                    c = 11;
                }
                xls.Cells[r, c] = (i + 1).ToString() + " -- " + t.TeamName.Trim();
                xlr = xls.get_Range(xls.Cells[r, c], xls.Cells[r, c + 3]);
                xlr.Merge();
                xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                xlr.Font.Bold = true;

                List<Player> rosterList = playerRepository.SelectAllByTeam(t.TeamId);
                if (rosterList.Count > 4)
                {
                    extra = rosterList.Count - 4;
                    if(extra > maxExtra)
                    {
                        maxExtra = extra;
                    }                     
                }


                for (int j = 0; j < rosterList.Count(); j++)
                {
                    //make the list of players
                    Player p = rosterList[j];
                    if (p.IsPlaying == true)
                    {
                        if (p.Member.IsHonored)
                        {
                            xls.Cells[r + 1 + j, c] = "*" + p.Member.FullName;
                            xls.get_Range(xls.Cells[r + 1 + j, c], xls.Cells[r + 1 + j, c]).Font.Italic = true;
                        }
                        else
                        {
                            xls.Cells[r + 1 + j, c] = p.Member.FullName;
                        }
                        if (p.IsCaptain == true)
                        {
                            xls.get_Range(xls.Cells[r + 1 + j, c], xls.Cells[r + 1 + j, c]).Font.Bold = true;   
                        }
                        if (p.Member.IsDeceased)
                        {
                            xls.get_Range(xls.Cells[r + 1 + j, c], xls.Cells[r + 1 + j, c+3]).Interior.Color = XlRgbColor.rgbLightGrey;
                        }
                    }
                    xlr = xls.get_Range(xls.Cells[r + 1 + j, c], xls.Cells[r + 1 + j, c + 1]); // Player name field
                    xlr.Merge();
                    xlr.ShrinkToFit = true;

                    if (p.Member.Telephone.Substring(1, 3) == "352")
                    {
                        xls.Cells[r + 1 + j, c + 2] = p.Member.Telephone.Substring(6, 8);
                    }
                    else
                    {
                        string tele = p.Member.Telephone.Substring(1, 3) + ".";
                        tele = tele + p.Member.Telephone.Substring(6, 3);
                        tele = tele + "." + p.Member.Telephone.Substring(10, 4);
                        xls.Cells[r + 1 + j, c + 2] = tele;
                    }
                    xlr = xls.get_Range(xls.Cells[r + 1 + j, c + 2], xls.Cells[r + 1 + j, c + 3]); // Player telephone
                    xlr.Merge();
                    xlr.ShrinkToFit = true;

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
                10,
                false);
            xlb.Close();

            return RedirectToAction("Roster");
        }


        public IActionResult MakeStandings(string year, string season)
        {
            //Session theSession = sessionRepository.SelectByCurrent();
            Session theSession = sessionRepository.SelectBySeason(year, season);
            var theSeason = (snType)int.Parse(theSession.Season);
            DateTime sDate = DateTime.Parse(theSession.StartDate);
            List<Team> teamsList = teamRepository.SelectAllBySeason(year, season);

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "Standings" + schedDate + "0" + theSession.Season;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/Standings/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/Standings/pdf/" + fileName;

            int hRow = 7;
            int startRow = 8;
            int savePoints = 0;
            int i = 0;
            int j = 1;
            bool found = false;
            string header = "SORTED WEEKLY TEAM STANDINGS"; 
       


            // Initialize Excel workbook
            Excel.Application xla = new Excel.Application();
            Excel.Workbook xlb = xla.Workbooks.Add();
            Excel.Worksheet xls = (Excel.Worksheet)xlb.Worksheets.get_Item(1);
            Excel.Range xlr;

            // Now set up the page
            xls.PageSetup.Orientation = XlPageOrientation.xlPortrait;
            xls.PageSetup.LeftMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.RightMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.TopMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.BottomMargin = xla.InchesToPoints(0.0);
            xls.PageSetup.HeaderMargin = xla.InchesToPoints(0);
            xls.PageSetup.FooterMargin = xla.InchesToPoints(0);
            xls.PageSetup.CenterHorizontally = true;
            //xls.PageSetup.CenterVertically = true;

            xlr = xls.get_Range(xls.Columns[1], xls.Columns[16]);
            xlr.NumberFormat = "@";
            xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            xlr.Font.Size = 10;

            // Set the column widths
            xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 6;    //Place
            xls.get_Range(xls.Columns[2], xls.Columns[2]).ColumnWidth = 6;    //Team
            xls.get_Range(xls.Columns[3], xls.Columns[3]).ColumnWidth = 7;    //Point
            xls.get_Range(xls.Columns[4], xls.Columns[4]).ColumnWidth = 21;   //Captain
            xls.get_Range(xls.Columns[5], xls.Columns[5]).ColumnWidth = 36;   //Team Name


            //  Make the sheet header lines
            xls.Cells[1, 1] = "OBG Men's Billiard Club";
            xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[1, 5]);
            xlr.Merge();
            xlr.Font.Size = 24;
            xlr.Font.Bold = true;
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xlr = xls.get_Range(xls.Cells[2, 1], xls.Cells[2, 5]);
            xlr.Merge();
            xlr.Font.Size = 16;
            xlr.Font.Bold = true;
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            xls.Cells[2, 1] = header;

            xls.Cells[3, 1] = theSeason + " " + theSession.Year.ToString();
            xlr = xls.get_Range(xls.Cells[3, 1], xls.Cells[3, 5]);
            xlr.Merge();
            xlr.Font.Size = 24;
            xlr.Font.Bold = true;
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xls.Cells[5, 1] = "AS OF " + DateTime.Now.ToShortDateString();
            xlr = xls.get_Range(xls.Cells[5, 1], xls.Cells[5, 5]);
            xlr.Select();
            xlr.Merge();
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            xlr.Font.Bold = true;
            xlr.Font.Size = 16;

            // Header row for the body below

            xls.Cells[hRow, 1] = "Place";
            xls.Cells[hRow, 2] = "Team";
            xls.Cells[hRow, 3] = "Points";
            xls.Cells[hRow, 4] = "Team Captain";
            xls.Cells[hRow, 5] = "Team Name";
            xlr = xls.get_Range(xls.Cells[hRow, 1], xls.Cells[hRow, 5]);
            xlr.Font.Bold = true;
            xlr.Font.Underline = true;

            // Now build the body of the sheet
            for (i = 0; i < teamsList.Count(); i++)
            {
                Team t = teamsList[i];
                if (i > 0)
                {
                    if (savePoints > t.TeamPoints)
                    {
                        j = i + 1;
                    }
                }
                xls.Cells[hRow + i + 1, 1] = j;
                xls.Cells[hRow + i + 1, 2] = t.TeamNumber;
                xls.Cells[hRow + i + 1, 3] = t.TeamPoints;
                found = false;
                List<Player> teamRoster = playerRepository.SelectAllByTeam(t.TeamId);
                for (int k = 0; k <= teamRoster.Count(); k++)
                {
                    Player teamPlayer = new Player();
                    teamPlayer = teamRoster[k];
                    if (teamPlayer.IsCaptain)
                    {
                        found = true;
                        xls.Cells[hRow + i + 1, 4] = teamPlayer.Member.FullName;
                        break;
                    }  
                }
                xls.Cells[hRow + i + 1, 5] = t.TeamName.Trim();
                savePoints = t.TeamPoints;
            }

            i = teamsList.Count;
            xls.get_Range(xls.Cells[hRow + 1, 1], xls.Cells[hRow + 1 + i, 3]).HorizontalAlignment = XlHAlign.xlHAlignCenter;
            xlr = xls.get_Range(xls.Cells[hRow, 1], xls.Cells[hRow + i, 5]);
            xlr.Select();
            xlr.Font.Size = 12;





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

            return RedirectToAction("List");
        }

        public IActionResult MakeWeeklyResults(string year, string season)
        {
            Session theSession = sessionRepository.SelectBySeason(year, season);
            var theSeason = (snType)int.Parse(theSession.Season);
            DateTime sDate = DateTime.Parse(theSession.StartDate);
            //List<Schedule> schedule = scheduleRepository.SelectAllBySessionId(id);
            //int teams = schedule.ElementAt(0).Teams;

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "WeeklyResults" + schedDate + "0" + theSession.Season;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/Weekly/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/Weekly/pdf/" + fileName;

            List<Team> teamsList = teamRepository.SelectAllByNumber(theSession.SessionId);
            int[] totals = new int[teamsList.Count + 1 + 1];

            int i;
            string header = "WEEKLY TEAM RESULTS";
            int hRow;
            int startRow;
            int tRow;
            int highWeek;
            tRow = 6;
            hRow = tRow + 1;
            startRow = hRow + 1;
            highWeek = 0;
            List<ScoreSheet> ssList = new List<ScoreSheet>();

           
            // Initialize Excel workbook/*
            Excel.Application xla = new Excel.Application();
            Excel.Workbook xlb = xla.Workbooks.Add();
            Excel.Worksheet xls = (Excel.Worksheet)xlb.Worksheets.get_Item(1);
            Excel.Range xlr;
          



            // Start building the sheet' Row 1 Header
            xls.Cells[1, 1] = "OBG Men's Billiard Club";
            xlr = xls.get_Range(xls.Rows[1], xls.Rows[1]);
            xlr.Select();
            i = teamsList.Count() + 1;
            xlr.Font.Bold = true;
            xlr.Font.Size = 16;
            xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[1, i]);
            xlr.Select();
            xlr.Merge();
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // Row 2 Header;
            xls.Cells[2, 1] = header;
            xlr = xls.get_Range(xls.Rows[2], xls.Rows[2]);
            xlr.Select();
            xlr.Font.Bold = true;
            xlr.Font.Size = 16;
            xlr = xls.get_Range(xls.Cells[2, 1], xls.Cells[2, i]);
            xlr.Select();
            xlr.Merge();
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // Row 3 Header
            xls.Cells[3, 1] = theSeason + " - " + theSession.Year;
            xlr = xls.get_Range(xls.Rows[3], xls.Rows[3]);
            xlr.Select();
            xlr.Font.Bold = true;
            xlr.Font.Size = 16;
            xlr = xls.get_Range(xls.Cells[3, 1], xls.Cells[3, i]);
            xlr.Select();
            xlr.Merge();
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            // Row 5 header
            xls.Cells[5, 1] = "AS OF " + DateTime.Today.ToShortDateString();
            xlr = xls.get_Range(xls.Rows[5], xls.Rows[5]);
            xlr.Select();
            xlr.Font.Bold = true;
            xlr.Font.Size = 16;
            xlr = xls.get_Range(xls.Cells[5, 1], xls.Cells[5, i]);
            xlr.Select();
            xlr.Merge();
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 7;
            for (i = 2; i < teamsList.Count() + 2; i++)
            {
                xls.get_Range(xls.Columns[i], xls.Columns[i]).ColumnWidth = 3.11;
            }
            xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[hRow + 14, teamsList.Count + 1]);
            xlr.Font.Name = "Arial";
            xlr = xls.get_Range(xls.Cells[tRow, 1], xls.Cells[hRow + 14, teamsList.Count + 1]);
            xlr.Font.Size = 10;
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // Row 6 header
            xlr = xls.get_Range(xls.Rows[tRow], xls.Rows[tRow]);
            xlr.Select();
            xlr.Rows.AutoFit();
            xlr.Orientation = 90;
            xlr.ShrinkToFit = false;
            xlr.WrapText = false;
            xlr.MergeCells = false;
            xlr.Font.Bold = true;    
            xlr.HorizontalAlignment = XlHAlign.xlHAlignGeneral;
            xlr.VerticalAlignment = XlVAlign.xlVAlignBottom;
            for (i = 1; i < teamsList.Count + 1; i++)
            {
                xls.Cells[tRow, i + 1] = teamsList[i - 1].TeamName.Trim();
            }

            xlr = xls.get_Range(xls.Rows[tRow+1], xls.Rows[tRow+1]);
            xlr.Font.Bold = true;

            xls.Cells[hRow, 1] = "Team";
            for (i = 1; i < teamsList.Count + 1; i++)
            {
                totals[i] = 0;
                xls.Cells[hRow, i + 1] = i;
            }

            ssList = scoresheetRepository.SelectAllBySession(year, season);
            ScoreSheet ss = new ScoreSheet();

            for (i=0; i < ssList.Count();i++)
            {
                ss = ssList[i];
                xls.Cells[hRow + ss.SsWeek, 1] = ss.SsDate;
                xls.get_Range(xls.Cells[hRow + ss.SsWeek, 1], 
                    xls.Cells[hRow + ss.SsWeek, 1]).NumberFormat = "d-mmm";
                xls.Cells[hRow + ss.SsWeek, ss.SsHteam + 1] = ss.SsHpoints;
                xls.Cells[hRow + ss.SsWeek, ss.SsVteam + 1] = ss.SsVpoints;
                totals[ss.SsHteam] = totals[ss.SsHteam] + ss.SsHpoints;
                totals[ss.SsVteam] = totals[ss.SsVteam] + ss.SsVpoints;
                if (ss.SsWeek > highWeek)
                        highWeek = ss.SsWeek;
            }

            xls.Cells[startRow + highWeek, 1] = "Totals";
            for (i=1; i < teamsList.Count()+1; i++)
            {
                xls.Cells[startRow + highWeek, i + 1] = totals[i];
            }

            for (i = 1; i <highWeek + 1; i++)
            {
                for (int k = 1; k < teamsList.Count() + 1; k++)
                {
                    xlr = xls.get_Range(xls.Cells[hRow + i, k + 1], xls.Cells[hRow + i, k + 1]);
                    if (xlr.Value2 == null)
                    {
                        xlr.Interior.ColorIndex = 6;
                    }
                    else if (xlr.Value2 == "")
                    {
                        xlr.Interior.ColorIndex = 6;
                    }
                }
            }
            xlr = xls.get_Range(xls.Cells[hRow + highWeek + 1, 1],
                xls.Cells[hRow + highWeek + 1, teamsList.Count + 1]);
            xlr.Font.Bold = true;

            xlr = xls.get_Range(xls.Cells[6, 1], xls.Cells[hRow + highWeek + 1, teamsList.Count + 1]);
            boxInterior(xlr);
            boxOutline(xlr);






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




            //  Save the worksheet and close the workbook          
            xlb.SaveAs(SaveFileName + ".xlsx");
            xlb.ExportAsFixedFormat(
                Excel.XlFixedFormatType.xlTypePDF,
                SavePdfName,
                Excel.XlFixedFormatQuality.xlQualityStandard,
                true,
                true,
                1,
                25,
                false);
            xlb.Close();

            return RedirectToAction("List");
        }



        public async Task<IActionResult> Accordian(string year, string season)
        {
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
            await FillCaptainsAsync(s.SessionId);
            await FillImagesAsync();
            await FillMembersAsync();
            await FillNonCaptainsAsync(s.SessionId);
            // List<Team> data = teamRepository.SelectAllBySeason(year, season);
            List<Team> data = teamRepository.SelectAllByNumber(s.SessionId, 1);
            return View(data);
        }




        private bool TeamExists(int id)
        {
          return (_context.Teams?.Any(e => e.TeamId == id)).GetValueOrDefault();
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
            List<Member> listMembers = memberRepository.SelectAll();
            List<Player> listPlayers = playerRepository.SelectAll();
            List<SelectListItem> players = (from p in listPlayers
                                            join m in listMembers
                                            on p.MemberId equals m.MemberId
                                            select new SelectListItem()
                                            {
                                                Text = m.FullName,
                                                Value = p.PlayerId.ToString()
                                            }).ToList();
            ViewBag.PlayerId = players;
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


        public async Task<bool> FillImagesAsync()
        {
            List<Portrait> listImages = portraitRepository.SelectAll();
            ViewBag.Images = listImages;
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
