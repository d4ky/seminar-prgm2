using System.Text;

namespace Prelevanivody
{
    internal class Program
    {
        private enum Operation { FILL1, FILL2, EMPTY1, EMPTY2, POUR12, POUR21, END};

        static void Main()
        {
            try
            {
                int max1 = int.Parse(Console.ReadLine());
                int max2 = int.Parse(Console.ReadLine());
                int target1 = int.Parse(Console.ReadLine());
                int target2 = int.Parse(Console.ReadLine());

                var result = GetShortestPath(max1, max2, target1, target2);

                DisplayResult(result);
            }
            catch
            {
                Console.WriteLine("\nMusis zadat pouze cisla :[");
            }
        }

        private static List<(int, int, Operation)> GetShortestPath(int max1, int max2, int target1, int target2)
        {
            var queue = new Queue<(int a, int b, List<(int, int, Operation)> path)>();
            var visited = new HashSet<(int, int)>();

            queue.Enqueue((0, 0, new List<(int, int, Operation)>())); 

            while (queue.Count > 0)
            {
                var (a, b, path) = queue.Dequeue();

                if (a == target1 && b == target2)
                {
                    path.Add((a, b, Operation.END));
                    return path;
                }

                if (visited.Contains((a, b)))
                {
                    continue;
                }

                visited.Add((a, b));

                var states = new List<(int a, int b, Operation op)>
                {
                    (a, 0, Operation.EMPTY2),
                    (0, b, Operation.EMPTY1),
                    (a, max2, Operation.FILL2),
                    (max1, b, Operation.FILL1),
                    (a + Math.Min(b, max1 - a), b - Math.Min(b, max1 - a), Operation.POUR21), // a naplnim bud o b nebo do max, b odectu bud cele nebo jen cast ktera se vesla a
                    (a - Math.Min(a, max2 - b), b + Math.Min(a, max2 - b), Operation.POUR12) 
                };

                foreach (var state in states)
                {
                    if (!visited.Contains((state.a, state.b)))
                    {
                        var newPath = new List<(int, int, Operation)>(path) { (a, b, state.op) };
                        queue.Enqueue((state.a, state.b, newPath));
                    }
                }
            }

            return [];
        }

        private static void DisplayResult(List<(int, int, Operation)> result)
        {
            int n = result.Count;

            if (n == 0)
            {
                Console.WriteLine("\nNelze");
                return;
            }

            var sb = new StringBuilder();
            var operationToString = new Dictionary<Operation, string>
            {
                { Operation.FILL1, "Naplnena 1. nadoba" },
                { Operation.FILL2, "Naplnena 2. nadoba" },
                { Operation.EMPTY1, "Vyprazdnena 1. nadoba" },
                { Operation.EMPTY2, "Vyprazdnena 2. nadoba" },
                { Operation.POUR12, "Prelit obsah 1. nadoby do 2. nadoby" },
                { Operation.POUR21, "Prelit obsah 2. nadoby do 1. nadoby" }
            };

            sb.AppendLine($"\nLze\n\nPocet kroku: {n - 1}\n\nKroky:\n[0; 0] Pocatecni stav");

            for (int i = 1; i < n; ++i)
            {
                sb.AppendLine($"[{result[i].Item1}; {result[i].Item2}] {operationToString[result[i - 1].Item3]}");
            }

            sb.Length -= 1; // pryc newline
            Console.WriteLine(sb.ToString());
        }
    }
}
