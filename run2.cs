using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// Чуть-чуть считерил и взял реализацию из нового .Net 
public class PriorityQueue<TElement, TPriority>
{
    private readonly IComparer<TPriority>? _comparer;
    private (TElement Element, TPriority Priority)[] _nodes;
    private int _version;
    private int _size;
    private const int Log2Arity = 2;
    private const int Arity = 4;

    public PriorityQueue()
    {
        _nodes = Array.Empty<(TElement, TPriority)>();
        _comparer = InitializeComparer(null);
    }

    private static IComparer<TPriority>? InitializeComparer(IComparer<TPriority>? comparer)
    {
        if (typeof(TPriority).IsValueType)
        {
            if (comparer == Comparer<TPriority>.Default)
                return null;

            return comparer;
        }
        else
            return comparer ?? Comparer<TPriority>.Default;
    }

    public int Count => _size;

    public void Enqueue(TElement element, TPriority priority)
    {
        int currentSize = _size;
        _version++;

        if (_nodes.Length == currentSize)
        {
            Grow(currentSize + 1);
        }

        _size = currentSize + 1;

        MoveUpDefaultComparer((element, priority), currentSize);
    }

    public bool TryDequeue(out TElement element, out TPriority priority)
    {
        if (_size != 0)
        {
            (element, priority) = _nodes[0];
            RemoveRootNode();
            return true;
        }

        element = default;
        priority = default;
        return false;
    }

    private void RemoveRootNode()
    {
        int lastNodeIndex = --_size;
        _version++;

        if (lastNodeIndex > 0)
        {
            (TElement Element, TPriority Priority) lastNode = _nodes[lastNodeIndex];

            MoveDownDefaultComparer(lastNode, 0);
        }
    }

    private void Grow(int minCapacity)
    {
        const int GrowFactor = 2;
        const int MinimumGrow = 4;

        int newcapacity = GrowFactor * _nodes.Length;

        if ((uint)newcapacity > Array.MaxLength)
            newcapacity = Array.MaxLength;

        newcapacity = Math.Max(newcapacity, _nodes.Length + MinimumGrow);

        if (newcapacity < minCapacity)
            newcapacity = minCapacity;

        Array.Resize(ref _nodes, newcapacity);
    }

    private void MoveUpDefaultComparer((TElement Element, TPriority Priority) node, int nodeIndex)
    {
        (TElement Element, TPriority Priority)[] nodes = _nodes;

        while (nodeIndex > 0)
        {
            int parentIndex = GetParentIndex(nodeIndex);
            (TElement Element, TPriority Priority) parent = nodes[parentIndex];

            if (Comparer<TPriority>.Default.Compare(node.Priority, parent.Priority) < 0)
            {
                nodes[nodeIndex] = parent;
                nodeIndex = parentIndex;
            }
            else
                break;
        }

        nodes[nodeIndex] = node;
    }

    private void MoveDownDefaultComparer((TElement Element, TPriority Priority) node, int nodeIndex)
    {
        (TElement Element, TPriority Priority)[] nodes = _nodes;
        int size = _size;

        int i;
        while ((i = GetFirstChildIndex(nodeIndex)) < size)
        {
            (TElement Element, TPriority Priority) minChild = nodes[i];
            int minChildIndex = i;

            int childIndexUpperBound = Math.Min(i + Arity, size);
            while (++i < childIndexUpperBound)
            {
                (TElement Element, TPriority Priority) nextChild = nodes[i];
                if (Comparer<TPriority>.Default.Compare(nextChild.Priority, minChild.Priority) < 0)
                {
                    minChild = nextChild;
                    minChildIndex = i;
                }
            }

            if (Comparer<TPriority>.Default.Compare(node.Priority, minChild.Priority) <= 0)
                break;

            nodes[nodeIndex] = minChild;
            nodeIndex = minChildIndex;
        }

        nodes[nodeIndex] = node;
    }

    private static int GetParentIndex(int index) => (index - 1) >> Log2Arity;
    private static int GetFirstChildIndex(int index) => (index << Log2Arity) + 1;
}

class Program
{
    // Константы для символов ключей и дверей
    static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();

    private static readonly (int dx, int dy)[] directions = new (int dx, int dy)[] { (0, 1), (0, -1), (1, 0), (-1, 0) };

