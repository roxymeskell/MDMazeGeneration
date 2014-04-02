using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDMazeGeneration
{
    enum Part
    {
        Floor = ' ',
        Bound = 'B',
        Ascending = '+',
        Descending = '-',
        Player = 'X'
    }

    static class World
    {
        static readonly int X = 0, Y = 1, Z = 2;
        public static readonly char Floor = (char)Part.Floor, Bound = (char)Part.Bound, Ascending = (char)Part.Ascending,
            Descending = (char)Part.Descending, PlayerMarker = (char)Part.Player;
        static readonly int MIN_INTERIOR_SCALE = 3, MIN_BOUND_SCALE = 1, MIN_PLAYER_SCALE = 1;
        static readonly ConsoleColor PLAYER_FG_COLOR = ConsoleColor.Black;
        static readonly ConsoleColor PLAYER_BG_COLOR = ConsoleColor.White;
        static readonly ConsoleColor[] COLORS = new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.DarkBlue,
                                                                     ConsoleColor.DarkCyan, ConsoleColor.DarkGray, ConsoleColor.DarkGreen, ConsoleColor.DarkMagenta,
                                                                     ConsoleColor.DarkRed, ConsoleColor.DarkYellow, ConsoleColor.Gray, ConsoleColor.Green,
                                                                     ConsoleColor.Magenta, ConsoleColor.Red, ConsoleColor.White, ConsoleColor.Yellow };

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
                    currDimensions = Maze.InitialDimensions.ToArray();
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
                if (Maze.Dimensions > 3)
                {
                    int _newD = value % Maze.Dimensions;
                    while (CurrentDimensions.Contains(_newD))
                        _newD = (_newD + 1) % Maze.Dimensions;
                    CurrentDimensions[X] = _newD;
                }
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
                if (Maze.Dimensions > 3)
                {
                    int _newD = value % Maze.Dimensions;
                    while (CurrentDimensions.Contains(_newD))
                        _newD = (_newD + 1) % Maze.Dimensions;
                    CurrentDimensions[Y] = _newD;
                }
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
                if (Maze.Dimensions > 3)
                {
                    int _newD = value % Maze.Dimensions;
                    while (CurrentDimensions.Contains(_newD))
                        _newD = (_newD + 1) % Maze.Dimensions;
                    CurrentDimensions[Z] = _newD;
                }
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

        /// <summary>
        /// Initializes maze and world
        /// </summary>
        /// <param name="_dInfo">Dimensions information</param>
        /// <param name="_interiorScale">Interior scale</param>
        /// <param name="_boundScale">Bound scale</param>
      /// <param name="_openingScale">Opening scale</param>
        public static void Initialize(int[] _dInfo, int _interiorScale, int _boundScale, int _openingScale)
        {
            Maze.Initialize(_dInfo);

            PlayerScale = 1;
            InteriorScale = _interiorScale;
            BoundScale = _boundScale;
            OpeningScale = _openingScale;

            Update();
        }

        /// <summary>
        /// Initializes world
        /// </summary>
        /// <param name="_interiorScale">Interior scale</param>
        /// <param name="_boundScale">Bound scale</param>
        /// <param name="_openingScale">Opening scale</param>
        public static void Initialize(int _interiorScale, int _boundScale, int _openingScale)
        {
            PlayerScale = 1;
            InteriorScale = _interiorScale;
            BoundScale = _boundScale; 
            OpeningScale = _openingScale;

            Update();
        }

        /// <summary>
        /// Sets up the map that can be viewed
        /// </summary>
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
                    _charMaze[_charX, _charY] = Floor;
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

        /// <summary>
        /// Draws world to console
        /// </summary>
        public static void Draw()
        {
            //Ensuring that walls are visiable
            Console.BackgroundColor = COLORS[(DimensionY + 1) % Maze.Dimensions];
            Console.Clear();

            //Start drawing
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
                    {
                        Console.ForegroundColor = PLAYER_FG_COLOR;
                        Console.BackgroundColor = PLAYER_BG_COLOR;
                        Console.Write(PlayerMarker);
                    }
                    else
                    {
                        switch (View[_x, _y])
                        {
                            case (char)Part.Floor:
                                Console.ForegroundColor = COLORS[DimensionX];
                                Console.BackgroundColor = COLORS[DimensionX];
                                break;
                            case (char)Part.Bound:
                                Console.ForegroundColor = COLORS[DimensionY];
                                Console.BackgroundColor = COLORS[DimensionY];
                                break;
                            case (char)Part.Ascending:
                                Console.ForegroundColor = COLORS[(DimensionZ + 1) % Maze.Dimensions];
                                Console.BackgroundColor = COLORS[DimensionZ];
                                break;
                            case (char)Part.Descending:
                                Console.ForegroundColor = COLORS[(DimensionZ + 1) % Maze.Dimensions];
                                Console.BackgroundColor = COLORS[DimensionZ];
                                break;
                        }
                        Console.Write(View[_x, _y]);
                    }
                }
            }
            
            Console.ResetColor();
        }

        /// <summary>
        /// Resets world variables
        /// </summary>
        public static void Reset()
        {
            playerScale = null;
            interiorScale = null;
            boundScale = null;
            openingScale = null;
            currDimensions = null;
            view = null;
        }

        /// <summary>
        /// Updates maze and world
        /// </summary>
        public static void Update()
        {
            Maze.SetupViewable2D();
            SetupMap();

            Player.PositionChanged = true;
        }
    }
}
