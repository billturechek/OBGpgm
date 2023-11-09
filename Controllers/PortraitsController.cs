using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using OBGpgm.Repositories;

namespace OBGpgm.Controllers
{
    public class PortraitsController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IPlayerRepository playerRepository;
        private readonly IPortraitRepository portraitRepository;
        private readonly IMemberRepository memberRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly IWebHostEnvironment hostEnvironment;
        public PortraitsController(HttpClient client,
                        IMemberRepository memberRepository,
                        IPlayerRepository playerRepository,
                        IPortraitRepository portraitRepository,
                        ISessionRepository sessionRepository,
                        IWebHostEnvironment hostEnvironment,
                        IConfiguration config)
        {
            this.client = client;
            this.memberRepository = memberRepository;
            this.playerRepository = playerRepository;   
            this.portraitRepository = portraitRepository;
            this.sessionRepository = sessionRepository; 
            this.hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> ListAsync()
        {
            List<Portrait> data = portraitRepository.SelectAll();
            return View(data);
        }

        public async Task<IActionResult> UploadListAsync(string year, string season)
        {
            await FillMembersAsync();
            //await FillPlayersAsync();
            await FillSessionsAsync();
            await FillYearsAsync();
            // Get the current session
            Session csession = sessionRepository.SelectByCurrent();

            if (String.IsNullOrEmpty(year))
            {
                if (HttpContext.Session.GetString("Year") == null)
                {
                    year = csession.Year;
                    season = csession.Season.ToString();
                    await FillSeasonsAsync(year);
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

            List<Player> uploadList2 = playerRepository.SelectAllMembers();
            var uploadList = uploadList2.OrderBy(p => p.Member.LastName);
            //List<Player> uploadList = playerRepository.SelectAllBySession(year, season);

            return View(uploadList);
        }

        public async Task<IActionResult> DisplayAsync(bool deceased)
        {
            await FillMembersAsync();
            if (deceased)
            {
                await FillImagesDeceasedAsync();
                return View("DisplayDeceased");
            }
            else
            {
                await FillImagesLivingAsync();
                return View("DisplayLiving");
            }
        }



        public async Task<IActionResult> GetAsync(int id)
        {
            Portrait model = portraitRepository.SelectByID(id);
            return View(model);
        }

        public async Task<IActionResult> ShowAsync(int id)
        {
            Portrait model = portraitRepository.SelectByID(id);

            return View(model);
        }


        public async Task<IActionResult> UploadAsync(string mid)
        {
            await FillMembersAsync();
            await FillImageIdsAsync();
            if(mid != null)
            {
                ViewData["MemberId"] = mid;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAsync(Portrait model)
        {

            // Save image to wwwRoot/images
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
            string extension = Path.GetExtension(model.ImageFile.FileName);
            string ImageName = wwwRootPath + "/images/" + fileName + extension;
            string ImageNameReduced = wwwRootPath + "/images/" + fileName + "" + "Reduced" + extension;
            string ImageNameThumb = wwwRootPath + "/images/" + fileName + "" + "Thumb" + extension;
            string path = Path.Combine(wwwRootPath + "/images/", fileName);


            await FillImageIdsAsync();

            var fileName1 = System.IO.Path.GetFileName(model.ImageFile.FileName);
            // Create new local file and copy contents of uploaded file
            // If file with same name exists delete it
            if (System.IO.File.Exists(ImageName))
            {
                System.IO.File.Delete(ImageName);
            }
            using (var localFile = System.IO.File.OpenWrite(ImageName))
            using (var uploadedFile = model.ImageFile.OpenReadStream())
            {
                uploadedFile.CopyTo(localFile);
            }

            // Make file smaller if necessary
            using (var source = Bitmap.FromFile(ImageName))
            {
                using (var reduced = (Bitmap)ResizeImageKeepAspectRatio(source, 1000, 600))
                {
                    // If file with same name exists delete it
                    if (System.IO.File.Exists(ImageNameReduced))
                    {
                        System.IO.File.Delete(ImageNameReduced);
                    }
                    reduced.Save(ImageNameReduced);
                }
            }
            FileStream fs = new FileStream(ImageNameReduced, FileMode.Open, FileAccess.Read);
            MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);
            model.LargeImage = ms.ToArray();
            fs.Close();

            // Make thumbnail image
            using (var source = Bitmap.FromFile(ImageNameReduced))
            {
                using (var reduced = (Bitmap)ResizeImageKeepAspectRatio(source, 100, 100))
                {
                    // If file with same name exists delete it
                    if (System.IO.File.Exists(ImageNameThumb))
                    {
                        System.IO.File.Delete(ImageNameThumb);
                    }
                    reduced.Save(ImageNameThumb);
                }
            }
            fs = new FileStream(ImageNameThumb, FileMode.Open, FileAccess.Read);
            ms = new MemoryStream();
            fs.CopyTo(ms);
            model.ThumbImage = ms.ToArray();
            fs.Close();

            if (model.Memberid > 0)
            {
                model.Member = memberRepository.SelectById(model.Memberid);
                if (model.Member.PortraitId != null && model.Member.PortraitId > 0)
                {
                    model.Id = model.Member.PortraitId ?? 0;
                }
                model.Notes = fileName + extension;
                model.Title = model.Member.FullName;
                if (model.Member.PortraitId == null)
                {
                    int newId = portraitRepository.Insert(model);
                    TempData["Message"] = model.Member.FullName + " - Image inserted successfully!";
                    model.Member.PortraitId = newId;
                    memberRepository.Update(model.Member);
                }
                else
                {
                    portraitRepository.Update(model);
                    //TempData["Message"] = "Image updated successfully!";
                }
            }
            else
            {
                TempData["Message"] = "Select a member!";
            }

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            // If files with given name exist delete 
            if (System.IO.File.Exists(ImageName))
            {
                System.IO.File.Delete(ImageName);
            }
            if (System.IO.File.Exists(ImageNameReduced))
            {
                System.IO.File.Delete(ImageNameReduced);
            }
            if (System.IO.File.Exists(ImageNameThumb))
            {
                System.IO.File.Delete(ImageNameThumb);
            }
            await FillMembersAsync();
            return View(model);
        }


        public async Task<IActionResult> UpdateAsync(int id)
        {
            Portrait model = portraitRepository.SelectByID(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAsync(Portrait model)
        {

            if (ModelState.IsValid)
            {
                portraitRepository.Update(model);
                TempData["Message"] = "Image record updated successfully!";
            }
            return View(model);
        }

        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDeleteAsync(int id)
        {
            Portrait model = portraitRepository.SelectByID(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            portraitRepository.Delete(id);
            TempData["Message"] = "Image deleted successfully!";
            return RedirectToAction("List");
        }


        public System.Drawing.Image ResizeImageKeepAspectRatio(System.Drawing.Image source, int width, int height)
        {
            System.Drawing.Image result = null;
            try
            {
                if (source.Height > height)
                {
                    // Resize image
                    float sourceRatio = (float)source.Width / (float)source.Height;
                    int newerWidth = (int)(height * (float)sourceRatio);
                    using (var target = new Bitmap(newerWidth, height))
                    {
                        using (var g = System.Drawing.Graphics.FromImage(target))
                        {
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.HighQuality;

                            // Scaling
                            float scaling;
                            float scalingY = (float)source.Height / height;
                            float scalingX = (float)source.Width / width;
                            if (scalingX < scalingY) scaling = scalingX;
                            else scaling = scalingY;

                            int newWidth = (int)(height * (float)sourceRatio);
                            int newHeight = height;

                            // Correct float to int rounding
                            // if (newWidth < width) newWidth = width;

                            if (newHeight < height) newHeight = height;



                            // See if image needs to be cropped
                            int shiftX = 0;
                            int shiftY = 0;
                            /*
                            if (newWidth < source.Width)
                            {
                                shiftX = (source.Width - newWidth) / 2;
                            }

                            if (source.Height > newHeight)
                            {
                                shiftY = (source.Height - height) / 2;
                            }
                            */
                            // Draw image
                            g.DrawImage(source, -shiftX, -shiftY, newWidth, newHeight);
                        }

                        result = (System.Drawing.Image)target.Clone();
                    }
                }
                else
                {
                    // Image size matched the given size
                    result = (System.Drawing.Image)source.Clone();
                }
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        public async Task<bool> FillMembersAsync()
        {
            List<Member> listVillages = memberRepository.SelectAll();
            List<SelectListItem> members = (from m in listVillages
                                            select new SelectListItem()
                                            { Text = m.FullName, Value = m.MemberId.ToString() }).ToList();
            ViewBag.Members = members;
            ViewBag.MembersCollection = listVillages;
            return true;
        }

        public async Task<bool> FillImageIdsAsync()
        {
            List<Member> listVillages = memberRepository.SelectAll();
            List<SelectListItem> members = (from m in listVillages
                                            select new SelectListItem()
                                            { Text = m.PortraitId.ToString(), Value = m.MemberId.ToString() }).ToList();
            ViewBag.ImageIds = members;
            return true;
        }

        public async Task<bool> FillImagesDeceasedAsync()
        {
            List<Portrait> data = portraitRepository.SelectAllByDeceased();
            ViewBag.Images = data;
            return true;
        }


        public async Task<bool> FillImagesLivingAsync()
        {
            List<Portrait> data = portraitRepository.SelectAllByLiving();
            ViewBag.Images = data;
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
