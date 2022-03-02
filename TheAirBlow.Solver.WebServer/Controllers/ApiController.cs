using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Spectre.Console;
using TheAirBlow.Solver.Library;

namespace TheAirBlow.Solver.WebServer.Controllers
{
    public class ApiController : Controller
    {
        public IActionResult LinkRedirect([FromQuery(Name = "link")] string link)
        {
            try {
                var split = link.Split('/');
                var offset = split[0] is "https:" or "http:" ? 2 : 0;
                switch (split[offset]) {
                    case "edu.skysmart.ru":
                        if (split[offset + 2] is not "task") {
                            if (!Regex.IsMatch(split[offset + 2],
                                    @"^[a-zA-Z]+$")
                                || split[offset + 1] is not "student")
                                throw new Exception("Invalid link!");
                        } else offset++;

                        var uuid1 = QueueSystem.Instance.Enqueue(new QueueSystem.QueueItem {
                            Website = QueueSystem.Website.SkySmart,
                            SolverInput = split[offset + 2]
                        });
                        return Redirect($"~/Solver/QueueStatus?uuid={uuid1}");
                    case "skills4u.ru":
                        if (split[offset + 1] is not "school")
                            throw new Exception("Invalid link!");
                        var uuid2 = QueueSystem.Instance.Enqueue(new QueueSystem.QueueItem {
                            Website = QueueSystem.Website.Skills4u,
                            SolverInput = split[offset + 2]
                        });
                        return Redirect($"~/Solver/QueueStatus?uuid={uuid2}");
                    case "saharina.ru":
                        var uuid3 = QueueSystem.Instance.Enqueue(new QueueSystem.QueueItem {
                            Website = QueueSystem.Website.Saharina,
                            SolverInput = link
                        });
                        return Redirect($"~/Solver/QueueStatus?uuid={uuid3}");
                    case "testedu.ru":
                        var uuid4 = QueueSystem.Instance.Enqueue(new QueueSystem.QueueItem {
                            Website = QueueSystem.Website.TestEdu,
                            SolverInput = link
                        });
                        return Redirect($"~/Solver/QueueStatus?uuid={uuid4}");
                    default:
                        throw new Exception("Invalid link!");
                }
            } catch {
                return Redirect("~/Home/Error?type=invalid_link");
            }
        }

        public string GetQueuePosition([FromQuery(Name = "uuid")] string uuid)
        {
            try {
                return QueueSystem.Instance
                    .GetPositionInQueue(uuid)
                    .ToString();
            } catch {
                return "-1";
            }
        }
        
        public string GetFinishedItem([FromQuery(Name = "uuid")] string uuid)
        {
            try {
                var result = (QueueSystem.QueueItem)
                    QueueSystem.Instance.GetFinishedResult
                        (uuid).Clone();
                result.SolverOutput = "а вот хуй тебе";
                return JsonConvert.SerializeObject(result);
            } catch {
                return "undefined";
            }
        }
        
        public string RemoveFinishedItem([FromQuery(Name = "uuid")] string uuid)
        {
            try {
                return JsonConvert.SerializeObject(
                    QueueSystem.Instance.RemoveFinishedResult
                        (uuid));
            } catch {
                return "undefined";
            }
        }
        
        public string GetLink([FromQuery(Name = "uuid")] string uuid)
        {
            try {
                var finished = QueueSystem.Instance.GetFinishedResult(uuid);
                switch (finished.Website) {
                    case QueueSystem.Website.Skills4u:
                        return $"/Solver/View1?uuid={uuid}";
                    case QueueSystem.Website.SkySmart:
                        return $"/Solver/View2?uuid={uuid}";
                    case QueueSystem.Website.TestEdu:
                    case QueueSystem.Website.Saharina:
                        return $"/Solver/View3?uuid={uuid}";
                }
                return "undefined";
            } catch { return "undefined"; }
        }
    }
}