using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    // Константы для символов ключей и дверей
    static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();

    private static readonly (int dx, int dy)[] directions = new (int dx, int dy)[] { (0, 1), (0, -1), (1, 0), (-1, 0) };

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

    struct State
    {
        public (int x, int y)[] Robots;
        public HashSet<char> CollectedKeys;
    }

    static int Solve(List<List<char>> data)
    {
        var height = data.Count;
        var width = data[0].Count;

        var startPositions = new List<(int x, int y)>();
        var allKeys = new HashSet<char>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var ch = data[y][x];

                if (ch == '@')
                    startPositions.Add((x, y));
                else if (ch >= 'a' && ch <= 'z')
                    allKeys.Add(ch);
            }
        }

        var distanceMap = new Dictionary<State, int>();
        var pq = new PriorityQueue<State, int>();

        var initState = new State { Robots = startPositions.ToArray(), CollectedKeys = new HashSet<char>() };

        distanceMap[initState] = 0;
        pq.Enqueue(initState, 0);

        var counter = 0;
        while (pq.Count > 0)
        {
            Console.WriteLine(counter++);
            pq.TryDequeue(out var state, out int dist);

            if (dist != distanceMap[state])
                continue;

            if (state.CollectedKeys.Count == allKeys.Count)
                return dist;

            for (int i = 0; i < state.Robots.Length; i++)
            {
                var pos = state.Robots[i];
                var reachable = FindReachableKeys(pos, state.CollectedKeys, data);
                foreach (var (key, steps, kx, ky) in reachable)
                {
                    var nextKeys = new HashSet<char>(state.CollectedKeys) { key };
                    var nextRobots = state.Robots.ToArray();

                    nextRobots[i] = (kx, ky);

                    var nextState = new State { Robots = nextRobots, CollectedKeys = nextKeys };

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
        HashSet<char> collectedKeys, List<List<char>> data)
    {
        var height = data.Count;
        var width = data[0].Count;
        var visited = new bool[height, width];
        var queue = new Queue<(int x, int y, int dist)>();

        visited[start.y, start.x] = true;
        queue.Enqueue((start.x, start.y, 0));

        var result = new List<(char, int, int, int)>();
        var minDistFound = int.MaxValue;

        while (queue.Count > 0)
        {
            var (cx, cy, cdist) = queue.Dequeue();

            if (cdist > minDistFound)
                break;

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
                    var neededKey = char.ToLower(cell);

                    if (!collectedKeys.Contains(neededKey))
                        continue;
                }

                visited[ny, nx] = true;
                var newDist = cdist + 1;

                if (cell >= 'a' && cell <= 'z' && !collectedKeys.Contains(cell))
                {
                    if (newDist <= minDistFound)
                    {
                        minDistFound = newDist;
                        result.Add((cell, newDist, nx, ny));
                    }

                    continue;
                }

                if (newDist < minDistFound)
                    queue.Enqueue((nx, ny, newDist));
            }
        }

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