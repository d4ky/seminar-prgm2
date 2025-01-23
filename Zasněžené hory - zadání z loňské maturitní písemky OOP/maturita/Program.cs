namespace maturita
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                var inputProcessor = new InputProcessor("2.txt");
                var (tilemap, moves) = inputProcessor.ProcessInput();

                var pathFinder = new PathFinder(tilemap, moves);
                var result = pathFinder.FindShortestPath();

                new OutputWriter("vystup.txt").WriteResult(result);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }

    public class Tilemap
    {
        public const int UNREACHABLE = int.MaxValue;
        public int Size { get; }
        private readonly int[,] _tiles;

        public Tilemap(int size, int[,] tiles)
        {
            Size = size;
            _tiles = tiles;
        }

        public int GetTileTime(int x, int y) => _tiles[x, y];
    }

    public class MoveSet
    {
        public IReadOnlyList<(int dx, int dy)> Moves { get; } 

        public MoveSet(IEnumerable<(int dx, int dy)> moves)
        {
            Moves = moves.ToArray();
        }
    }

    public class PathResult
    {
        public int Time { get; }
        public IReadOnlyList<(int x, int y)> Path { get; }
        public bool IsValid => Time != -1;

        public PathResult(int time, List<(int x, int y)> path)
        {
            Time = time;
            Path = path?.AsReadOnly() ?? new List<(int x, int y)>().AsReadOnly();
        }
    }

    public class InputProcessor
    {
        private readonly string _inputFile;

        public InputProcessor(string inputFile)
        {
            _inputFile = inputFile;
        }

        public (Tilemap tilemap, MoveSet moves) ProcessInput()
        {
            using var sr = new StreamReader(_inputFile); 

            int n = int.Parse(sr.ReadLine());
            int m = int.Parse(sr.ReadLine());

            var tilemap = CreateTilemap(n, m, sr);
            var moves = ReadMoves(sr);

            return (tilemap, moves);
        }

        private Tilemap CreateTilemap(int n, int m, StreamReader sr)
        {
            int[,] tiles = new int[n, n];
            InitializeTiles(tiles, n);
            ReadTileData(tiles, m, sr);
            return new Tilemap(n, tiles);
        }

        private void InitializeTiles(int[,] tiles, int n)
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    tiles[i, j] = Tilemap.UNREACHABLE;
        }

        private void ReadTileData(int[,] tiles, int m, StreamReader sr)
        {
            for (int i = 0; i < m; i++)
            {
                var data = Array.ConvertAll(sr.ReadLine().Split(' '), int.Parse);
                tiles[data[0], data[1]] = data[2];
            }
        }

        private MoveSet ReadMoves(StreamReader sr)
        {
            int k = int.Parse(sr.ReadLine());
            var moves = new List<(int, int)>();

            for (int i = 0; i < k; i++)
            {
                var move = Array.ConvertAll(sr.ReadLine().Split(' '), int.Parse);
                moves.Add((move[0], move[1]));
            }

            return new MoveSet(moves);
        }
    }

    public class PathFinder
    {
        private readonly Tilemap _tilemap;
        private readonly MoveSet _moves;
        private readonly MovementValidator _validator;

        public PathFinder(Tilemap tilemap, MoveSet moves)
        {
            _tilemap = tilemap;
            _moves = moves;
            _validator = new MovementValidator(tilemap);
        }

        public PathResult FindShortestPath()
        {
            var queue = new Queue<SearchState>();
            var visited = new bool[_tilemap.Size, _tilemap.Size];

            InitializeStart(queue, visited);

            return BFS(queue, visited);
        }

        private void InitializeStart(Queue<SearchState> queue, bool[,] visited)
        {
            queue.Enqueue(new SearchState(0, 0, 0, new List<(int, int)> { (0, 0) }));
            visited[0, 0] = true;
        }

        private PathResult BFS(Queue<SearchState> queue, bool[,] visited)
        {
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (IsDestinationReached(current))
                    return new PathResult(current.Time, current.Path);

                ProcessPossibleMoves(current, queue, visited);
            }

            return new PathResult(-1, null);
        }

        private bool IsDestinationReached(SearchState state) =>
            state.X == _tilemap.Size - 1 &&
            state.Y == _tilemap.Size - 1;

        private void ProcessPossibleMoves(SearchState current, Queue<SearchState> queue, bool[,] visited)
        {
            foreach (var (dx, dy) in _moves.Moves)
            {
                var newX = current.X + dx;
                var newY = current.Y + dy;

                if (_validator.CanMove(current.X, current.Y, dx, dy, current.Time) && !visited[newX, newY])
                {
                    visited[newX, newY] = true;
                    queue.Enqueue(current.CreateNextState(newX, newY));
                }
            }
        }
    }

    public class SearchState
    {
        public int X { get; }
        public int Y { get; }
        public int Time { get; }
        public List<(int x, int y)> Path { get; }

        public SearchState(int x, int y, int time, List<(int, int)> path)
        {
            X = x;
            Y = y;
            Time = time;
            Path = new List<(int, int)>(path);
        }

        public SearchState CreateNextState(int newX, int newY)
        {
            var newPath = new List<(int, int)>(Path) { (newX, newY) };
            return new SearchState(newX, newY, Time + 1, newPath);
        }
    }

    public class MovementValidator
    {
        private readonly Tilemap _tilemap;

        public MovementValidator(Tilemap tilemap)
        {
            _tilemap = tilemap;
        }

        public bool CanMove(int x, int y, int dx, int dy, int time)
        {
            var (newX, newY) = (x + dx, y + dy);
            dx = dx >= 1 ? 1 : 0;
            dy = dy >= 1 ? 1 : 0;
            x += dx;
            y += dy;

            if (!IsWithinBounds(newX, newY)) return false;

            while (true)
            {
                if (x == newX && y == newY)
                    return time < _tilemap.GetTileTime(x, y);

                if (time >= _tilemap.GetTileTime(x, y))
                    return false;

                x += dx;
                y += dy;
            }
        }

        private bool IsWithinBounds(int x, int y) =>
            x >= 0 && y >= 0 &&
            x < _tilemap.Size &&
            y < _tilemap.Size;
    }

    public class OutputWriter
    {
        private readonly string _outputFile;

        public OutputWriter(string outputFile)
        {
            _outputFile = outputFile;
        }

        public void WriteResult(PathResult result)
        {
            using var sw = new StreamWriter(_outputFile);
            sw.WriteLine(result.Time);

            if (result.IsValid)
            {
                sw.WriteLine(string.Join(' ', result.Path.Select(e => $"[{e.x},{e.y}]")));
            }
        }
    }
}

