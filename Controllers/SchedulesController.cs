using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Excel = Microsoft.Office.Interop.Excel;
using NuGet.Common;
using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using OBGpgm.Repositories;
using OBGpgm.ViewModels;
using Microsoft.Office.Interop.Excel;
using System.Drawing;
using Microsoft.CodeAnalysis.Text;
using static System.Net.Mime.MediaTypeNames;

namespace OBGpgm.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IPlayerRepository playerRepository;
        private readonly IScheduleRepository scheduleRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IWebHostEnvironment hostEnvironment;

        private readonly string[] numTablePairs = { "", "", "", "", "", "", "", "",
                                            "4", "4", "3 2", "3 2", "3 3", "3 3", "4 3", "4 3", "4 4",
                                            "", "3 3 3", "", "4 3 3", "", "4 4 3", "", "4 4 4"};
        private readonly string[] tableAssignment = { "", "", "1&2 5&6", "1&2 5&6 7&8", "1&2 3&4 5&6 7&8" };
        private readonly string[] sessionTimes = { "11:00", "12:45", "2:30" };
        public SchedulesController(HttpClient client,
                                    IPlayerRepository playerRepository,
                                    ISessionRepository sessionRepository,
                                    IScheduleRepository scheduleRepository,
                                    ITeamRepository teamRepository,
                                    IWebHostEnvironment hostEnvironment,
                                    IConfiguration config)
        {
            this.client = client;
            this.playerRepository = playerRepository;
            this.sessionRepository = sessionRepository;
            this.scheduleRepository = scheduleRepository;
            this.teamRepository = teamRepository;
            this.hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            List<Schedule> schedules = scheduleRepository.SelectAllSessions();
            return View(schedules);
        }

        public IActionResult Customize(string id)
        {
            int sessId = int.Parse(id);
            List<Schedule> schedules = scheduleRepository.SelectAllBySessionId(sessId);
            int teams = schedules.ElementAt(0).Teams;
            int numMatches = teams / 2;
            int totalSessions = 0;
            string tbls = "";
            switch (numMatches)
            {
                case < 5:
                    totalSessions = 1;
                    break;
                case < 9:
                    totalSessions = 2;
                    break;
                default:
                    totalSessions = 3;
                    break;
            }

            int[] ttables = new int[4];
            string tablesBySession = numTablePairs[teams];
            string[] tabs = tablesBySession.Split(' ');

            for (int z = 0; z < (totalSessions); z++)
            {
                tbls = tbls + tableAssignment[int.Parse(tabs[z])] + " ";
            }

            Session curSession = sessionRepository.SelectByCurrent();

            ViewBag.curSession = curSession;
            ViewBag.teams = teams;
            ViewBag.sessionTimes = sessionTimes;
            ViewBag.numTables = tabs;
            ViewBag.tableAssignment = tbls;
            ViewBag.numMatches = numMatches;
            ViewBag.tablesByMatches = tableAssignment;


            ScheduleViewModel vm = new ScheduleViewModel();
            vm.Sid = sessId.ToString();
            vm.Teams = teams.ToString();
            vm.DataList = schedules;
            return View(vm);
        }




        [HttpPost]
        public IActionResult InsertWeek(ScheduleViewModel vmi)
        {
            int sid = int.Parse(vmi.Sid);
            int week = int.Parse(vmi.Week);
            int teams = int.Parse(vmi.Teams);

            ViewData["Teams"] = teams;
            HttpContext.Session.SetString("Teams", vmi.Teams);
            ViewBag.teams = teams;


            int numMatches = teams / 2;
            int totalSessions = 0;
            string tbls = "";
            switch (numMatches)
            {
                case < 5:
                    totalSessions = 1;
                    break;
                case < 9:
                    totalSessions = 2;
                    break;
                default:
                    totalSessions = 3;
                    break;
            }

            int[] ttables = new int[4];
            string tablesBySession = numTablePairs[teams];
            string[] tabs = tablesBySession.Split(' ');

            for (int z = 0; z < (totalSessions); z++)
            {
                tbls = tbls + tableAssignment[int.Parse(tabs[z])] + " ";
            }

            Session curSession = sessionRepository.SelectById(sid);

            ViewBag.curSession = curSession;
            ViewBag.teams = teams;
            ViewBag.sessionTimes = sessionTimes;
            ViewBag.numTables = tabs;
            ViewBag.tableAssignment = tbls;
            ViewBag.numMatches = numMatches;
            ViewBag.tablesByMatches = tableAssignment;

            // Get remaining weeks of schedule 
            List<Schedule> higherWeeks = scheduleRepository.SelectAllWeeksHigherBySessionId(sid, week);

            // Add a week to the entries after selected date
            foreach (Schedule schedule in higherWeeks)
            {
                schedule.Week++;
                scheduleRepository.Update(schedule);
            }

            // Add entry for week off
            Schedule weekOff = new Schedule();
            weekOff.Week = week + 1;
            weekOff.Note = vmi.Note;
            weekOff.SessionId = sid;
            weekOff.Teams = teams;
            weekOff.HomeTeam = 0;
            weekOff.VisitingTeam = 0;
            weekOff.TimeSlot = 0;
            weekOff.TableGroup = 0;
            weekOff.Id = 0;
            scheduleRepository.Insert(weekOff);
            return Ok();
            //return RedirectToAction(nameof(Index));


            // Return view of updated schedule
            //List<Schedule> data = scheduleRepository.SelectAllBySessionId(sid);
            //ScheduleViewModel vm = new ScheduleViewModel();
            //vm.Sid = sid.ToString();
            //vm.Teams = teams.ToString();
            //vm.DataList = data;
            //return View(vm);
        }



        [HttpPost]
        public IActionResult DeleteWeek(string id, string week)
        {
            int dWeek = int.Parse(week);
            int sid = int.Parse(id);
            // This routine is called to delete a week of play from a schedule

            List<Schedule> deleteWeek = scheduleRepository.SelectAllByWeek(sid, dWeek);
            foreach (Schedule schedule in deleteWeek)
            {
                scheduleRepository.Delete(schedule.Id);
            }

            // Get remaining weeks of schedule 
            List<Schedule> higherWeeks = scheduleRepository.SelectAllWeeksHigherBySessionId(sid, dWeek);

            // Subtract a week from each entry after selected date
            foreach (Schedule schedule in higherWeeks)
            {
                schedule.Week = schedule.Week - 1;
                scheduleRepository.Update(schedule);
            }

            return RedirectToAction(nameof(Index));

            // Return view of updated schedule
            //List<Schedule> data = scheduleRepository.SelectAllBySessionId(sid);
            //ScheduleViewModel vm = new ScheduleViewModel();

            //vm.Sid = sid.ToString();
            //vm.Teams = data[0].Teams.ToString();
            //vm.DataList = data;
            //return View();
        }



        public IActionResult View(string id)
        {
            int sessId = int.Parse(id);
            List<Schedule> schedules = scheduleRepository.SelectAllBySessionId(sessId);
            int teams = schedules.ElementAt(0).Teams;
            int numMatches = teams / 2;
            int totalSessions = 0;
            string tbls = "";
            switch (numMatches)
            {
                case < 5:
                    totalSessions = 1;
                    break;
                case < 9:
                    totalSessions = 2;
                    break;
                default:
                    totalSessions = 3;
                    break;
            }

            int[] ttables = new int[4];
            string tablesBySession = numTablePairs[teams];
            string[] tabs = tablesBySession.Split(' ');

            for (int z = 0; z < (totalSessions); z++)
            {
                tbls = tbls + tableAssignment[int.Parse(tabs[z])] + " ";
            }

            //Session curSession = sessionRepository.SelectByCurrent();
            Session curSession = sessionRepository.SelectById(sessId);

            ViewBag.curSession = curSession;
            ViewBag.teams = teams;
            ViewBag.sessionTimes = sessionTimes;
            ViewBag.numTables = tabs;
            ViewBag.tableAssignment = tbls;
            ViewBag.numMatches = numMatches;
            ViewBag.tablesByMatches = tableAssignment;

            // Get the master schedule -- SessionId = input session id
            List<Schedule> data = scheduleRepository.SelectAllBySessionId(sessId);

            ScheduleViewModel vm = new ScheduleViewModel();
            vm.DataList = data;
            return View(vm);
        }


        public async Task<IActionResult> SelectAsync(int id, string teams)
        {
            Session curSession = sessionRepository.SelectByCurrent();
            if (id == 0)
            {
                id = curSession.SessionId;
            }

            if (String.IsNullOrEmpty(teams))
            {
                if (HttpContext.Session.GetString("Teams") == null)
                {
                    teams = "8";
                }
                else
                {
                    teams = HttpContext.Session.GetString("Teams");
                }
            }

            ViewData["SessionId"] = id;
            ViewData["Teams"] = teams;
            HttpContext.Session.SetString("Teams", teams);
            ViewBag.teams = teams;


            int numMatches = int.Parse(teams) / 2;
            int totalSessions = 0;
            string tbls = "";
            switch (numMatches)
            {
                case < 5:
                    totalSessions = 1;
                    break;
                case < 9:
                    totalSessions = 2;
                    break;
                default:
                    totalSessions = 3;
                    break;
            }

            int[] ttables = new int[4];
            string tablesBySession = numTablePairs[int.Parse(teams)];
            string[] tabs = tablesBySession.Split(' ');

            for (int z = 0; z < (totalSessions); z++)
            {
                tbls = tbls + tableAssignment[int.Parse(tabs[z])] + " ";
            }


            ViewBag.curSession = curSession;
            ViewBag.teams = teams;
            ViewBag.sessionTimes = sessionTimes;
            ViewBag.numTables = tabs;
            ViewBag.tableAssignment = tbls;
            ViewBag.numMatches = numMatches;
            ViewBag.tablesByMatches = tableAssignment;

            // Get the master schedule -- SessionId = 0
            List<Schedule> master = scheduleRepository.SelectAllByTeams(int.Parse(teams), 0);

            // Make a copy of the master schedule for updating and saving
            List<Schedule> data = new List<Schedule>();

            foreach (Schedule sched in master)
            {
                Schedule cs = new Schedule();
                cs.SessionId = sched.SessionId;
                cs.Teams = sched.Teams;
                cs.Week = sched.Week;
                cs.TimeSlot = sched.TimeSlot;
                cs.TableGroup = sched.TableGroup;
                cs.HomeTeam = sched.HomeTeam;
                cs.VisitingTeam = sched.VisitingTeam;
                cs.Id = 0;  // Id will be set automatically if created
                data.Add(cs);
            }

            await FillSessionsAsync();
            ScheduleViewModel vm = new ScheduleViewModel();
            vm.DataList = data;

            return View(vm);


        }




        [HttpPost]
        public IActionResult Select(int id, int teams)
        {
            List<Schedule> schedules = scheduleRepository.SelectAllBySessionId(id);
            // if there was a schedule for this session, delete its entries
            if (schedules.Count > 0)
            {
                foreach (Schedule schedule in schedules)
                {
                    scheduleRepository.Delete(schedule.Id);
                }
            }
            schedules = null;
            int newId = 0;

            // Get the schedule blank for the desired number of teams
            List<Schedule> scheds = scheduleRepository.SelectAllByTeams(teams, 0);

            // Update each entry with sessionId and save in database
            foreach (Schedule sched in scheds)
            {
                sched.Id = 0;
                sched.SessionId = id;
                newId = scheduleRepository.Insert(sched);
                sched.Id = newId;
                ;
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Display(string teams)
        {
            if (String.IsNullOrEmpty(teams))
            {
                if (HttpContext.Session.GetString("Teams") == null)
                {
                    teams = "8";
                }
                else
                {
                    teams = HttpContext.Session.GetString("Teams");
                }
            }

            ViewData["Teams"] = teams;
            HttpContext.Session.SetString("Teams", teams);
            ViewBag.teams = teams;

            int numMatches = int.Parse(teams) / 2;
            int totalSessions = 0;
            string tbls = "";
            switch (numMatches)
            {
                case < 5:
                    totalSessions = 1;
                    break;
                case < 9:
                    totalSessions = 2;
                    break;
                default:
                    totalSessions = 3;
                    break;
            }

            int[] ttables = new int[4];
            string tablesBySession = numTablePairs[int.Parse(teams)];
            string[] tabs = tablesBySession.Split(' ');

            for (int z = 0; z < (totalSessions); z++)
            {
                tbls = tbls + tableAssignment[int.Parse(tabs[z])] + " ";
            }

            ViewBag.sessionTimes = sessionTimes;
            ViewBag.numTables = tabs;
            ViewBag.tableAssignment = tbls;
            ViewBag.numMatches = numMatches;
            List<Schedule> data = scheduleRepository.SelectAllByTeams(int.Parse(teams), 0);
            return View(data);
        }
        public async Task<IActionResult> MakeAsync(string teams)
        {
            if (String.IsNullOrEmpty(teams))
            {
                if (HttpContext.Session.GetString("Teams") == null)
                {
                    teams = "8";
                }
                else
                {
                    teams = HttpContext.Session.GetString("Teams");
                }
            }

            ViewData["Teams"] = teams;
            HttpContext.Session.SetString("Teams", teams);
            ViewBag.teams = teams;

            int numMatches = int.Parse(teams) / 2;
            int totalSessions = 0;
            string tbls = "";
            switch (numMatches)
            {
                case < 5:
                    totalSessions = 1;
                    break;
                case < 9:
                    totalSessions = 2;
                    break;
                default:
                    totalSessions = 3;
                    break;
            }

            int[] ttables = new int[4];
            string tablesBySession = numTablePairs[int.Parse(teams)];
            string[] tabs = tablesBySession.Split(' ');

            for (int z = 0; z < (totalSessions); z++)
            {
                tbls = tbls + tableAssignment[int.Parse(tabs[z])] + " ";
            }

            Session curSession = sessionRepository.SelectByCurrent();

            ViewBag.curSession = curSession;
            ViewBag.sessionTimes = sessionTimes;
            ViewBag.numTables = tabs;
            ViewBag.tableAssignment = tbls;
            ViewBag.numMatches = numMatches;
            // Get the master schedule -- SessionId = 0
            List<Schedule> master = scheduleRepository.SelectAllByTeams(int.Parse(teams), 0);

            // Make a copy of the master schedule for updating and saving
            List<Schedule> data = new List<Schedule>();

            foreach (Schedule sched in master)
            {
                Schedule cs = new Schedule();
                cs.SessionId = sched.SessionId;
                cs.Teams = sched.Teams;
                cs.Week = sched.Week;
                cs.TimeSlot = sched.TimeSlot;
                cs.TableGroup = sched.TableGroup;
                cs.HomeTeam = sched.HomeTeam;
                cs.VisitingTeam = sched.VisitingTeam;
                cs.Id = 0;  // Id will be set automatically if created
                data.Add(cs);
            }
            ScheduleViewModel vm = new ScheduleViewModel();
            vm.DataList = data;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> MakeAsync(ScheduleViewModel vm)
        {
            IEnumerable<Schedule> data = vm.DataList;
            foreach (Schedule schedule in data)
            {
                scheduleRepository.Insert(schedule);
            }
            TempData["Message"] = "Schedule inserted successfully!";
            return View(data);
        }


        public IActionResult List(string teams, string week)
        {
            if (String.IsNullOrEmpty(teams))
            {
                if (HttpContext.Session.GetString("Teams") == null)
                {
                    teams = "24";
                    week = "1";
                }
                else
                {
                    teams = HttpContext.Session.GetString("Teams");
                    week = HttpContext.Session.GetString("Week");
                }
            }
            ViewData["Teams"] = teams;
            ViewData["Week"] = week;
            HttpContext.Session.SetString("Teams", teams);
            HttpContext.Session.SetString("Week", week);

            List<Schedule> data = scheduleRepository.SelectAllByTeams(int.Parse(teams), int.Parse(week));
            return View(data);
        }

        public async Task<IActionResult> GetAsync(int id)
        {
            Schedule model = scheduleRepository.SelectByID(id);
            return View(model);
        }


        public async Task<IActionResult> InsertAsync()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertAsync(Schedule model)
        {
            if (ModelState.IsValid)
            {
                scheduleRepository.Insert(model);
                TempData["Message"] = "Schedule inserted successfully!";
            }
            //return RedirectToAction("List");
            return View(model);
        }

        public async Task<IActionResult> UpdateAsync(int id)
        {
            Schedule model = scheduleRepository.SelectByID(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAsync(Schedule model)
        {
            if (ModelState.IsValid)
            {
                scheduleRepository.Update(model);
                TempData["Message"] = "Schedule entry updated successfully!";
            }
            //return View(model);
            return RedirectToAction("List");
        }

        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDeleteAsync(int id)
        {
            Schedule model = scheduleRepository.SelectByID(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            scheduleRepository.Delete(id);
            TempData["Message"] = "Schedule deleted successfully!";
            return RedirectToAction("List");
        }

        public IActionResult MakeSheet(int id)
        {
            Session theSession = sessionRepository.SelectById(id);
            var season = theSession.Season.ToString();
            //    (snType)int.Parse(theSession.Season);

            DateTime sDate = DateTime.Parse(theSession.StartDate);
            List<Schedule> schedule = scheduleRepository.SelectAllBySessionId(id);
            int teams = schedule.ElementAt(0).Teams;

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "Schedule" + schedDate + season;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/Schedules/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/Schedules/pdf/" + fileName;

            int[] ttables = new int[4];
            string tablesBySession = numTablePairs[teams];
            string[] tabs = tablesBySession.Split(' ');
            string[] theTables = tabs;
            int matchesInSession = 0;
            int numberOfColumns = (teams / 2) + (teams % 2) + 1;

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


            xlr = xls.get_Range(xls.Columns[1], xls.Columns[22]);
            xlr.NumberFormat = "@";
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            xlr.Font.Size = 14;
            xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
            xlr.NumberFormat = "MM/DD/YYYY";

            //  Make the sheet header lines
            xls.Cells[1, 1] = "OBG Men's Billiard Club";
            xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[1, numberOfColumns]);
            xlr.Merge();
            xlr.Font.Size = 24;
            xlr.Font.Bold = true;
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xls.Cells[2, 1] = season + " " + theSession.Year.ToString() + " Schedule";
            xlr = xls.get_Range(xls.Cells[2, 1], xls.Cells[2, numberOfColumns]);
            xlr.Merge();
            xlr.Font.Size = 24;
            xlr.Font.Bold = true;
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xlr = xls.get_Range(xls.Cells[3, 1], xls.Cells[3, numberOfColumns]);
            xlr.Merge();
            xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xls.Cells[3, 1] = "";
            int c = 2;
            int x = 0;
            int tr = 4;       // Time Row
            int tblr = 5;     // Table Row
            int startRow = 6; // Start schedule weeks Row


            //The following block is for the time row *@
            foreach (string numOfMatches in theTables)
            {
                matchesInSession = int.Parse(numOfMatches);
                xls.Cells[tr, c + x] = sessionTimes[x];
                xlr = xls.get_Range(xls.Cells[tr, c], xls.Cells[tr, c + matchesInSession - 1]);
                xlr.Merge();
                xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                x++;
                c = c + matchesInSession;
            }
            if (teams % 2 > 0)
            {
                xls.Cells[tr, c + teams] = "";
            }


            //  The following block is for the tables row *@
            c = 2;
            xls.Cells[tblr, 1] = "Date";
            foreach (string numOfMatches in theTables)
            {
                matchesInSession = int.Parse(numOfMatches);
                string slotTables = tableAssignment[matchesInSession];
                string[] tableArray = slotTables.Split(" ");

                for (int i = 0; i < @matchesInSession; i++)
                {
                    xls.Cells[tblr, c + i] = tableArray[i];
                    xlr = xls.get_Range(xls.Cells[tblr, c + i], xls.Cells[tblr, c + i]);
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                }
                c = c + matchesInSession;
            }
            if (teams % 2 > 0)
            {
                xls.Cells[4, c + teams] = "Bye";
            }
            xlr = xls.get_Range(xls.Rows[tr], xls.Rows[tblr]);
            xlr.Font.Bold = true;

            var matches = 0;
            if (teams % 2 == 0)
            {
                matches = teams / 2;
            }
            else
            {
                matches = (teams / 2) + 1;
            }
            var trows = schedule.Count() / matches;
            int increm = matches;
            int r = startRow;

            //  Now all the weeks of the season are placed in the schedule in the following block
            for (int i = 0; i < schedule.Count(); i = i + increm)
            {
                xls.Cells[r, 1] = sDate.AddDays(7 * (schedule.ElementAt(i).Week - 1)).ToShortDateString();
                if (schedule.ElementAt(i).HomeTeam == 0)
                {
                    increm = 1;
                    xls.Cells[r, 2] = schedule.ElementAt(i).Note;
                    xlr = xls.get_Range(xls.Cells[r, 2], xls.Cells[r, 2 + matches - 1]);
                    xlr.Merge();
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    xlr.Font.Bold = true;
                }
                else
                {
                    increm = matches;
                    for (int m = 0; m < matches; m++)
                    {
                        if (schedule.ElementAt(i + m).VisitingTeam == 0)
                        {
                            xls.Cells[r, 2 + m] = schedule.ElementAt(i + m).HomeTeam;
                        }
                        else
                        {
                            xls.Cells[r, 2 + m] = schedule.ElementAt(i + m).HomeTeam + "-" + schedule.ElementAt(i + m).VisitingTeam;
                        }
                    }
                }
                trows = r;
                r++;
            }

            //  Final formatting
            xlr = xls.get_Range(xls.Cells[tr, 1], xls.Cells[trows, 1]);
            xlr.Columns.AutoFit();
            xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 14;

            //   Draw lines for the boxed part of page
            xlr = xls.get_Range(xls.Cells[tr, 1], xls.Cells[trows, matches + 1]);
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlMedium;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeBottom].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlMedium;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeRight].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlMedium;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeLeft].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlMedium;
            xlr.Borders[Excel.XlBordersIndex.xlEdgeTop].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlInsideHorizontal].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlInsideHorizontal].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

            xlr.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
            xlr.Borders[Excel.XlBordersIndex.xlInsideVertical].Weight = XlBorderWeight.xlThin;
            xlr.Borders[Excel.XlBordersIndex.xlInsideVertical].ColorIndex = XlColorIndex.xlColorIndexAutomatic;

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

            return RedirectToAction("Index");

        }


        public IActionResult MakeTeamSheet(int id)
        {
            Session theSession = sessionRepository.SelectById(id);
            var season = theSession.Season.ToString();
            //    (snType)int.Parse(theSession.Season);

            DateTime sDate = DateTime.Parse(theSession.StartDate);
            List<Schedule> schedule = scheduleRepository.SelectAllBySessionId(id);
            int teams = schedule.ElementAt(0).Teams;

            // Save files to wwwRoot/Archives
            string schedDate = sDate.ToString("yyyy");
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = "TeamSchedules" + schedDate + "0" + theSession.Season;
            string extension = ".xlsx";
            string SaveFileName = wwwRootPath + "/Archives/TeamSchedules/xlsx/" + fileName;
            string SavePdfName = wwwRootPath + "/Archives/TeamSchedules/pdf/" + fileName;
            string SaveTeamName = SavePdfName + "_Team_";
            string thisTeam;

            List<Team> tList = teamRepository.SelectAllByNumber(theSession.SessionId);

            int[] ttables = new int[4];
            string tablesBySession = numTablePairs[teams];
            string[] tabs = tablesBySession.Split(' ');
            string[] theTables = tabs;
            int matchesInSession = 0;
            int numberOfColumns = (teams / 2) + (teams % 2) + 1;
            const int MINTEAMS = 17;
            string[] tableChoices = { "", "1&2", "3&4", "5&6", "7&8" };


            // Initialize Excel workbook
            Excel.Application xla = new Excel.Application();
            Excel.Workbook xlb = xla.Workbooks.Add();
            Excel.Worksheet xls = new Excel.Worksheet();
            Excel.Range xlr;

            string leading = "0";
            for (int n = 0; n < tList.Count(); n++)
            {
                if (n > 8)
                {
                    leading= "";
                }
                thisTeam = leading + (n + 1).ToString();

                Team t = tList[n];
                xls = (Excel.Worksheet)xlb.Sheets.Add(After: xlb.Sheets[xlb.Sheets.Count]);
                xls.Name = "Team " + (t.TeamNumber).ToString();
                string theCaptain = playerRepository.SelectByTeamCaptain(t.TeamId).Member.FullName;

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


                xlr = xls.get_Range(xls.Columns[1], xls.Columns[22]);
                xlr.NumberFormat = "@";
                xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                xlr.Font.Size = 12;
                xlr = xls.get_Range(xls.Columns[1], xls.Columns[1]);
                xlr.NumberFormat = "MM/DD/YYYY";

                //  Make the sheet header lines
                xls.Cells[1, 1] = "OBG Men's Billiard Club";
                xlr = xls.get_Range(xls.Cells[1, 1], xls.Cells[1, numberOfColumns]);
                xlr.Merge();
                xlr.Font.Bold = true;
                xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                xls.Cells[2, 1] = season + " " + theSession.Year.ToString() + " Schedule";
                xlr = xls.get_Range(xls.Cells[2, 1], xls.Cells[2, numberOfColumns]);
                xlr.Merge();
                xlr.Font.Bold = true;
                xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;


                xlr = xls.get_Range(xls.Cells[3, 1], xls.Cells[3, numberOfColumns]);
                xlr.Merge();
                xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                xls.Cells[3, 1] = "";
                string teamLine = "Team " +
                                    tList[n].TeamNumber.ToString() + ":  " +
                                    tList[n].TeamName.Trim();
                teamLine = teamLine + ", Captain:  " + theCaptain;
                xls.Cells[3, 1] = teamLine;

                xlr = xls.get_Range(xls.Rows[1], xls.Rows[3]);
                xlr.Font.Size = 16;
                xlr.Font.Bold = true;

                int c = 2;
                int x = 0;
                int tr = 4;       // Time Row
                int tblr = 5;     // Table Row
                int startRow = 6; // Start schedule weeks Row

                //The following block is for the time row *@
                foreach (string numOfMatches in theTables)
                {
                    matchesInSession = int.Parse(numOfMatches);
                    xls.Cells[tr, c + x] = sessionTimes[x];
                    xlr = xls.get_Range(xls.Cells[tr, c], xls.Cells[tr, c + matchesInSession - 1]);
                    xlr.Merge();
                    xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    x++;
                    c = c + matchesInSession;
                }
                if (teams % 2 > 0)
                {
                    xls.Cells[tr, c + teams] = "";
                }


                //  The following block is for the tables row *@
                c = 2;
                xls.Cells[tblr, 1] = "Date";
                foreach (string numOfMatches in theTables)
                {
                    matchesInSession = int.Parse(numOfMatches);
                    string slotTables = tableAssignment[matchesInSession];
                    string[] tableArray = slotTables.Split(" ");

                    for (int i = 0; i < @matchesInSession; i++)
                    {
                        xls.Cells[tblr, c + i] = tableArray[i];
                        xlr = xls.get_Range(xls.Cells[tblr, c + i], xls.Cells[tblr, c + i]);
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    }
                    c = c + matchesInSession;
                }
                if (teams % 2 > 0)
                {
                    xls.Cells[4, c + teams] = "Bye";
                }
                xlr = xls.get_Range(xls.Rows[tr], xls.Rows[tblr]);
                xlr.Font.Bold = true;

                var matches = 0;
                if (teams % 2 == 0)
                {
                    matches = teams / 2;
                }
                else
                {
                    matches = (teams / 2) + 1;
                }
                var trows = schedule.Count() / matches;
                int increm = matches;
                int r = startRow;

                //  Now all the weeks of the season are placed in the schedule in the following block
                for (int i = 0; i < schedule.Count(); i = i + increm)
                {
                    xlr = xls.get_Range(xls.Cells[r, 1], xls.Cells[r, 2 + matches - 1]);
                    if (i % 2 == 0)
                    {
                        xlr.Interior.Color = XlRgbColor.rgbLightYellow;
                    }
                    else
                    {
                        xlr.Interior.Color = XlRgbColor.rgbKhaki;
                    }

                    xls.Cells[r, 1] = sDate.AddDays(7 * (schedule.ElementAt(i).Week - 1)).ToShortDateString();
                    if (schedule.ElementAt(i).HomeTeam == 0)
                    {
                        increm = 1;
                        xls.Cells[r, 2] = schedule.ElementAt(i).Note;
                        xlr = xls.get_Range(xls.Cells[r, 2], xls.Cells[r, 2 + matches - 1]);
                        xlr.Merge();
                        xlr.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                        xlr.Font.Bold = true;
                    }
                    else
                    {
                        increm = matches;
                        for (int m = 0; m < matches; m++)
                        {
                            if (schedule.ElementAt(i + m).VisitingTeam == 0)
                            {
                                xls.Cells[r, 2 + m] = schedule.ElementAt(i + m).HomeTeam;
                            }
                            else
                            {
                                xls.Cells[r, 2 + m] = schedule.ElementAt(i + m).HomeTeam + "-" + schedule.ElementAt(i + m).VisitingTeam;
                            }
                            if (schedule.ElementAt(i + m).HomeTeam == (n + 1) ||
                                schedule.ElementAt(i + m).VisitingTeam == (n + 1))
                            {
                                xlr = xls.get_Range(xls.Cells[r, 2 + m], xls.Cells[r, 2 + m]);
                                xlr.Interior.Color = XlRgbColor.rgbHotPink;
                            }
                        }
                    }
                    trows = r;
                    r++;
                }

                //  Final formatting
                xlr = xls.get_Range(xls.Cells[tr, 1], xls.Cells[trows, 1]);
                xlr.Columns.AutoFit();
                xls.get_Range(xls.Columns[1], xls.Columns[1]).ColumnWidth = 14;

                //   Draw lines for the boxed part of schedule
                xlr = xls.get_Range(xls.Cells[tr, 1], xls.Cells[trows, matches + 1]);
                boxInterior(xlr);
                boxOutline(xlr);
                xlr = xls.get_Range(xls.Cells[tr, 1], xls.Cells[tr, matches + 1]);
                boxOutline(xlr);
                xlr = xls.get_Range(xls.Cells[tblr, 1], xls.Cells[tblr, matches + 1]);
                boxOutline(xlr);
                
        
                string[] numTabPairs = numTablePairs[teams].Split(" ");
                int totalCols = 0;
                int startcols = 2;
                for(int j = 0; j < numTabPairs.Length; j++)
                {
                    totalCols += int.Parse(numTabPairs[j]);
                    xlr = xls.get_Range(xls.Cells[tr, startcols], 
                        xls.Cells[trows, totalCols + 1]);
                    boxOutline(xlr);
                    startcols = totalCols + 2;
                }


                Team oppTeam = new Team();
                Player oppCaptain = new Player();
                r = trows + 3;
                startRow = r;
                //  Now all the weeks of the season are placed in schedule with team names
                for (int i = 0; i < schedule.Count(); i = i + increm)
                {
                    if (tList.Count() > MINTEAMS)
                    {
                        xls.Cells[r, 2] = schedule.ElementAt(i).Week.ToString();
                        xls.Cells[r, 3] = sDate.AddDays(7 * (schedule.ElementAt(i).Week - 1)).ToShortDateString();
                        xlr = xls.get_Range(xls.Cells[r, 3], xls.Cells[r, 4]);
                        xlr.Merge();
                        xlr.NumberFormat = "MM/DD/YYYY";

                        xlr = xls.get_Range(xls.Cells[r, 2], xls.Cells[r, 11]);
                        if (i % 2 == 0)
                        {
                            xlr.Interior.Color = XlRgbColor.rgbLightYellow;
                        }
                        else
                        {
                            xlr.Interior.Color = XlRgbColor.rgbKhaki;
                        }

                        if (schedule.ElementAt(i).HomeTeam == 0)
                        {
                            // This week has no matches scheduled
                            increm = 1;
                            xlr = xls.get_Range(xls.Cells[r, 5], xls.Cells[r, 11]);
                            xlr.Merge();
                        }
                        else
                        {
                            increm = matches;
                            for (int m = 0; m < matches; m++)
                            {
                                if (schedule.ElementAt(i + m).VisitingTeam == 0)
                                {
                                    //This is a bye
                                }
                                else
                                {
                                    if (schedule.ElementAt(i + m).HomeTeam == (n + 1))
                                    {
                                        // This match is being played by the selected team
                                        oppTeam = tList[schedule.ElementAt(i + m).VisitingTeam - 1];
                                        oppCaptain = playerRepository.SelectByTeamCaptain(oppTeam.TeamId);
                                        xls.Cells[r, 5] = schedule.ElementAt(i + m).VisitingTeam.ToString().Trim();
                                        xls.Cells[r, 6] = sessionTimes[schedule.ElementAt(i + m).TimeSlot - 1];

                                        string tablePair = tableChoices[schedule.ElementAt(i + m).TableGroup];

                                        xls.Cells[r, 7] = tablePair;
                                        xls.Cells[r, 8] = oppTeam.TeamName.Trim();
                                        xlr = xls.get_Range(xls.Cells[r, 8], xls.Cells[r, 9]);
                                        xlr.Merge();
                                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                                        xls.Cells[r, 10] = oppCaptain.Member.FullName.Trim();
                                        xlr = xls.get_Range(xls.Cells[r, 10], xls.Cells[r, 11]);
                                        xlr.Merge();
                                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                                        break;
                                    }
                                    else if (schedule.ElementAt(i + m).VisitingTeam == (n + 1))
                                    {
                                        // This match is being played by the selected team                                        
                                        oppTeam = tList[schedule.ElementAt(i + m).HomeTeam - 1];
                                        oppCaptain = playerRepository.SelectByTeamCaptain(oppTeam.TeamId);
                                        xls.Cells[r, 5] = schedule.ElementAt(i + m).HomeTeam.ToString().Trim();
                                        xls.Cells[r, 6] = sessionTimes[schedule.ElementAt(i + m).TimeSlot - 1];

                                        string tablePair = tableChoices[schedule.ElementAt(i + m).TableGroup];

                                        xls.Cells[r, 7] = tablePair;
                                        xls.Cells[r, 8] = oppTeam.TeamName.Trim();
                                        xlr = xls.get_Range(xls.Cells[r, 8], xls.Cells[r, 9]);
                                        xlr.Merge();
                                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                                        xls.Cells[r, 10] = oppCaptain.Member.FullName.Trim();
                                        xlr = xls.get_Range(xls.Cells[r, 10], xls.Cells[r, 11]);
                                        xlr.Merge();
                                        xlr.HorizontalAlignment = XlHAlign.xlHAlignLeft;

                                        break;
                                    }
                                }
                            }

                        }
                    }
                    trows = r;
                    r++;
                }


                xlr = xls.get_Range(xls.Cells[startRow, 2], xls.Cells[trows, 11]);
                boxInterior(xlr);
                boxOutline(xlr);


                //  Save the worksheet and close the workbook   
                xlb.ExportAsFixedFormat(
                    Excel.XlFixedFormatType.xlTypePDF,
                    SaveTeamName + thisTeam,
                    Excel.XlFixedFormatQuality.xlQualityStandard,
                    true,
                    true,
                    n+1,
                    n+1,
                    false);

            }


            // Remove extraneous worksheets
            foreach (Worksheet wks in xlb.Worksheets)
            {
                if (!wks.Name.StartsWith("Team"))
                {
                    wks.Delete();
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
                45,
                false);
            xlb.Close();

            return RedirectToAction("Index");
        }


        private void boxSlot(Excel.Range xlr)
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





        public async Task<bool> FillYearsAsync()
        {
            List<string> listSessions = sessionRepository.SelectByYears();
            List<SelectListItem> sessions = (from s in listSessions
                                             select new SelectListItem()
                                             { Text = s, Value = s }).ToList();
            ViewBag.Years = sessions;
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
            ViewBag.sessionList = sessions;
            //ViewData["SessionId"] = sessions;
            return true;
        }
    }
}

