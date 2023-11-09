using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OBGpgm.Data;
using OBGpgm.Models;
using OBGpgm.Interfaces;
using OBGpgm.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Http;

namespace OBGpgm.Controllers
{
    public class ScoreSheetsController : Controller
    {
        private readonly ObgDbContext _context;
        private readonly HttpClient client = null;
        private readonly IMemberRepository memberRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly IScheduleRepository scheduleRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly IScoreSheetRepository scoreSheetRepository;
        private readonly ISharkRepository sharkRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IWebHostEnvironment hostEnvironment;
        public ScoreSheetsController(ObgDbContext context,
                                HttpClient client,
                                ISharkRepository sharkRepository,
                                IMemberRepository memberRepository,
                                IPlayerRepository playerRepository,
                                IScheduleRepository scheduleRepository,
                                IScoreSheetRepository scoreSheetRepository,
                                ISessionRepository sessionRepository,
                                ITeamRepository teamRepository,
                                IWebHostEnvironment hostEnvironment,
                                IConfiguration config)
        {
            _context = context;
            this.client = client;
            this.sharkRepository = sharkRepository;
            this.memberRepository = memberRepository;
            this.playerRepository = playerRepository;
            this.scoreSheetRepository = scoreSheetRepository;
            this.scheduleRepository = scheduleRepository;
            this.sessionRepository = sessionRepository;
            this.teamRepository = teamRepository;
            this.hostEnvironment = hostEnvironment;
        }


        // Constant arrays used for table assignment
        private readonly string[] pairsPerSession = { "", "", "", "", "", "", "", "",
                                            "4", "4", "3 2", "3 2", "3 3", "3 3", "4 3", "4 3", "4 4",
                                            "", "3 3 3", "", "4 3 3", "", "4 4 3", "", "4 4 4"};

        private readonly string[] tablesToAssign = { "", "", "1&2 5&6", "1&2 5&6 7&8", "1&2 3&4 5&6 7&8" }
        ;
        private readonly string[] timeSlots = { "", "11:00", "12:45", "2:30" };

        public async Task<IActionResult> ListAsync(string session, string week)
        {
            await FillSessionsAsync();

            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            if (String.IsNullOrEmpty(session))
            {
                if (HttpContext.Session.GetString("Year") == null)
                {
                    session = csession.SessionId.ToString();
                    week = csession.CurrentWeek;
                }
                else
                {
                    session = HttpContext.Session.GetString("Session");
                    week = HttpContext.Session.GetString("Week");
                }
            }
            
            ViewData["Week"] = week;
            ViewData["Session"] = session;
            HttpContext.Session.SetString("Session", session);
            HttpContext.Session.SetString("Week", week);
            int sid = Convert.ToInt32(session);
            await FillWeeksAsync(sid);
            bool found = false;
            foreach (SelectListItem item in ViewBag.Weeks)
            {
                if(item.Text == week)
                {
                    found = true;
                    break;
                }
            }
            if(!found)
            {
                week = ViewBag.Week;
            }
            List<ScoreSheet> data = scoreSheetRepository.SelectAllByWeek(sid, Convert.ToInt32(week));

            return View(data);
        }
        public IActionResult Get(int id, int week, int hteam)
        {
            ScoreSheet model = scoreSheetRepository.SelectByID(id, week, hteam);
            return View(model);
        }
        // DETAILS: ScoreSheets/Details
        public IActionResult Details(int id, int week, int hteam, int pg = 1)
        {     
            ScoreSheet scoresheet = scoreSheetRepository.SelectByID(id, week, hteam);
                
            if (scoresheet == null)
            {
                return NotFound();
            }

            ViewBag.returnPage = pg;
            return View(scoresheet);
        }

        // GET: ScoreSheets/Edit/5
        public IActionResult Edit(int id, int week, int hTeam, int pg = 1)
        {
            if (id == null || _context.ScoreSheets == null)
            {
                return NotFound();
            }

            var scoresheet = scoreSheetRepository.SelectByID(id, week, hTeam);
            if (scoresheet == null)
            {
                return NotFound();
            }
            ViewBag.returnPage = pg;
            return View(scoresheet);
        }


