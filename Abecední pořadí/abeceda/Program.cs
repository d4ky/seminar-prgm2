using System.Text;

namespace abeceda
{
    internal class Program
    {
        /* BONUS:
         * 1. Ne nemusí být vždy jednoznačné.
         * 2. Příklad: "xyz xz", jediné, co víme jistě je, že y -> z,
         * takže mohou být dvě možné řešení: x -> y -> z; y -> x -> z
         */
        static void Main()
        {
            String[] lexiOrder = Console.ReadLine().Split(' ');
            Dictionary<char, int> inDegreeCount = new Dictionary<char, int>();
            Dictionary<char, HashSet<char>> dependencies = new Dictionary<char, HashSet<char>>();

            foreach (string word in lexiOrder)
            {
                foreach (char ch in word)
                {
                    if (!inDegreeCount.ContainsKey(ch))
                        inDegreeCount[ch] = 0;
                }
            }

            for (int i = 0; i < lexiOrder.Length - 1; i++)
            {
                string firstWord = lexiOrder[i];
                string secondWord = lexiOrder[i + 1];
                bool foundDiff = false;

                for (int j = 0; j < Math.Min(firstWord.Length, secondWord.Length); j++)
                {
                    if (firstWord[j] != secondWord[j])
                    {
                        if (!dependencies.TryGetValue(firstWord[j], out var dependantChars))
                        {
                            dependantChars = new HashSet<char>();
                            dependencies[firstWord[j]] = dependantChars;
                        }
                        if (dependantChars.Add(secondWord[j]))
                        {
                            inDegreeCount[secondWord[j]]++;
                        }
                        foundDiff = true;
                        break;
                    }
                }

                if (!foundDiff && firstWord.Length > secondWord.Length)
                {
                    Console.WriteLine("Abecední pořadí neexistuje.");
                    return;
                }
            }

            Queue<char> queue = new Queue<char>();
            foreach (var kvp in inDegreeCount)
            {
                if (kvp.Value == 0)
                    queue.Enqueue(kvp.Key);
            }

            StringBuilder alphabet = new StringBuilder();
            while (queue.Count > 0)
            {
                char curr = queue.Dequeue();
                alphabet.Append(curr);

                if (dependencies.TryGetValue(curr, out var dependants))
                {
                    foreach (char ch in dependants)
                    {
                        if (--inDegreeCount[ch] == 0)
                            queue.Enqueue(ch);
                    }
                }
            }

            if (alphabet.Length == inDegreeCount.Count)
            {
                Console.WriteLine(string.Join(" -> ", alphabet.ToString().ToCharArray()));
            }
            else
            {
                Console.WriteLine("Abecedni poradi neexistuje.");
            }
        }
    }
}
