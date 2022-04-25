using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;

namespace TheAirBlow.Solver.Library
{
    public static class SkySmart
    {
        /// <summary>
        /// Exercise data
        /// </summary>
        public class ExerciseData
        {
            public string Uuid;
            public string Data;
            public string Title;
            public bool IsRandom;
            public bool IsInteractive;
        }
        
        /// <summary>
        /// Information JSON
        /// </summary>
        public class UserInformation
        {
            [JsonProperty("name")] public string Name;
            [JsonProperty("surname")] public string Surname;
        }
        
        /// <summary>
        /// XML content JSON
        /// </summary>
        public class ExerciseXml
        {
            public XmlDocument XmlContent;
            [JsonProperty("uuid")] public string Uuid;
            [JsonProperty("title")] public string Title;
            [JsonProperty("content")] public string Content;
            [JsonProperty("isRandom")] public bool IsRandom;
            [JsonProperty("isInteractive")] public bool IsInteractive;
            [JsonProperty("stepRevId")] public int ExerciseIdentifier;
        }
        
        /// <summary>
        /// Exercises' UUIDs JSON
        /// </summary>
        public class ExerciseMeta
        {
            public class MetaClass {
                public class TitleClass {
                    [JsonProperty("title")] public string Title;
                }

                [JsonProperty("stepUuids")] public string[] Uuids;
                [JsonProperty("subject")] public TitleClass Subject;
                [JsonProperty("teacher")] public UserInformation TeacherInformation;
                [JsonProperty("stepsMeta")] public Dictionary<string, TitleClass> StepsMeta;
                
            }

            [JsonProperty("meta")] public MetaClass Meta;
        }

        /// <summary>
        /// A resource
        /// </summary>
        public class Resource
        {
            [JsonProperty("link")] public string Link;
            [JsonProperty("key")] public string Key;
            [JsonProperty("url")] public string Url;
        }
        
        /// <summary>
        /// Login/password pair JSON
        /// </summary>
        private class LoginPasswordPair
        {
            [JsonProperty("phoneOrEmail")] public string Login;
            [JsonProperty("password")] public string Password;
        }
        
        private const string LoginRequest = "https://api-edu.skysmart.ru/api/v2/auth/auth/student";
        private const string Xml = "https://api-edu.skysmart.ru/api/v1/content/step/load?stepUuid=";
        private const string Image = "https://api.vimbox.skyeng.ru/api/v1/resources/images";
        private const string Preview = "https://api-edu.skysmart.ru/api/v1/task/preview";
        private const string Video = "https://content.vimbox.skyeng.ru/cms/api/video";
        private const string AesKey = "A2B5EE3A4B73F8FCDA27FA32DA4EDBC8";
        private const string AesIv = "xnMcr9WXXaSnzyph";
        public static string Token = "";

        /// <summary>
        /// Encrypt a string and convert to Base64
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Output</returns>
        public static string EncryptAndBase64(string input)
        {
            using var aes = Aes.Create();
            using var enc = 
                aes.CreateEncryptor(
                    Encoding.UTF8.GetBytes(AesKey), 
                    Encoding.UTF8.GetBytes(AesIv));
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt,
                enc, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
                swEncrypt.Write(input);
            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        /// <summary>
        /// Get resources
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>Resources array</returns>
        public static Resource[] GetImages(string[] ids)
        {
            using var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Token}");
            var idsString = HttpUtility.UrlEncode(
                EncryptAndBase64($"\"{string.Join(",", ids)}\""));
            var address = $"{Image}?ids={idsString}&isRetina=0";
            var answer = client.DownloadString(address);
            var array = JArray.Parse(answer); return 
                array.ToObject<Resource[]>()!;
        }
        
        /// <summary>
        /// Get resources
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>Resources array</returns>
        public static Resource[] GetVideos(string[] ids)
        {
            using var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Token}");
            var idsString = HttpUtility.UrlEncode(
                EncryptAndBase64($"\"{string.Join(",", ids)}\""));
            var address = $"{Video}?ids={idsString}";
            var answer = client.DownloadString(address);
            var array = JArray.Parse(answer); return 
                array.ToObject<Resource[]>()!;
        }

