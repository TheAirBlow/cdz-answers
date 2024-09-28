using System.Net;
using System.Net.Http.Headers;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Scripting;
using Spectre.Console;

namespace TheAirBlow.Solver.Library;

public static class Saharina
{
    private static string CookieUid;
    
    public static async Task<string> SendData(IEnumerable<IElement> elements, string link)
    {
        var cookieContainer = new CookieContainer();
        using var handler = new HttpClientHandler {
            CookieContainer = cookieContainer
        };
        using var client = new HttpClient();
        var req = new HttpRequestMessage();
        if (!string.IsNullOrEmpty(CookieUid))
            cookieContainer.Add(new Uri("https://saharina.ru"), 
                new Cookie("uid", CookieUid));
        var data = ""; foreach (var i in elements) {
            if (string.IsNullOrEmpty(i.GetAttribute("name"))) continue;
            var urlEncoded = HttpUtility.UrlEncode(
                i.GetAttribute("value"));
            data += $"&{i.GetAttribute("name")}={urlEncoded}";
        }
        data += "&i_time=1043&o_time=1653";
        data = data.Substring(1, 
            data.Length - 1);
        req.Content = new StringContent(data);
        req.Content!.Headers.ContentType = new MediaTypeHeaderValue
            ("application/x-www-form-urlencoded");
        req.RequestUri = new Uri(link);
        req.Method = HttpMethod.Post;
        var res = await client.SendAsync(req);
        if (cookieContainer.GetAllCookies()["uid"] != null)
            CookieUid = cookieContainer.GetAllCookies()["uid"]!.Value;
        return await res.Content.ReadAsStringAsync();
    }
    
    public static async Task<List<string>> GetAnswersXml(string link)
    {
        var context = BrowsingContext.New(Configuration.Default
            .WithJs().WithDefaultLoader().WithMetaRefresh().WithCss());
        var document = await context.OpenAsync(link);
        await document.WaitForReadyAsync();
        var js = context.GetService<JsScriptingService>();
        var inputs = document
            .QuerySelector("form.js-interactive-test")!
            .QuerySelectorAll("input");
        var random = new Random();
        foreach (var i in inputs) {
            switch (i.GetAttribute("type")) {
                case "url":
                case "file":
                case "email":
                case "image":
                case "reset":
                case "submit":
                case "hidden":
                    // Ignore these
                    break;
                case "password":
                case "search":
                case "text":
                    i.SetAttribute("value", 
                        "Гандон");
                    break;
                case "number":
                case "tel":
                    i.SetAttribute("value", 
                        "1");
                    break;
                case "radio":
                case "checkbox":
                    AnsiConsole.MarkupLine("[yellow]Checkbox detected![/]");
                    break;
                case "range":
                    i.SetAttribute("value", i
                        .GetAttribute("max")!);
                    break;
                default:
                    AnsiConsole.MarkupLine($"[yellow]Found new input type: {i.GetAttribute("type")}[/]");
                    break;
            }
        }

        document = new HtmlParser().ParseDocument(await SendData(inputs, link));
        foreach (var i in document.QuerySelectorAll(
                     ".answer-user, .tools-box, .explanation"))
            i.Remove();
        return document.QuerySelectorAll(".task-container")
            .Select(i => i.InnerHtml).ToList();
    }
}