        public IActionResult Insert(int pg=1)
        {
            ViewBag.returnPage = pg;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Insert(ScoreSheet model)
        {
            if (ModelState.IsValid)
            {
                scoreSheetRepository.Insert(model);
                ViewBag.Message = "ScoreSheet inserted successfully!";
            }
            return View(model);
        }
        public IActionResult Update(int id, int week, int hteam)
        {
            ScoreSheet model = scoreSheetRepository.SelectByID(id, week, hteam);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(ScoreSheet model)
        {
            if (ModelState.IsValid)
            {
                scoreSheetRepository.Update(model);
                ViewBag.Message = "ScoreSheet updated successfully!";
            }
            return View(model);
        }

        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int id, int week, int hteam)
        {
            ScoreSheet model = scoreSheetRepository.SelectByID(id, week, hteam);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, int week, int hteam)
        {
            {
                scoreSheetRepository.Delete(id, week, hteam);
                TempData["Message"] = "ScoreSheet deleted successfully!";
            }
            return RedirectToAction("List");
        }


        public async Task<IActionResult> EnterAsync()
        {
            await FillPointsAsync();
            await FillSessionsAsync();
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            string thisSession = "";
            string curWeek = "1";

            if (String.IsNullOrEmpty(thisSession))
            {
                if (HttpContext.Session.GetString("thisSession") == null)
                {
                    thisSession = csession.SessionId.ToString();
                    curWeek = "1";
                }
                else
                {
                    thisSession = HttpContext.Session.GetString("thisSession");
                    curWeek = HttpContext.Session.GetString("curWeek");
                }
            }

            ViewBag.vPointsEnabled = false;

            ViewData["thisSession"] = thisSession;
            HttpContext.Session.SetString("thisSession", thisSession);
            ViewData["curWeek"] = curWeek;
            HttpContext.Session.SetString("curWeek", curWeek);

            int id = Convert.ToInt32(thisSession);
            Session tSess = sessionRepository.SelectById(id);
            int sid = tSess.SessionId;
            int wid = Convert.ToInt32(tSess.CurrentWeek);

            ViewBag.Captains = playerRepository.SelectAllByCaptain(sid);

            List<ScoreSheet> data = scoreSheetRepository.SelectAllByWeek(sid, wid);
            return View(data);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterAsync(List<ScoreSheet> model)
        {
            if (!ModelState.IsValid)
            {

            }

            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            foreach (ScoreSheet ss in model)
            {
                // Get the high score by a team this session
                int highScore = sharkRepository.SelectHighBySession(ss.SsSessionId);

                scoreSheetRepository.Update(ss);
                TempData["Message"] = "ScoreSheet updated successfully!";
                // Did this home team make a high score for session?
                if (ss.SsHpoints >= highScore)
                {
                    //int points = ss.SsHpoints ?? default(int);
                    int points = ss.SsHpoints;
                    await UpdateSharks(ss, ss.SsHteam, points);
                }

                highScore = sharkRepository.SelectHighBySession(ss.SsSessionId);

                // Did this viitor make a high score for session?
                if (ss.SsVpoints >= highScore)
                {
                    //int points = ss.SsVpoints ?? default(int);
                    //int Vteam = ss.SsVteam ?? default(int);
                    int points = ss.SsVpoints;
                    int Vteam = ss.SsVteam;
                    await UpdateSharks(ss, Vteam, points);
                }
            }
            // Update the current week in the current session
            int curWeek = Convert.ToInt32(csession.CurrentWeek);
            curWeek++;
            csession.CurrentWeek = curWeek.ToString();

            // Update the current session

            sessionRepository.Update(csession);
            TempData["Message"] = "All score sheets inserted successfully!";
            return View(model);
        }


        public IActionResult Index(int pg=1)
        {
            List<ScoreSheet> scoresheets = scoreSheetRepository.SelectAll()
                .OrderByDescending(s => s.SsSessionId)
                .ThenByDescending(s => s.SsWeek)
                .ThenBy(S => S.SsHteam)
                .ToList();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = scoresheets.Count();
            var pager = new Pager("ScoreSheets", recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = scoresheets.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            this.ViewBag.returnPage = pg;
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAsync()
        {
            await FillSessionsAsync();
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            string session = csession.SessionId.ToString();
            string numWeeks = "12";

            if (String.IsNullOrEmpty(session))
            {
                if (HttpContext.Session.GetString("session") == null)
                {
                    session = csession.SessionId.ToString();
                    numWeeks = "12";
                }
                else
                {
                    session = HttpContext.Session.GetString("session");
                    numWeeks = HttpContext.Session.GetString("numWeeks");
                }
            }
            ViewData["session"] = session;
            HttpContext.Session.SetString("session", session);
            ViewData["numWeeks"] = numWeeks;
            HttpContext.Session.SetString("numWeeks", numWeeks);

            return View();
        }
         
        [HttpPost]
        public async Task<IActionResult> CreateAsync(string submit, string session)
        {
            string strDate;
            await FillSessionsAsync();
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            if (String.IsNullOrEmpty(session))
            {
                if (HttpContext.Session.GetString("session") == null)
                {
                    session = csession.SessionId.ToString();
                    //numWeeks = "12";
                }
                else
                {
                    session = HttpContext.Session.GetString("session");
                    //numWeeks = HttpContext.Session.GetString("numWeeks");
                }
            }

            Session thisSession = sessionRepository.SelectById(Convert.ToInt32(session));
            int sid = thisSession.SessionId;
            strDate = thisSession.StartDate;
            DateTime startDate = DateTime.Parse(strDate);

            if (startDate != null)
            {
                string sDate = startDate.ToShortDateString();
                HttpContext.Session.SetString("startDate", sDate);
                ViewBag.startDate = startDate.ToString("yyyy-MM-dd"); ;
            }

            ViewData["session"] = session;
            HttpContext.Session.SetString("session", session);

            if (submit == "Create")
            {
                // Now see if any score sheets already exist for this session
                string year = thisSession.Year;
                string season = thisSession.Season;
                List<ScoreSheet> data = scoreSheetRepository.SelectAllBySession(year, season);
                if (data != null && data.Count > 0)
                {
                    TempData["Message"] = "ScoreSheets already exist for session!";
                    return View();
                }
                List<Schedule> schedules = scheduleRepository.SelectAllBySessionId(sid);
                if (schedules == null)
                {
                    TempData["Message"] = "Schedule template not found for session!";
                    return View();
                }

                int curWeek = 0;
                DateTime curDate = startDate.AddDays(-7).Date;
                foreach (Schedule sched in schedules)
                {
                    ScoreSheet ss = new ScoreSheet();
                    ss.SsSessionId = thisSession.SessionId;
                    ss.SsDivision = 1;
                    if (sched.Week != curWeek)
                    {
                        curWeek = sched.Week;
                        curDate = curDate.AddDays(7).Date;
                    }
                    if (sched.HomeTeam != 0)
                    {
                        ss.SsWeek = sched.Week;
                        ss.SsHteam = sched.HomeTeam;
                        ss.SsHpoints = 0;
                        ss.SsVteam = sched.VisitingTeam;
                        ss.SsVpoints = 0;
                        ss.SsDate = curDate.Date;
                        scoreSheetRepository.Insert(ss);
                    }                    
                }
                TempData["Message"] = "Score Sheets built for session!";
                return View();
            }
            if(submit == "Make Printable")
            {
                Session theSession = thisSession;
                int id = theSession.SessionId;
                var season = (snType)int.Parse(theSession.Season);
                DateTime sDate = DateTime.Parse(theSession.StartDate);
                List<Schedule> schedule = scheduleRepository.SelectAllBySessionId(id);
                int teams = schedule.ElementAt(0).Teams;
                List<Team> teamList = teamRepository.SelectAllBySession(theSession.SessionId.ToString());
                List<Player> captains = playerRepository.SelectAllByCaptain(theSession.SessionId);


                // Save files to wwwRoot/Archives
                string schedDate = sDate.ToString("yyyy");
                string wwwRootPath = hostEnvironment.WebRootPath;
                string fileName = "ScoreSheets" + schedDate + season;
                string extension = ".xlsx";
                string SaveFileName = wwwRootPath + "/Archives/ScoreSheets/xlsx/" + fileName;
                string SavePdfName = wwwRootPath + "/Archives/ScoreSheets/pdf/" + fileName;


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
                xls.PageSetup.CenterVertically = true;

                // Set the column widths
                xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 8;    
                xls.get_Range(xls.Columns[2], xls.Columns[2]).ColumnWidth = 11;    
                xls.get_Range(xls.Columns[3], xls.Columns[3]).ColumnWidth = 8;    
                xls.get_Range(xls.Columns[4], xls.Columns[4]).ColumnWidth = 11;   
                xls.get_Range(xls.Columns[5], xls.Columns[5]).ColumnWidth = 2.5;    
                xls.get_Range(xls.Columns[6], xls.Columns[6]).ColumnWidth = 8;    
                xls.get_Range(xls.Columns[7], xls.Columns[7]).ColumnWidth = 11;   
                xls.get_Range(xls.Columns[8], xls.Columns[8]).ColumnWidth = 8;    
                xls.get_Range(xls.Columns[9], xls.Columns[9]).ColumnWidth = 11;

                //Set global attributes
                // 
                Excel.Style style = xlb.Styles.Add("NewStyle");
                style.Font.Name = "Arial";
                style.Font.Size = 12;
                style.Font.Bold = true;
                xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[1000, 15]);
                xlr.Style = style;
                xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                xlr.RowHeight = 20; 

                int r = 1;
                int c = 1;
                int p = 1;
                int pairs = 0; 
                int curTimeSlot = 1;
                string[] numTables = pairsPerSession[theSession.TeamsD1].Split(" ");
                string[] theTables = tablesToAssign[(int.Parse(numTables[0]))].Split(" ");

                List<Schedule> schedules = scheduleRepository.SelectAllBySessionId(id);
                foreach (Schedule sched in schedules)
                {
                    if(sched.TimeSlot != curTimeSlot)
                    {                        
                        curTimeSlot = sched.TimeSlot;
                        pairs = 0;
                        theTables= tablesToAssign[(int.Parse(numTables[curTimeSlot-1]))].Split(" ");
                    }
                    xls.Cells[r, c + 1] = "Tables:";
                    xls.get_Range(xls.Cells[r, c + 1], xls.Cells[r, c + 1])
                        .HorizontalAlignment = XlHAlign.xlHAlignRight;
                    xls.Cells[r, c+2] = theTables[pairs];
                    xlr = xls.get_Range(xls.Cells[r, c + 2], xls.Cells[r, c + 2]);
                    xlr.Interior.ColorIndex = 6;
                    pairs++;
                    xls.Cells[r, c + 3] = "Week: " + sched.Week.ToString();

                    //  Below is the Date and Time line
                    xls.Cells[r + 1, c] = "Date:";
                    xls.get_Range(xls.Cells[r + 1, c], xls.Cells[r + 1, c])
                        .HorizontalAlignment = XlHAlign.xlHAlignRight;
                    xls.Cells[r + 1, c + 1] = startDate.AddDays((sched.Week-1) * 7).Date;
                    xls.Cells[r + 1, c + 2] = "Time:";
                    xls.get_Range(xls.Cells[r + 1, c + 1], xls.Cells[r + 1, c + 3])
                        .HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    xls.Cells[r + 1, c + 3] = timeSlots[sched.TimeSlot];
                    xlr = xls.get_Range(xls.Cells[r + 1, c + 3], xls.Cells[r + 1, c + 3]);
                    xlr.Interior.ColorIndex = 6;



                    //  Below is the Team names line
                    xlr = xls.get_Range(xls.Cells[r + 2, c], xls.Cells[r + 2, c + 1]);
                    xlr.Merge();
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    xlr = xls.get_Range(xls.Cells[r + 2, c + 2], xls.Cells[r + 2, c + 3]);
                    xlr.Merge();
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    Team homeTeam = teamRepository.SelectIdByNumber(theSession.SessionId, sched.HomeTeam);
                    Team visitTeam = teamRepository.SelectIdByNumber(theSession.SessionId, sched.VisitingTeam);
                    xls.Cells[r + 2, c] = sched.HomeTeam.ToString() + " - " + homeTeam.TeamName.Trim();
                    xls.Cells[r + 2, c+2] = sched.VisitingTeam.ToString() + " - " + visitTeam.TeamName.Trim();
                    xlr = xls.get_Range(xls.Cells[r + 2, c], xls.Cells[r + 2, c + 4]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    //  Below is the Captain's line
                    Player homeCaptain = playerRepository.SelectByTeamCaptain(homeTeam.TeamId);
                    Player visitCaptain = playerRepository.SelectByTeamCaptain(visitTeam.TeamId);
                    xls.Cells[r + 3, c] = "Captain:";
                    xls.Cells[r + 3, c + 1] = homeCaptain.Member.LastName.Trim();
                    xls.Cells[r + 3, c + 2] = "Captain:";
                    xls.Cells[r+3, c + 3] = visitCaptain.Member.LastName.Trim();
                    xlr = xls.get_Range(xls.Cells[r + 3, c + 1], xls.Cells[r + 3, c + 1]);
                    xlr.Interior.ColorIndex = 6;
                    xlr = xls.get_Range(xls.Cells[r + 3, c + 3], xls.Cells[r + 3, c + 3]);
                    xlr.Interior.ColorIndex = 6;

                    xls.Cells[r + 4, c] = "Match 1:";
                    xls.Cells[r + 4, c + 2] = "Match 1:";
                    xls.Cells[r + 5, c] = "Match 2:";
                    xls.Cells[r + 5, c + 2] = "Match 2:";
                    xls.Cells[r + 6, c] = "Match 3:";
                    xls.Cells[r + 6, c + 2] = "Match 3:";
                    xls.Cells[r + 7, c] = "Match 4:";
                    xls.Cells[r + 7, c + 2] = "Match 4:";
                    xls.Cells[r + 8, c] = "Total:";
                    xls.Cells[r + 8, c + 2] = "Total:";
                    if (c == 1)
                    {
                        xls.Cells[r, c + 4] = "|";
                        xls.Cells[r + 1, c + 4] = "|";
                        xls.Cells[r + 2, c + 4] = "|";
                        xls.Cells[r + 3, c + 4] = "|";
                        xls.Cells[r + 4, c + 4] = "|";
                        xls.Cells[r + 5, c + 4] = "|";
                        xls.Cells[r + 6, c + 4] = "|";
                        xls.Cells[r + 7, c + 4] = "|";
                        xls.Cells[r + 8, c + 4] = "|";
                        xls.Cells[r + 9, c + 4] = "|";
                    }

                    xls.get_Range(xls.Rows[r + 9], xls.Rows[r+9]).RowHeight = 8;

                    for (int i = 4; i < 9; i++)
                    {
                        xlr = xls.get_Range(xls.Cells[r + i, c + 1], xls.Cells[r + i, c + 1]);
                        underlineCells(xlr, "Medium");
                        xlr = xls.get_Range(xls.Cells[r + i, c + 3], xls.Cells[r + i, c + 3]);
                        underlineCells(xlr, "Medium");
                    }

                    xlr = xls.get_Range(xls.Cells[r + 9, c], xls.Cells[r + 9, c + 3]);
                    underlineCells(xlr, "Thin");


                    if (c < 6)
                    {
                        c = c + 5;
                    }
                    else
                    {
                        p++;
                        c = 1;
                        r = r + 10;
                        if(p>4)
                        {
                            p = 1;
                            xls.HPageBreaks.Add(xls.get_Range(xls.Rows[r], xls.Rows[r]));
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


                return View();
            }


            return View();
        }

        private static void underlineCells(Excel.Range aRange, string weight = "Thin")
        {
            var withBlock = aRange.Borders[XlBordersIndex.xlEdgeBottom];
            withBlock.LineStyle = XlLineStyle.xlContinuous;
            if(weight == "Medium")
            {
                withBlock.Weight = XlBorderWeight.xlMedium;
            }
            else
            {
                withBlock.Weight = XlBorderWeight.xlThin;
            }
            withBlock.ColorIndex = XlColorIndex.xlColorIndexAutomatic;
        }


 


            public async Task<bool> FillPointsAsync()
        {
            List<SelectListItem> points = new List<SelectListItem>();

            for (int i = 0; i < 17; i++)
            {
                points.Add(new SelectListItem()
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }
            ViewBag.points = points;
            return true;
        }
        public async Task<bool> FillSessionsAsync()
        {
            List<Session> listSessions = (List<Session>)sessionRepository.SelectAll();
            List<SelectListItem> sessions = (from s in listSessions
                                             select new SelectListItem()
                                             {
                                                 Text = s.Year + " - " + Enum.GetName(typeof(snType), Convert.ToInt32(s.Season)),
                                                 Value = s.SessionId.ToString()
                                             }).ToList();
            //ViewBag.SessionId = sessions;
            ViewData["SessionId"] = sessions;
            return true;
        }

        public async Task<bool> FillWeeksAsync(int sid)
        {
            List<ScoreSheet> listScores = scoreSheetRepository.SelectFirstByWeek(sid);

            List<SelectListItem> weeks = (from s in listScores
                                             select new SelectListItem()
                                             {
                                                 Text = s.SsWeek.ToString(),
                                                 Value = s.SsWeek.ToString()
                                             }).ToList(); 
            ViewBag.Weeks = weeks;
            ViewBag.Week = weeks[0].Text;
            return true;
        }
        public async Task<bool> FillYearsAsync()
        {
            List<string> listSessions = sessionRepository.SelectByYears();
            List<SelectListItem> sessions = (from s in listSessions select new SelectListItem() 
                                                                { Text = s, Value = s }).ToList();
            ViewBag.Years = sessions;
            return true;
        }
        private async Task<bool> UpdateSharks(ScoreSheet ss, int team, int points)
        {
            Shark newShark = new Shark();
            newShark.SessionId = ss.SsSessionId;
            newShark.SharkType = SharkType.MostWins;
            newShark.SharkDate = DateTime.Now.Date;
            newShark.Points = points;

            Team highTeam = teamRepository.SelectIdByNumber(ss.SsSessionId, team);
            newShark.TeamId = highTeam.TeamId;
            Player thisCaptain = playerRepository.SelectByTeamCaptain(highTeam.TeamId);
            newShark.PlayerId = thisCaptain.PlayerId;
            newShark.MemberId = thisCaptain.MemberId;

            sharkRepository.Insert(newShark);
            return true;
        }
    }
}