        /// <summary>
        /// Authenticate (login)
        /// </summary>
        public static void Authenticate(string login, string password)
        {
            using var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            client.Headers.Add(HttpRequestHeader.Accept, "application/json; charset=UTF-8");
            var data = JsonConvert.SerializeObject(new LoginPasswordPair {
                Password = password,
                Login = login
            });
            var answer = client.UploadString(LoginRequest, "POST", data);
            dynamic json = JObject.Parse(answer);
            Token = json.jwtToken;
        }

        /// <summary>
        /// Get all exercises' XML UUIDs
        /// </summary>
        /// <param name="taskHash">Task Hash</param>
        /// <returns>UUIDs</returns>
        public static ExerciseMeta GetAnswerXmlsUuids(string taskHash)
        {
            using var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Token}");
            var data = "{\"taskHash\":\"" + taskHash + "\"}";
            var answer = client.UploadString(Preview, "POST", data);
            var json = JsonConvert.DeserializeObject<ExerciseMeta>(answer);
            return json;
        }

        /// <summary>
        /// Get answer XML from UUID
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <param name="uuids">UUIDs Meta</param>
        /// <returns>Answer XML</returns>
        public static ExerciseXml GetAnswerXml(string uuid, ExerciseMeta uuids)
        {
            using var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Token}");
            var answer = client.DownloadString(Xml + uuid);
            var json = JsonConvert.DeserializeObject<ExerciseXml>(answer);
            var content = json.Content.Replace(
                "<br>", "<br/>");
            var doc = new XmlDocument();
            doc.LoadXml(content);
            json.XmlContent = doc;
            json.Title = uuids.Meta.StepsMeta[uuid].Title;
            json.Uuid = uuid;
            return json;
        }

        /// <summary>
        /// Answers with metadata
        /// </summary>
        public class AnswersAndMeta
        {
            public List<ExerciseData> Answers = new();
            public string SubjectName;
            public string TeacherName;
        }

        /// <summary>
        /// Get answers
        /// </summary>
        /// <param name="taskHash">Task Hack</param>
        /// <returns>All answers pairs</returns>
        public static AnswersAndMeta GetAnswers(string taskHash)
        {
            var uuids = GetAnswerXmlsUuids(taskHash);
            var data = new AnswersAndMeta {
                TeacherName = $"{uuids.Meta.TeacherInformation.Surname} {uuids.Meta.TeacherInformation.Name}",
                SubjectName = uuids.Meta.Subject.Title
            };
            
            for (var i = 0; i < uuids.Meta.Uuids.Length; i++) {
                var uuid = uuids.Meta.Uuids[i];
                var xml = GetAnswerXml(uuid, uuids);
                var root = xml.XmlContent["div"];
                var title = $"Задание №{i + 1}: {xml.Title}";
                
                #region Image Drag&Drop Question (Set)
                foreach (XmlNode k in root?.SelectNodes($"//vim-dnd-image-set")!) {
                    var drags = k["vim-dnd-image-set-drags"];
                    var nodes = k.SelectNodes("//vim-dnd-image-set-drop");
                    for (var h = 0; h < nodes!.Count; h++) {
                        var item = (XmlElement)nodes[h]!;
                        var ids = item.GetAttribute("drag-ids").Split(',');
                        var list2 = ids.Select(id => (XmlElement)drags?
                            .SelectSingleNode($"//*[@answer-id='{id}']")!).ToList();
                        var div = xml.XmlContent.CreateElement("div");
                        var img = xml.XmlContent.CreateElement("img");
                        img.SetAttribute("src", item.GetAttribute("image"));
                        div.SetAttribute("class", "block");
                        div.AppendChild(img); div.InnerXml += "<br/>";
                        for (var j = 0; j < list2.Count; j++) {
                            list2[j].SetAttribute("class", 
                                "block-text");
                            div.AppendChild(list2[j]);
                            if (j != list2.Count - 1)
                                div.InnerXml += " или ";
                        }

                        div.InnerXml = $"<b>{div.InnerXml}</b>";
                        k.AppendChild(div);
                    }
                }
                #endregion
                #region Image Drag&Drop Question (On)
                foreach (XmlElement k in root.SelectNodes($"//vim-dnd-image")!) {
                    var rootDiv = xml.XmlContent.CreateElement("div");
                    rootDiv.SetAttribute("style", "position:relative" +
                                               ";display:inline-block");
                    var root2 = xml.XmlContent.CreateElement("div");
                    root2.SetAttribute("style", "position:static;");
                    var drags = k["vim-dnd-image-drags"];
                    var nodes = k.SelectNodes("//vim-dnd-image-drop");
                    var usedUp = new List<string>();
                    for (var h = 0; h < nodes!.Count; h++) {
                        var item = (XmlElement)nodes[h]!;
                        var ids = item.GetAttribute("drag-ids").Split(',');
                        var list2 = ids.Select(id => (XmlElement)drags?
                            .SelectSingleNode($"//*[@answer-id='{id}']")!).ToList();
                        XmlElement selected = null!;
                        foreach (var l in list2)
                            if (!usedUp.Contains(l.GetAttribute("answer-id"))) {
                                usedUp.Add(l.GetAttribute("answer-id"));
                                selected = l;
                                break;
                            }

                        if (selected == null!)
                            item.InnerText = "Ошибка!";
                        else {
                            var x = item.GetAttribute("x");
                            var y = item.GetAttribute("y");
                            item.SetAttribute("style",
                                "position:absolute;margin-left:" +
                                "-18px;margin-top:-18px;" +
                                $"top:{y}%;left:{x}%;");
                            item.AppendChild(selected);
                        }

                        root2.AppendChild(item);
                    }

                    rootDiv.AppendChild(root2);
                    var img = xml.XmlContent.CreateElement("img");
                    img.SetAttribute("src", k.GetAttribute("image"));
                    rootDiv.AppendChild(img);
                    k.AppendChild(rootDiv);
                }
                #endregion
                #region Sentence Analysis Question
                foreach (XmlElement k in root?.SelectNodes($"//kids-sentence-analysis")!) {
                    var type = k.GetAttribute("type");
                    switch(type) {
                        case "color":
                            foreach (XmlElement j in k.FirstChild!.ChildNodes) {
                                var color = j.GetAttribute("value");
                                color = color.Replace("salad", 
                                    "limegreen");
                                j.SetAttribute("style",
                                    "display:inline-block;" +
                                    "background:#303030;border-radius" +
                                    ":5px;margin-bottom:2px;margin-right" +
                                    ":6px;margin-left: 6px;padding: 5px;" +
                                    "border-bottom-width:4px;border-" +
                                    $"bottom-color:{color};border-" +
                                    "bottom-style:solid;");
                            }
                            break;
                        default:
                            k.InnerXml = $"[Неизвестный тип задания \"{type}\"]";
                            AnsiConsole.MarkupLine($"[yellow]Found new Sentence Analysis type: {type}[/]");
                            break;
                    }
                }
                #endregion
                #region Text Drag&Drop Question
                foreach (XmlNode k in root?.SelectNodes($"//vim-dnd-text")!) {
                    var drags = k["vim-dnd-text-drags"];
                    var nodes = k.SelectNodes("//vim-dnd-text-drop");
                    var usedUp = new List<string>();
                    for (var h = 0; h < nodes!.Count; h++) {
                        var item = nodes[h];
                        var ids = item!.Attributes?["drag-ids"]!.InnerText.Split(',');
                        var list2 = ids!.Select(id => drags?.SelectSingleNode(
                            $"//*[@answer-id='{id}']")!).ToList();
                        
                        XmlElement selected = null!;
                        foreach (XmlElement l in list2)
                            if (!usedUp.Contains(l.GetAttribute("answer-id"))) {
                                usedUp.Add(l.GetAttribute("answer-id"));
                                selected = l;
                                break;
                            }
                        
                        if (selected == null!)
                            item.InnerText = "Ошибка!";
                        else item.AppendChild(selected);
                        
                        item.InnerXml = $"<b>{item.InnerXml}</b>";
                    }
                }
                #endregion
                #region Strike Out Question
                foreach (XmlElement k in root.SelectNodes($"//vim-strike-out")!) {
                    var delimiter = k.GetAttribute("delimiter");
                    foreach (XmlNode l in k.SelectNodes("vim-strike-out-item[@striked='true']")!)
                        l.InnerXml = $"<s>{l.InnerXml}</s>";

                    for (var j = 0; j < k.ChildNodes.Count; j++) {
                        var node = (XmlElement)k.ChildNodes[j]!;
                        if (j != k.ChildNodes.Count - 1)
                            node.InnerXml += delimiter;
                    }
                }
                #endregion
                #region Math Input Question
                foreach (XmlNode k in root.SelectNodes($"//math-input")!) {
                    var first = k.FirstChild!;
                    k.RemoveAll();
                    k.AppendChild(first);
                }
                #endregion
                #region Drag&Drop Question
                foreach (XmlNode k in root.SelectNodes($"//vim-dnd-group")!) {
                    var drags = k["vim-dnd-group-drags"];
                    var newNode = xml.XmlContent.CreateElement(
                        "div", k.NamespaceURI);
                    var nodes = k.SelectNodes("//vim-dnd-group-item");
                    var table = xml.XmlContent.CreateElement("table");
                    var tr1 = xml.XmlContent.CreateElement("tr");
                    var tr2 = xml.XmlContent.CreateElement("tr");
                    table.SetAttribute("class", "text-white");
                    tr1.SetAttribute("class", "bg-neardark");
                    table.AppendChild(tr1); table.AppendChild(tr2);
                    newNode.AppendChild(table);

                    for (var h = 0; h < nodes!.Count; h++) {
                        var item = nodes[h];
                        var ids = item!.Attributes?["drag-ids"]!.InnerText.Split(',');
                        var list2 = ids!.Select(id => drags?.SelectSingleNode(
                            $"//*[@answer-id='{id}']")!).ToList();
                        var thTitle = xml.XmlContent
                            .CreateElement("th");
                        var thElements = xml.XmlContent
                            .CreateElement("th");
                        thTitle.InnerText = item.InnerText;
                        
                        foreach (var t in list2) {
                            var div = xml.XmlContent
                                .CreateElement("div");
                            div.SetAttribute("class", 
                                "small-text-box");
                            var regex1 = new Regex("\\.*\\(.*\\)");
                            var regex2 = new Regex("\\.*{.*}");
                            if (regex1.Match(t.InnerText).Length != 0
                                || regex2.Match(t.InnerText).Length != 0)
                                t.InnerText = $"\\({t.InnerText}\\)";
                            div.InnerText = t.InnerText;
                            thElements.AppendChild(div);
                        }

                        tr2.AppendChild(thElements);
                        tr1.AppendChild(thTitle);
                    }

                    k.AppendChild(newNode);
                }
                #endregion
                #region Select Question
                foreach (XmlNode k in root.SelectNodes($"//vim-select")!) {
                    var first = k.FirstChild?.SelectSingleNode("*[@correct='true']");
                    k.FirstChild?.RemoveAll();
                    k.AppendChild(first!);
                }
                #endregion
                #region Groups Question
                foreach (XmlNode k in root?.SelectNodes($"//vim-groups")!)
                foreach (XmlNode vim in k?.ChildNodes!) {
                    var first = Encoding.UTF8.GetString(Convert.FromBase64String
                        (vim.ChildNodes[0]?.Attributes?["text"]?.InnerText!));
                    var second = Encoding.UTF8.GetString(Convert.FromBase64String
                        (vim.ChildNodes[1]?.Attributes?["text"]?.InnerText!));
                    first = first.Contains('\\') || first.Contains('~') ? $"\\({first}\\)" : first;
                    second = second.Contains('\\') || second.Contains('~') ? $" \\({second}\\)" : second;
                    vim.InnerXml = ""; vim.InnerText = $"{first} → {second}";
                }
                #endregion
                #region Input Question
                foreach (XmlNode k in root.SelectNodes($"//vim-input")!) {
                    var first = k.FirstChild?.FirstChild!;
                    k.FirstChild?.RemoveAll();
                    k.AppendChild(first);
                }
                #endregion
                #region Test Question
                foreach (XmlNode k in root.SelectNodes($"//vim-test")!) {
                    var nodes = k.ChildNodes;
                    for (var l = 0; l < nodes.Count; l++) {
                        var text = nodes[l]?["vim-test-question-text"];
                        text!.InnerXml = $"❓ {text.InnerXml}<br/>";
                        foreach (XmlElement j in nodes[l]?["vim-test-answers"]?.ChildNodes!) {
                            var original = j.InnerXml; j.InnerXml = "";
                            var checkbox = xml.XmlContent.CreateElement("input");
                            if (j.HasAttribute("correct") && j.GetAttribute(
                                    "correct") == "true")
                                checkbox.SetAttribute("checked", "true");
                            checkbox.SetAttribute("onclick", "return false;");
                            checkbox.SetAttribute("type", "checkbox");
                            j.AppendChild(checkbox); j.InnerXml += $" {original}<br/>";
                        }

                        if (l != nodes.Count - 1)
                            nodes[l]!.InnerXml += "<br/>";
                    }
                }
                #endregion
                #region Images
                var images = root!.SelectNodes("//vim-image");
                if (images!.Count != 0) {
                    var ids = new List<string>();
                    foreach (XmlElement j in images)
                        ids.Add(j.GetAttribute("resource-id"));
                    var resources = GetImages(ids.ToArray());
                    for (int j = 0; j < images.Count; j++) {
                        var item = images[j] as XmlElement;
                        var img = xml.XmlContent
                            .CreateElement("img");
                        img.SetAttribute("src",
                            resources[j].Link);
                        img.SetAttribute("class", 
                            "image");
                        item!.AppendChild(img);
                    }
                }
                #endregion
                #region Videos
                var videos = root.SelectNodes("//vim-video");
                if (videos!.Count != 0) {
                    var ids = new List<string>();
                    foreach (XmlElement j in videos)
                        ids.Add(j.GetAttribute("resource-id"));
                    var resources = GetVideos(ids.ToArray());
                    for (int j = 0; j < videos.Count; j++) {
                        var item = videos[j] as XmlElement;
                        var resource = resources[j];
                        if (resource.Url.Contains("youtube.com")) {
                            var iframe = xml.XmlContent.CreateElement("iframe");
                            var div = xml.XmlContent.CreateElement("div");
                            iframe.SetAttribute("allow",
                                "accelerometer; autoplay; " +
                                "clipboard-write; encrypted-media; " +
                                "gyroscope; picture-in-picture");
                            iframe.SetAttribute("src",
                                $"https://www.youtube.com/embed/{resource.Key}" +
                                $"?controls=1&amp;disablekb=1&amp;playsinline" +
                                $"=1&amp;enablejsapi=1&amp;iv_load_policy=3&amp;" +
                                $"cc_load_policy=3&amp;rel=0&amp;showinfo=0&amp;" +
                                $"color=white");
                            iframe.SetAttribute("frameborder", "0");
                            iframe.SetAttribute("class", "video");
                            iframe.SetAttribute("height", "100%");
                            iframe.SetAttribute("width", "100%");
                            div.AppendChild(iframe);
                            item!.AppendChild(div);
                        } else {
                            item!.InnerText = "[Данное видео не поддерживается]";
                        }
                    }
                }
                #endregion

                // Slightly refactor the XML
                foreach (var j in new[] { "vim-math", "math-input-answer" })
                foreach (XmlNode k in root.SelectNodes($"//{j}")!)
                    k.InnerText = $"\\({k.InnerText}\\)";

                foreach (XmlElement k in root.SelectNodes($"//vim-text")!) {
                    switch (k.GetAttribute("type")) {
                        case "strong":
                            k.InnerXml = $"<b>{k.InnerXml}</b>";
                            break;
                        case "em":
                            k.InnerXml = $"<em>{k.InnerXml}</em>";
                            break;
                        case "em strong":
                        case "strong em":
                            k.InnerXml = $"<em><b>{k.InnerXml}</b></em>";
                            break;
                        default:
                            AnsiConsole.MarkupLine($"[yellow]Found new vim-text type: {k.GetAttribute("type")}[/]");
                            break;
                    }
                }
                
                foreach (XmlNode k in root.SelectNodes($"//vim-iframe")!)
                    k.InnerText = "[Тут находится медиа-контент или игра]";

                foreach (var j in new[] { "vim-dnd-text-drags", "vim-dnd-group-drags", 
                             "vim-dnd-group-groups", "vim-instruction", "vim-source-list",
                             "vim-dnd-image-set-drags", "vim-dnd-image-set-images", 
                             "edu-interactive-hint", "vim-dnd-image-drags" })
                foreach (XmlNode k in root.SelectNodes($"//{j}")!)
                    k.ParentNode?.RemoveChild(k);
                    
                data.Answers.Add(new ExerciseData {
                    IsInteractive = xml.IsInteractive,
                    IsRandom = xml.IsRandom,
                    Data = root.InnerXml,
                    Uuid = xml.Uuid,
                    Title = title
                });
            }
            
            return data;
        }
    }
}
