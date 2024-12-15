namespace maturita
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                string inputFile = "1.txt";
                using (StreamReader sr = new(inputFile))
                {
                    int n = int.Parse(sr.ReadLine());
                    int m = int.Parse(sr.ReadLine());

                    int[,] tilemap = InitializeTilemap(n, m, sr);

                    (int dx, int dy)[] moves = ReadMoves(sr);
                  
                    var (path, time) = BreadthFirstSearch(tilemap, moves, n);

                    Console.WriteLine(time);
                    if (time != -1)
                    {
                        Console.WriteLine(string.Join(' ', path.Select(e => $"[{e.x},{e.y}]")));
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        private static (List<(int x, int y)>, int time) BreadthFirstSearch(int[,] tilemap, (int dx, int dy)[] moves, int n)
        {
            bool[,] visited = new bool[n, n];
            Queue<(int x, int y, int time, List<(int, int)> path)> queue = new();

            queue.Enqueue((0, 0, 0, new List<(int, int)>{ (0, 0) }));
            visited[0, 0] = true;

            while (queue.Count > 0)
            {
                var (x, y, time, path) = queue.Dequeue();

                if (x == n - 1 && y == n - 1) return (path, time);

                foreach (var (dx, dy) in moves)
                {
                    int newX = x + dx, newY = y + dy;

                    if (CanMove(x, y, dx, dy, tilemap, n, time) && !visited[newX, newY])
                    {
                        visited[newX, newY] = true;
                        var newPath = new List<(int, int)>(path) { (newX, newY) };
                        queue.Enqueue((newX, newY, time + 1, newPath));
                    }
                }
            }

            return (new List<(int, int)>(), -1);
        }

        private static bool CanMove(int x, int y, int dx, int dy, int[,] tilemap, int n, int time)
        {
            (int newX, int newY) = (x + dx, y + dy);
            dx = dx >= 1 ? 1 : 0;
            dy = dy >= 1 ? 1 : 0;
            x += dx; y += dy;
            if (newX < 0 || newY < 0 || newX >= n || newY >= n) return false;
            
            while (true)
            {
                if (x == newX && y == newY)
                    return time < tilemap[x, y];

                if (time >= tilemap[x, y])
                    return false;

                x += dx;
                y += dy;
            }
        }

        private const int UNREACHABLE = int.MaxValue;

        private static int[,] InitializeTilemap(int n, int m, StreamReader sr)
        {
            int[,] tilemap = new int[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    tilemap[i, j] = UNREACHABLE;

            for (int i = 0; i < m; i++)
            {
                int[] tileData = Array.ConvertAll(sr.ReadLine().Split(' '), int.Parse);
                tilemap[tileData[0], tileData[1]] = tileData[2];
            }

            return tilemap;
        }

        private static (int x, int y)[] ReadMoves(StreamReader sr)
        {
            int k = int.Parse(sr.ReadLine());
            (int x, int y)[] moves = new (int x, int y)[k];

            for (int i = 0; i < k; i++)
            {
                int[] move = Array.ConvertAll(sr.ReadLine().Split(' '), int.Parse);
                moves[i] = (move[0], move[1]);
            }

            return moves;
        }
    }
}
