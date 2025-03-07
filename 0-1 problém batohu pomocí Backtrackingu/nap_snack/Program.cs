namespace nap_snack
{
    internal class Program
    {
        static void Main()
        {
            int[] weights = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);
            int[] values = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);

            int capacity = int.Parse(Console.ReadLine());

            List<Item> items = [];
            for (int i = 0; i < weights.Length; i++)
            {
                items.Add(new Item(i, weights[i], values[i]));
            };
            
            var knapsack = new Knapsack(capacity, [.. items]);
            (int maxValue, List<int> indicies) = knapsack.Solve();
            
            indicies.Sort();

            Console.WriteLine(maxValue);
            Console.WriteLine(string.Join(" ", indicies.Select(x => x+1))); // INDEXOVAT OD 1 JE KRIMINÁLNÍ
        }

        public class Knapsack
        {
            private readonly Item[] _items;
            public int Capacity { get; }

            public Knapsack(int capacity, Item[] items)
            {
                Capacity = capacity;
                Array.Sort(items, (a, b) => b.Ratio.CompareTo(a.Ratio));
                _items = items;

            }

            public (int MaxValue, List<int> IncludedIndices) Solve()
            {
                PriorityQueue queue = new();
                Node root = new() { UpperBound = CalculateUpperBound(0, 0, 0) };

                queue.Enqueue(root);

                int maxValue = 0;
                Node? bestNode = null;

                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();

                    if (node.UpperBound <= maxValue) continue;

                    if (node.Level == _items.Length)
                    {
                        if (node.Value > maxValue)
                        {
                            maxValue = node.Value;
                            bestNode = node;
                        }
                        continue;
                    }

                    var currentItem = _items[node.Level];

                    if (node.Weight + currentItem.Weight <= Capacity)
                    {
                        Node include = new(node)
                        {
                            Level = node.Level + 1,
                            Value = node.Value + currentItem.Value,
                            Weight = node.Weight + currentItem.Weight,
                            IsIncluded = true,
                            Parent = node,
                            UpperBound = CalculateUpperBound(node.Value + currentItem.Value, node.Weight + currentItem.Weight, node.Level + 1),
                        };

                        if (include.UpperBound > maxValue) queue.Enqueue(include);
                    }

                    Node exclude = new(node)
                    {
                        Level = node.Level + 1,
                        UpperBound = CalculateUpperBound(node.Value, node.Weight, node.Level + 1),
                        Parent = node,
                        IsIncluded = false,
                    };

                    if (exclude.UpperBound > maxValue) queue.Enqueue(exclude);
                }
                return (maxValue, bestNode?.GetIncludedIndices(_items) ?? []);
            }

            private double CalculateUpperBound(int currentValue, int currentWeight, int level)
            {
                if (currentWeight > Capacity) return 0;

                double bound = currentValue;
                int remainingCapacity = Capacity - currentWeight;

                for (int i = level; i < _items.Length && remainingCapacity > 0; i++)
                {
                    if (_items[i].Weight <= remainingCapacity)
                    {
                        bound += _items[i].Value;
                        remainingCapacity -= _items[i].Weight;
                    }
                    else
                    {
                        bound += _items[i].Ratio * remainingCapacity;
                        break;
                    }
                }
                return bound;
            }
        }



        public class Item(int index, int weight, int value)
        {
            public int Index { get; } = index;
            public int Weight { get; } = weight;
            public int Value { get; } = value;
            public double Ratio { get; } = (double) value / weight;
        }

        public class Node
        {
            public int Level { get; init; }
            public int Value { get; init; }
            public int Weight { get; init; }
            public double UpperBound { get; init; }
            public Node Parent { get; init; }
            public bool IsIncluded { get; init; }

            public Node() {}

            public Node(Node parent)
            {
                Level = parent.Level;
                Value = parent.Value;
                Weight = parent.Weight;
                UpperBound = parent.UpperBound;
                Parent = parent.Parent;
                IsIncluded = parent.IsIncluded;
            }

            public List<int> GetIncludedIndices(Item[] items)
            {
                var indices = new List<int>();
                Node current = this;
                while (current != null)
                {
                    if (current.IsIncluded && current.Level > 0)
                        indices.Add(items[current.Level - 1].Index);
                    current = current.Parent;
                }
                return indices;
            }
        }

        public class PriorityQueue
        {
            private readonly List<Node> _heap = [];

            public int Count => _heap.Count;

            public void Enqueue(Node node)
            {
                _heap.Add(node);

                int i = _heap.Count - 1;
                while (i > 0)
                {
                    int parent = (i - 1) / 2;
                    if (_heap[parent].UpperBound >= _heap[i].UpperBound) break;
                    Swap(parent, i);
                    i = parent;
                }
            }

            public Node Dequeue()
            {
                if (_heap.Count == 0)
                {
                    throw new InvalidOperationException("queue is empty :(");
                }

                Node result = _heap[0];
                _heap[0] = _heap[^1];
                _heap.RemoveAt(_heap.Count - 1);

                int i = 0;
                while (true)
                {
                    int largest = i;
                    int left = 2 * i + 1;
                    int right = 2 * i + 2;

                    if (left < _heap.Count && _heap[left].UpperBound > _heap[largest].UpperBound)
                    {
                        largest = left;
                    }
                    if (right < _heap.Count && _heap[right].UpperBound > _heap[largest].UpperBound)
                    {
                        largest = right;
                    }

                    if (largest == i) break;
                    Swap(i, largest);
                    i = largest;
                }
                return result;
            }

            private void Swap(int i, int j)
            {
                (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
            }
        }
        

    }
}
