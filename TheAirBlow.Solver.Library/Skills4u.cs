using System.Xml;
using AngleSharp;
using AngleSharp.Scripting;
using Jint.Native;
using Jint.Native.Array;

namespace TheAirBlow.Solver.Library;

// ReSharper disable once InconsistentNaming
public static class Skills4u
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
        var jsValues = AngleHelper.NativeToArray(lesson);
        for (var i = 0; i < jsValues!.Count() - 1; i++) {
            var item = jsValues![i];
            var obj = item.AsObject();
            var question = $"{obj.GetProperty("question").Value.AsString()}";
            var answer = $"{obj.GetProperty("answer").Value.AsString()}";
            question = question.Replace("<br>", "");
            question = question.Replace("</br>", "");
            answer = answer.Replace("<br>", "");
            answer = answer.Replace("</br>", "");
            pairs.Add(new QuestionAnswerPair {
                Question = question,
                Answer = answer
            });
        }

        return pairs;
    }
}