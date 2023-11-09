using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;

namespace OBGpgm.Controllers
{
    public class SessionsController : Controller
    {
        private readonly ObgDbContext _context;
        private readonly HttpClient client = null;
        private readonly IDraftRepository draftRepository;
        private readonly IMemberRepository memberRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly ITeamRepository teamRepository;
        public SessionsController(HttpClient client,
                                ObgDbContext context,
                                IDraftRepository draftRepository,
                                IMemberRepository memberRepository,
                                IPlayerRepository playerRepository,
                                ISessionRepository sessionRepository,
                                ITeamRepository teamRepository,
                                IConfiguration config)
        {
            _context = context;
            this.client = client;
            this.draftRepository = draftRepository;
            this.memberRepository = memberRepository;
            this.playerRepository = playerRepository;
            this.sessionRepository = sessionRepository;
            this.teamRepository = teamRepository;
        }
        /*
        public SessionsController(ObgDbContext context)
        {
            _context = context;
        }
        */
        // GET: Sessions
        public IActionResult Index(int pg=1)
        {
            List<Session> sessions = _context.Sessions
                .OrderByDescending(s => s.SessionId)
                .ToList();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = sessions.Count();
            var pager = new Pager("Sessions", recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = sessions.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            this.ViewBag.returnPage = pg;
            return View(data);
        }

        // GET: Sessions/Details/5
        public async Task<IActionResult> Details(int? id, int pg=1)
        {
            if (id == null || _context.Sessions == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (session == null)
            {
                return NotFound();
            }
            ViewBag.returnPage = pg;
            return View(session);
        }

        // GET: Sessions/Create
        public IActionResult CreateOld()
        {
            return View();
        }

        // POST: Sessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOld([Bind("SessionId,Year,Season,TeamsD1,TeamsD2,StartDate,CurrentWeek,CurrentSeason,President,VicePresident,Secretary,Treasurer,SecondVp1,SecondVp2,SecondVp3,SecondVp4,DraftType")] Session session)
        {
            if (ModelState.IsValid)
            {
                _context.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(session);
        }

        // GET: Sessions/Edit/5
        public async Task<IActionResult> Edit(int? id, int pg = 1)
        {
            if (id == null || _context.Sessions == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            ViewBag.returnPage = pg;
            return View(session);
        }

        // POST: Sessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,Year,Season,TeamsD1,TeamsD2,StartDate,CurrentWeek,CurrentSeason,President,VicePresident,Secretary,Treasurer,SecondVp1,SecondVp2,SecondVp3,SecondVp4,DraftType")] Session session)
        {
            if (id != session.SessionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.SessionId))
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
            return View(session);
        }

        // GET: Sessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Sessions == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Sessions == null)
            {
                return Problem("Entity set 'OBGcoreContext.Sessions'  is null.");
            }
            var session = await _context.Sessions.FindAsync(id);
            if (session != null)
            {
                _context.Sessions.Remove(session);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> CreateAsync()
        {
            Session csession = sessionRepository.SelectByCurrent();

            int i = 0;
            List<Team> teamList = teamRepository.SelectAllBySeason(csession.Year, csession.Season);

            List<Player> orderedCaptains = new List<Player>();

            // Get list of captains last season
            List<Player> oldCaptains = playerRepository.SelectAllByCaptain(csession.SessionId - 1);
            foreach (Player p in oldCaptains)
            {
                p.Member = memberRepository.SelectById(p.MemberId);
            }

            // Get list of captains for this season
            List<Player> newCaptains = playerRepository.SelectAllByNewCaptain(csession.SessionId);
            foreach (Player p in newCaptains)
            {
                p.Member = memberRepository.SelectById(p.MemberId);
            }

            int returningCaptains = 0;
            foreach (Player p in oldCaptains)
            {
                if (p.Member.WillCaptainNextSession)
                {
                    returningCaptains++;
                }
            }

            if ((csession.TeamsD1 == returningCaptains) || (csession.TeamsD1 < returningCaptains))
            // Have enough captains returning
            {
                Player newCaptain = new Player();
                foreach (Player oldCaptain in oldCaptains)
                {
                    for (i = 0; (i <= (newCaptains.Count - 1)); i++)
                    {
                        newCaptain = newCaptains[i];
                        if ((newCaptain.MemberId == oldCaptain.MemberId))
                        {
                            //  in here if old captain is returning as a captain
                            orderedCaptains.Add(newCaptain);
                            newCaptains.Remove(newCaptain);
                            break;
                        }
                    }
                }
            }
            else  // Need new captains to fill the teams
            {
                Player newCaptain = new Player();
                foreach (Player oldCaptain in oldCaptains)
                {
                    for (i = 0; (i <= (newCaptains.Count - 1)); i++)
                    {
                        newCaptain = newCaptains[i];
                        if ((newCaptain.MemberId == oldCaptain.MemberId))
                        {
                            //  in here if old captain is returning as a captain
                            orderedCaptains.Add(newCaptain);
                            newCaptains.Remove(newCaptain);
                            break;
                        }
                    }
                }
                if (newCaptains.Count > 0)
                {
                    foreach (Player brandNewCaptain in newCaptains)
                    {
                        orderedCaptains.Insert(0, brandNewCaptain);
                    }
                }
                // Check if right number for new session
            }
            foreach (Player c in orderedCaptains)
            {
                c.Member = memberRepository.SelectById(c.MemberId);
            }

            ViewBag.teamsNeeded = csession.TeamsD1;
            ViewBag.teamsHave = orderedCaptains.Count;
            string prefix = "";
            string assign = "";
            if (csession.TeamsD1 > orderedCaptains.Count)
            {
                int teamsShort = csession.TeamsD1 - orderedCaptains.Count;
                prefix = "Still need " + teamsShort.ToString() + " Captains.";
                assign = " disabled='disabled' ";
            }
            if (csession.TeamsD1 < orderedCaptains.Count)
            {
                int teamsOver = orderedCaptains.Count - csession.TeamsD1;
                prefix = "Have an extra " + teamsOver.ToString() + " captains.";
                assign = " disabled='disabled' ";
            }
            if (csession.TeamsD1 == orderedCaptains.Count)
            {
                prefix = "Have the correct number of captains for the teams.";
                assign = "";
            }
            ViewBag.teamStatus = prefix;
            ViewBag.assign = assign;
            return View(orderedCaptains);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateDraftViewModel model)
        {
            int i;
            int n;
            int upperBound;
            Player p;
            int thisTeamNumber = 0;
            int[] teams = new int[50];
            int[] TeamNumber = new int[50];
            List<Team> teamList;

            // Get current session
            Session csession = sessionRepository.SelectByCurrent();

            teamList = teamRepository.SelectAllBySeason(csession.Year, csession.Season);

            List<Player> orderedCaptains = new List<Player>();

            // Get list of captains last season
            List<Player> oldCaptains = playerRepository.SelectAllByCaptain(csession.SessionId - 1);

            // Get list of captains for this season
            List<Player> newCaptains = playerRepository.SelectAllByNewCaptain(csession.SessionId);
            List<Player> clist = newCaptains;

            //  Generate an ordered list of captains for the draft
            Player newCaptain = new Player();
            foreach (Player oldCaptain in oldCaptains)
            {
                for (i = 0; (i <= (newCaptains.Count - 1)); i++)
                {
                    newCaptain = newCaptains[i];
                    if ((newCaptain.MemberId == oldCaptain.MemberId))
                    {
                        //  in here if old captain is returning as a captain

                        newCaptain.Member = memberRepository.SelectById(newCaptain.MemberId);

                        orderedCaptains.Add(newCaptain);
                        newCaptains.Remove(newCaptain);
                        break;
                    }
                }
            }
            if (newCaptains.Count > 0)
            {
                foreach (Player brandNewCaptain in newCaptains)
                {
                    brandNewCaptain.Member = memberRepository.SelectById(brandNewCaptain.MemberId);
                    orderedCaptains.Insert(0, brandNewCaptain);
                }
            }

            for (i = 1; (i <= orderedCaptains.Count); i++)
            {
                //  Generate a random team number for each Captain
                //  Get the draft position
                thisTeamNumber = 0;
                upperBound = orderedCaptains.Count;
                Random random = new Random();
                //int Rnd = random.Next(1, orderedCaptains.Count);

                // Loop to find an unassigned random number within bounds
                do
                {
                    //  Generate a random draft position number

                    if (i == orderedCaptains.Count)
                    {
                        thisTeamNumber = orderedCaptains.Count;
                    }
                    else
                    {
                        thisTeamNumber = random.Next(1, orderedCaptains.Count);
                    }
                    // has any team number been assigned yet
                    if (i > 0)
                    {
                        // Check array to see if number assigned yet
                        if ((TeamNumber[thisTeamNumber] == 0))
                        {
                            //  This team wasn't assigned yet

                            Player aPlayer = new Player();
                            aPlayer = orderedCaptains[(i - 1)];
                            Player newPlayer = new Player();
                            //newPlayer = PlayerDB.CopyPlayer(aPlayer, newPlayer);
                            newPlayer = aPlayer;
                            Team aTeam = new Team();
                            Team newTeam = new Team();

                            TeamNumber[thisTeamNumber] = aPlayer.PlayerId;
                            aTeam = teamList[(thisTeamNumber - 1)];
                            //newTeam = TeamDB.CopyTeam(aTeam, newTeam);
                            newTeam = aTeam;
                            // newPlayer.TeamID = aTeam.TeamID
                            aPlayer.TeamId = aTeam.TeamId;
                            aTeam.TeamName = aPlayer.Member.TeamNameIfCaptain;
                            // update the player
                            playerRepository.Update(aPlayer);

                            teamRepository.Update(aTeam);
                        }
                        else // number already assigned 
                        {
                            // indicate assigned so keep looking
                            thisTeamNumber = 0;
                        }
                    }
                    else // No team number assigned yet
                    {
                        //  First team no need to see if assigned

                        Player aPlayer = new Player();
                        aPlayer = orderedCaptains[(i - 1)];
                        Player newPlayer = new Player();
                        newPlayer = aPlayer;
                        Team aTeam = new Team();
                        Team newTeam = new Team();

                        TeamNumber[thisTeamNumber] = aPlayer.PlayerId;
                        aTeam = teamList[(thisTeamNumber - 1)];
                        newTeam = aTeam;
                        aPlayer.TeamId = aTeam.TeamId;
                        aTeam.TeamName = aPlayer.Member.TeamNameIfCaptain;

                        // update the player
                        playerRepository.Update(aPlayer);

                        teamRepository.Update(aTeam);
                    }
                    // Keep looping to look for an unassigned number
                } while (thisTeamNumber == 0);
            }


            n = csession.TeamsD1;
            for (i = 0; (i <= ((n * 4) - 1)); i++)
            {
                int j = 0;
                Draft d = new Draft();
                d.DraftSessionId = csession.SessionId;
                d.DraftType = DraftTypes.NewReverse;
                //  1st and 2nd round in reverse order
                d.DraftDivision = 1;
                if ((i >= 0) && (i <= (n - 1)))
                {
                    // Select Case i
                    // Case 0 To (n - 1) ' These are entries for the captains
                    j = i;
                    d.DraftPosition = (i + 1);
                    d.DraftRound = 0;
                    d.DraftSelection = 0;
                    p = orderedCaptains[j];

                    Team newTeam = teamRepository.SelectByID(p.TeamId);

                    d.DraftTeamId = p.TeamId;
                    d.DraftPlayerId = p.PlayerId;
                    p.DraftId = d.DraftId;

                    playerRepository.Update(p);
                }
                else if ((i <= ((2 * n) - 1)) && (i >= n))
                {
                    // Case n To ((2 * n) - 1)  ' First round draft picks usual reverse order
                    j = (i - n);
                    d.DraftPosition = ((i - n) + 1);
                    d.DraftRound = 1;
                    d.DraftSelection = d.DraftPosition;
                    p = orderedCaptains[j];

                    //Team newTeam = teamRepository.SelectByID(p.TeamId);
                    d.DraftTeamId = p.TeamId;
                }
                else if ((i <= ((3 * n) - 1)) && (i >= (2 * n)))
                {
                    // Case (2 * n) To ((3 * n) - 1)  ' Second round draft picks usual reverse order
                    j = (i - (2 * n));
                    d.DraftPosition = ((i - (2 * n)) + 1);
                    d.DraftRound = 2;
                    // d.DraftSelection = d.DraftPosition + (2 * n)
                    d.DraftSelection = (d.DraftPosition + n);
                    p = orderedCaptains[j];

                    //Team newTeam = teamRepository.SelectByID(p.TeamId);
                    d.DraftTeamId = p.TeamId;
                }
                else
                {
                    //  Third round draft picks order is previous finish
                    j = ((4 * n) - (i + 1));
                    d.DraftPosition = ((i - (3 * n)) + 1);
                    d.DraftRound = 3;
                    // d.DraftSelection = d.DraftPosition + n
                    d.DraftSelection = (d.DraftPosition + (2 * n));
                    p = orderedCaptains[j];

                    //Team newTeam = teamRepository.SelectByID(p.TeamId);
                    d.DraftTeamId = p.TeamId;
                }

                draftRepository.Insert(d);
            }

            TempData["Message"] = "Teams assigned successfully!";

            return RedirectToAction("List", "Drafts");
        }




        public async Task<IActionResult> InsertAsync()
        {
            await FillLiving();

            Session csession = sessionRepository.SelectByCurrent();
            CreateSessionViewModel model = new CreateSessionViewModel();

            model.aCount = await GetAvail(csession);
            int teamNum = model.aCount / 4;
            int extra = model.aCount % 4;
            if ((teamNum % 8) > 0)
            {
                if (teamNum % 8 > 4)
                {
                    teamNum++;
                }
                else
                {
                    teamNum = teamNum + 2;
                }
            }
            model.teamCount = teamNum;
            model.csession = csession;
            model.sDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertAsync(CreateSessionViewModel model)
        {
            await FillLiving();

            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            model.aCount = await GetAvail(csession);
            int teamNum = model.aCount / 4;
            int extra = model.aCount % 4;
            if ((teamNum % 8) > 0)
            {
                if (teamNum % 8 > 4)
                {
                    teamNum++;
                }
                else
                {
                    teamNum = teamNum + 2;
                }
            }
            model.teamCount = teamNum;
            model.csession = csession;
            model.sDate = DateTime.Now;

            if (1 == 1)
            {
                // get new session info
                Session sess = model.Session;
                // create new session
                int newSessionId = sessionRepository.Insert(sess);
                if (!(sess == null))
                {
                    sessionRepository.ResetCurrent();
                    sessionRepository.SetCurrent(newSessionId);
                    sess = sessionRepository.SelectByCurrent();

                    // Get list of players from previous session
                    List<Player> data = playerRepository.SelectAllBySession(csession.Year, csession.Season);

                    // add a player for each member that will play new season
                    foreach (Player p in data)
                    {
                        if (p.MemberId > 0)
                        {
                            p.Member = memberRepository.SelectById(p.MemberId);

                            if (p.Member.WillPlayNextSession && p.Member.IsActive)
                            {
                                Player newPlayer = new Player();
                                newPlayer.MemberId = p.MemberId;
                                newPlayer.SessionId = sess.SessionId;
                                newPlayer.TeamId = 0;
                                newPlayer.DraftId = 0;
                                newPlayer.StartWeek = "1";
                                newPlayer.EndWeek = "12";
                                newPlayer.DraftRound = "2";
                                newPlayer.SkillLevel = "B";
                                newPlayer.IsInDraft = true;
                                newPlayer.IsPlaying = true;
                                newPlayer.IsBeingTraded = false;
                                newPlayer.IsCaptain = p.Member.WillCaptainNextSession;
                                // add player to new session
                                playerRepository.Insert(newPlayer);
                            }
                        }
                    }

                    // add the number of teams required
                    for (int n = 1; n <= model.Session.TeamsD1; n++)
                    {
                        Team newTeam = new Team();
                        newTeam.TeamName = "Team " + n.ToString();
                        newTeam.TeamNumber = n;
                        newTeam.TeamPoints = 0;
                        newTeam.SessionId = sess.SessionId;
                        newTeam.Division = 1;
                        newTeam.IsChampion = false;
                        newTeam.IsRunnerUp = false;
                        teamRepository.Insert(newTeam);
                    }
                    ViewBag.Message = "Session inserted successfully!";
                }
                else
                {
                    ViewBag.Message = "Error while calling Web API!";
                }

            }
            return View(model);
        }


        private bool SessionExists(int id)
        {
          return (_context.Sessions?.Any(e => e.SessionId == id)).GetValueOrDefault();
        }



        [HttpGet]
        [Route("[action]/{alive}")]
        public async Task<bool> FillLiving()
        {
            List<Member> listAlive = memberRepository.SelectAlive();
            List<SelectListItem> livingMembers = (from m in listAlive select new SelectListItem() { Text = m.FullName, Value = m.MemberId.ToString() }).ToList();
            ViewBag.LivingMembers = livingMembers;
            return true;
        }

        public async Task<int> GetAvail(Session csession)
        {
            List<Player> listPlayers = playerRepository.SelectAllBySession(csession.Year, csession.Season);
            int count = 0;
            foreach (Player p in listPlayers)
            {
                if (p.MemberId > 0)
                {
                    if (p.TeamId > 0)
                    {
                        Member mem = memberRepository.SelectById(p.MemberId);
                        if (mem.IsActive && mem.WillPlayNextSession)
                        {
                            count++;
                        }
                    }
                }
            }
            ViewBag.AvailableCount = count;
            return count;
        }
    }
}
