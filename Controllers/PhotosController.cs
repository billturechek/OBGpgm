using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OBGpgm.Areas.Identity.Data;
using OBGpgm.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using OBGpgm.Repositories;

namespace OBpgm.Controllers
{
    public class PhotosController : Controller
    {
        private readonly HttpClient client = null;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMemberRepository memberRepository;
        private readonly IPhotoRepository photoRepository;
        private readonly IWebHostEnvironment hostEnvironment;
        public PhotosController(HttpClient client,
                                IMemberRepository memberRepository,
                                IPhotoRepository photoRepository,
                                UserManager<ApplicationUser> userManager,
                                IWebHostEnvironment hostEnvironment,
                                IConfiguration config)
        {
            this.client = client;
            this.userManager = userManager;
            this.memberRepository = memberRepository;
            this.photoRepository = photoRepository;
            this.hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> ListAsync()
        {
            List<Photo> data = photoRepository.SelectAll();
            return View(data);
        }

        public async Task<IActionResult> GetAsync(int id)
        {
            Photo model = photoRepository.SelectByID(id);
            return View(model);
        }

        public async Task<IActionResult> UploadAsync(int id, int articleId, int groupId)
        {
            await FillMemberEmailAsync();
            Photo model = new Photo();
            foreach (SelectListItem item in ViewBag.MemberEmail)
            {
                if (User.Identity.Name == item.Text)
                {
                    model.memberId = Convert.ToInt32(item.Value);
                    //string aid = HttpContext.Request.Query["articleId"].ToString();
                    model.articleId = articleId;
                    model.groupId = groupId;
                    model.id = id;
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAsync(Photo model)
        {
            // Save image to wwwRoot/images
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
            string extension = Path.GetExtension(model.ImageFile.FileName);
            string ImageName = wwwRootPath + "/images/" + fileName + extension;
            string ImageNameReduced = wwwRootPath + "/images/" + fileName + "" + "Reduced" + extension;
            string ImageNameThumb = wwwRootPath + "/images/" + fileName + "" + "Thumb" + extension;
            string path = Path.Combine(wwwRootPath + "/images/", fileName);

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
                using (var reduced = (Bitmap)ResizeImageKeepAspectRatio(source, 700, 500))
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
            model.largeImage = ms.ToArray();
            fs.Close();

            // Make thumbnail image
            using (var source = Bitmap.FromFile(ImageNameReduced))
            {
                using (var reduced = (Bitmap)ResizeImageKeepAspectRatio(source, 120, 120))
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
            model.thumbImage = ms.ToArray();
            fs.Close();

            if (model.groupId == null && model.groupName!= null)
            {
                if (model.groupName.Length > 0)
                {
                    int newGroup = photoRepository.SelectHighGroup();
                    newGroup++;
                    model.groupId = newGroup;
                }
            }




            int newId = photoRepository.Insert(model);
            TempData["Message"] = "Image inserted successfully!";


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
            if (model.articleId == 0)
            {
                return View("Gather", model);
            }
            return View(model);
        }

        public async Task<IActionResult> UpdateAsync(int id)
        {
            Photo model = photoRepository.SelectByID(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAsync(Photo model)
        {
            if (ModelState.IsValid)
            {
                photoRepository.Update(model);
                ViewBag.Message = "Photo updated successfully!";
            }
            return View(model);
        }

        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDeleteAsync(int id)
        {
            Photo model = photoRepository.SelectByID(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            photoRepository.Delete(id);
            TempData["Message"] = "Photo deleted successfully!";
            return RedirectToAction("List");
        }

        
        public async Task<IActionResult> GatherAsync()
        {
            await FillMemberEmailAsync();
            Photo model = new Photo();
            foreach (SelectListItem item in ViewBag.MemberEmail)
            {
                if (User.Identity.Name == item.Text)
                {
                    model.memberId = Convert.ToInt32(item.Value);
                    //string aid = HttpContext.Request.Query["articleId"].ToString();
                    model.articleId = 0;
                    model.groupId = 0;
                }
            }
            await FillGroupsAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GatherAsync(Photo model)
        {
            return View();
        }



        public async Task<bool> FillGroupsAsync()
        {
            List<Photo> listGroups = photoRepository.SelectFirstByGroup();
            if (listGroups.Count > 0)
            {
                List<SelectListItem> groupIds = (from p in listGroups
                                                 select new SelectListItem()
                                                 {
                                                     Text = p.groupId.ToString() + " " + p.groupName,
                                                     Value = p.groupId.ToString()
                                                 }).ToList();
                ViewBag.GroupIds = groupIds;
                ViewBag.Groups = listGroups;
            }
            
            return true;
        }

        public async Task<bool> FillMemberEmailAsync()
        {
            List<Member> listVillages = memberRepository.SelectAll();
            List<SelectListItem> memberIds = (from m in listVillages
                                              select new SelectListItem()
                                              {
                                                  Text = m.Email,
                                                  Value = m.MemberId.ToString()
                                              }).ToList();
            ViewBag.MemberEmail = memberIds;
            return true;
        }

        public async Task<bool> FillMembersAsync()
        {
            List<Member> listVillages = memberRepository.SelectAll();
            List<SelectListItem> members = (from m in listVillages select new SelectListItem() 
            { 
                Text = m.FullName, 
                Value = m.MemberId.ToString() }).ToList();
            ViewBag.Members = members;
            return true;
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
    }
}
