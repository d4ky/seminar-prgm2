using System.Text.RegularExpressions;

namespace ůklůkjaglkj_hblkj_puijo_wewpqt_ihju_
{
    internal class Program
    {
        static void Main()
        {
            string inputFile = Console.ReadLine();
            string outputFile = Console.ReadLine();

            try
            {
                using TextAnalyzer tA = new(inputFile);
                using StreamWriter sW = new(outputFile);

                sW.WriteLine($"Počet slov: {tA.WordCount}");
                sW.WriteLine($"Počet znaků (bez mezer): {tA.CharactersNoSpacesCount}");
                sW.WriteLine($"Počet znaků (s mezerami): {tA.CharactersCount} \n");
                sW.WriteLine(tA.GetWordFreq() + '\n');
                sW.WriteLine(tA.GetSingleSpaceText());
            }
            catch (Exception)
            {
                Console.WriteLine("File error!");
            }
        }

        private class TextAnalyzer : StreamReader
        {
            private readonly Dictionary<string, int> _words;
            private readonly string fileContent;
            public int WordCount { get; }
            public int CharactersNoSpacesCount { get; private set; }   
            public int CharactersCount { get; private set; }

            public TextAnalyzer(string filePath) : base(filePath)
            {
                _words = [];
                try
                {
                    fileContent = ReadToEnd();
                    AnalyzeText(fileContent);
                } 
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"Soubor jménem {filePath} neexistuje.");
                    //throw;
                } 
                catch (Exception)
                {
                    Console.WriteLine("File error!");
                    //throw;
                }
            }

            private void AnalyzeText(string text)
            {
                CharactersCount = text.Length;
                CharactersNoSpacesCount = text.Count(c => !char.IsWhiteSpace(c)); // Pokud jen 4 zadane whitespace: text.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "").Length;

                string[] words = text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                WordCount = words.Length;

                foreach (string word in words)
                {
                    _words[word] = _words.GetValueOrDefault(word, 0) + 1;
                }
            }

            public string GetWordFreq()
            {
                List<string> result = [];

                foreach (var kvp in _words)
                {
                    result.Add($"{kvp.Key}: {kvp.Value}");
                }

                return string.Join('\n', result);
            }

            public string GetSingleSpaceText()
            {
                List<string> result = [];

                foreach (string line in fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                {
                    string singleSpaceLine = string.Join(' ', line.Split(new[] { ' ', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
                    result.Add(singleSpaceLine);
                }

                return string.Join('\n', result);
            }

        }
    }
}
