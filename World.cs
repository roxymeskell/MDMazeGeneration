using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDMazeGeneration
{
    static class World
    {
        static readonly int X = 0, Y = 1, Z = 2;
        public static readonly char Bound = 'B', Ascending = 'A', Descending = 'D', PlayerMarker = 'X';
        static readonly int MIN_INTERIOR_SCALE = 3, MIN_BOUND_SCALE = 1, MIN_PLAYER_SCALE = 1;
        static int? playerScale, interiorScale, boundScale, openingScale;
        static int[] currDimensions;
        static char[,] view;
        public static int PlayerScale
        {
            get { return playerScale.HasValue ? playerScale.Value : MIN_PLAYER_SCALE; }
            set { playerScale = value > 0 ? value : PlayerScale; }
        }
        public static int InteriorScale
        {
            get { return interiorScale.HasValue ? interiorScale.Value * PlayerScale : MIN_INTERIOR_SCALE * PlayerScale; }
            set
            {
                interiorScale = value / PlayerScale < MIN_INTERIOR_SCALE && value > OpeningScale * 2 ? InteriorScale / PlayerScale :
                    ((value / PlayerScale) % 2 != 1 ? (value / PlayerScale) + 1 : (value / PlayerScale));
            }
        }
        public static int BoundScale
        {
            get { return boundScale.HasValue ? boundScale.Value * PlayerScale : MIN_BOUND_SCALE * PlayerScale; }
            set { boundScale = value / PlayerScale < MIN_BOUND_SCALE ? BoundScale / PlayerScale : value / PlayerScale; }
        }
        public static int OpeningScale
        {
            get { return openingScale.HasValue ? openingScale.Value : (int)Math.Floor((double)InteriorScale / 2) * PlayerScale; }
            set { openingScale = value / PlayerScale > (int)Math.Floor((double)InteriorScale / 2) ? OpeningScale / PlayerScale : value / PlayerScale; }
        }
        public static int CellScale { get { return InteriorScale + BoundScale; } }
        public static int[] CurrentDimensions
        {
            get
            {
                if (currDimensions == null)
                    currDimensions = Maze.IntialDimensions;
                return currDimensions;
            }
            set
            {
                if (currDimensions == null)
                    currDimensions = new int[] { X, Y, Z };
                if (value == null)
                    return;

                int[] _newD = value;
                for (int _i = 0; _i < _newD.Length; _i++)
                    _newD[_i] %= Maze.Dimensions;

                Array.Copy(_newD.Distinct().ToArray(), currDimensions, _newD.Distinct().ToArray().Length < currDimensions.Length ?
                    _newD.Distinct().ToArray().Length : currDimensions.Length);
            }
        }
        public static int[] CenterCell { get { return Player.CurrentCell; } set { Player.CurrentCell = value; } }
        public static char[,] View
        {
            get
            {
                if (view == null)
                    SetupMap();
                return view;
            }
        }
        public static int DimensionX
        {
            get
            {
                return CurrentDimensions[X];
            }
            set
            {
                int _newD = value % Maze.Dimensions;
                while (CurrentDimensions.Contains(_newD))
                    _newD = (_newD + 1) % Maze.Dimensions;
                CurrentDimensions[X] = _newD;
            }
        }
        public static int DimensionY
        {
            get
            {
                return CurrentDimensions[Y];
            }
            set
            {
                int _newD = value % Maze.Dimensions;
                while (CurrentDimensions.Contains(_newD))
                    _newD = (_newD + 1) % Maze.Dimensions;
                CurrentDimensions[Y] = _newD;
            }
        }
        public static int DimensionZ
        {
            get
            {
                return CurrentDimensions[Z];
            }
            set
            {
                int _newD = value % Maze.Dimensions;
                while (CurrentDimensions.Contains(_newD))
                    _newD = (_newD + 1) % Maze.Dimensions;
                CurrentDimensions[Z] = _newD;
            }
        }
        public static int WorldScale { get { return (CellScale * 3) + BoundScale; } }
        public static int InfoBufferY { get { return 1; } }
        public static int InfoBufferLines { get { return 2; } }
        public static int DInfoBufferY { get { return InfoBufferY + InfoBufferLines + 1; } }
        public static int DInfoBufferLines { get { return 2; } }
        public static string MapTitle { get { return "Current Visable Maze:"; } }
        public static int MapBufferY { get { return DInfoBufferY + DInfoBufferLines + 1; } }
        public static int MapBufferLines { get { return 1 + Maze.Viewable2D.GetLength(Y); } }
        public static int ControlsBufferY { get { return MapBufferY + MapBufferLines + 1; } }
        public static int ControlBufferLines { get { return 1 + 10; } }
        public static int BufferHeight { get { return 1 + ((ControlsBufferY + ControlBufferLines) > WorldScale ? (ControlsBufferY + ControlBufferLines) : WorldScale); } }
        public static int BufferWidth { get { return 8 + (WorldScale + (MapTitle.Length + 1 < Maze.Viewable2D.GetLength(X) + 1 ? Maze.Viewable2D.GetLength(X) + 1 : (MapTitle.Length + 1 < (Maze.Dimensions * 3) + 2 ? (Maze.Dimensions * 4) + 2 : MapTitle.Length + 1))); } }

        public static void Initialize(int _interiorScale, int _boundScale, int _openingScale)
        {
            PlayerScale = 1;
            InteriorScale = _interiorScale;
            BoundScale = _boundScale; 
            OpeningScale = _openingScale;

            Update();
        }

        public static void SetupMap()
        {
            if (Player.ZChanged)
            {
                Maze.SetupViewable2D();
                Player.ZChanged = false;
            }

            char[,] _charMaze = new char[(Maze.DimensionInfo[CurrentDimensions[X]] * CellScale) + BoundScale,
                (Maze.DimensionInfo[CurrentDimensions[Y]] * CellScale) + BoundScale];
            for (int _charX = 0; _charX < _charMaze.GetLength(0); _charX++)
            {
                for (int _charY = 0; _charY < _charMaze.GetLength(1); _charY++)
                {
                    _charMaze[_charX, _charY] = ' ';
                }
            }

            for ( int _x = 0; _x < Maze.Viewable2D.GetLength(X); _x++)
            {
                for ( int _y = 0; _y < Maze.Viewable2D.GetLength(Y); _y++)
                {
                    int[,] _valInfo;
                    //If _x or _y is even, bound, else, interior
                    if( _x % 2 == 0 || _y % 2 == 0)
                    {
                        _valInfo = Maze.GetBoundInfo(new int[] { _x, _y}, Maze.Viewable2D[_x, _y]);
                        for (int _b = 0; _b < _valInfo.GetLength(0); _b++)
                        {
                            //if (_valInfo[_b, 0] >= 0 && _valInfo[_b, 1] >= 0 && _valInfo[_b, 2] >= 0 && _valInfo[_b, 3] >= 0)
                                for (int _charX = _valInfo[_b, 0] - (int)Math.Floor((double)_valInfo[_b, 2] / 2);
                                    _charX <= _valInfo[_b, 0] + (int)Math.Floor((double)_valInfo[_b, 2] / 2); _charX++)
                                {
                                    for (int _charY = _valInfo[_b, 1] - (int)Math.Floor((double)_valInfo[_b, 3] / 2);
                                    _charY <= _valInfo[_b, 1] + (int)Math.Floor((double)_valInfo[_b, 3] / 2); _charY++)
                                    {
                                        if(_charX >= 0 && _charX < _charMaze.GetLength(0) && _charY >= 0 && _charY < _charMaze.GetLength(1))
                                            _charMaze[_charX, _charY] = Bound;
                                    }
                                }
                        }

                    }
                    else
                    {
                        _valInfo = Maze.GetInteriorInfo(new int[] { _x, _y}, Maze.Viewable2D[_x, _y]);
                        //Ascending
                        if (_valInfo[0, 2] >= 0 && _valInfo[0, 3] >= 0)
                            for (int _charX = _valInfo[0, 0] - (int)Math.Floor((double)_valInfo[0, 2] / 2);
                                    _charX <= _valInfo[0, 0] + (int)Math.Floor((double)_valInfo[0, 2] / 2); _charX++)
                            {
                                for (int _charY = _valInfo[0, 1] - (int)Math.Floor((double)_valInfo[0, 3] / 2);
                                _charY <= _valInfo[0, 1] + (int)Math.Floor((double)_valInfo[0, 3] / 2); _charY++)
                                {
                                    if (_charX > 0 && _charX < _charMaze.GetLength(0) && _charY > 0 && _charY < _charMaze.GetLength(1))
                                        _charMaze[_charX, _charY] = Ascending;
                                }
                            }
                        //Descending
                        if (_valInfo[1, 2] >= 0 && _valInfo[1, 3] >= 0)
                            for (int _charX = _valInfo[1, 0] - (int)Math.Floor((double)_valInfo[1, 2] / 2);
                                    _charX <= _valInfo[1, 0] + (int)Math.Floor((double)_valInfo[1, 2] / 2); _charX++)
                            {
                                for (int _charY = _valInfo[1, 1] - (int)Math.Floor((double)_valInfo[1, 3] / 2);
                                _charY <= _valInfo[1, 1] + (int)Math.Floor((double)_valInfo[1, 3] / 2); _charY++)
                                {
                                    if(_charX > 0 && _charX < _charMaze.GetLength(0) && _charY > 0 && _charY < _charMaze.GetLength(1))
                                        _charMaze[_charX, _charY] = Descending;
                                }
                            }
                    }
                }
            }
            view = _charMaze;
        }

        public static void PrintWorld()
        {
            if (!Player.PositionChanged)
                return;

            Console.Clear();
            Player.PositionChanged = false;

            Console.WindowWidth = BufferWidth % 200;
            Console.WindowHeight = BufferHeight % 70;
            Console.BufferWidth = BufferWidth;
            Console.BufferHeight = BufferHeight;
            //Console.SetWindowSize(BufferWidth % 200, BufferHeight % 70);
            //Console.SetBufferSize(BufferWidth, BufferHeight);

            int _startX, _endX, _startY, _endY;
            _startX = (CenterCell[DimensionX] - 1) * CellScale;
            _startX = _startX < 0 ? 0 : _startX;
            _endX = ((CenterCell[DimensionX] + 2) * CellScale) + BoundScale;
            _endX = _endX > View.GetLength(0) ? View.GetLength(0) : _endX;
            _startY = (CenterCell[DimensionY] - 1) * CellScale;
            _startY = _startY < 0 ? 0 : _startY;
            _endY = ((CenterCell[DimensionY] + 2) * CellScale) + BoundScale;
            _endY = _endY > View.GetLength(1) ? View.GetLength(1) : _endY;
            if (_endX - _startX < (3 * CellScale) + BoundScale)
            {
                _startX = (_endX - ((3 * CellScale) + BoundScale)) < 0 ? 0 : (_endX - ((3 * CellScale) + BoundScale));
                _endX = (_startX + ((3 * CellScale) + BoundScale)) > View.GetLength(0) ? View.GetLength(0) : (_startX + ((3 * CellScale) + BoundScale));
            }
            if (_endY - _startY < (3 * CellScale) + BoundScale)
            {
                _startY = (_endY - ((3 * CellScale) + BoundScale)) < 0 ? 0 : (_endY - ((3 * CellScale) + BoundScale));
                _endY = (_startY + ((3 * CellScale) + BoundScale)) > View.GetLength(1) ? View.GetLength(1) : (_startY + ((3 * CellScale) + BoundScale));
            }

            for (int _y = _startY; _y < _endY; _y++)
            {
                for (int _x = _startX; _x < _endX; _x++)
                {
                    Console.SetCursorPosition(_x - _startX, _y - _startY);

                    if (_x == Player.PlayerX && _y == Player.PlayerY)
                        Console.Write(PlayerMarker);
                    else
                        Console.Write(View[_x, _y]);
                }
            }

            Console.SetCursorPosition(0, WorldScale);

            PrintCellInfo();
            PrintCurrDimensions();
            PrintMap();
        }

        public static void PrintCellInfo()
        {
            bool _entrance = true, _exit = true;

            for (int _d = 0; _d < Maze.Dimensions; _d++)
            {
                _entrance = Player.CurrentCell[_d] == Maze.Entrance[_d] && _entrance;
                _exit = Player.CurrentCell[_d] == Maze.Exit[_d] && _exit;
            }

            if (_entrance || _exit)
            {
                Console.SetCursorPosition(WorldScale + 1, InfoBufferY);
                Console.Write("{0}", _entrance ? "Entrance" : "Exit");
            }

            Console.SetCursorPosition(WorldScale + 1, InfoBufferY + 1);
            Console.Write("[{0}", CenterCell[0]);
            for (int _d = 1; _d < Maze.Dimensions - 1; _d++)
                Console.Write(", {0}", CenterCell[_d]);
            Console.Write(", {0}]", CenterCell[Maze.Dimensions - 1]);

            Console.SetCursorPosition(0, WorldScale);
        }

        public static void PrintCurrDimensions()
        {
            Console.SetCursorPosition(WorldScale + 1, DInfoBufferY);
            Console.Write("Current Dimensions:");
            Console.SetCursorPosition(WorldScale + 1, DInfoBufferY + 1);
            Console.Write("\tX: {0}\tY: {1}\tZ: {2}", DimensionX, DimensionY, DimensionZ);
            Console.SetCursorPosition(0, WorldScale);
        }

        public static void PrintMap()
        {
            Console.SetCursorPosition(WorldScale + 1, MapBufferY);
            Console.Write(MapTitle);

            for (int _y = 0; _y < Maze.Viewable2D.GetLength(1); _y++)
            {
                for (int _x = 0; _x < Maze.Viewable2D.GetLength(0); _x++)
                {
                    Console.SetCursorPosition(WorldScale + 2 + _x, MapBufferY + 1 + _y);
                    switch (Maze.Viewable2D[_x, _y])
                    {
                        case 3:
                            Console.Write('#');
                            break;
                        case 2:
                            Console.Write('/');
                            break;
                        case 4:
                            Console.Write('0');
                            break;
                        case 6:
                            Console.Write('%');
                            break;
                        default:
                            Console.Write(' ');
                            break;

                    }
                }
            }
            Console.SetCursorPosition(0, WorldScale);
        }
        
        public static void Update()
        {
            Maze.SetupViewable2D();
            SetupMap();

            Player.PositionChanged = true;
        }

        public static void PrintDimensionInfo()
        {
            Console.WriteLine("Current Dimensions:");
            Console.WriteLine("\tX: {0}\tY: {1}\tZ: {2}", DimensionX, DimensionY, DimensionZ);
            Console.WriteLine("Maze Dimensions:");
            for (int _d = 0; _d < Maze.Dimensions; _d++)
                Console.Write("\t{0}: {1}", _d, Maze.DimensionInfo[_d]);
            Console.WriteLine();
        }

        public static void PrintPlayerInfo()
        {
            Console.WriteLine("Player--");
            Console.WriteLine("\tCoordinates:");
            Console.WriteLine("\t\tOverall: X: {0}\tY: {1}\tZ: {2}", Player.PlayerX, Player.PlayerY, Player.PlayerZ);
            Console.WriteLine("\t\tIn Cell: X: {0}\tY: {1}", Player.CoorInCell[X], Player.CoorInCell[Y]);
            Console.Write("\t\tIn Maze: ");
            for (int _d = 0; _d < Maze.Dimensions; _d++)
                Console.Write("{0}: {1}\t", _d, Player.CurrentCell[_d]);
            Console.WriteLine();
        }

        public static void PrintInfo()
        {
            Console.WriteLine("Current Dimensions:");
            Console.WriteLine("\tX: {0}\tY: {1}\tZ: {2}", DimensionX, DimensionY, DimensionZ);
            Console.WriteLine();

            Console.WriteLine("Player--");
            Console.WriteLine("\tCan Switch Dimensions: {0}", Player.CanSwitchDimensions);
            Console.WriteLine("\tCoordinates:");
            Console.WriteLine("\t\tOverall: X: {0}\tY: {1}\tZ: {2}", Player.PlayerX, Player.PlayerY, Player.PlayerZ);
            Console.WriteLine("\t\tIn Cell: X: {0}\tY: {1}", Player.CoorInCell[X], Player.CoorInCell[Y]);
            Console.Write("\t\tIn Maze: ");
            for (int _d = 0; _d < Maze.Dimensions; _d++)
                Console.Write("{0}: {1}\t", _d, Player.CurrentCell[_d]);
            Console.WriteLine();
            Console.WriteLine("\t\tTo... negX: {0}\tposX: {1}\tnegY: {2}\tposY: {3}", Player.InNegX, Player.InPosX, Player.InNegY, Player.InPosY);
            Console.WriteLine("\t\tCan move... negX: {0}\tposX: {1}\tnegY: {2}\tposY: {3}", Player.CanNegX, Player.CanPosX, Player.CanNegY, Player.CanPosY);
            Console.WriteLine();

            Console.WriteLine("Cell information--");
            Console.Write("\tEntrance: ");
            for (int _d = 0; _d < Maze.Dimensions; _d++)
                Console.Write("\t{0}: {1}", _d, Maze.Entrance[_d]);
            Console.WriteLine("\t {0}", Maze.EntranceViewable ? "Veiwable" : "");
            Console.Write("\tCurrent: ");
            for (int _d = 0; _d < Maze.Dimensions; _d++)
                Console.Write("\t{0}: {1}", _d, Player.CurrentCell[_d]);
            Console.WriteLine();
            Console.Write("\tExit:\t ");
            for (int _d = 0; _d < Maze.Dimensions; _d++)
                Console.Write("\t{0}: {1}", _d, Maze.Exit[_d]);
            Console.WriteLine("\t {0}", Maze.ExitViewable ? "Veiwable" : "");



            for (int _y = 0; _y < Maze.Viewable2D.GetLength(1); _y++)
            {
                for (int _x = 0; _x < Maze.Viewable2D.GetLength(0); _x++)
                {
                    switch (Maze.Viewable2D[_x, _y])
                    {
                        case 3:
                            Console.Write('#');
                            break;
                        case 2:
                            Console.Write('/');
                            break;
                        case 4:
                            Console.Write('0');
                            break;
                        case 6:
                            Console.Write('%');
                            break;
                        default:
                            Console.Write(' ');
                            break;

                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
