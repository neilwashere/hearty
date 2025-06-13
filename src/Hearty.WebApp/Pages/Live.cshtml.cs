using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hearty.WebApp.Pages
{
    public class LiveModel : PageModel
    {
        private readonly ILogger<LiveModel> _logger;

        public LiveModel(ILogger<LiveModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // This method is called when the page is requested.
            // We don't need to prepare any data from the server for this page,
            // as it's all handled by client-side JavaScript.
        }
    }
}
