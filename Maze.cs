using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace MDMazeGeneration
{
    //Flags for bound values
    [Flags]
    enum Bound : ushort
    {
        None        = 0x0,
        Dimension0  = 0x1,
        Dimension1  = 0x2,
        Dimension2  = 0x4,
        Dimension3  = 0x8,
        Dimension4  = 0x10,
        Dimension5  = 0x20,
        Dimension6  = 0x40,
        Dimension7  = 0x80,
        Dimension8  = 0x100,
        Dimension9  = 0x200,
        Dimension10 = 0x400,
        Dimension11 = 0x800,
        Dimension12 = 0x1000,
        Dimension13 = 0x2000,
        Dimension14 = 0x4000,
        Dimension15 = 0x8000
    }


    /// <summary>
    /// A static class that generates and holds a maze spanning multiple dimensions
    /// </summary>
    static class Maze
    {
        //The max number of dimensions the maze can have
        static readonly int MAX_DIMENSIONS = 16;
        //The interior scale of a cell in the viewable grid
        static readonly int CELL_SCALE = 1;
        //The scale of a bound in the viewable grid
        static readonly int BOUND_SCALE = 1;
        //Viewable dimension constants
        static readonly int X = 0, Y = 1, Z = 2;

        static int dimensions;
        static int[] dimensionInfo;
        static int[] entranceCoor, exitCoor, initialD;
        static List<Cell> sets;
        //Storage arrays
        static Array worldBounds;
        static int[,] viewable2D;

        /// <summary>
        /// The number of dimensions the maze extends in
        /// </summary>
        public static int Dimensions { get { return dimensions; } }
        /// <summary>
        /// A readonly array of information, mainly the sizes of, each dimension
        /// </summary>
        public static ReadOnlyCollection<int> DimensionInfo { get { return Array.AsReadOnly<int>(dimensionInfo); } }
        /// <summary>
        /// A readonly array for the coordinates of the entrance cell in the maze
        /// </summary>
        public static ReadOnlyCollection<int> Entrance { get { return Array.AsReadOnly<int>(entranceCoor); } }
        /// <summary>
        /// A readonly array for the coordinates of the exit cell in the maze
        /// </summary>
        public static ReadOnlyCollection<int> Exit { get { return Array.AsReadOnly<int>(exitCoor); } }
        /// <summary>
        /// A readonly array for initially viewable dimensions when starting the maze
        /// </summary>
        public static ReadOnlyCollection<int> InitialDimensions { get { return Array.AsReadOnly<int>(initialD); } }

        /// <summary>
        /// Current 2D viewable part of maze
        /// </summary>
        public static int[,] Viewable2D { get { return viewable2D; } }

        /// <summary>
        /// Boolean if entrance is currently viewable
        /// </summary>
        public static bool EntranceViewable
        {
            get
            {
                for (int _d = 0; _d < dimensions; _d++)
                    if (_d != World.DimensionX && _d != World.DimensionY && Entrance[_d] != World.CenterCell[_d])
                        return false;
                return true;
            }
        }
        /// <summary>
        /// Boolean if exit is currently viewable
        /// </summary>
        public static bool ExitViewable
        {
            get
            {
                for (int _d = 0; _d < dimensions; _d++)
                    if (_d != World.DimensionX && _d != World.DimensionY && Exit[_d] != World.CenterCell[_d])
                        return false;
                return true;
            }
        }
        /// <summary>
        /// Center X coordinate of entrance in viewable maze
        /// </summary>
        public static int EntranceViewCenterX { get { return BOUND_SCALE + entranceCoor[World.DimensionX] * (BOUND_SCALE + CELL_SCALE);  } }
        /// <summary>
        /// Center Y coordinate of entrance in viewable maze
        /// </summary>
        public static int EntranceViewCenterY { get { return BOUND_SCALE + entranceCoor[World.DimensionY] * (BOUND_SCALE + CELL_SCALE); } }
        /// <summary>
        /// Center X coordinate of exit in viewable maze
        /// </summary>
        public static int ExitViewCenterX { get { return BOUND_SCALE + exitCoor[World.DimensionX] * (BOUND_SCALE + CELL_SCALE); } }
        /// <summary>
        /// Center Y coordinate of exit in viewable maze
        /// </summary>
        public static int ExitViewCenterY { get { return BOUND_SCALE + exitCoor[World.DimensionY] * (BOUND_SCALE + CELL_SCALE); } }
        

        /// <summary>
        /// Constructor for Maze
        /// </summary>
        /// <param name="_dInfo">Dimension information</param>
        /// <param name="_cellScale">Size of cell</param>
        /// <param name="_safeZonePercent">Inner percent of cell that is always open and safe to switch dimensions in.</param>
        public static void Initialize(int[] _dInfo)
        {

            dimensionInfo = null;

            if (_dInfo.Length > MAX_DIMENSIONS)
                Array.Resize<int>(ref _dInfo, MAX_DIMENSIONS);

            dimensionInfo = _dInfo;
            dimensions = dimensionInfo.Length;

            sets = new List<Cell>();

            InitStorage();

            Build();

            SetupViewable2D();
        }

        /// <summary>
        /// Initiate storage arrays
        /// </summary>
        static void InitStorage()
        {
            //Initiate storage arrays
            worldBounds = Array.CreateInstance(typeof(ushort), dimensionInfo);
        }

        /// <summary>
        /// Builds the maze
        /// </summary>
        static void Build()
        {
            //Get coordinates of root cell in the maze
            int[] _coor = new int[dimensions];
            for (int _i = 0; _i < _coor.Length; _i++)
            {
                _coor[_i] = 0;
            }

            //Create root cell
            Cell _rootCell = GetFirstCell();

            //Expand in all dimensions last to first
            for (int _d = dimensions - 1; _d >= 0; _d--)
            {
                ExpandDimension(_d, _rootCell);
            }

            //Complete maze by joining all sets
            JoinSets();

            //Write all remaining cells to the maze
            sets.ElementAt(0).WriteSetToMaze();

            //Get entrance and exit
            GetOpenings();
        }

        /// <summary>
        /// Creates and returns the first cell in th maze
        /// </summary>
        /// <returns>The first cell in the maze</returns>
        static Cell GetFirstCell()
        {
            int[] _rootCoor = new int[dimensions];
            for (int i = 0; i < dimensions; i++)
                _rootCoor[i] = 0;
            return new Cell(_rootCoor);
        }

        /// <summary>
        /// Expands cells in specified dimension
        /// </summary>
        /// <param name="_d">Specified dimension</param>
        /// <param name="_cell">Cell being expanded from</param>
        static void ExpandDimension(int _d, Cell _cell)
        {
            Cell _current = _cell;
            for (int i = 0; i < (dimensionInfo[_d] - 1); i++)
            {
                _current = _current.GetNeighbor(_d);
                if (_current == null)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Merges sets randomly until only one remains
        /// </summary>
        static void JoinSets()
        {
            while (sets.Count > 1)
            {
                sets.RemoveAll(x => x.parent != null);

                bool hasDuplicates;

                do
                {
                    foreach (Cell _c in sets)
                    {
                        if (sets.Count(delegate(Cell _inSet) { return (_inSet == _c); }) > 1)
                        {
                            sets.RemoveAll(x => x == _c);
                            sets.Add(_c);
                            break;
                        }
                    }
                    hasDuplicates = false;
                    foreach (Cell _c in sets)
                    {
                        if (sets.Count(delegate(Cell _inSet) { return (_inSet == _c); }) > 1)
                        {
                            hasDuplicates = true;
                            break;
                        }
                    }
                } while (hasDuplicates);

                if (sets.Count > 1)
                {
                    //sets.ElementAt(Randomize.RandInt(sets.Count - 1)).MergeSet();
                    for (int _i = 0; _i < sets.Count; _i++)
                    {
                        sets.ElementAt(_i).MergeSet();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the entrance and exit to the maze
        /// </summary>
        static void GetOpenings()
        {
            //Randomly generate entrance and exit coordinates
            entranceCoor = Randomize.RandOpening(dimensionInfo);
            exitCoor = Randomize.RandOpening(dimensionInfo, entranceCoor);

            //Get initial viewable dimensions
            initialD = new int[3];
            int _d = 0;
            for (int _i = 0; _i < Dimensions; _i++)
            {
                if (Entrance[_i] == 0 || Entrance[_i] == DimensionInfo[_i] - 1)
                {
                    initialD[_d] = _i;
                    _d++;
                }
                if (_d >= initialD.Length)
                    return;
            }
            for (int _i = 0; _i < Dimensions; _i++)
            {
                if (!initialD.Contains(_i))
                {
                    initialD[_d] = _i;
                    _d++;
                }
                if (_d >= initialD.Length)
                    return;
            }
        }

        /// <summary>
        /// Sets up Viewable2D according to information provided by the world class
        /// </summary>
        static public void SetupViewable2D()
        {
            //Initialize view array
            int _viewX = BOUND_SCALE + dimensionInfo[World.DimensionX] * (BOUND_SCALE + CELL_SCALE),
                _viewY = BOUND_SCALE + dimensionInfo[World.DimensionY] * (BOUND_SCALE + CELL_SCALE);
            viewable2D = new int[_viewX, _viewY];

            //Find coordinates of cell (0, 0) in current view
            int[] _toWrite = new int[dimensions];
            for (int _d = 0; _d < dimensions; _d++)
            {
                _toWrite[_d] = _d == World.DimensionX|| _d == World.DimensionY ? 0 : World.CenterCell[_d];
            }

            //Fill view array
            //Intialize view array
            for (int _x = 0; _x < _viewX; _x++)
            {
                for (int _y = 0; _y < _viewY; _y++)
                {
                    //Find IDs
                    int _xID = (_x - BOUND_SCALE) - ((BOUND_SCALE + CELL_SCALE) * (int)Math.Floor((double)(_x - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE))),
                        _yID = (_y - BOUND_SCALE) - ((BOUND_SCALE + CELL_SCALE) * (int)Math.Floor((double)(_y - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE)));
                    //Initiate values using IDs
                    viewable2D[_x, _y] = SetValType(0, _xID >= CELL_SCALE || _yID >= CELL_SCALE);
                    viewable2D[_x, _y] = SetBound(viewable2D[_x, _y], !((_xID >= CELL_SCALE && _yID >= CELL_SCALE) || (_x == 0 || _y == 0)));
                }
            }

            //Write cells into view
            for (int _x = BOUND_SCALE; _x < _viewX; _x += BOUND_SCALE + CELL_SCALE)
            {
                for (int _y = BOUND_SCALE; _y < _viewY; _y += BOUND_SCALE + CELL_SCALE)
                {
                    //Write interior
                    viewable2D[_x, _y] = SetAscending(viewable2D[_x, _y], GetBit((ushort)worldBounds.GetValue(_toWrite), World.DimensionZ) == 0);
                    if (_toWrite[World.DimensionZ] != 0)
                    {
                        //Get if _toWrite has path form neighbor in negative Z dimension
                        _toWrite[World.DimensionZ]--;
                        viewable2D[_x, _y] = SetDescending(viewable2D[_x, _y], GetBit((ushort)worldBounds.GetValue(_toWrite), World.DimensionZ) == 0);
                        _toWrite[World.DimensionZ]++;
                    }
                    else
                    {
                        viewable2D[_x, _y] = SetDescending(viewable2D[_x, _y], false);
                    }

                    //Write bound on X
                    viewable2D[_x + CELL_SCALE, _y] = SetBound(viewable2D[_x + CELL_SCALE, _y], GetBit((ushort)worldBounds.GetValue(_toWrite), World.DimensionX) == 0);

                    //Write bound on Y
                    viewable2D[_x, _y + CELL_SCALE] = SetBound(viewable2D[_x, _y + CELL_SCALE], GetBit((ushort)worldBounds.GetValue(_toWrite), World.DimensionY) == 0);

                    //Increment Y value of _toWrite
                    _toWrite[World.DimensionY]++;
                }

                //Increment X value of _toWrite and set Y value to zero
                _toWrite[World.DimensionY] = 0;
                _toWrite[World.DimensionX]++;
            }

            //Open all outside walls of entrance and exit cells if cells are viewable
            //Check entrance
            if (EntranceViewable)
            {
                //Check and Open X
                viewable2D[0, EntranceViewCenterY] =
                    SetBound(viewable2D[0, EntranceViewCenterY], (Entrance[World.DimensionX] == 0) || OpenBound(viewable2D[0, EntranceViewCenterY]));
                viewable2D[_viewX - 1, EntranceViewCenterY] =
                    SetBound(viewable2D[_viewX - 1, EntranceViewCenterY], (Entrance[World.DimensionX] == dimensionInfo[World.DimensionX] - 1) ||
                    OpenBound(viewable2D[_viewX - 1, EntranceViewCenterY]));
                //Check and Open Y
                viewable2D[EntranceViewCenterX, 0] =
                    SetBound(viewable2D[EntranceViewCenterX, 0], (Entrance[World.DimensionY] == 0) || OpenBound(viewable2D[EntranceViewCenterX, 0]));
                viewable2D[EntranceViewCenterX, _viewY - 1] =
                    SetBound(viewable2D[EntranceViewCenterX, _viewY - 1], (Entrance[World.DimensionY] == dimensionInfo[World.DimensionY] - 1) ||
                    OpenBound(viewable2D[EntranceViewCenterX, _viewY - 1]));
                //Check and Open Z
                viewable2D[EntranceViewCenterX, EntranceViewCenterY] = SetDescending(viewable2D[EntranceViewCenterX, EntranceViewCenterY],
                     (Entrance[World.DimensionZ] == 0) || Descending(viewable2D[EntranceViewCenterX, EntranceViewCenterY]));
                viewable2D[EntranceViewCenterX, EntranceViewCenterY] = SetAscending(viewable2D[EntranceViewCenterX, EntranceViewCenterY],
                     (Entrance[World.DimensionZ] == dimensionInfo[World.DimensionZ] - 1) || Ascending(viewable2D[EntranceViewCenterX, EntranceViewCenterY]));
            }
            //Check exit
            if (ExitViewable)
            {
                //Check and Open X
                viewable2D[0, ExitViewCenterY] =
                    SetBound(viewable2D[0, ExitViewCenterY], (Exit[World.DimensionX] == 0) || OpenBound(viewable2D[0, ExitViewCenterY]));
                viewable2D[_viewX - 1, ExitViewCenterY] =
                    SetBound(viewable2D[_viewX - 1, ExitViewCenterY], (Exit[World.DimensionX] == dimensionInfo[World.DimensionX] - 1) ||
                    OpenBound(viewable2D[_viewX - 1, ExitViewCenterY]));
                //Check and Open Y
                viewable2D[ExitViewCenterX, 0] =
                    SetBound(viewable2D[ExitViewCenterX, 0], (Exit[World.DimensionY] == 0) || OpenBound(viewable2D[ExitViewCenterX, 0]));
                viewable2D[ExitViewCenterX, _viewY - 1] =
                    SetBound(viewable2D[ExitViewCenterX, _viewY - 1], (Exit[World.DimensionY] == dimensionInfo[World.DimensionY] - 1) ||
                    OpenBound(viewable2D[ExitViewCenterX, _viewY - 1]));
                //Check and Open Z
                viewable2D[ExitViewCenterX, ExitViewCenterY] = SetDescending(viewable2D[ExitViewCenterX, ExitViewCenterY],
                     (Exit[World.DimensionZ] == 0) || Descending(viewable2D[ExitViewCenterX, ExitViewCenterY]));
                viewable2D[ExitViewCenterX, ExitViewCenterY] = SetAscending(viewable2D[ExitViewCenterX, ExitViewCenterY],
                     (Exit[World.DimensionZ] == dimensionInfo[World.DimensionZ] - 1) || Ascending(viewable2D[ExitViewCenterX, ExitViewCenterY]));
            }
        }

        /// <summary>
        /// Changes a value to represent a bound if _bound is true,
        /// otherwise changes value to represent a cell interior
        /// </summary>
        /// <param name="_val">Value to set to represent a type</param>
        /// <param name="_isBound">If value is to be set to a bound</param>
        /// <returns>A value representing a bound (if true) or interior (if false)</returns>
        static int SetValType(int _val, bool _isBound)
        {
            return SetBitTo(_val, 0, _isBound ? 1 : 0);
        }

        /// <summary>
        /// Checks and returns ifa value represents a bound
        /// </summary>
        /// <param name="_val">Value to check</param>
        /// <returns>True if value represents a bound</returns>
        public static bool IsBound(int _val)
        {
            return GetBit(_val, 0) == 1;
        }

        /// <summary>
        /// Changes a given value to reflect that it represents an open or closed bound
        /// </summary>
        /// <param name="_val">Given value</param>
        /// <param name="_open">Represents if bound is to be opened (0) or closed (1)</param>
        /// <returns>Value reflecting bound</returns>
        static int SetBound(int _val, bool _open)
        {
            return IsBound(_val) ? SetBitTo(_val, 1, _open ? 0 : 1) : _val;
        }

        /// <summary>
        /// Checks if given value represents a closed bound
        /// </summary>
        /// <param name="_val">Given value</param>
        /// <returns>True if represents a closed bound</returns>
        static bool ClosedBound(int _val)
        {
            return IsBound(_val) && GetBit(_val, 1) == 1;
        }

        /// <summary>
        /// Checks if given value represents a open bound
        /// </summary>
        /// <param name="_val">Given value</param>
        /// <returns>True if represents a open bound</returns>
        static bool OpenBound(int _val)
        {
            return IsBound(_val) && GetBit(_val, 1) == 0;
        }

        /// <summary>
        /// Checks and returns ifa value represents a cell interior
        /// </summary>
        /// <param name="_val">Value to check</param>
        /// <returns>True if value represents a cell interior</returns>
        public static bool IsInterior(int _val)
        {
            return GetBit(_val, 0) == 0;
        }

        /// <summary>
        /// Changes a given value to reflect that the cell is ascending or not
        /// </summary>
        /// <param name="_val">Given value</param>
        /// <param name="_open">Represents if cell is ascending (1) or not (0)</param>
        /// <returns>Value reflecting cell interior</returns>
        static int SetAscending(int _val, bool _isAscending)
        {
            return IsInterior(_val) ? SetBitTo(_val, 1, _isAscending ? 1 : 0) : _val;
        }

        /// <summary>
        /// Changes a given value to reflect that the cell is descending or not
        /// </summary>
        /// <param name="_val">Given value</param>
        /// <param name="_open">Represents if cell is descending (1) or not (0)</param>
        /// <returns>Value reflecting cell interior</returns>
        static int SetDescending(int _val, bool _isDescending)
        {
            return IsInterior(_val) ? SetBitTo(_val, 2, _isDescending ? 1 : 0) : _val;
        }

        /// <summary>
        /// Checks if given value represents an ascending cell
        /// </summary>
        /// <param name="_val">Given value</param>
        /// <returns>True if represents an ascending cell</returns>
        static bool Ascending(int _val)
        {
            return IsInterior(_val) && GetBit(_val, 1) == 1;
        }

        /// <summary>
        /// Checks if given value represents a descending cell
        /// </summary>
        /// <param name="_val">Given value</param>
        /// <returns>True if represents a descending cell</returns>
        static bool Descending(int _val)
        {
            return IsInterior(_val) && GetBit(_val, 2) == 1;
        }

        /*/// <summary>
        /// Calculates and returns information about bounds from a value and other information
        /// Will return a 2D array of integers that contains n objects and four pieces of information for each object
        /// [cX, cY, w, d]
        /// cX - the x value on which the object is centered
        /// cY - the y value on which the object is centered
        /// w - the width (scale on x-axis) of the object
        /// d - the depth (scale on y-axis) of the object
        /// VALUES ENTERED AND RETURNED SHOULD BE IN TERMS OF PLAYER SCALE
        /// </summary>
        /// <param name="_viewCoor">The coordinates of the value in the viewable grid (as of right now should only be two)</param>
        /// <param name="_currCell">A current cell displayed in the veiwable grid</param>
        /// <param name="_currD">The current dimensions the viewable grid is showing</param>
        /// <param name="_val">The value</param>
        /// <param name="_cScale">The desired scale of the cell interior</param>
        /// <param name="_bScale">The desired scale of bounds</param>
        /// <param name="_oScale">The desired scale of opening</param>
        /// <returns>A 2D array with information about the bound represented by the given value, if value given does not represent a bound, an empty 2D array</returns>
        public static int[,] GetBoundInfo(int[] _viewCoor, int[] _currCell, int[] _currD, int _val, int _cScale, int _bScale, int _oScale)
        {
            //If not a bound value, return an empty array
            if (!IsBound(_val))
                return new int[0, 0];

            int[,] _bInfo;

            //Find and fill in information
            //Get upper left corner coordinate of cell in drawing
            int[] _cellULCoor = new int[2];
            _cellULCoor[X] = _bScale + ((int)Math.Floor((double)(_viewCoor[X] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE)) * (_bScale + _cScale));
            _cellULCoor[Y] = _bScale + ((int)Math.Floor((double)(_viewCoor[Y] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE)) * (_bScale + _cScale));

            //If closed bound, one set of information, else two sets of information
            if (ClosedBound(_val))
            {
                _bInfo = new int[1, 4];
                //Check if on X
                if (((int)Math.Abs(_viewCoor[X] - BOUND_SCALE) % (BOUND_SCALE + CELL_SCALE)) >= CELL_SCALE)
                {
                    _bInfo[0, X] = _cScale + (_bScale / 2) + _cellULCoor[X];
                    _bInfo[0, 2] = _bScale;
                }
                else
                {
                    _bInfo[0, X] = (_cScale / 2) + _cellULCoor[X];
                    _bInfo[0, 2] = _cScale;
                }
                //Check if on Y
                if (((int)Math.Abs(_viewCoor[Y] - BOUND_SCALE) % (BOUND_SCALE + CELL_SCALE)) >= CELL_SCALE)
                {
                    _bInfo[0, Y] = _cScale + (_bScale / 2) + _cellULCoor[Y];
                    _bInfo[0, 3] = _bScale;
                }
                else
                {
                    _bInfo[0, Y] = (_cScale / 2) + _cellULCoor[Y];
                    _bInfo[0, 3] = _cScale;
                }
            }
            else
            {
                _bInfo = new int[2, 4];
                //Get opening center
                int[] _centerCoor = FindOpeningCenter(X, true, _currD, _currCell);

                //Start and end points within cell for bounds
                int[,] _startEndPoints = new int[2, 2];
                _startEndPoints[0, 0] = 0;
                _startEndPoints[1, 1] = _cScale;

                //Must be on X or on Y, never a corner
                //If on X
                if (((int)Math.Abs(_viewCoor[X] - BOUND_SCALE) % (BOUND_SCALE + CELL_SCALE)) >= CELL_SCALE)
                {
                    _bInfo[0, X] = _cScale + (_bScale / 2) + _cellULCoor[X];
                    _bInfo[0, 2] = _bScale;
                    _bInfo[1, X] = _cScale + (_bScale / 2) + _cellULCoor[X];
                    _bInfo[1, 2] = _bScale;

                    _startEndPoints[0, 1] = _centerCoor[Y] - (_oScale / 2);
                    _startEndPoints[1, 0] = _centerCoor[Y] + (_oScale / 2);

                    _bInfo[0, Y] = ((_startEndPoints[0, 0] + _startEndPoints[0, 1]) / 2) + _cellULCoor[Y];
                    _bInfo[1, Y] = ((_startEndPoints[1, 0] + _startEndPoints[1, 1]) / 2) + _cellULCoor[Y];
                    _bInfo[0, 3] = (int)Math.Abs((double)_startEndPoints[0, 0] - _startEndPoints[0, 1]);
                    _bInfo[1, 3] = (int)Math.Abs((double)_startEndPoints[1, 0] - _startEndPoints[1, 1]);
                }
                else
                {
                    _bInfo[0, Y] = _cScale + (_bScale / 2) + _cellULCoor[Y];
                    _bInfo[0, 3] = _bScale;
                    _bInfo[1, Y] = _cScale + (_bScale / 2) + _cellULCoor[Y];
                    _bInfo[1, 3] = _bScale;

                    _startEndPoints[0, 1] = _centerCoor[X] - (_oScale / 2);
                    _startEndPoints[1, 0] = _centerCoor[X] + (_oScale / 2);

                    _bInfo[0, X] = ((_startEndPoints[0, 0] + _startEndPoints[0, 1]) / 2) + _cellULCoor[X];
                    _bInfo[1, X] = ((_startEndPoints[1, 0] + _startEndPoints[1, 1]) / 2) + _cellULCoor[X];
                    _bInfo[0, 2] = (int)Math.Abs((double)_startEndPoints[0, 0] - _startEndPoints[0, 1]);
                    _bInfo[1, 2] = (int)Math.Abs((double)_startEndPoints[1, 0] - _startEndPoints[1, 1]);
                }
            }

            //Return information
            return _bInfo;
        }*/

        /// <summary>
        /// Calculates and returns information about bounds from a value and other information
        /// Will return a 2D array of integers that contains n objects and four pieces of information for each object
        /// [cX, cY, w, d]
        /// cX - the x value on which the object is centered
        /// cY - the y value on which the object is centered
        /// w - the width (scale on x-axis) of the object
        /// d - the depth (scale on y-axis) of the object
        /// VALUES ENTERED AND RETURNED SHOULD BE IN TERMS OF PLAYER SCALE
        /// </summary>
        /// <param name="_viewCoor">The coordinates of the value in the viewable grid (as of right now should only be two)</param>
        /// <param name="_val">The value</param>
        /// <returns>A 2D array with information about the bound represented by the given value, if value given does not represent a bound, an empty 2D array</returns>
        public static int[,] GetBoundInfo(int[] _viewCoor, int _val)
        {
            //return GetBoundInfo(_viewCoor, GetCurrentCell(_viewCoor), World.CurrentDimensions, _val, World.InteriorScale, World.BoundScale, World.OpeningScale);

            //If not a bound value, return an empty array
            if (!IsBound(_val))
                return new int[0, 0];

            int[,] _bInfo;

            //Find and fill in information
            //Get upper left corner coordinate of cell in drawing
            int[] _cellULCoor = new int[2];
            _cellULCoor[X] = World.BoundScale + ((int)Math.Floor((double)(_viewCoor[X] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE)) * (World.BoundScale + World.InteriorScale));
            _cellULCoor[Y] = World.BoundScale + ((int)Math.Floor((double)(_viewCoor[Y] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE)) * (World.BoundScale + World.InteriorScale));

            //If closed bound, one set of information, else two sets of information
            if (ClosedBound(_val))
            {
                _bInfo = new int[1, 4];
                //Check if on X
                if (((int)Math.Abs(_viewCoor[X] - BOUND_SCALE) % (BOUND_SCALE + CELL_SCALE)) >= CELL_SCALE)
                {
                    _bInfo[0, X] = World.InteriorScale + (World.BoundScale / 2) + _cellULCoor[X];
                    _bInfo[0, 2] = World.BoundScale;
                }
                else
                {
                    _bInfo[0, X] = (World.InteriorScale / 2) + _cellULCoor[X];
                    _bInfo[0, 2] = World.InteriorScale;
                }
                //Check if on Y
                if (((int)Math.Abs(_viewCoor[Y] - BOUND_SCALE) % (BOUND_SCALE + CELL_SCALE)) >= CELL_SCALE)
                {
                    _bInfo[0, Y] = World.InteriorScale + (World.BoundScale / 2) + _cellULCoor[Y];
                    _bInfo[0, 3] = World.BoundScale;
                }
                else
                {
                    _bInfo[0, Y] = (World.InteriorScale / 2) + _cellULCoor[Y];
                    _bInfo[0, 3] = World.InteriorScale;
                }
            }
            else
            {
                _bInfo = new int[2, 4];
                //Get opening center
                int[] _centerCoor = FindOpeningCenter(X, true, World.CurrentDimensions, GetCurrentCell(_viewCoor));

                //Start and end points within cell for bounds
                int[,] _startEndPoints = new int[2, 2];
                _startEndPoints[0, 0] = 0;
                _startEndPoints[1, 1] = World.InteriorScale;

                //Must be on X or on Y, never a corner
                //If on X
                if (((int)Math.Abs(_viewCoor[X] - BOUND_SCALE) % (BOUND_SCALE + CELL_SCALE)) >= CELL_SCALE)
                {
                    _bInfo[0, X] = World.InteriorScale + (World.BoundScale / 2) + _cellULCoor[X];
                    _bInfo[0, 2] = World.BoundScale;
                    _bInfo[1, X] = World.InteriorScale + (World.BoundScale / 2) + _cellULCoor[X];
                    _bInfo[1, 2] = World.BoundScale;

                    _startEndPoints[0, 1] = _centerCoor[Y] - (World.OpeningScale / 2);
                    _startEndPoints[1, 0] = _centerCoor[Y] + (World.OpeningScale / 2);

                    _bInfo[0, Y] = ((_startEndPoints[0, 0] + _startEndPoints[0, 1]) / 2) + _cellULCoor[Y];
                    _bInfo[1, Y] = ((_startEndPoints[1, 0] + _startEndPoints[1, 1]) / 2) + _cellULCoor[Y];
                    _bInfo[0, 3] = (int)Math.Abs((double)_startEndPoints[0, 0] - _startEndPoints[0, 1]);
                    _bInfo[1, 3] = (int)Math.Abs((double)_startEndPoints[1, 0] - _startEndPoints[1, 1]);
                }
                else
                {
                    _bInfo[0, Y] = World.InteriorScale + (World.BoundScale / 2) + _cellULCoor[Y];
                    _bInfo[0, 3] = World.BoundScale;
                    _bInfo[1, Y] = World.InteriorScale + (World.BoundScale / 2) + _cellULCoor[Y];
                    _bInfo[1, 3] = World.BoundScale;

                    _startEndPoints[0, 1] = _centerCoor[X] - (World.OpeningScale / 2);
                    _startEndPoints[1, 0] = _centerCoor[X] + (World.OpeningScale / 2);

                    _bInfo[0, X] = ((_startEndPoints[0, 0] + _startEndPoints[0, 1]) / 2) + _cellULCoor[X];
                    _bInfo[1, X] = ((_startEndPoints[1, 0] + _startEndPoints[1, 1]) / 2) + _cellULCoor[X];
                    _bInfo[0, 2] = (int)Math.Abs((double)_startEndPoints[0, 0] - _startEndPoints[0, 1]);
                    _bInfo[1, 2] = (int)Math.Abs((double)_startEndPoints[1, 0] - _startEndPoints[1, 1]);
                }
            }

            //Return information
            return _bInfo;
        }

        /*/// <summary>
        /// Calculates and returns information about a cell interior from a value and other information
        /// Will return a 2D array of integers that contains n objects and four pieces of information for each object
        /// [cX, cY, w, d]
        /// cX - the x value on which the object is centered (-1 if object is not in interior)
        /// cY - the y value on which the object is centered (-1 if object is not in interior)
        /// w - the width (scale on x-axis) of the object (-1 if object is not in interior)
        /// d - the depth (scale on y-axis) of the object (-1 if object is not in interior)
        /// 0 - opening ascending
        /// 1 - opening descending
        /// VALUES ENTERED AND RETURNED SHOULD BE IN TERMS OF PLAYER SCALE
        /// </summary>
        /// <param name="_viewCoor">The coordinates of the value in the viewable grid (as of right now should only be two)</param>
        /// <param name="_currCell">A current cell displayed in the veiwable grid</param>
        /// <param name="_currD">The current dimensions the viewable grid is showing</param>
        /// <param name="_val">The value</param>
        /// <param name="_cScale">The desired scale of the cell interior</param>
        /// <param name="_bScale">The desired scale of bounds</param>
        /// <param name="_oScale">The desired scale of opening</param>
        /// <returns>A 2D array with information about the interior represented by the given value, if value given does not represent cell interior, an empty 2D array</returns>
        public static int[,] GetInteriorInfo(int[] _viewCoor, int[] _currCell, int[] _currD, int _val, int _cScale, int _bScale, int _oScale)
        {
            //If not a bound value, return an empty array
            if (!IsInterior(_val))
                return new int[0, 0];

            //Create iInfo
            int[,] _iInfo = new int[2, 4];
            for (int _i = 0; _i < _iInfo.GetLength(0); _i++)
                for (int _j = 0; _j < _iInfo.GetLength(1); _j++)
                    _iInfo[_i, _j] = -1;

            //Find and fill in information
            //Get upper left corner coordinate of cell in drawing
            int[] _cellULCoor = new int[2];
            _cellULCoor[X] = _bScale + ((int)Math.Floor((double)(_viewCoor[X] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE)) * (_bScale + _cScale));
            _cellULCoor[Y] = _bScale + ((int)Math.Floor((double)(_viewCoor[Y] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE)) * (_bScale + _cScale));
            //If ascending
            if (Ascending(_val))
            {
                int[] _openingC = FindOpeningCenter(Z, true, _currD, _currCell);
                _iInfo[0, X] = _openingC[X] + _cellULCoor[X];
                _iInfo[0, Y] = _openingC[Y] + _cellULCoor[Y];
                _iInfo[0, 2] = _oScale;
                _iInfo[0, 3] = _oScale;
            }
            //If descending
            if (Descending(_val))
            {
                int[] _openingC = FindOpeningCenter(Z, false, _currD, _currCell);
                _iInfo[1, X] = _openingC[X] + _cellULCoor[X];
                _iInfo[1, Y] = _openingC[Y] + _cellULCoor[Y];
                _iInfo[1, 2] = _oScale;
                _iInfo[1, 3] = _oScale;
            }

            if (((_iInfo[0, X] - _cellULCoor[X] < _oScale / 2 ||
                _iInfo[0, X] - _cellULCoor[X] > _cScale - (_oScale / 2)) &&
                 _iInfo[0, X] != -1) ||
                ((_iInfo[0, Y] - _cellULCoor[Y] < _oScale / 2 ||
                _iInfo[0, Y] - _cellULCoor[Y] > _cScale - (_oScale / 2)) &&
                 _iInfo[0, Y] != -1) ||
                ((_iInfo[1, X] - _cellULCoor[X] < _oScale / 2 ||
                _iInfo[1, X] - _cellULCoor[X] > _cScale - (_oScale / 2)) &&
                 _iInfo[1, X] != -1) ||
                ((_iInfo[1, Y] - _cellULCoor[Y] < _oScale / 2 ||
                _iInfo[1, Y] - _cellULCoor[Y] > _cScale - (_oScale / 2)) &&
                 _iInfo[1, Y] != -1))
                Console.ReadLine();

            //Return information
            return _iInfo;
        }*/

        /// <summary>
        /// Calculates and returns information about a cell interior from a value and other information
        /// Will return a 2D array of integers that contains n objects and four pieces of information for each object
        /// [cX, cY, w, d]
        /// cX - the x value on which the object is centered (-1 if object is not in interior)
        /// cY - the y value on which the object is centered (-1 if object is not in interior)
        /// w - the width (scale on x-axis) of the object (-1 if object is not in interior)
        /// d - the depth (scale on y-axis) of the object (-1 if object is not in interior)
        /// 0 - opening ascending
        /// 1 - opening descending
        /// VALUES ENTERED AND RETURNED SHOULD BE IN TERMS OF PLAYER SCALE
        /// </summary>
        /// <param name="_viewCoor">The coordinates of the value in the viewable grid (as of right now should only be two)</param>
        /// <param name="_val">The value</param>
        /// <returns>A 2D array with information about the interior represented by the given value, if value given does not represent cell interior, an empty 2D array</returns>
        public static int[,] GetInteriorInfo(int[] _viewCoor, int _val)
        {
            //return GetInteriorInfo(_viewCoor, GetCurrentCell(_viewCoor), World.CurrentDimensions, _val, World.InteriorScale, World.BoundScale, World.OpeningScale);

            //If not a bound value, return an empty array
            if (!IsInterior(_val))
                return new int[0, 0];

            //Create iInfo
            int[,] _iInfo = new int[2, 4];
            for (int _i = 0; _i < _iInfo.GetLength(0); _i++)
                for (int _j = 0; _j < _iInfo.GetLength(1); _j++)
                    _iInfo[_i, _j] = -1;

            //Find and fill in information
            //Get upper left corner coordinate of cell in drawing
            int[] _cellULCoor = new int[2];
            _cellULCoor[X] = World.BoundScale + ((int)Math.Floor((double)(_viewCoor[X] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE)) * (World.BoundScale + World.InteriorScale));
            _cellULCoor[Y] = World.BoundScale + ((int)Math.Floor((double)(_viewCoor[Y] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE)) * (World.BoundScale + World.InteriorScale));
            //If ascending
            if (Ascending(_val))
            {
                int[] _openingC = FindOpeningCenter(Z, true, World.CurrentDimensions, GetCurrentCell(_viewCoor));
                _iInfo[0, X] = _openingC[X] + _cellULCoor[X];
                _iInfo[0, Y] = _openingC[Y] + _cellULCoor[Y];
                _iInfo[0, 2] = World.OpeningScale;
                _iInfo[0, 3] = World.OpeningScale;
            }
            //If descending
            if (Descending(_val))
            {
                int[] _openingC = FindOpeningCenter(Z, false, World.CurrentDimensions, GetCurrentCell(_viewCoor));
                _iInfo[1, X] = _openingC[X] + _cellULCoor[X];
                _iInfo[1, Y] = _openingC[Y] + _cellULCoor[Y];
                _iInfo[1, 2] = World.OpeningScale;
                _iInfo[1, 3] = World.OpeningScale;
            }

            if (((_iInfo[0, X] - _cellULCoor[X] < World.OpeningScale / 2 ||
                _iInfo[0, X] - _cellULCoor[X] > World.InteriorScale - (World.OpeningScale / 2)) &&
                 _iInfo[0, X] != -1) ||
                ((_iInfo[0, Y] - _cellULCoor[Y] < World.OpeningScale / 2 ||
                _iInfo[0, Y] - _cellULCoor[Y] > World.InteriorScale - (World.OpeningScale / 2)) &&
                 _iInfo[0, Y] != -1) ||
                ((_iInfo[1, X] - _cellULCoor[X] < World.OpeningScale / 2 ||
                _iInfo[1, X] - _cellULCoor[X] > World.InteriorScale - (World.OpeningScale / 2)) &&
                 _iInfo[1, X] != -1) ||
                ((_iInfo[1, Y] - _cellULCoor[Y] < World.OpeningScale / 2 ||
                _iInfo[1, Y] - _cellULCoor[Y] > World.InteriorScale - (World.OpeningScale / 2)) &&
                 _iInfo[1, Y] != -1))
                Console.ReadLine();

            //Return information
            return _iInfo;
        }

        /// <summary>
        /// Gets the current cell based on viewable coordinates
        /// </summary>
        /// <param name="_viewCoor">Viewable coordinates [X, Y]</param>
        /// <returns>Coordinates of current cell in maze</returns>
        public static int[] GetCurrentCell(int[] _viewCoor)
        {
            int[] _currCell = new int[dimensions];
            Array.Copy(World.CenterCell, _currCell, dimensions);
            _currCell[World.DimensionX] = (int)Math.Floor((double)(_viewCoor[X] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE));
            _currCell[World.DimensionY] = (int)Math.Floor((double)(_viewCoor[Y] - BOUND_SCALE) / (BOUND_SCALE + CELL_SCALE));
            return _currCell;
        }

        /// <summary>
        /// Finds and returns the X and Y center points of an opening in a specified dimension in a cell
        /// </summary>
        /// <param name="_openingD">The X, Y, or Z dimension of the cell</param>
        /// <param name="_forwards">True if getting opening in forwards direction</param>
        /// <param name="_viewCoor">Coordinates in the viewable grid</param>
        /// <param name="_currD">Current dimensions</param>
        /// <param name="_currCell">The full coordinates of a currently viewable cell</param>
        /// <param name="_cScale">The scale of the cell interior</param>
        /// <param name="_bScale">The scale of bounds</param>
        /// <returns>The center point of an opening within its cell</returns>
        static int[] FindOpeningCenter(int _openingD, bool _forwards, int[] _currD, int[] _currCell)
        {
            //Creater center
            int[] _center = new int[2];

            //Get constants
            int[] _constants = new int[] { 0, 0 };
            for (int _i = 0; _i < dimensions; _i++)
            {
                if (_i < _currD[_openingD])
                    _constants[X] += _currCell[_i];
                if (_i > _currD[_openingD])
                    _constants[Y] += _currCell[_i];
            }
            
            //Get center points
            _center[X] = Randomize.OpeningCoor(_constants[X], _currCell[_currD[_openingD]] - (_forwards ? 0 : 1), 1, World.InteriorScale, World.OpeningScale);
            _center[Y] = Randomize.OpeningCoor(_constants[Y], _currCell[_currD[_openingD]] - (_forwards ? 0 : 1), 3, World.InteriorScale, World.OpeningScale);

            //Return center
            return _center;
        }

        /*/// <summary>
        /// Checks if the cell at the specified coordinates is viewable in the current 2D layer
        /// </summary>
        /// <param name="_toCheck">Cell to check</param>
        /// <param name="_currCell">Current cell in layer</param>
        /// <param name="_x">X dimension of layer</param>
        /// <param name="_y">Y dimension of layer</param>
        /// <returns>If cell is viewable in layer</returns>
        static bool IsViewable(int[] _toCheck, int[] _currCell, int _x, int _y)
        {
            for (int _d = 0; _d < dimensions; _d++)
                if (_d != _x && _d != _y && _toCheck[_d] != _currCell[_d])
                    return false;
            return true;
        }*/

        /// <summary>
        /// Gets a specified bit from a value
        /// </summary>
        /// <param name="_val">Value to get bit from</param>
        /// <param name="_n">Position of specified bit</param>
        /// <returns>Value of bit</returns>
        static int GetBit(int _val, int _n)
        {
            return ((_val >> _n) & 1);
        }

        /// <summary>
        /// Gets a specified bit from a value
        /// </summary>
        /// <param name="_val">Value to get bit from</param>
        /// <param name="_n">Position of specified bit</param>
        /// <returns>Value of bit</returns>
        static int GetBit(ushort _val, int _n)
        {
            return ((_val >> _n) & 1);
        }

        /// <summary>
        /// Flips a specified bit in a value
        /// </summary>
        /// <param name="_val">Value to flip bit in</param>
        /// <param name="_n">Position of bit to be flipped</param>
        /// <returns>New value with bit flipped</returns>
        static int FlipBit(int _val, int _n)
        {
            return _val + (GetBit(_val, _n) > 0 ? -(1 << _n) : (1 << _n));
        }

        /// <summary>
        /// Sets a specified bit in a value to specified value (1 or 0)
        /// </summary>
        /// <param name="_val">Value to set bit in</param>
        /// <param name="_n">Position of bit to be set</param>
        /// <param name="_bitVal">Value bit is to be set to</param>
        /// <returns></returns>
        static int SetBitTo(int _val, int _n, int _bitVal)
        {
            return (GetBit(_val, _n) == (_bitVal > 0 ? 1 : 0) ? _val : FlipBit(_val, _n));
        }

        /// <summary>
        /// An abstract class to define cells in the maze
        /// </summary>
        internal class Cell : IComparable<Cell>
        {
            internal Cell parent;
            internal List<Cell> childern;
            protected Cell[] neighbors;
            protected int[] coordinates;
            protected ushort bounds;

            /// <summary>
            /// Constructor
            /// Intializes variables, Gets a random type, and creates a new set for the cell
            /// </summary>
            /// <param name="_coor">Coordinates of cell in maze</param>
            internal Cell(int[] _coor)
            {
                childern = new List<Cell>();
                neighbors = new Cell[dimensions];
                neighbors.DefaultIfEmpty(null);
                coordinates = _coor;
                InitiateStorage();
                NewSet();
            }

            /// <summary>
            /// Initiates storage for cell data
            /// </summary>
            protected void InitiateStorage()
            {
                bounds = ushort.MaxValue;
            }

            /// <summary>
            /// Creates and returns a new instance of a Cell
            /// </summary>
            /// <param name="_coor">Coordinates of new instance</param>
            /// <returns>New instance of a class extending Cell</returns>
            protected Cell CreateNewCell(int[] _coor)
            {
                return new Cell(_coor);
            }

            /// <summary>
            /// Releases resources of cell
            /// Also removes cell from its set while not destroying its actual set.
            /// </summary>
            protected virtual void Close()
            {
                //Remove cell from set
                RemoveFromSet();
            }

            /// <summary>
            /// Get's the neighboring cell in the specified dimension
            /// Creates the cell if necessary
            /// Will expand maze in dimensions higher than specified dimension as well
            /// If specified dimension is 0, it is assumed the last dimension has been reached
            ///     and will write all previous cells to world arrays and close the cells.
            /// </summary> 
            /// <param name="_d">Dimension</param>
            /// <returns>Neighboring cell</returns>
            internal Cell GetNeighbor(int _d)
            {
                if (neighbors[_d] == null)
                {
                    //Get neighbor coordinates
                    int[] _coor = new int[dimensions];
                    coordinates.CopyTo(_coor, 0);
                    _coor[_d]++;

                    //Create neighbor
                    neighbors[_d] = CreateNewCell(_coor);

                    //Define bound for current cell in dimension
                    DefineBound(_d);

                    //Get neighboring cells of neighbor
                    //for (int _n = (_d + 1); _n < neighbors.Length; _n++)
                    for (int _n = neighbors.Length - 1; _n > _d; _n--)
                    {
                        if (HasNeighbor(_n))
                        {
                            //Get neighbors[_n].neighbors[_d] and set as neighbors[_d].neighbors[_n]
                            neighbors[_d].neighbors[_n] = neighbors[_n].GetNeighbor(_d);

                            //Define path in dimension _n for neighbors[_d]
                            neighbors[_d].DefineBound(_n);
                        }
                    }
                    //If _d is 0, last dimension expansion. Write current neighbor cells to bitWorld array in BitmapMaze.
                    //  Close written cells.
                    if (_d == 0)
                    {
                        //Write path information to bitWorld and close cell
                        WriteToMaze();
                    }
                }
                return neighbors[_d];
            }

            /// <summary>
            /// Writes the cell's path information to the actual maze arrays
            /// </summary>
            protected virtual void WriteToMaze()
            {
                worldBounds.SetValue(bounds, coordinates);
                //Close cell when done writing it
                Close();
            }

            /// <summary>
            /// For an entire set, recursivly writes cells' path information to the actual maze arrays
            /// Then closes the cells
            /// </summary>
            internal void WriteSetToMaze()
            {
                while (childern.Count > 0)
                {
                    childern.ElementAt(0).WriteSetToMaze();
                }
                WriteToMaze();
            }

            /// <summary>
            /// Sets the cell as the first cell of a new type
            /// </summary>
            protected void NewSet()
            {
                parent = null;
                if (!sets.Contains(this))
                {
                    sets.Add(this);
                }
            }

            /// <summary>
            /// Removes cell's parent
            /// </summary>
            internal void RemoveParent()
            {
                if (parent == null)
                {
                    return;
                }
                parent.childern.Remove(this);
                parent = null;
                if (!sets.Contains(this))
                {
                    sets.Add(this);
                }
            }

            /// <summary>
            /// Add a new child cell
            /// </summary>
            /// <param name="_c">New Child cell</param>
            protected void AddChild(Cell _c)
            {
                //Check to ensure cell does not parent itself
                if (this == _c)
                {
                    return;
                }
                sets.Remove(_c);
                childern.Add(_c);
                _c.parent = this;
            }

            /// <summary>
            /// Checks if cells are part of the same set
            /// </summary>
            /// <param name="_c">Cell to check</param>
            /// <returns>If cells are part of the same set</returns>
            protected bool SameSet(Cell _c)
            {
                return (GetFirstInSet() == _c.GetFirstInSet());
            }

            /// <summary>
            /// Checks if current cell and its neighbor in the specified dimension are part of the same set
            /// Returns false if cell has no neighbor in specifed dimension
            /// </summary>
            /// <param name="_d">Specified dimension</param>
            /// <returns>If current cell and its neighbor (if it exists) are part of the same set</returns>
            protected bool SameSet(int _d)
            {
                //Check if neighbor exsists
                if (neighbors[_d] == null)
                {
                    return false;
                }
                return (GetFirstInSet() == neighbors[_d].GetFirstInSet());
            }

            /// <summary>
            /// Adds the cell to a specified set
            /// </summary>
            /// <param name="_newSet">New set to be added to</param>
            protected void AddToSet(Cell _newSet)
            {
                //DEBUGGING printing name and paramters for method
                //PrintCellInfo();

                //Check if same set
                if (SameSet(_newSet))
                {
                    return;
                }

                //Create cells
                Cell _first, _new;

                //Check which set is bigger
                if (GetSetSize() > _newSet.GetSetSize())
                {
                    _first = _newSet.GetFirstInSet();
                    _new = GetFirstInSet();
                }
                else
                {
                    _first = GetFirstInSet();
                    _new = _newSet.GetFirstInSet();
                }

                //Console.WriteLine("Set {0} << Set {1}", sets.IndexOf(_new), sets.IndexOf(_first));

                //While _first is not part of new set, add cells from current set to new set.
                while (!_first.SameSet(_new))
                {
                    Cell _toAdd = _first.GetLastInSet();
                    _new.AddCellToSet(_toAdd);
                    _new.GetFirstInSet().SortSet();
                }

                //DEBUGGING
                //Maze.PrintCellInfo();
            }

            /// <summary>
            /// Adds a single cell to the current set if the cell does not already belong to the set
            /// </summary>
            /// <param name="_c"></param>
            protected void AddCellToSet(Cell _c)
            {
                //Check if same set
                if (SameSet(_c))
                {
                    return;
                }

                _c.RemoveFromSet();
                GetCellToAddChildTo().AddChild(_c);
            }

            /// <summary>
            /// Starting at the start of the set, gets the next cell in a set that can recieve a child cell
            /// </summary>
            /// <returns>The next cell that can recieve a child cell</returns>
            protected Cell GetCellToAddChildTo()
            {
                //Get the next child cell with room for childern
                Cell _current, _hold;
                _current = GetFirstInSet();
                _hold = GetFirstInSet();

                while (_current.childern.Count >= dimensions)
                {
                    List<Cell> _notFull = _current.childern.FindAll(x => (x.childern.Count < dimensions));
                    _notFull.Sort();
                    if (_notFull.Count == 0)
                    {
                        _current = _current.childern.Min();
                    }
                    else
                    {
                        _notFull.Sort();
                        _notFull.Sort((x, y) => -(x.childern.Count - y.childern.Count));
                        _current = _notFull.ElementAt(0);
                    }
                }
                return _current;
            }

            /// <summary>
            /// Starting at the start of the set, gets the cell with the least number of siblings and no childern
            /// </summary>
            /// <returns>The cell with the least number of sibling and no childern</returns>
            protected Cell GetLastInSet()
            {
                Cell _last;
                _last = GetFirstInSet();

                //Gets the cell with no childern and the least number of siblings
                while (_last.childern.Count > 0)
                {
                    //List<Cell> _notFull = _current.childern.FindAll(x => (x.childern.Count < dimensions && x.childern.Count > 0));
                    List<Cell> _notEmpty = _last.childern.FindAll(x => (x.childern.Count != 0));
                    List<Cell> _empty = _last.childern.FindAll(x => (x.childern.Count == 0));

                    //Console.WriteLine("Looping in GetLastinSet " + ToString() + " current: " + _current.ToString());
                    if (_notEmpty.Count != 0)
                    {
                        _notEmpty.Sort((x, y) => x.childern.Count - y.childern.Count);
                        _last = _notEmpty.ElementAt(0);
                    }
                    else
                    {
                        _last = _empty.ElementAt(0);
                    }

                }
                return _last;
            }

            /// <summary>
            /// Gets the size of a set
            /// </summary>
            /// <returns>The size of a set</returns>
            public int GetSetSize()
            {
                return GetSetSize(GetFirstInSet());
            }

            /// <summary>
            /// Gets the set size recursuvely
            /// </summary>
            /// <param name="_c">Cell currently looking at</param>
            /// <returns>Set size from given cell</returns>
            protected int GetSetSize(Cell _c)
            {
                int _size = 1;
                if (_c.childern.Count > 0)
                    foreach (Cell _child in _c.childern)
                        _size += GetSetSize(_child);
                return _size;
            }

            /// <summary>
            /// Removes self from parent or sets if has no childern
            /// </summary>
            protected void NullSet()
            {
                foreach (Cell _child in childern.ToArray())
                {
                    _child.NullParent();
                }
                childern.Clear();
                NullParent();
            }

            /// <summary>
            /// Removes self from parent without creating itself a new set
            /// </summary>
            protected void NullParent()
            {
                sets.Remove(this);
                if (parent == null)
                {
                    return;
                }
                parent.childern.Remove(this);
                parent = null;
            }

            /// <summary>
            /// Removes cell from its set
            /// </summary>
            protected void RemoveFromSet()
            {
                //If no parent or childern, simply remove cell from sets
                if ((parent == null) && (childern.Count == 0))
                {
                    if (sets.Contains(this))
                    {
                        sets.Remove(this);
                    }
                    return;
                }

                //Replace cell with last cell in set, removing current cell's childern and parent
                //Then sort set
                ReplaceWithLast().SortSet();
            }

            /// <summary>
            /// Max heapsorts the set starting from the current node
            /// </summary>
            /// <returns>True if current cell is switched</returns>
            protected bool SortSet()
            {

                //If no childern, return false
                if (childern.Count == 0)
                {
                    return false;
                }

                bool _change = false, _childChange = false;

                _change = SwitchWithMaxChild();

                for (int _c = 0; _c < childern.Count; _c++)
                {
                    _childChange |= childern.ElementAt(_c).SortSet();
                }

                if (_childChange)
                {
                    _change = SwitchWithMaxChild();
                }

                return _change;
            }

            /// <summary>
            /// Switches a cell in a set with its max child if its max child is larger than it.
            /// </summary>
            /// <returns>True if switch is made.</returns>
            protected bool SwitchWithMaxChild()
            {
                //Return if no childern
                if (childern.Count == 0)
                {
                    return false;
                }

                //Return if larger than max child
                if (this.CompareTo(childern.Max()) > 0)
                {
                    return false;
                }

                //Create variables to hold childern and parent
                Cell[] _maxChildChildern, _childern;
                Cell _parent;

                //Hold current childern and parent
                _childern = childern.ToArray<Cell>();
                _parent = parent;

                //Hold max child
                Cell _maxChild = childern.Max();
                //  Hold max childern
                _maxChildChildern = _maxChild.childern.ToArray<Cell>();

                //Null current cell's set
                NullSet();

                //  Null max child's set
                _maxChild.NullSet();

                //Switch childern
                foreach (Cell _child in _maxChildChildern)
                {
                    AddChild(_child);
                }
                foreach (Cell _child in _childern)
                {
                    _maxChild.AddChild(_child);
                }

                //Add current as child of max child
                _maxChild.AddChild(this);

                //Assign max child to old parent, if any
                if (_parent == null)
                {
                    sets.Add(_maxChild);
                }
                else
                {
                    _parent.AddChild(_maxChild);
                }

                //Return true if switch is made
                return true;
            }

            /// <summary>
            /// Removes and replaces the current cell with the last cell in its set
            /// </summary>
            /// <returns>Start of old set</returns>
            protected Cell ReplaceWithLast()
            {
                Cell _last;

                _last = GetLastInSet();

                //Get start of set
                Cell _start = _last.GetFirstInSet();

                //Null set of last cell
                _last.NullSet();

                //Check if current is last
                if (this == _last)
                {
                    return _start;
                }

                //Store current cell's childern and parent
                Cell _parent = parent;
                Cell[] _childern = childern.ToArray<Cell>();

                //Nulls current cell's set
                NullSet();

                //If stored parent is not null, add last cell as a child, otherwise, add last cell to sets.
                if (_parent != null)
                {
                    _parent.AddChild(_last);
                }
                else
                {
                    sets.Add(_last);
                }

                //Add stored childern to last cell
                foreach (Cell _child in _childern)
                {
                    _last.AddChild(_child);
                }

                //Returns start of old set
                return _last.GetFirstInSet();
            }

            /// <summary>
            /// Gets the first cell in a set
            /// </summary>
            /// <returns>The first cell in a set</returns>
            protected Cell GetFirstInSet()
            {
                Cell _first = this;
                while (_first.parent != null)
                {
                    _first = _first.parent;
                }
                return _first;
            }

            /// <summary>
            /// Compares cell to specified cell and returns which is greater based on cell coodinates
            /// </summary>
            /// <param name="_c">Specified cell to compare to</param>
            /// <returns>-1 if cell is lesser than specified cell; 1 if cell is greater than specified cell; 0 if cells are equal</returns>
            public int CompareTo(Cell _c)
            {
                //Check to sort by coordinates (Decreasing to starting point)
                for (int _i = 0; _i < dimensions; _i++)
                {
                    if (coordinates[_i] < _c.coordinates[_i])
                    {
                        return -1;
                    }
                    if (coordinates[_i] > _c.coordinates[_i])
                    {
                        return 1;
                    }
                }

                //UNREACHABLE, NO CELLS HAVE IDENTICAL COORDINATES UNLESS THEY ARE THE SAME
                //Check to sort by number of childern (Most to least number of childern)
                if (childern.Count < _c.childern.Count)
                {
                    return -1;
                }
                if (childern.Count > _c.childern.Count)
                {
                    return 1;
                }

                //Cells are equal
                return 0;
            }

            /// <summary>
            /// BFS of set until the set successful merges with another set
            /// </summary>
            internal void MergeSet()
            {
                Cell _current;
                Queue<Cell> _queue = new Queue<Cell>();
                _queue.Enqueue(this);
                while (_queue.Count != 0)
                {
                    _current = _queue.Dequeue();

                    //Randomly decide to try to merge sets from current cell
                    /*if (Randomize.RandBool())
                    {
                        if (_current.RandMerge())
                        {
                            break;
                        }
                    }*/
                    if (_current.RandMerge())
                    {
                        return;
                    }

                    //Add childern of current set to queue
                    foreach (Cell _child in _current.childern)
                    {
                        _queue.Enqueue(_child);
                    }
                }
            }

            /// <summary>
            /// If possible, randomly merges two neighboring sets
            /// </summary>
            /// <returns>If two sets were merged</returns>
            protected bool RandMerge()
            {
                //Get dimensions of neighbors not in same set
                int[] _notInSet = NeighborsNotInSet();
                int _d;

                //Check if any sets to merge, return if not
                if (_notInSet.Length == 0)
                    return false;

                //Pick a random dimension to merge in
                _d = _notInSet[Randomize.RandInt(_notInSet.GetUpperBound(0))];

                //Force open bound
                ForceOpenBound(_d);

                return true;
            }

            /// <summary>
            /// Checks if cell has a neighbor in the specified dimension
            /// </summary>
            /// <param name="_d">Specified dimension</param>
            /// <returns>True or flase if cell has neighbor</returns>
            protected bool HasNeighbor(int _d)
            {
                return (neighbors[_d] != null);
            }

            /// <summary>
            /// Returns int array of dimensions that has neighbors not in its set
            /// </summary>
            /// <returns>Int array</returns>
            protected int[] NeighborsNotInSet()
            {
                List<int> _notInSet = new List<int>();
                for (int _d = 0; _d < neighbors.Length; _d++)
                {
                    if (neighbors[_d] != null)
                        if (!SameSet(_d))
                                _notInSet.Add(_d);
                }
                return _notInSet.ToArray();
            }

            /// <summary>
            /// Adds neighboring cell in a specified dimension to the current cell's set should the neigher exist and is not already part of set
            /// </summary>
            /// <param name="_d">Specified dimension</param>
            protected void AddNeighborToSet(int _d)
            {
                //Check if neighbor exists
                if (!HasNeighbor(_d))
                {
                    return;
                }

                //Check if same set
                if (SameSet(neighbors[_d]))
                {
                    return;
                }

                //Add neighbor to set
                neighbors[_d].AddToSet(this);
            }

            /// <summary>
            /// Defines the bounds in a specified dimension of a cell
            /// </summary>
            /// <param name="_d">Specified dimension</param>
            protected void DefineBound(int _d)
            {
                //Find bit value
                //Randomize
                int _b = Randomize.RandInt(1);
                //If neighbor in set or end of dimension, close
                if (SameSet(_d) || EndOfDimension(_d))
                    _b = 1;
                //If must extend set, open
                if (MustExtendSet(_d))
                    _b = 0;

                //Define bounds
                bounds = (ushort)SetBitTo(bounds, _d, _b);

                //If open, add neighbor to set
                if (Open(_d))
                {
                    AddNeighborToSet(_d);
                }
            }

            /// <summary>
            /// Opens the bounds in a specified dimension of a cell
            /// </summary>
            /// <param name="_d">Specified dimension</param>
            protected void ForceOpenBound(int _d)
            {
                bounds = (ushort)SetBitTo(bounds, _d, 0);
                //If open, add neighbor to set
                if (Open(_d))
                {
                    AddNeighborToSet(_d);
                }
            }

            /// <summary>
            /// Returns if a bound in a specified dimension is open
            /// </summary>
            /// <param name="_d">Specified dimension</param>
            /// <returns>True if bound is open</returns>
            protected bool Open(int _d)
            {
                return GetBit(bounds, _d) == 0;
            }

            /// <summary>
            /// Checks if a set must extend in a specified dimension from the current cell
            /// </summary>
            /// <param name="_d">Specified dimension</param>
            /// <returns>True if set must extend</returns>
            protected bool MustExtendSet(int _d)
            {
                //If last dimension, set parent, and not end of dimension, must extend set
                return _d == 0 && parent == null && !EndOfDimension(_d);
            }

            /// <summary>
            /// Checks if cell is at end of dimension
            /// </summary>
            /// <param name="_d">Dimension</param>
            /// <returns>If cell is at end of dimension</returns>
            protected bool EndOfDimension(int _d)
            {
                return coordinates[_d] == dimensionInfo[_d] - 1;
            }



            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            //      DEBUGGING METHODS
            //////////////////////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Gets a string representation of the cell
            /// </summary>
            /// <returns>String representation of the cell</returns>
            public override String ToString()
            {
                String _s = "{";
                int _i = 0;
                while (_i < coordinates.Length - 1)
                {
                    _s += coordinates[_i] + ", ";
                    _i++;
                }
                _s += coordinates[_i] + "}";
                return _s;
            }

            /// <summary>
            /// Gets a string representation of the cell, its neighbors, parent, and childern
            /// </summary>
            /// <returns>String representation of the cell</returns>
            protected String InfoToString()
            {
                String _s = this + "\n   Neighbors: ";
                int _i;
                _i = 0;
                while (_i < neighbors.Length - 1)
                {
                    _s += (neighbors[_i] != null ? neighbors[_i].ToString() : "null") + ", ";
                    _i++;
                }
                _s += (neighbors[_i] != null ? neighbors[_i].ToString() : "null") + "\n   Parent: " +
                      (parent != null ? parent.ToString() : "null") + "\n   Childern: ";
                _i = 0;
                while (_i < childern.Count - 1)
                {
                    _s += childern.ElementAt(_i) + ", ";
                    _i++;
                }
                _s += childern.LastOrDefault();
                return _s;
            }

            /// <summary>
            /// Gets a string representation of the cell, and its childern recursively for the entire set
            /// </summary>
            /// <returns>String representation of the cell and its set</returns>
            public String SetToString()
            {
                Cell _first = GetFirstInSet();
                char?[,] _coorInfo = _first.SetToCharArray();
                String s = "";
                for (int _y = 0; _y < _coorInfo.GetLength(1); _y++)
                {
                    for (int _x = 0; _x < _coorInfo.GetLength(0); _x++)
                    {
                        if (_coorInfo[_x, _y].HasValue)
                        {
                            s += _coorInfo[_x, _y];
                        }
                        else
                        {
                            s += ' ';
                        }
                    }
                    s += '\n';
                }
                return s;
            }

            /// <summary>
            /// Starts at the first cell in the set and prints the set to the console
            /// </summary>
            internal void PrintSet()
            {
                Cell _first = GetFirstInSet();
                char?[,] _coorInfo = _first.SetToCharArray();
                for (int _y = 0; _y < _coorInfo.GetLength(1); _y++)
                {
                    for (int _x = 0; _x < _coorInfo.GetLength(0); _x++)
                    {
                        if (_coorInfo[_x, _y].HasValue)
                        {
                            Console.Write(_coorInfo[_x, _y]);
                        }
                        else
                        {
                            Console.Write(' ');
                        }
                    }
                    Console.WriteLine();
                }
            }

            /// <summary>
            /// Creates an array of characters the described a set
            /// </summary>
            /// <returns>An array of chars that describes a set</returns>
            private char?[,] SetToCharArray()
            {
                char?[,] _setArray;
                char[] _coorChar = ToString().ToCharArray();

                if (childern.Count == 0)
                {
                    _setArray = new char?[1, ((_coorChar.Length / 3) + 2)];
                    _setArray[0, 0] = '-';
                    _setArray[0, ((_coorChar.Length / 3) + 1)] = '-';
                    for (int _n = 0; _n < (_coorChar.Length / 3); _n++)
                    {
                        _setArray[0, (_n + 1)] = _coorChar[(1 + (_n * 3))];
                    }
                }
                else
                {
                    childern.Sort();
                    char?[][,] _childArrays = new char?[childern.Count][,];
                    int _childArrayX = 0, _childArrayY = 0;
                    for (int _c = 0; _c < childern.Count; _c++)
                    {
                        _childArrays[_c] = childern.ElementAt(_c).SetToCharArray();
                        _childArrayX++;
                        if (_childArrays[_c].GetLength(1) > _childArrayY)
                        {
                            _childArrayY = _childArrays[_c].GetLength(1);
                        }
                    }
                    _childArrayX = (int)Math.Pow(dimensions, ((_childArrayY + 1) / (dimensions + 3)));

                    int _setXbase, _setYbase, _setCenter, _childPlaceMod;
                    _setXbase = 0;
                    _setYbase = ((_coorChar.Length / 3) + 3);
                    _setCenter = (_childArrayX / 2);
                    _childPlaceMod = (_childArrayX - (childern.Count * (int)Math.Pow(dimensions, ((_childArrayY + 1) / (dimensions + 3)) - 1))) / 2;
                    _setArray = new char?[_childArrayX, _setYbase + _childArrayY];

                    for (int _n = 0; _n < (_coorChar.Length / 3); _n++)
                    {
                        _setArray[_setCenter, (_n + 1)] = _coorChar[(1 + (_n * 3))];
                    }
                    _setArray[_setCenter, 0] = '-';
                    _setArray[_setCenter, ((_coorChar.Length / 3) + 1)] = '-';
                    _setArray[_setCenter, ((_coorChar.Length / 3) + 2)] = 'v';

                    for (int _c = 0; _c < childern.Count; _c++)
                    {
                        _setXbase = (_c * (int)Math.Pow(dimensions, ((_childArrayY + 1) / (dimensions + 3)) - 1)) + _childPlaceMod;
                        for (int _x = 0; _x < _childArrays[_c].GetLength(0); _x++)
                        {
                            for (int _y = 0; _y < _childArrays[_c].GetLength(1); _y++)
                            {
                                _setArray[_setXbase + _x, _setYbase + _y] = _childArrays[_c][_x, _y];
                            }
                        }
                    }
                }

                return _setArray;
            }


            /*/// <summary>
            /// Counts cells in set that are in last part of last dimension
            /// </summary>
            /// <returns>Count of cells</returns>
            public int CountSetCellsinLast()
            {
                return CountSetCellsinLast(GetFirstInSet());
            }

            /// <summary>
            /// Counts cells in set that are in last part of last dimension
            /// </summary>
            /// <param name="_c">Cell currently looking at</param>
            /// <returns>Count of cells</returns>
            protected int CountSetCellsinLast(Cell _c)
            {
                int _count = 0;
                if (coordinates[0] == DimensionInfo[0] - 1)
                    _count++;
                if (_c.childern.Count > 0)
                    foreach (Cell _child in _c.childern)
                        _count += GetSetSize(_child);
                return _count;
            }*/


            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            //      UNUSED METHODS
            //////////////////////////////////////////////////////////////////////////////////////////////////////////

            /* /// <summary>
             /// Closes the bounds in a specified dimension of a cell
             /// </summary>
             /// <param name="_d">Specified dimension</param>
             protected void ForceCloseBound(int _d)
             {
                 bounds = (ushort)SetBitTo(bounds, _d, 1);
                 //If not open, remove neighbor from set
                 if (!Open(_d))
                 {
                     RemoveNeighborToSet(_d);
                 }
             }*/


            /*/// <summary>
            /// Rebalances the max heap sorted set of cells recursively
            /// For after a set is edited.
            /// </summary>
            protected void RebalanceSet()
            {
                //Return if cell has no chilern
                if (childern.Count == 0)
                {
                    return;
                }

                //If cell has less childern than it should
                if (childern.Count < dimensions)
                {
                    Cell _current = childern.Max();

                    while ((childern.Count < dimensions) && (_current.childern.Count != 0))
                    {
                        Cell _holdChild = _current.childern.Max();
                        _holdChild.RemoveParent();
                        AddChild(_holdChild);
                    }
                    _current.RebalanceSet();
                }

                //If cell has more childern than it should
                if (childern.Count > dimensions)
                {
                    //List<Cell> _hold = new List<Cell>();

                    while (childern.Count > dimensions)
                    {
                        Cell _holdChild = childern.Min();
                        _holdChild.RemoveParent();
                        childern.Min().AddChild(_holdChild);
                    }
                }
            }*/


            /*/// <summary>
            /// Removes a neighboring cell in a specified dimension from the current cell's set should the neigher exist and is part of set
            /// </summary>
            /// <param name="_d">Specified dimension</param>
            protected void RemoveNeighborFromSet(int _d)
            {
                //Check if neighbor exists
                if (!HasNeighbor(_d))
                {
                    return;
                }

                //Check if same set
                if (!SameSet(neighbors[_d]))
                {
                    return;
                }

                //Add neighbor to set
                neighbors[_d].RemoveFromSet();
            }*/


        }
    }
}
