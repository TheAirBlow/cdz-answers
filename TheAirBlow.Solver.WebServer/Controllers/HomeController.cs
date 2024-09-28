using Microsoft.AspNetCore.Mvc;
using TheAirBlow.Solver.Library;

namespace TheAirBlow.Solver.WebServer.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(ILoggerFactory logger)
            => GlobalLogger.Instance = logger
                .CreateLogger("TheAirBlow.Solver.Logging");

        public IActionResult Index() => View();
        
        public IActionResult Error() => View();
        
        // ReSharper disable once InconsistentNaming
        public IActionResult API() => View();
        
        public IActionResult Changelogs() => View();
    }
}