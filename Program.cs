using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace MDMazeGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            /*Console.BufferWidth = 80;
            Console.BufferHeight = 50;
            Console.WindowWidth = 80;
            Console.WindowHeight = 50;
            int interiorScale = 7;
            int boundScale = 1;
            int openingScale = 3;

            int[] dInfo;

            dInfo = new int[16];
            for (int _d = 0; _d < 16; _d++)
                dInfo[_d] = 2;

            Maze.Initialize(dInfo);

            /*int maxCells = 1000;
            int currentCells = 1;
            int minSize = 2;
            int cellsLeft, maxSize;
            int minD = 3;
            int maxD = (int)Math.Floor(Math.Log(maxCells, minSize));
            maxD = maxD > 16 ? 16 : (maxD < 3 ? 3 : maxD);
            dInfo = new int[Randomize.RandInt(minD, maxD)];
            maxSize = (int)Math.Ceiling(Math.Pow(maxCells, (double)1 / dInfo.Length));
            cellsLeft = (maxCells / currentCells);
            for (int _d = 0; _d < dInfo.Length; _d++)
            {

                dInfo[_d] = Randomize.RandInt(cellsLeft < minSize ? cellsLeft : minSize, cellsLeft < maxSize ? cellsLeft : maxSize);
                currentCells *= dInfo[_d];
                cellsLeft = (int)Math.Floor((double)maxCells / currentCells);
            }
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Maze.Initialize(dInfo);
            stopwatch.Stop();

            Console.Write("Dimension Information:");
            for (int _d = 0; _d < dInfo.Length; _d++)
                Console.Write("\t{0}: {1}", _d, dInfo[_d]);
            Console.WriteLine();
            Console.WriteLine("Cell count: {0}", currentCells);
            Console.WriteLine("Maze initialization - {0} milliseconds\nGeneration Speed: {1} cells per millisecond", stopwatch.ElapsedMilliseconds, (double)currentCells / stopwatch.ElapsedMilliseconds);
            Console.ReadLine();

            World.Initialize(interiorScale, boundScale, openingScale);

            ConsoleKey input;

            string s = "ooo";
            int lines = (s.Split(new char[] { '\n' })).Length;*/


            Console.Title = "Multidimensional Maze Generation";
            do
            {
                Console.Clear();
                Start();
            } while (Reset());
        }

        /// <summary>
        /// Method to get values and initialize Maze
        /// </summary>
        static void Start()
        {
            //Use initializer to setup maze
            Initializer.SetupMaze();

            //Clear console
            Console.Clear();

            //Run
            Runner.Run();
        }
        
        /// <summary>
        /// Method to check if player wants to reset maze once finishing it
        /// </summary>
        /// <returns>True of false whether or not to reset maze</returns>
        static bool Reset()
        {
            char input;
            do
            {
                Console.Clear();
                Console.WriteLine("Would you like to play again (Y/N)?");
                input = Console.ReadKey().KeyChar;
            } while (input != 'y' && input != 'n');

            return (input == 'y');
        }
    }
}
