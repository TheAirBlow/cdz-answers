using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Scripting;
using Jint.Native;
using Jint.Native.Array;
using Newtonsoft.Json.Linq;
using Spectre.Console;

namespace TheAirBlow.Solver.Library;

public static class TestEdu
{
    /// <summary>
    /// Shift charcodes by one
    /// </summary>
    /// <param name="input">Input</param>
    /// <returns>Output</returns>
    private static string GetShiftedString(string input)
        => input.Select(i => Encoding.UTF8.GetBytes(i.ToString())[0] - 1)
            .Aggregate("", (current, newCharCode) => current + Encoding
                .UTF8.GetString(new[] {(byte) newCharCode}));

    /// <summary>
    /// Get all checkboxes' values
    /// </summary>
    /// <param name="rez">REZ integer</param>
    /// <param name="vinq">VINQ integer</param>
    /// <returns></returns>
    private static bool[] GetCheckBoxesValues(int rez, int vinq)
    {
        var array = new BitArray(new[] { rez });
        var output = new bool[array.Length + vinq];
        var actualLength = (int)Math.Log(rez, 2.0) + 1;
        AnsiConsole.WriteLine($"{actualLength} {vinq}");
        array.CopyTo(output, vinq - actualLength);
        Array.Resize(ref output, vinq);
        return output;
    }

    /// <summary>
    /// As AngleSharp.JS is a piece of shit,
    /// we are gonna parse it by ourselves
    /// </summary>
    /// <param name="variable">Variable's name</param>
    /// <param name="content">HTML content</param>
    /// <returns>Variable's content (JSON Array only)</returns>
    private static JArray GetVariableContent(string variable, string content)
    {
        var index = content.IndexOf($"var {variable}=", StringComparison.Ordinal);
        index += $"var {variable}=".Length;
        var endIndex = content[index..].IndexOf(
            "\n", StringComparison.Ordinal) - 1;
        var rawContent = content[index..][..endIndex];
        return JArray.Parse(rawContent);
    }
    
    /// <summary>
    /// Get answers
    /// </summary>
    /// <param name="link">Link</param>
    /// <returns>Answers</returns>
    public static async Task<List<string>> GetAnswers(string link)
    {
        var answers = new List<string>();
        var context = BrowsingContext.New(Configuration.Default
            .WithJs().WithDefaultLoader());
        var document = await context.OpenAsync(link);
        await document.WaitForReadyAsync();
        var rez = GetVariableContent("rez", document.Head!.InnerHtml);
        var vinq = GetVariableContent("vinq", document.Head.InnerHtml);
        var svob = GetVariableContent("svob", document.Head.InnerHtml);
        var checkBoxesUsed = 0;
        var textBoxesUsed = 0;
        foreach (var i in document.QuerySelectorAll("font"))
            i.RemoveAttribute("color");
        foreach (IHtmlDivElement i in document.QuerySelectorAll(
                     "div.onetest")) {
            var textBoxes = i.QuerySelectorAll(
                "input[type='text']");
            if (textBoxes.Length != 0) {
                foreach (IHtmlInputElement j in i.QuerySelectorAll(
                             "input[type='text']")) {
                    var newElement = document.CreateElement("div");
                    newElement.SetAttribute("class", "num_ege");
                    newElement.InnerHtml = GetShiftedString(
                        svob[textBoxesUsed][0].ToString());
                    j.Replace(newElement);
                    textBoxesUsed++;
                }
            } else {
                var checkBoxesValues = GetCheckBoxesValues(
                    (int)rez[checkBoxesUsed], (int)vinq[checkBoxesUsed]);
                AnsiConsole.WriteLine(string.Join("", checkBoxesValues));
                var query = i
                    .QuerySelectorAll("input");
                for (var j = 0; j < query.Length; j++) {
                    var item = query[j];
                    if (checkBoxesValues[j])
                        item.SetAttribute("checked", "true");
                    item.SetAttribute("onclick", "return false;");
                }
                checkBoxesUsed++;
            }
            answers.Add(i.InnerHtml);
        }
        return answers;
    }
}