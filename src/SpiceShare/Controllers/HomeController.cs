using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace SpiceShare.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _hostingEnv;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnv = hostingEnvironment;
        }
        public IActionResult Index()
        {                      
            return  View();
        }

        public IActionResult Share()
        {
            return View();
        }

        public IActionResult Expired()
        {
            return View("Expired");
        }
        
    }
}