    private static Dictionary<(int x, int y, int keysMask), List<(char key, int steps, int x, int y)>> reachCache
        = new Dictionary<(int, int, int), List<(char, int, int, int)>>();

    // Метод для чтения входных данных
    static List<List<char>> GetInput()
    {
        var data = new List<List<char>>();
        string line;
        while ((line = Console.ReadLine()) != null && line != "")
        {
            data.Add(line.ToCharArray().ToList());
        }

        return data;
    }

    public struct State : IEquatable<State>
    {
        public (int x, int y)[] Robots;
        public int KeysMask;

        public bool Equals(State other)
            => KeysMask == other.KeysMask
               && Robots.SequenceEqual(other.Robots);

        public override bool Equals(object obj)
            => obj is State other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = KeysMask;
                foreach (var (x, y) in Robots)
                    hash = (hash * 397) ^ (x * 31 + y);
                return hash;
            }
        }
    }

    private static int Solve(List<List<char>> data)
    {
        reachCache.Clear();
        var height = data.Count;
        var width = data[0].Count;

        var startPositions = new List<(int x, int y)>();
        var allKeysMask = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var ch = data[y][x];

                if (ch == '@')
                    startPositions.Add((x, y));
                else if (ch >= 'a' && ch <= 'z')
                    allKeysMask |= 1 << (ch - 'a');
            }
        }

        var distanceMap = new Dictionary<State, int>();
        var pq = new PriorityQueue<State, int>();

        var initState = new State { Robots = startPositions.ToArray(), KeysMask = 0 };

        distanceMap[initState] = 0;
        pq.Enqueue(initState, 0);

        while (pq.Count > 0)
        {
            pq.TryDequeue(out var state, out int dist);

            if (dist != distanceMap[state])
                continue;

            if (state.KeysMask == allKeysMask)
                return dist;

            for (int i = 0; i < state.Robots.Length; i++)
            {
                var pos = state.Robots[i];
                var reachable = FindReachableKeys(pos, state.KeysMask, data);
                foreach (var (key, steps, kx, ky) in reachable)
                {
                    var newMask = state.KeysMask | (1 << (key - 'a'));
                    var nextRobots = state.Robots.ToArray();

                    nextRobots[i] = (kx, ky);

                    var nextState = new State { Robots = nextRobots, KeysMask = newMask };

                    var newDist = dist + steps;

                    if (!distanceMap.TryGetValue(nextState, out var oldDist) || newDist < oldDist)
                    {
                        distanceMap[nextState] = newDist;
                        pq.Enqueue(nextState, newDist);
                    }
                }
            }
        }

        return -1;
    }

    private static List<(char key, int steps, int x, int y)> FindReachableKeys((int x, int y) start,
        int keysMask, List<List<char>> data)
    {
        var cacheKey = (start.x, start.y, keysMask);
        if (reachCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var height = data.Count;
        var width = data[0].Count;
        var visited = new bool[height, width];
        var queue = new Queue<(int x, int y, int dist)>();

        visited[start.y, start.x] = true;
        queue.Enqueue((start.x, start.y, 0));

        var result = new List<(char, int, int, int)>();

        while (queue.Count > 0)
        {
            var (cx, cy, cdist) = queue.Dequeue();

            foreach (var (dx, dy) in directions)
            {
                var nx = cx + dx;
                var ny = cy + dy;

                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;

                if (visited[ny, nx])
                    continue;

                var cell = data[ny][nx];

                if (cell == '#')
                    continue;

                if (cell >= 'A' && cell <= 'Z')
                {
                    var bit = 1 << (char.ToLower(cell) - 'a');

                    if ((keysMask & bit) == 0)
                        continue;
                }

                visited[ny, nx] = true;
                var newDist = cdist + 1;

                if (cell >= 'a' && cell <= 'z' && (keysMask & (1 << (cell - 'a'))) == 0)
                    result.Add((cell, newDist, nx, ny));

                queue.Enqueue((nx, ny, newDist));
            }
        }

        reachCache[cacheKey] = result;
        return result;
    }

    static void Main()
    {
        var data = GetInput();

        int result = Solve(data);
        if (result == -1)
        {
            Console.WriteLine("No solution found");
        }
        else
        {
            Console.WriteLine(result);
        }
    }
}
