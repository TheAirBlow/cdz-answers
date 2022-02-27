using Microsoft.AspNetCore.Mvc;

namespace TheAirBlow.Solver.WebServer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        
        public IActionResult Error() => View();
        
        // ReSharper disable once InconsistentNaming
        public IActionResult API() => View();
        
        public IActionResult Changelogs() => View();
    }
}