using System;
using System.Collections.Generic;
using System.Linq;


class Program
{
    // Константы для символов ключей и дверей
    static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();
    private static readonly (int dx, int dy)[] directions = new (int, int)[]
    {
        (0,  1), (0, -1), (1,  0), (-1,  0)  
    };
    private static readonly HashSet<char> takenKeys = new HashSet<char>();
    private static HashSet<char> keysOnMap = new HashSet<char>();
    private static readonly HashSet<char> keysCharHashSet = keys_char.ToHashSet();
    
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


    static int Solve(List<List<char>> data)
    {
        var steps = 0;
        var robotsPosition = new List<(int x, int y)>();
        
        for (var i = 0; i < data.Count; i++)
        {
            for (var j = 0; j < data[i].Count; j++)
            {
                if (data[i][j].Equals('@'))
                    robotsPosition.Add((j, i));

                if (keysCharHashSet.Contains(data[i][j]))
                    keysOnMap.Add(data[i][j]);
            }
        }

        while (takenKeys.Count != keysOnMap.Count)
        {
            var minSteps = int.MaxValue;
            var robotIndex = 0;
            var nextRobotsPosition = (0, 0);
            var takenKey = '\0';
                
            for (var i = 0; i < robotsPosition.Count; i++)
            {
                var nextPosition = BFS(robotsPosition[i], data);
                if (nextPosition != null && nextPosition.Value.steps < minSteps)
                {
                    minSteps = nextPosition.Value.steps;
                    robotIndex = i;
                    nextRobotsPosition = (nextPosition.Value.x, nextPosition.Value.y);
                    takenKey = nextPosition.Value.takenKey;
                }
            }

            if (minSteps != int.MaxValue)
            {
                robotsPosition[robotIndex] = nextRobotsPosition;
                takenKeys.Add(takenKey);
                steps+=minSteps;
            }
            else
                return -1;
        }
        
        return steps;
    }

    private static (int x, int y, int steps, char takenKey)? BFS((int xStart, int yStart) start, List<List<char>> data)
    {
        var visited = new HashSet<(int x, int y)>();
        var queue = new Queue<(int x, int y, int dist)>();
        
        visited.Add((start.xStart, start.yStart));
        queue.Enqueue((start.xStart, start.yStart, 0));

        while (queue.Count > 0)
        {
            var pair = queue.Dequeue();
            // Console.WriteLine($"Visited: {u}");

            foreach (var (dx, dy) in directions)
            {
                var newState = (x: pair.x + dx, y: pair.y + dy, dist: pair.dist + 1);
                
                if (newState.x < 0 || newState.y < 0 || newState.y >= data.Count || newState.x >= data[0].Count)
                    continue;
                
                if (keysCharHashSet.Contains(data[newState.y][newState.x]) && !takenKeys.Contains(data[newState.y][newState.x]))
                {
                    return (newState.x, newState.y, newState.dist, data[newState.y][newState.x]);
                }
                
                if (!visited.Contains((newState.x, newState.y)) && (data[newState.y][newState.x] == '.' ||
                                                                    takenKeys.Contains(char.ToLower(data[newState.y][newState.x]))))
                {
                    visited.Add((newState.x, newState.y));
                    queue.Enqueue(newState);
                }
            }
        }

        return null;
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
