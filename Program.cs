using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDMazeGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BufferWidth = 800;
            Console.BufferHeight = 400;
            Console.WindowWidth = 100;
            Console.WindowHeight = 35;
            int dSize = 2;
            bool loop = true;

            /*int[] dInfo = new int[] {Randomize.RandInt(1, 5),
                Randomize.RandInt(1, 5), 
                Randomize.RandInt(1, 5), 
                Randomize.RandInt(1, 5), 
                Randomize.RandInt(1, 5) };*/

            int[] dInfo = new int[] { dSize, dSize, dSize, dSize, dSize };

            Maze.Initialize(dInfo);
            World.Initialize(15, 1, 5);
            Maze.SetViewable2D();

            /*int[,] viewable = Maze.Get2DViewable(Maze.IntialDimensions, Maze.Entrance);*/

            while (loop)
            {
                Console.Clear();

                for (int y = 0; y < Maze.Viewable2D.GetLength(1); y++)
                {
                    for (int x = 0; x < Maze.Viewable2D.GetLength(0); x++)
                    {
                        Console.Write(Maze.Viewable2D[x, y]);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();

                for (int y = 0; y < Maze.Viewable2D.GetLength(1); y++)
                {
                    for (int x = 0; x < Maze.Viewable2D.GetLength(0); x++)
                    {
                        if (Maze.IsBound(Maze.Viewable2D[x, y]))
                            Console.Write("X");
                        else
                            Console.Write(" ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();


                World.PrintWorld();

                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Escape:
                        loop = false;
                        break;
                    case ConsoleKey.X:
                        World.DimensionX = World.DimensionX + 1;
                        break;
                    case ConsoleKey.Y:
                        World.DimensionY = World.DimensionY + 1;
                        break;
                    case ConsoleKey.Z:
                        World.DimensionZ = World.DimensionZ + 1;
                        break;
                }
            }
        }
    }
}
