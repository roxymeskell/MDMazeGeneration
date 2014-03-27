using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDMazeGeneration
{
    static class Runner
    {
        static readonly ConsoleColor WIN_FG = ConsoleColor.Yellow, WIN_BG = ConsoleColor.Red;

        static string cellInfo = "Current Cell:\n     {0}";
        static string currDimensionInfo = "Current Dimensions:\n     X: {0}     Y: {1}     Z: {2}";
        static string visableMap = "Map:{0}";
        static string controls = "Shift Dimensions:\n     [1][2][3]\n\nMovement:\n        [W]\n     [A][S][D]\n\nTraverse Staircases:\n     [Spacebar]";
        static string winMessage = "Player has completed maze!\nPress [enter] to continue.";

        public static string CellInfo
        {
            get
            {
                string _cell = String.Format("[{0}", Player.CurrentCell[0]);
                for (int _d = 1; _d < Maze.Dimensions - 1; _d++)
                    _cell += String.Format(", {0}", Player.CurrentCell[_d]);
                _cell += String.Format(", {0}]", Player.CurrentCell[Maze.Dimensions - 1]);

                return String.Format(cellInfo, _cell);
            }
        }
        public static string CurrDimensionInfo { get { return String.Format(currDimensionInfo, World.DimensionX, World.DimensionY, World.DimensionZ); } }
        public static string VisableMap
        {
            get
            {
                string _map = "";
                for (int _y = 0; _y < Maze.Viewable2D.GetLength(1); _y++)
                {
                    _map += "\n     ";
                    for (int _x = 0; _x < Maze.Viewable2D.GetLength(0); _x++)
                    {
                        switch (Maze.Viewable2D[_x, _y])
                        {
                            case 3:
                                _map += "#";
                                break;
                            case 2:
                                _map += "/";
                                break;
                            case 4:
                                _map += "0";
                                break;
                            case 6:
                                _map += "%";
                                break;
                            default:
                                _map += " ";
                                break;
                        }
                    }
                }

                return String.Format(visableMap, _map);
            }
        }
        public static string Controls { get { return controls; } }
<<<<<<< HEAD
        public static string Information { get { return CellInfo + "\n\n" + CurrDimensionInfo + "\n\n" + VisableMap + "\n\n" + Controls; } }
=======
        public static string Information { get { return CellInfo + "\n" + CurrDimensionInfo + "\n" + VisableMap + "\n" + Controls; } }
>>>>>>> 202df96dd0d1c31fc5e15bb9161b75da6f4cadf3
        public static int InfoWidth { get { return Information.Split(new char[] { '\n' }).Max(s => s.Length); } }
        public static int InfoHeight { get { return Information.Split(new char[] { '\n' }).Length; } }
        public static int InfoLeft { get { return World.WorldScale + 1; } }
        public static int InfoTop { get { return 1; } }
        public static string WinMessage { get { return winMessage; } }
        public static int WinWidth { get { return WinMessage.Split(new char[] { '\n' }).Max(s => s.Length); } }
        public static int WinHeight { get { return WinMessage.Split(new char[] { '\n' }).Length; } }
        public static int WinLeft { get { return World.WorldScale + 1; } }
        public static int WinTop { get { return InfoTop + InfoHeight + 1; } }

        public static int BufferWidth { get { return InfoLeft + (InfoWidth > WinWidth ? InfoWidth : WinWidth) + 1; } }
        public static int BufferHeight { get { return 1 + (World.View.GetLength(1) > (WinHeight + WinTop) ? World.View.GetLength(1) : (WinHeight + WinTop)); } }

        /// <summary>
        /// Runs the maze until it is completed
        /// </summary>
        public static void Run()
        {
            Draw();
            while (!Player.HasWon)
            {
                if (Console.KeyAvailable)
                {
                    //Move();
                    Player.Input(Console.ReadKey(false).Key);
                    Draw();
                }
            }
            Win();
        }

        /// <summary>
        /// Draws the maze
        /// </summary>
        static void Draw()
        {
            //Setup console
            Console.Clear();
            Console.WindowWidth = BufferWidth % 200;
            Console.WindowHeight = BufferHeight % 70;
            Console.BufferWidth = BufferWidth;
            Console.BufferHeight = BufferHeight;

            //Draw world
            World.Draw();

            //Draw information
            string[] _info = Information.Split(new char[] { '\n' });
            for (int _i = 0; _i < _info.Length; _i++)
            {
                Console.SetCursorPosition(InfoLeft, InfoTop + _i);
                Console.Write(_info[_i]);
            }

            Console.CursorVisible = false;
            //World.PrintWorld();
            //WriteMaze();
            //GetMoves();
        }

        /// <summary>
        /// Prints that user has solved maze
        /// </summary>
        static void Win()
        {
            Draw();

            Console.SetCursorPosition(WinLeft, WinTop);
            Console.ForegroundColor = WIN_FG;
            Console.BackgroundColor = WIN_BG;

            string[] _win = WinMessage.Split(new char[] { '\n' });
            for (int _i = 0; _i < _win.Length; _i++)
            {
                Console.SetCursorPosition(WinLeft, WinTop + _i);
                Console.Write(_win[_i]);
            }
            Console.ReadLine();
            Console.Clear();
            Console.ResetColor();
        }
    }
}
