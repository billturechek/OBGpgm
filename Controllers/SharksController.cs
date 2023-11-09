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
using Microsoft.CodeAnalysis.Differencing;
using static System.Net.Mime.MediaTypeNames;

namespace OBGpgm.Controllers
{
    public class SharksController : Controller
    {
        private readonly ObgDbContext _context;
        private readonly HttpClient client = null;
        private readonly ISharkRepository sharkRepository;
        private readonly IMemberRepository memberRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IWebHostEnvironment hostEnvironment;
        public SharksController(HttpClient client,
                                ObgDbContext context,
                                ISharkRepository sharkRepository,
                                IMemberRepository memberRepository,
                                IPlayerRepository playerRepository,
                                ISessionRepository sessionRepository,
                                ITeamRepository teamRepository,
                                IWebHostEnvironment hostEnvironment,
                                IConfiguration config)
        {
            this.client = client;
            this.sharkRepository = sharkRepository;
            this.memberRepository = memberRepository;
            this.playerRepository = playerRepository;
            this.sessionRepository = sessionRepository;
            this.teamRepository = teamRepository;
            this.hostEnvironment = hostEnvironment;
            _context = context;
        }


        // GET: Sharks
        public IActionResult Index(int pg=1)
        {
            List<Shark> sharks = _context.Sharks
                .Include(s => s.Player)
                .Include(s => s.Player.Member)
                .OrderByDescending(s => s.SharkDate)
                .ToList();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = sharks.Count();
            var pager = new Pager("Sharks", recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = sharks.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            this.ViewBag.returnPage = pg;
            return View(data);
            /*
            var oBGdbContext = _context.Sharks.Include(s => s.Player).Include(s => s.Session);
            return View(await oBGdbContext.ToListAsync()); */
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

            List<Shark> data = sharkRepository.SelectAllBySession(year, season);
            return View(data);
        }


        // GET: Sharks/Details/5
        public async Task<IActionResult> Details(int? id, int pg=1)
        {
            if (id == null || _context.Sharks == null)
            {
                return NotFound();
            }

            var shark = await _context.Sharks
                .Include(s => s.Player)
                .Include(s => s.Session)
                .FirstOrDefaultAsync(m => m.SharkId == id);
            if (shark == null)
            {
                return NotFound();
            }

            ViewBag.returnPage = pg;
            return View(shark);
        }

        // GET: Sharks/Create
        public IActionResult Create()
        {
            ViewData["PlayerId"] = new SelectList(_context.Players, "PlayerId", "PlayerId");
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId");
            return View();
        }

        // POST: Sharks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SharkId,SessionId,PlayerId,MemberId,SharkDate,SharkType,TeamId,Points")] Shark shark)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shark);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PlayerId"] = new SelectList(_context.Players, "PlayerId", "PlayerId", shark.PlayerId);
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", shark.SessionId);
            return View(shark);
        }

        // GET: Sharks/Edit/5
        public async Task<IActionResult> Edit(int? id, int pg=1)
        {
            if (id == null || _context.Sharks == null)
            {
                return NotFound();
            }

            var shark = await _context.Sharks.FindAsync(id);
            if (shark == null)
            {
                return NotFound();
            }
            ViewData["PlayerId"] = new SelectList(_context.Players, "PlayerId", "PlayerId", shark.PlayerId);
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", shark.SessionId);
            ViewBag.returnPage = pg;
            return View(shark);
        }

        // POST: Sharks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SharkId,SessionId,PlayerId,MemberId,SharkDate,SharkType,TeamId,Points")] Shark shark)
        {
            if (id != shark.SharkId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shark);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SharkExists(shark.SharkId))
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
            ViewData["PlayerId"] = new SelectList(_context.Players, "PlayerId", "PlayerId", shark.PlayerId);
            ViewData["SessionId"] = new SelectList(_context.Sessions, "SessionId", "SessionId", shark.SessionId);
            return View(shark);
        }

        // GET: Sharks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Sharks == null)
            {
                return NotFound();
            }

            var shark = await _context.Sharks
                .Include(s => s.Player)
                .Include(s => s.Session)
                .FirstOrDefaultAsync(m => m.SharkId == id);
            if (shark == null)
            {
                return NotFound();
            }

