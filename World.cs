using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDMazeGeneration
{
    static class World
    {
        static readonly int X = 0, Y = 1, Z = 2;
        static readonly int MIN_INTERIOR_SCALE = 3, MIN_BOUND_SCALE = 1, MIN_PLAYER_SCALE = 1;
        static int? playerScale, interiorScale, boundScale, openingScale;
        static int[] currDimensions, centerCell;
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
                /*int[] _currD = new int[] { X, Y, Z };
                Array.Copy(currDimensions.Distinct().ToArray(), _currD, currDimensions.Distinct().ToArray().Length < _currD.Length ?
                    currDimensions.Distinct().ToArray().Length : _currD.Length);*/
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
        public static int[] CenterCell
        {
            get
            {
                if (centerCell == null)
                {
                    centerCell = Maze.Entrance;
                    centerCell.Initialize();
                }
                return centerCell;
            }
        }
        public static char[,] View
        {
            get
            {
                if (view == null)
                    ViewToCharArray();
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

        public static void Initialize(int _interiorScale, int _boundScale, int _openingScale)
        {
            PlayerScale = 1;
            InteriorScale = _interiorScale;
            BoundScale = _boundScale;
            OpeningScale = _openingScale;
        }

        public static void ViewToCharArray()
        {
            Maze.SetViewable2D();

            char[,] _charMaze = new char[(Maze.DimensionInfo[CurrentDimensions[X]] * CellScale) + BoundScale + CellScale * 4,
                (Maze.DimensionInfo[CurrentDimensions[Y]] * CellScale) + BoundScale + CellScale * 4];
            for (int _charX = 0; _charX < _charMaze.GetLength(0); _charX++)
            {
                for (int _charY = 0; _charY < _charMaze.GetLength(1); _charY++)
                {
                    _charMaze[_charX, _charY] = ' ';
                }
            }

            for ( int _x = 0; _x < Maze.Viewable2D.GetLength(X); _x++)
            {
                for ( int _y = 0; _y < Maze.Viewable2D.GetLength(X); _y++)
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
                                        _charMaze[_charX + CellScale, _charY + CellScale] = 'B';
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
                                    _charMaze[_charX + CellScale, _charY + CellScale] = 'A';
                                }
                            }
                        //Descending
                        if (_valInfo[1, 2] >= 0 && _valInfo[1, 3] >= 0)
                            for (int _charX = _valInfo[0, 0] - (int)Math.Floor((double)_valInfo[0, 2] / 2);
                                    _charX <= _valInfo[0, 0] + (int)Math.Floor((double)_valInfo[0, 2] / 2); _charX++)
                            {
                                for (int _charY = _valInfo[0, 1] - (int)Math.Floor((double)_valInfo[0, 3] / 2);
                                _charY <= _valInfo[0, 1] + (int)Math.Floor((double)_valInfo[0, 3] / 2); _charY++)
                                {
                                    _charMaze[_charX + CellScale, _charY + CellScale] = 'D';
                                }
                            }
                    }
                }
            }
            view = _charMaze;
        }

        public static void PrintWorld()
        {
            for (int _y = 0; _y < View.GetLength(1); _y++)
            {
                for (int _x = 0; _x < View.GetLength(0); _x++)
                {
                    Console.Write(View[_x, _y]);
                }
                Console.WriteLine();
            }

            Console.WriteLine("Current Dimensions:");
            Console.WriteLine("X: {0}\tY: {1}\tZ: {2}", DimensionX, DimensionY, DimensionZ);
        }
    }
}
