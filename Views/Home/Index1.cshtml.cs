using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OBGpgm.FileUploadService;

namespace OBGpgm.Views.Home
{
    public class Index1Model : PageModel
    {
        private readonly ILogger<Index1Model> _logger;
        private readonly IFileUploadService fileUploadService;
        public string FilePath;
        public Index1Model(ILogger<Index1Model> logger, IFileUploadService fileUploadService)
        {
            _logger = logger;
            this.fileUploadService = fileUploadService;
        }

        public ILogger<Index1Model> Logger { get; }

        public void OnGet()
        {
        }
        public async void OnPost(IFormFile file)
        {
            if (file != null)
            {
                FilePath = await fileUploadService.UploadFileAsync(file);
            }
        }
    }
}
