using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OBGpgm.Areas.Identity.Data;
using OBGpgm.Interfaces;
using OBGpgm.Models;
using OBGpgm.Repositories;

namespace OBGpgm.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IArticleRepository articleRepository;
        private readonly ICommentRepository commentRepository;
        private readonly IMemberRepository memberRepository;
        private readonly IPhotoRepository photoRepository;
        private readonly SignInManager<ApplicationUser> signInManager;
        public ArticlesController(IArticleRepository articleRepository,
                                SignInManager<ApplicationUser> signInManager,
                                ICommentRepository commentRepository,
                                IPhotoRepository photoRepository,
                                IMemberRepository memberRepository)
        {
            this.articleRepository = articleRepository;
            this.commentRepository = commentRepository;
            this.memberRepository = memberRepository;
            this.photoRepository = photoRepository;
            this.signInManager = signInManager;
        }

        // GET: Articles
        public async Task<IActionResult> ListAsync()
        {
            await FillMemberNamesAsync();
            List<Article> data = articleRepository.SelectAll();
            return View(data);
        }

        // GET: Lost and Found
        public async Task<IActionResult> ListLostAsync(int category, int topic)
        {
            await FillMemberNamesAsync();
            List<Article> data = articleRepository.SelectAllByLost();
            return View(data);
        }
        
        public async Task<IActionResult> ListMy()
        {
            await FillMemberEmailAsync();
            await FillMemberNamesAsync();
            int id = 0;
            foreach (SelectListItem m in ViewBag.MemberEmail)
            {
                if (m.Text == User.Identity.Name)
                {
                    id = Convert.ToInt32(m.Value);
                    ViewBag.thisMemberId = id;
                    break;
                }
            }
            List<Article> data = articleRepository.SelectAllById(id);
            return View(data);
        }

        public async Task<IActionResult> Scan(int category)
        {
            await FillMemberNamesAsync();
            List<Article> data = articleRepository.SelectAllByCategory(category);
            ViewBag.Category = category;
            return View(data);
        }

        // GET: News
        public async Task<IActionResult> ListNewsAsync(int category, int topic)
        {
            await FillMemberNamesAsync();
            List<Article> data = articleRepository.SelectAllByCategory(category);
            return View(data);
        }

        // GET: Editorials/Questions
        public async Task<IActionResult> ListTopicAsync(int category, int topic)
        {
            await FillMemberNamesAsync();
            List<Article> data = articleRepository.SelectAllByTopic(topic);
            if (topic == 3)
            {
                return View("ListQuestion", data);
            }
            else
            {
                return View("ListOpinions", data);
            }            
        }
        public async Task<IActionResult> ListOpinionAsync()
        {
            await FillMemberNamesAsync();
            List<Article> data = articleRepository.SelectAllByTopic(2);
            return View("ListOpinion", data);
        }

        public async Task<IActionResult> ListQuestionAsync()
        {
            await FillMemberNamesAsync();
            List<Article> data = articleRepository.SelectAllByTopic(3);
            return View("ListQuestion", data);
        }


        // GET: Articles/Details/5
        public ActionResult Get(int id)
        {
            Article article = articleRepository.SelectByID(id);
            return View(article);
        }

        public async Task<IActionResult> Read(int id)
        {
            Article article = articleRepository.SelectByID(id);
            await FillPhotosAsync(id);
            await FillMemberEmailAsync();
            await FillMemberNamesAsync();
            await CheckCommentsAsync(id);
            return View(article);
        }

        // INSERT: Articles/Insert
        public async Task<IActionResult> Insert()
        {
            await FillMemberEmailAsync();
            Article model = new Article();
            foreach (SelectListItem item in ViewBag.MemberEmail)
            {
                if (User.Identity.Name == item.Text)
                {
                    model.authId = Convert.ToInt32(item.Value);
                    model.pubDate = DateTime.Now;
                    model.lastModified = DateTime.Now;
                    model.isPublished = false;
                    if (!User.IsInRole("Admin"))
                    {
                        model.category = 2;
                    }
                }
            }
            return View(model);
        }

        // POST: Articles/Insert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Insert(Article model)
        {
            if (ModelState.IsValid)
            {
                model.isPublished = true;
                model.articleId = articleRepository.Insert(model);
                TempData["Message"] = "Article inserted successfully!";
            }
            await FillMemberEmailAsync();
            return View(model);
        }




        // INSERT: Articles/Write
        public async Task<IActionResult> Write(int category)
        {
            await FillMemberEmailAsync();
            Article model = new Article();
            model.category = category;
            foreach (SelectListItem item in ViewBag.MemberEmail)
            {
                if (User.Identity.Name == item.Text)
                {
                    model.topic = 1;
                    model.authId = Convert.ToInt32(item.Value);
                    model.pubDate = DateTime.Now;
                    model.lastModified = DateTime.Now;
                    model.isPublished = false;
                    if (!User.IsInRole("Admin"))
                    {
                        model.category = 2;
                    }
                }
            }
            return View(model);
        }

        // POST: Articles/Write
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Write(Article model)
        {
            if (ModelState.IsValid)
            {
                model.isPublished = true;
                model.articleId = articleRepository.Insert(model);
                TempData["Message"] = "Article inserted successfully!";
            }
            await FillMemberEmailAsync();
            return View(model);
        }




        // INSERT: Articles/Report
        public async Task<IActionResult> Report()
        {
            await FillMemberEmailAsync();
            Article model = new Article();
            foreach (SelectListItem item in ViewBag.MemberEmail)
            {
                if (User.Identity.Name == item.Text)
                {
                    model.authId = Convert.ToInt32(item.Value);
                    model.pubDate = DateTime.Now;
                    model.lastModified = DateTime.Now;
                    model.isPublished = false;
                    model.topic = 4;
                    model.category = 2;
                    model.topItem = true;
                    if (!User.IsInRole("Admin"))
                    {
                        model.category = 2;
                    }
                }
            }
            return View(model);
        }

        // POST: Articles/Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(Article model)
        {
                if (model.hasbeenFound)
                {
                    model.topItem = false;
                }
                model.slug = model.title;
                model.isPublished = true;
                model.articleId = articleRepository.Insert(model);
                TempData["Message"] = "Report filed successfully!";
                await FillMemberEmailAsync();
                return RedirectToAction("ListLost");
        }



        // INSERT: Articles/Ask
        public async Task<IActionResult> Ask()
        {
            await FillMemberEmailAsync();
            Article model = new Article();
            foreach (SelectListItem item in ViewBag.MemberEmail)
            {
                if (User.Identity.Name == item.Text)
                {
                    model.authId = Convert.ToInt32(item.Value);
                    model.pubDate = DateTime.Now;
                    model.lastModified = DateTime.Now;
                    model.isPublished = false;
                    model.topic = 3;
                    model.category = 2;
                    model.topItem = true;
                    if (!User.IsInRole("Admin"))
                    {
                        model.category = 2;
                    }
                }
            }
            return View(model);
        }

        // POST: Articles/Ask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(Article model)
        {
            if (model.hasbeenFound)
            {
                model.topItem = false;
            }
            model.slug = model.title;
            model.isPublished = true;
            model.articleId = articleRepository.Insert(model);
            TempData["Message"] = "Report filed successfully!";
            await FillMemberEmailAsync();
            return RedirectToAction("ListQuestion");
        }



        // INSERT: Articles/Opine
        public async Task<IActionResult> Opine(int category)
        {
            await FillMemberEmailAsync();
            Article model = new Article();
            foreach (SelectListItem item in ViewBag.MemberEmail)
            {
                if (User.Identity.Name == item.Text)
                {
                    model.authId = Convert.ToInt32(item.Value);
                    model.pubDate = DateTime.Now;
                    model.lastModified = DateTime.Now;
                    model.isPublished = false;
                    model.topic = 2;
                    model.category = category;
                    model.topItem = true;
                    if (!User.IsInRole("Admin"))
                    {
                        model.category = 2;
                    }
                }
            }
            return View(model);
        }

        // POST: Articles/Opine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Opine(Article model)
        {
            if (model.hasbeenFound)
            {
                model.topItem = false;
            }
            model.slug = model.title;
            model.isPublished = true;
            model.articleId = articleRepository.Insert(model);
            TempData["Message"] = "Report filed successfully!";
            await FillMemberEmailAsync();
            return RedirectToAction("ListQuestion");
        }

        // GET: Articles/Edit/5
        public ActionResult Update(int id)
        {
            Article model = articleRepository.SelectByID(id);
            return View(model);
        }

        // POST: Articles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int id, Article article)
        {
            articleRepository.Update(article);
            return View(article);
        }

        // GET: Articles/Delete/5
        public ActionResult Delete(int id)
        {
            articleRepository.SelectByID(id);
            return View();
        }

        // POST: Articles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Article article)
        {
            try
            {
                articleRepository.Delete(id);
                return RedirectToAction(nameof(ListAsync));
            }
            catch
            {
                return View();
            }
        }

        public async Task<bool> CheckCommentsAsync(int id)
        {
            List<Comment> listComments = commentRepository.SelectAllByArticle(id);
            if (listComments.Count > 0)
            {
                ViewBag.CommentsExist = true;
            }
            else
            {
                ViewBag.CommentsExist = false;
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
        public async Task<bool> FillMemberNamesAsync()
        {
            List<Member> listVillages = memberRepository.SelectAll();
            List<SelectListItem> memberIds = (from m in listVillages
                                              select new SelectListItem()
                                              {
                                                  Text = m.FullName,
                                                  Value = m.MemberId.ToString()
                                              }).ToList();
            ViewBag.MemberName = memberIds;
            return true;
        }
        public async Task<bool> FillPhotosAsync(int id)
        {
            List<Photo> listPhotos = photoRepository.SelectAll(id);
            
            ViewBag.Photos = listPhotos;
            return true;
        }
    }
}