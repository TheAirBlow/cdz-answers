using System.Xml;
using AngleSharp;
using AngleSharp.Scripting;
using Jint.Native;
using Jint.Native.Array;

namespace TheAirBlow.Solver.Library;

// ReSharper disable once InconsistentNaming
public class Skills4u
{
    /// <summary>
    /// Question and Answer Pair
    /// </summary>
    public class QuestionAnswerPair
    {
        public string Question;
        public string Answer;
    }
    
    /// <summary>
    /// Test ID and Number Pair
    /// </summary>
    private class TestIdNumberPair
    {
        public string TestId;
        public string Number;
    }

    /// <summary>
    /// Get answers
    /// </summary>
    /// <param name="page">Page</param>
    /// <returns>Answers</returns>
    public static async Task<List<QuestionAnswerPair>> GetAnswers(string page)
    {
        var link = $"https://skills4u.ru/school/{page}";
        var context = BrowsingContext.New(Configuration.Default
            .WithJs().WithDefaultLoader());
        var js = context.GetService<JsScriptingService>();
        var document = await context.OpenAsync(link);
        var lesson = (ArrayInstance)js?.EvaluateScript(document, 
            "window.lesson")!;
        var pairs = new List<QuestionAnswerPair>();
        var list = lesson?.GetOwnProperties().Select(
            p => p.Value.Value)!;
        var jsValues = list as JsValue[] ?? list?.ToArray();
        for (var i = 0; i < jsValues!.Count() - 1; i++) {
            var item = jsValues[i];
            var obj = item.AsObject();
            var question = $"<root>{obj.GetProperty("question").Value.AsString()}</root>";
            var answer = $"<root>{obj.GetProperty("answer").Value.AsString()}</root>";
            var xml = new XmlDocument();
            xml.LoadXml(question); var str1 = xml.InnerText;
            xml.LoadXml(answer); var str2 = xml.InnerText;
            pairs.Add(new QuestionAnswerPair {
                Question = str1,
                Answer = str2
            });
        }

        return pairs;
    }
}