using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MDMazeGeneration
{
    /// <summary>
    /// An enum define the next movement to be made by the player
    /// </summary>
    enum Movement
    {
        None, NegX, PosX, NegY, PosY, NegZ, PosZ
    }

    /// <summary>
    /// A static class keeping track of the player's position and movents
    /// </summary>
    static class Player
    {
        static readonly int X = 0, Y = 1, Z = 2; // Some constants defined for the sake of less headache

        static int[] cellCoor2D, currCell; // Stores values for player

        static bool win; // Stores if player has won

        /// <summary>
        /// A boolean value representing if it is safe to shift dimensions
        /// </summary>
        public static bool CanSwitchDimensions { get { return CoorInCell[X] > 0 && CoorInCell[X] <= World.InteriorScale && CoorInCell[Y] > 0 && CoorInCell[Y] <= World.InteriorScale; } }
        /// <summary>
        /// The 2D coordinates of where on the viewable gird the player currently is
        /// </summary>
        public static int[] CoorOnGrid { get { return new int[] { CoorInCell[X] + (CurrentCell[World.DimensionX] * World.CellScale), CoorInCell[Y] + (CurrentCell[World.DimensionY] * World.CellScale) }; } }
        /// <summary>
        /// The 2D corrdinates of where in a cell the player is
        /// </summary>
        public static int[] CoorInCell
        {
            get
            {
                if (cellCoor2D == null)
                    cellCoor2D = new int[] { World.InteriorScale / 2, World.InteriorScale / 2 };
                return cellCoor2D;
            }
            set
            {
                if (value.Length < 2)
                    return;
                CurrentCell[World.DimensionX] += (int)Math.Floor((double)value[X] / World.CellScale);
                CurrentCell[World.DimensionY] += (int)Math.Floor((double)value[Y] / World.CellScale);
                cellCoor2D[X] = (value[X] % World.CellScale) + (value[X] % World.CellScale < 0 ?  World.CellScale : 0);
                CurrentCell[World.DimensionY] += (int)Math.Floor((double)value[Y] / World.CellScale);
                cellCoor2D[Y] = (value[Y] % World.CellScale) + (value[Y] % World.CellScale < 0 ? World.CellScale : 0);
            }
        }
        /// <summary>
        /// The cell coordinates the player is currently in
        /// </summary>
        public static int[] CurrentCell
        {
            get
            {
                if (currCell == null)
                    currCell = Maze.Entrance.ToArray();
                return currCell;
            }
            set
            {
                if (value.Length < Maze.Dimensions)
                    return;
                for (int _d = 0; _d < Maze.Dimensions; _d++)
                    currCell[_d] = value[_d] < 0 ? 0 : (value[_d] < Maze.DimensionInfo[_d] ? value[_d] : (Maze.DimensionInfo[_d] - 1));
            }
        }
        /// <summary>
        /// The X position of the player on the viewable grid
        /// </summary>
        public static int PlayerX
        {
            get { return CoorOnGrid[X]; }
            set 
                {
                    if ((value < PlayerX && !CanNegX) || (value > PlayerX && !CanPosX))
                        return;

                    int _currCellVal = ((int)Math.Floor((double)value / World.CellScale) < 0 ? 0 :
                        ((int)Math.Floor((double)value / World.CellScale) < Maze.DimensionInfo[World.DimensionX] ?
                        (int)Math.Floor((double)value / World.CellScale) : (Maze.DimensionInfo[World.DimensionX] - 1)));

                    win = AtExit && ((int)Math.Floor((double)value / World.CellScale) < 0 ||
                        (int)Math.Floor((double)value / World.CellScale) >= Maze.DimensionInfo[World.DimensionX]);

                    int _inCellVal = value == World.View.GetLength(X) ? CoorInCell[X] :
                        ((value % World.CellScale) + (value % World.CellScale < 0 ? World.CellScale : 0));

                    CurrentCell[World.DimensionX] = _currCellVal;
                    CoorInCell[X] = _inCellVal;
                }
        }
        /// <summary>
        /// The Y position of the player on the viewable grid
        /// </summary>
        public static int PlayerY
        {
            get { return CoorOnGrid[Y]; }
            set
            {
                if ((value < PlayerY && !CanNegY) || (value > PlayerY && !CanPosY))
                    return;

                int _currCellVal = ((int)Math.Floor((double)value / World.CellScale) < 0 ? 0 :
                    ((int)Math.Floor((double)value / World.CellScale) < Maze.DimensionInfo[World.DimensionY] ?
                    (int)Math.Floor((double)value / World.CellScale) : (Maze.DimensionInfo[World.DimensionY] - 1)));

                win = AtExit && ((int)Math.Floor((double)value / World.CellScale) < 0 ||
                        (int)Math.Floor((double)value / World.CellScale) >= Maze.DimensionInfo[World.DimensionY]);

                int _inCellVal = value == World.View.GetLength(Y) ? CoorInCell[Y] :
                        ((value % World.CellScale) + (value % World.CellScale < 0 ? World.CellScale : 0));

                CurrentCell[World.DimensionY] = _currCellVal;
                CoorInCell[Y] = _inCellVal;
            }
        }
        /// <summary>
        /// The position of the player in the current Z dimension
        /// </summary>
        public static int PlayerZ
        {
            get { return CurrentCell[World.DimensionZ]; }
            set
            {
                if ((value < PlayerZ && !CanNegZ) || (value > PlayerZ && !CanPosZ))
                    return;
                CurrentCell[World.DimensionZ] = ((value < CurrentCell[World.DimensionZ] && CanNegZ) ||
                    (value > CurrentCell[World.DimensionZ] && CanPosZ)) ? (value < 0 ? 0 :
                    (value < Maze.DimensionInfo[World.DimensionZ] ? value : (Maze.DimensionInfo[World.DimensionZ] - 1)))
                    : CurrentCell[World.DimensionZ];
                win = AtExit && (((value < CurrentCell[World.DimensionZ] && CanNegZ) ||
                    (value > CurrentCell[World.DimensionZ] && CanPosZ)) && (value < 0 || value >= Maze.DimensionInfo[World.DimensionZ]));
            }
        }
        public static char InNegX
        {
            get
            {
                int[] _negX = new int[2];
                Array.Copy(CoorOnGrid, _negX, 2);
                _negX[X]--;
                if (_negX[X] < 0 || _negX[X] >= World.View.GetLength(X))
                    return World.Bound;
                return (char)World.View.GetValue(_negX);
            }
        }
        public static char InPosX
        {
            get
            {
                int[] _posX = new int[2];
                Array.Copy(CoorOnGrid, _posX, 2);
                _posX[X]++;
                if (_posX[X] < 0 || _posX[X] >= World.View.GetLength(X))
                    return World.Bound;
                return (char)World.View.GetValue(_posX);
            }
        }
        public static char InNegY
        {
            get
            {
                int[] _negY = new int[2];
                Array.Copy(CoorOnGrid, _negY, 2);
                _negY[Y]--;
                if (_negY[Y] < 0 || _negY[Y] >= World.View.GetLength(Y))
                    return World.Bound;
                return (char)World.View.GetValue(_negY);
            }
        }
        public static char InPosY
        {
            get
            {
                int[] _posY = new int[2];
                Array.Copy(CoorOnGrid, _posY, 2);
                _posY[Y]++;
                if (_posY[Y] < 0 || _posY[Y] >= World.View.GetLength(Y))
                    return World.Bound;
                return (char)World.View.GetValue(_posY);
            }
        }
        public static char AtCurrent { get { return (char)World.View.GetValue(CoorOnGrid); } }
        public static bool CanNegX { get { return InNegX != World.Bound; } }
        public static bool CanPosX { get { return InPosX != World.Bound; } }
        public static bool CanNegY { get { return InNegY != World.Bound; } }
        public static bool CanPosY { get { return InPosY != World.Bound; } }
        public static bool CanPosZ { get { return AtCurrent == World.Ascending; } }
        public static bool CanNegZ { get { return AtCurrent == World.Descending; } }

        static bool pChanged;
        public static bool PositionChanged { get { return pChanged; } set { pChanged = value; } }
        static bool zChanged;
        public static bool ZChanged { get { return zChanged; } set { zChanged = value; } }

        /// <summary>
        /// Boolean value specifiying if the player is in the exit cell
        /// </summary>
        public static bool AtExit
        {
            get
            {
                for (int _d = 0; _d < Maze.Dimensions; _d++)
                    if (CurrentCell[_d] != Maze.Exit[_d])
                        return false;
                return true;
            }
        }

        /// <summary>
        /// Boolean value specifiying if the player has won
        /// </summary>
        public static bool HasWon { get { return win; } }

        public static bool Input()
        {
            win = false;

            bool _loop = true;
            int _holdX = PlayerX, _holdY = PlayerY, _holdZ = PlayerZ;

            while (_loop)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.A:
                        PlayerX--;
                        _loop = _holdX == PlayerX;
                        break;
                    case ConsoleKey.D:
                        PlayerX++;
                        _loop = _holdX == PlayerX;
                        break;
                    case ConsoleKey.W:
                        PlayerY--;
                        _loop = _holdY == PlayerY;
                        break;
                    case ConsoleKey.S:
                        PlayerY++;
                        _loop = _holdY == PlayerY;
                        break;
                    case ConsoleKey.Spacebar:
                        if (CanNegZ)
                            PlayerZ--;
                        else
                            if (CanPosZ)
                                PlayerZ++;
                        _loop = _holdZ == PlayerZ;
                        World.Update();
                        break;
                    case ConsoleKey.D1:
                        _loop = !ShiftD(X);
                        break;
                    case ConsoleKey.D2:
                        _loop = !ShiftD(Y);
                        break;
                    case ConsoleKey.D3:
                        _loop = !ShiftD(Z);
                        break;
                    case ConsoleKey.Escape:
                        return false;
                    default:
                        _loop = true;
                        break;
                }
                Thread.Sleep(30);
            }

            return true;
        }

        public static bool Input(ConsoleKey _key)
        {
            int _holdX = PlayerX, _holdY = PlayerY, _holdZ = PlayerZ;

            win = false;

                switch (_key)
                {
                    case ConsoleKey.A:
                        PlayerX--;
                        PositionChanged = _holdX != PlayerX;
                        break;
                    case ConsoleKey.D:
                        PlayerX++;
                        PositionChanged = _holdX != PlayerX;
                        break;
                    case ConsoleKey.W:
                        PlayerY--;
                        PositionChanged = _holdY != PlayerY;
                        break;
                    case ConsoleKey.S:
                        PlayerY++;
                        PositionChanged = _holdY != PlayerY;
                        break;
                    case ConsoleKey.Spacebar:
                        if (CanNegZ)
                            PlayerZ--;
                        else
                            if (CanPosZ)
                                PlayerZ++;
                        World.Update();
                        ZChanged = _holdZ != PlayerZ;
                        break;
                    case ConsoleKey.D1:
                        ShiftD(X);
                        break;
                    case ConsoleKey.D2:
                        ShiftD(Y);
                        break;
                    case ConsoleKey.D3:
                        ShiftD(Z);
                        break;
                    case ConsoleKey.Escape:
                        return false;
                    default:
                        break;
                }

            return true;
        }

        private static bool ShiftD(int _d)
        {
            if (!CanSwitchDimensions)
                return false;

            switch (_d)
            {
                case 0:
                    World.DimensionX = World.DimensionX + 1;
                    break;
                case 1:
                    World.DimensionY = World.DimensionY + 1;
                    break;
                case 2:
                    World.DimensionZ = World.DimensionZ + 1;
                    break;
                default:
                    return false;
            }

            World.Update();

            return true;
        }
    }
}
