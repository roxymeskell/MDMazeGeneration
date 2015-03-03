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
        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
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
