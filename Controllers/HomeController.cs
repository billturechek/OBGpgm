using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using NuGet.ProjectModel;
using OBGpgm.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace OBGpgm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment Environment;

        public HomeController(IWebHostEnvironment _environment, ILogger<HomeController> logger)
        {
            Environment = _environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Index2(string report="Schedules", string format="pdf")
        {
            string dir = "Archives/" + report + "/" + format + "/";
            //Fetch all files in the Folder (Direfilectory).
            string lpath = Environment.WebRootPath;
            string lfile = "";
            string[] filePaths = Directory.GetFiles(Path.Combine(Environment.WebRootPath, dir));

            //Copy File names to Model collection.
            List<FileModel> files = new List<FileModel>();
            foreach (string filePath in filePaths)
            {
                lfile = filePath.Substring(filePath.LastIndexOf(lpath) + 1);
                files.Add(new FileModel { FileName = Path.GetFileName(filePath), FileDir = lpath });
            }

            var newfiles = files.OrderByDescending(x => x.FileName).ToList();

            return View(newfiles);
        }
        public FileResult DownloadFile1(string fileName)
        {
            //Build the File Path.
            string path = Path.Combine(this.Environment.WebRootPath, "Archives/") + fileName;

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }
        public FileResult DownloadFile(string fileName)
        {
            //Build the File Path.
            string path = Path.Combine(this.Environment.WebRootPath, "Archives/");
            if (fileName.StartsWith("Schedule"))
            {
                path = path + "Schedules/";
            }
            else
            {
                if (fileName.StartsWith("Roster"))
                {
                    path = path + "Rosters/";
                }
            }

            if (fileName.EndsWith(".pdf"))
            {
                path = path + "pdf/";
            }
            else
            {
                if (fileName.EndsWith(".xlsx"))
                {
                    path = path + "xlsx/";
                }
            }

            path = path + fileName;    
                    
            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}