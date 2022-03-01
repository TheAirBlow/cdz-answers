using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Spectre.Console;
using TheAirBlow.Solver.Library;
using TheAirBlow.Solver.WebServer.Models;

namespace TheAirBlow.Solver.WebServer.Controllers;

public class SolverController : Controller
{
    public IActionResult QueueStatus([FromQuery(Name = "uuid")] string uuid)
    {
        if (!QueueSystem.Instance.ExistsInQueueOrFinished(uuid))
            return Redirect("/Home/Error?type=invalid_uuid");

        return View();
    }

    public IActionResult View1([FromQuery(Name = "uuid")] string uuid)
    {
        if (!QueueSystem.Instance.ExistsInQueueOrFinished(uuid))
            return Redirect("/Home/Error?type=invalid_uuid");

        if (!QueueSystem.Instance.IsFinished(uuid))
            return Redirect($"/Solver/QueueStatus?uuid={uuid}");
        
        if (QueueSystem.Instance.GetFinishedResult(uuid).Failed)
            return Redirect("/Home/Error?type=solving_error");
        
        if (QueueSystem.Instance.GetFinishedResult(uuid)
                .Website != QueueSystem.Website.Skills4u)
            return Redirect("/Home/Error?type=dolbaeb");

        return View();
    }
    
    public IActionResult View2([FromQuery(Name = "uuid")] string uuid)
    {
        if (!QueueSystem.Instance.ExistsInQueueOrFinished(uuid))
            return Redirect("/Home/Error?type=invalid_uuid");

        if (!QueueSystem.Instance.IsFinished(uuid))
            return Redirect($"/Solver/QueueStatus?uuid={uuid}");
        
        if (QueueSystem.Instance.GetFinishedResult(uuid).Failed)
            return Redirect("/Home/Error?type=solving_error");
        
        if (QueueSystem.Instance.GetFinishedResult(uuid)
                .Website != QueueSystem.Website.SkySmart)
            return Redirect("/Home/Error?type=dolbaeb");

        return View();
    }
    
    public IActionResult View3([FromQuery(Name = "uuid")] string uuid)
    {
        if (!QueueSystem.Instance.ExistsInQueueOrFinished(uuid))
            return Redirect("/Home/Error?type=invalid_uuid");

        if (!QueueSystem.Instance.IsFinished(uuid))
            return Redirect($"/Solver/QueueStatus?uuid={uuid}");
        
        if (QueueSystem.Instance.GetFinishedResult(uuid).Failed)
            return Redirect("/Home/Error?type=solving_error");

        var website = QueueSystem.Instance
            .GetFinishedResult(uuid).Website;
        if (website != QueueSystem.Website.Saharina 
            && website != QueueSystem.Website.TestEdu)
            return Redirect("/Home/Error?type=dolbaeb");

        return View();
    }
}