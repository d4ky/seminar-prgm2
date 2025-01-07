using System.Text;

namespace zarovnani_do_blok
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "LoremIpsum.txt";
            try
            {
                int lineWidth = int.Parse(Console.ReadLine());
                if (lineWidth < 0) throw new ArgumentException();

                using StreamReader sr = new StreamReader(filePath);
                List<List<string>> paragraphs = new List<List<string>> { new List<string>() };

                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (words.Length > 0)
                    {
                        paragraphs[^1].AddRange(words);
                        continue;
                    }

                    if (paragraphs[^1].Count > 0)
                    {
                        paragraphs.Add(new List<string>());
                    }
                }

                StringBuilder sb = new StringBuilder();

                foreach (var paragraph in paragraphs)
                {
                    int currLength = 0;
                    int n = paragraph.Count;
                    List<string> currLine = new List<string>();

                    for (int i = 0; i < n; ++i)
                    {
                        if (currLength + paragraph[i].Length <= lineWidth)
                        {
                            currLine.Add(paragraph[i]);
                            currLine.Add(" ");
                            currLength += paragraph[i].Length + 1;
                        }
                        else if (currLine.Count == 0)
                        {
                            sb.Append(paragraph[i]);
                            sb.Append('\n');
                            currLine.Clear();
                            currLength = 0;
                        }
                        else
                        {
                            int wordCount = currLine.Count;
                            currLine.RemoveAt(wordCount - 1);
                            currLength--;
                            wordCount--;

                            int numOfCharsToAdd = lineWidth - currLength;
                            int spaceCount = (wordCount - 1) / 2;
                            if (spaceCount == 0) spaceCount = 1;
                            int extraSpaceLength = numOfCharsToAdd / spaceCount;
                            int overflowChars = numOfCharsToAdd % spaceCount;

                            for (int j = 1; j < wordCount; j += 2)
                            {
                                currLine[j] += new string(' ', extraSpaceLength); // toto je neefektivni, mohl jsem radsi storovat tuple (string, int) kde int by urcil velikost mezery a nemusel bych takto zvetsovat string 
                            }

                            for (int j = 0; j < overflowChars; ++j)
                            {
                                currLine[2 * j + 1] += ' ';
                            }

                            foreach (string word in currLine)
                            {
                                sb.Append(word);
                            }
                            sb.Append('\n');
                            currLine.Clear();
                            currLength = 0;

                            --i;
                        }
                    }

                    if (currLine.Count > 0)
                    {
                        sb.Append(string.Join(" ", currLine.Where(word => !string.IsNullOrWhiteSpace(word))).TrimEnd());
                        sb.Append("\n");
                    }
                    sb.Append("\n");
                }
                
                while (sb[^1] == '\n')
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                sb.Append('\n');
                try
                {
                    using StreamWriter sw = new StreamWriter("output.txt");
                    sw.Write(sb.ToString());
                }
                catch
                {
                    Console.WriteLine("Output file error");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Argument Exception");
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Argument Exception");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Soubor jménem {filePath} neexistuje.");
            }
            catch (Exception)
            {
                Console.WriteLine("Input File Error");
            }
        }
    }
}
