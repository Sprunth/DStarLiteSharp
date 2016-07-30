using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DStarLiteSharp;

namespace DStarLiteSharpSample
{
    class Program
    {
        private static readonly int[,] maze = new int[,]
        {
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 0, 1, 0, 0, 0, 1, 0, 0, 1},
            {1, 0, 0, 0, 1, 0, 1, 1, 0, 1},
            {1, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            {1, 0, 1, 1, 1, 1, 0, 1, 1, 1},
            {1, 0, 0, 0, 1, 0, 0, 0, 0, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        };

        private const int mazeWidth = 10;
        private const int mazeHeight = 7;

        public static void Main(string[] args)
        {
            int startX = 1, startY = 1;
            int endX = 8, endY = 5;

            var pathfinder = new DStarLite();
            pathfinder.init(startX, startY, endX, endY);
            for (var row = 0; row < mazeHeight; row++)
            {
                for (var col = 0; col < mazeWidth; col++)
                {
                    var mazeVal = maze[row, col];
                    if (mazeVal == 1)
                        pathfinder.updateCell(col, row, -1);
                }
            }
            Console.WriteLine($"Start node: ({startX}, {startY})");
            Console.WriteLine($"End node: ({endX}, {endY})");
            // Time the replanning
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            pathfinder.replan();
            stopwatch.Stop();
            var replanTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Time: {replanTime} ms");
            var path = pathfinder.getPath();
            foreach (var i in path)
            {
                Console.WriteLine($"x: {i.x} y: {i.y}");
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
                    else if ((col == endX) && (row == endY))
                    {
                        Console.Write('E');
                        continue;
                    }
                    bool written = false;
                    path.ForEach(state =>
                    {
                        if (col == state.x && row == state.y)
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