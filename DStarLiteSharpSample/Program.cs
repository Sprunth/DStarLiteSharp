using System;
using System.Diagnostics;
using DStarLiteSharp;

namespace DStarLiteSharpSample
{
    internal static class Program
    {
        private const int mazeWidth = 10;
        private const int mazeHeight = 7;

        private static readonly int[,] maze =
        {
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 0, 1, 0, 0, 0, 1, 0, 0, 1},
            {1, 0, 0, 0, 1, 0, 1, 1, 0, 1},
            {1, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            {1, 0, 1, 1, 1, 1, 0, 1, 1, 1},
            {1, 0, 0, 0, 1, 0, 0, 0, 0, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        };

        public static void Main(string[] args)
        {
            int startX = 1, startY = 1;
            int endX = 8, endY = 5;

            var pathfinder = new DStarLite(1000, false);
            pathfinder.Init(startX, startY, endX, endY);
            for (var row = 0; row < mazeHeight; row++)
            {
                for (var col = 0; col < mazeWidth; col++)
                {
                    var mazeVal = maze[row, col];
                    if (mazeVal == 1)
                        pathfinder.UpdateCell(col, row, -1);
                }
            }
            Console.WriteLine($"Start node: ({startX}, {startY})");
            Console.WriteLine($"End node: ({endX}, {endY})");
            // Time the replanning
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            pathfinder.Replan();
            stopwatch.Stop();
            var replanTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Time: {replanTime} ms");
            var path = pathfinder.GetPath();
            foreach (var i in path)
            {
                Console.WriteLine($"x: {i.X} y: {i.Y}");
            }

            Console.WriteLine("S=Start E=End *=Path");
            for (var row = 0; row < mazeHeight; row++)
            {
                for (var col = 0; col < mazeWidth; col++)
                {
                    if (col == startX && row == startY)
                    {
                        Console.Write('S');
                        continue;
                    }
                    if ((col == endX) && (row == endY))
                    {
                        Console.Write('E');
                        continue;
                    }
                    var written = false;
                    path.ForEach(state =>
                    {
                        if (col == state.X && row == state.Y)
                        {
                            Console.Write('*');
                            written = true;
                        }
                    });
                    if (written)
                        continue;

                    Console.Write(maze[row, col] == 1 ? 'X' : ' ');
                }
                Console.WriteLine();
            }

            Console.ReadLine();
        }
    }
}