            return View(shark);
        }

        // POST: Sharks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Sharks == null)
            {
                return Problem("Entity set 'OBGcoreContext.Sharks'  is null.");
            }
            var shark = await _context.Sharks.FindAsync(id);
            if (shark != null)
            {
                _context.Sharks.Remove(shark);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SharkExists(int id)
        {
          return (_context.Sharks?.Any(e => e.SharkId == id)).GetValueOrDefault();
        }



        public IActionResult MakeSheet(string year, string season)
        {
            //Session theSession = sessionRepository.SelectByCurrent();
            Session theSession = sessionRepository.SelectBySeason(year, season);
            var theSeason = (snType)int.Parse(theSession.Season);
            DateTime sDate = DateTime.Parse(theSession.StartDate);
            List<Team> teamsList = teamRepository.SelectAllBySeason(year, season);

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "Sharks" + schedDate + theSeason;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/Sharks/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/Sharks/pdf/" + fileName;


            int r = 1;
            int c = 1;
            int i = 0;
            int hRow;
            int sRow;
            bool captains = false;
            string sortOrder = "Lastname, Firstname";
            Session s = sessionRepository.SelectBySeason(year, season);
            List<Player> playerList = playerRepository.SelectAllByTeamsInSession(year, season);
            List<Player> captainList = playerRepository.SelectAllByCaptain(s.SessionId);
            List<Team> teamList = teamRepository.SelectAllByNumberSeason(year, season);
            List<Shark> sharkList = sharkRepository.SelectAllBySession(year, season);
            Player p = new Player();
            Player oldp = new Player();
            Team t = new Team();
            bool notFound = true;
            string sharkInfo = "";

            // Initialize Excel workbook
            Excel.Application xla = new Excel.Application();
            Excel.Workbook xlb = xla.Workbooks.Add();
            Excel.Worksheet xls = (Excel.Worksheet)xlb.Worksheets.get_Item(1);
            Excel.Range xlr;


            xls.Name = "Sharks";

            if (playerList.Count > 0)
            {
                xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                xlr.ColumnWidth = 92;
                hRow = 1;
                sRow = hRow + 5;

                // add picture to top of page
                xls.Cells[hRow, 1] = "";
                xlr = xls.get_Range(xls.Rows[hRow], xls.Rows[hRow]);
                xlr.RowHeight = 85;
                xlr.Font.Bold = true;                
                string sImageName = wwwRootPath + "/images/shark-attack.gif";
                xls.Shapes.AddPicture(sImageName, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoTrue, 190, 5, 129, 76);



                xls.Cells[hRow + 1, 1] = "OBG MEN'S BILLIARD CLUB";
                xlr = xls.get_Range(xls.Rows[hRow + 1], xls.Rows[hRow + 1]);
                xlr.Font.Bold = true;
                xlr.Font.Italic = true;
                xlr.Font.Size = 18;
                xlr.Font.Name = "Arial";

                xls.Cells[hRow + 2, 1] = "SHARK'S CLUB";
                xlr = xls.get_Range(xls.Rows[hRow + 2], xls.Rows[hRow + 2]);
                xlr.Font.Bold = true;
                xlr.Font.Italic = true;
                xlr.Font.Size = 20;
                xlr.Font.ColorIndex = 3;               // Red

                xls.Cells[hRow + 3, 1] = theSeason + " " + s.Year;
                xlr = xls.get_Range(xls.Rows[hRow + 3], xls.Rows[hRow + 3]);
                xlr.Font.Bold = true;
                xlr.Font.Size = 14;
                xlr.Font.Name = "Arial";

                xls.Cells[sRow, 1] = "MOST WIN'S THIS SEASON";
                xlr = xls.get_Range(xls.Rows[sRow], xls.Rows[sRow]);
                xlr.Font.Bold = true;
                xlr.Font.Size = 14;
                xlr.Font.Name = "Arial";
                xlr.Font.ColorIndex = 3;               // Red

                i = 0;
                foreach (Shark shark in sharkList)
                {
                    if (shark.SharkType == SharkType.MostWins)
                    {
                        i += 1;
                        string captainName = "";
                        foreach (Player player in playerList)
                        {
                            if (shark.TeamId == player.TeamId)
                            {
                                if (player.IsCaptain)
                                {
                                    if (player.Member.FullName.EndsWith("s"))
                                    {
                                        captainName = player.Member.FullName + "', ";
                                    }
                                    else
                                    {
                                        captainName = player.Member.FullName + "'s, ";
                                    }
                                    break;
                                }
                            }
                        }
                        sharkInfo = captainName;

                        foreach (Team team in teamList)
                        {
                            if (team.TeamId == shark.TeamId)
                            {
                                sharkInfo = sharkInfo + team.TeamName.Trim() + ", ";
                                break;
                            }
                        }

                        sharkInfo += shark.SharkDate.ToShortDateString() + ", ";
                        sharkInfo += shark.Points.ToString();

                        xls.Cells[sRow + i, 1] = sharkInfo;
                        xlr = xls.get_Range(xls.Rows[sRow + i], xls.Rows[sRow + i]);
                        xlr.Font.Size = 12;
                        xlr.Font.Name = "Arial";

                    }
                }


                sRow = sRow + i + 2;
                xls.Cells[sRow, 1] = "8-BALL BREAK AND RUN";
                xlr = xls.get_Range(xls.Rows[sRow], xls.Rows[sRow]);
                xlr.Font.Bold = true;
                xlr.Font.Size = 14;
                xlr.Font.Name = "Arial";
                xlr.Font.ColorIndex = 3;                 // Red;

                i = 0;
                foreach(Shark shark in sharkList)
                {
                    if (shark.SharkType == SharkType.BreakRun8Ball)
                    {
                        i += 1;
                        foreach(Player player in playerList)
                        {
                            if (shark.PlayerId == player.PlayerId)
                            {
                                sharkInfo = player.Member.FullName + ", ";
                                break;
                            }
                        }
                        foreach (Team team in teamList)
                        {
                            if (team.TeamId == shark.TeamId)
                            {
                                sharkInfo = sharkInfo + team.TeamName.Trim() + ", ";
                                break;
                            }            
                        }
                        sharkInfo += shark.SharkDate.ToShortDateString();

                        xls.Cells[sRow + i, 1] = sharkInfo;
                        xlr = xls.get_Range(xls.Rows[sRow + i], xls.Rows[sRow + i]);
                        xlr.Font.Size = 12;
                        xlr.Font.Name = "Arial";
                    }
                }


                sRow = sRow + i + 2;

                xls.Cells[sRow, 1] = "8-BALL RUN OUT";
                xlr = xls.get_Range(xls.Rows[sRow], xls.Rows[sRow]);
                xlr.Font.Bold = true;
                xlr.Font.Size = 14;
                xlr.Font.Name = "Arial";
                xlr.Font.ColorIndex = 3;                  // Red

                i = 0;

                foreach (Shark shark in sharkList)
                {
                    if (shark.SharkType == SharkType.RunOut8Ball)
                    {
                        i += 1;
                        foreach(Player player in playerList)
                        {
                            if (shark.PlayerId == player.PlayerId)
                            {
                                sharkInfo = player.Member.FullName + ", ";
                                break;
                            }
                        }
                        foreach(Team team in teamsList)
                        {
                            if (team.TeamId== shark.TeamId)
                            {
                                sharkInfo = sharkInfo + team.TeamName.Trim() + ", ";
                                break;
                            }
                        }

                        sharkInfo += shark.SharkDate.ToShortDateString();
                        xls.Cells[sRow + i, 1] = sharkInfo;
                        xlr = xls.get_Range(xls.Rows[sRow + i], xls.Rows[sRow + i]);
                        xlr.Font.Size = 12;
                        xlr.Font.Name = "Arial";
                    }
                }


                sRow = sRow + i + 2;

                xls.Cells[sRow, 1] = "9-BALL BREAK AND RUN";
                xlr = xls.get_Range(xls.Rows[sRow], xls.Rows[sRow]);
                xlr.Font.Bold = true;
                xlr.Font.Size = 14;
                xlr.Font.Name = "Arial";
                xlr.Font.ColorIndex = 3;               //Red

                i = 0;
                foreach(Shark shark in sharkList)
                {
                    if (shark.SharkType == SharkType.BreakRun9Ball)
                    {
                        i += 1;
                        foreach (Player player in playerList)
                        {
                            if (shark.PlayerId == player.PlayerId)
                            {
                                sharkInfo = player.Member.FullName + ", ";
                                break;
                            }
                        }
                        foreach (Team team in teamsList)
                        {
                            if (team.TeamId == shark.TeamId)
                            {
                                sharkInfo = sharkInfo + team.TeamName.Trim() + ", ";
                                break;
                            }
                        }

                        sharkInfo += shark.SharkDate.ToShortDateString();
                        xls.Cells[sRow + i, 1] = sharkInfo;
                        xlr = xls.get_Range(xls.Rows[sRow + i], xls.Rows[sRow + i]);
                        xlr.Font.Size = 12;
                        xlr.Font.Name = "Arial";
                    }

                }

                sRow = sRow + i + 2;

                xls.Cells[sRow, 1] = "9-BALL RUN OUT";
                xlr = xls.get_Range(xls.Rows[sRow], xls.Rows[sRow]);
                xlr.Font.Bold = true;
                xlr.Font.Size = 14;
                xlr.Font.Name = "Arial";
                xlr.Font.ColorIndex = 3;                 // Red

                i = 0;
                foreach (Shark shark in sharkList)
                {
                    if (shark.SharkType == SharkType.RunOut9Ball)
                    {
                        i += 1;
                        foreach (Player player in playerList)
                        {
                            if (shark.PlayerId == player.PlayerId)
                            {
                                sharkInfo = player.Member.FullName + ", ";
                                break;
                            }
                        }
                        foreach (Team team in teamsList)
                        {
                            if (team.TeamId == shark.TeamId)
                            {
                                sharkInfo = sharkInfo + team.TeamName.Trim() + ", ";
                                break;
                            }
                        }
                        sharkInfo += shark.SharkDate.ToShortDateString();
                        xls.Cells[sRow + i, 1] = sharkInfo;
                        xlr = xls.get_Range(xls.Rows[sRow + i], xls.Rows[sRow + i]);
                        xlr.Font.Size = 12;
                        xlr.Font.Name = "Arial";
                    }
                }


                sRow = sRow + i + 2;

                xls.Cells[sRow, 1] = "9-BALL ON THE BREAK";
                xlr = xls.get_Range(xls.Rows[sRow], xls.Rows[sRow]);
                xlr.Font.Bold = true;
                xlr.Font.Size = 14;
                xlr.Font.Name = "Arial";
                xlr.Font.ColorIndex = 3;              // ' Red

                i = 0;
                foreach (Shark shark in sharkList)
                {
                    if (shark.SharkType == SharkType.OnBreak9Ball)
                    {
                        i += 1;
                        foreach (Player player in playerList)
                        {
                            if (shark.PlayerId == player.PlayerId)
                            {
                                sharkInfo = player.Member.FullName + ", ";
                                break;
                            }
                        }
                        foreach (Team team in teamsList)
                        {
                            if (team.TeamId == shark.TeamId)
                            {
                                sharkInfo = sharkInfo + team.TeamName.Trim() + ", ";
                                break;
                            }
                        }
                        sharkInfo += shark.SharkDate.ToShortDateString();
                        xls.Cells[sRow + i, 1] = sharkInfo;
                        xlr = xls.get_Range(xls.Rows[sRow + i], xls.Rows[sRow + i]);
                        xlr.Font.Size = 12;
                        xlr.Font.Name = "Arial";
                    }
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


            return RedirectToAction("List");
        }


        public async Task<bool> FillMembersAsync()
        {
            List<Member> listMembers = memberRepository.SelectAll();
            List<SelectListItem> members = (from m in listMembers
                                            select new SelectListItem()
                                            { Text = m.FullName, Value = m.MemberId.ToString() }).ToList();
            ViewBag.MemberId = members;
            ViewBag.Members = listMembers;
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
            ViewBag.Teams = listTeams;
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


    }
}
