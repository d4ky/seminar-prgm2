using System;
using System.Threading.Channels;

namespace dosApp
{
    internal class Program
    {
        static async Task Main()
        {

            ProcessConent(await RequestContent("https://xkcd.com/2913/"), "page2913.html");

            //int numberOfFiles = int.Parse(Console.ReadLine());

            //for (int i = 1; i < numberOfFiles + 1; i++)
            //{
            //    if (ProcessConent(await RequestContent($"https://xkcd.com/{i}"), $"page{i}.html"))
            //        Console.WriteLine($"Sucessfully changed content [{i}]");
            //    else
            //    {
            //        Console.WriteLine($"Failed to get content [{i}]");
            //        break; // protoze vim, ze indexuji postupne
            //    }
            //}
        }

        private static bool ProcessConent(string? content, string output)
        {
            if (string.IsNullOrEmpty(content)) return false;

            int startIndex = content.IndexOf("title=\"", content.IndexOf("<div id=\"comic\"")) + 7;  // 7 je title="
            int endIndex = content.IndexOf("\"", startIndex);

            string titleText = content.Substring(startIndex, endIndex - startIndex);

            int imgTagEndIndex = content.IndexOf(">", endIndex) + 1;
            string outputContent = content.Insert(imgTagEndIndex, $"\n<p>{titleText}</p>");

            File.WriteAllText(output, outputContent);
            return true;
        }

        private static readonly HttpClient client = new();

        private static async Task<string?> RequestContent(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync()
                : null;
        }
    }
}
