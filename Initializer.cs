using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDMazeGeneration
{
    /// <summary>
    /// An enum defining generation options
    /// </summary>
    enum GenOption
    {
        predefined = 0, userDefined = 1, maxCellsDNum = 2, defineDNum = 3, maxCells = 4
    }

    /// <summary>
    /// An enum defining cell scale options
    /// </summary>
    enum CellOption
    {
        predefined = 0, userDefined = 1
    }

    /// <summary>
    /// A static class to get option selections from users
    /// </summary>
    static class Initializer
    {
        static readonly ConsoleColor HighlightFG = ConsoleColor.White;
        static readonly ConsoleColor HighlightBG = ConsoleColor.Green;
        static readonly int OptionLinesDisplayed = 5;
        static readonly int DefaultMaxCells = 1000;
        static readonly int DefaultMinCells = 2;
        static readonly int DefaultMinDSize = 2;
        static readonly int MinDCount = 3;
        static readonly int MaxDCount = 16;
        static readonly int DefaultInteriorScale = 15;
        static readonly int DefaultBoundScale = 1;
        static readonly int DefaultOpeningScale = 5;

        /// <summary>
        /// Strings for generation options
        /// </summary>
        static readonly string[] GenOptionString = new string[] { "Default generation",
                                                                  "User defined dimension count and sizes",
                                                                  "Random sizes with user defined dimension count and maximum number of cells",
                                                                  "Random sizes with user defined dimension count",
                                                                  "Random dimension count and sizes with user defined maximum number of cells"};
        /// <summary>
        /// Strings for cell scale options
        /// </summary>
        static readonly string[] CellOptionString = new string[] { "Default cells",
                                                                   "User defined cells"};
        static int GenOptionCount { get { return Enum.GetNames(typeof(GenOption)).Length; } }
        static int CellOptionCount { get { return Enum.GetNames(typeof(CellOption)).Length; } }

        /// <summary>
        /// Initializes Maze and World
        /// </summary>
        public static void SetupMaze()
        {
            int interiorScale = -1;
            int boundScale = -1;
            int openingScale = -1;
            int[] dInfo;
            GenOption genOption = GetGenOption();
            CellOption cellOption = GetCellOption();

            //Get input
            Console.WriteLine("Please input:");

            switch (genOption)
            {
                case GenOption.userDefined:
                    dInfo = DSizeAndCountByInput();
                    break;
                case GenOption.defineDNum:
                    dInfo = InputDCount();
                    break;
                case GenOption.maxCellsDNum:
                    dInfo = InputMaxCellsAndCount();
                    break;
                case GenOption.maxCells:
                    dInfo = InputMaxCellsAndGenCount();
                    break;
                default:
                    dInfo = GenDCountAndSizes();
                    break;
            }

            if (cellOption == CellOption.predefined)
            {
                interiorScale = DefaultInteriorScale;
                boundScale = DefaultBoundScale;
                openingScale = DefaultOpeningScale;
                goto Initialize;
            }

            //Get interior scale
            while (interiorScale < 2)
            {
                try
                {
                    Console.Write(" Cell Interior Scale: ");
                    interiorScale = Convert.ToInt32(Console.ReadLine());
                    if (interiorScale < 2)
                    {
                        Console.WriteLine("Please input an integer greater than 1.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please input an integer greater than 1.");
                }
            }
            //Get bound scale
            while (boundScale <= 0)
            {
                try
                {
                    Console.Write(" Cell Bound Scale: ");
                    boundScale = Convert.ToInt32(Console.ReadLine());
                    if (boundScale <= 0)
                    {
                        Console.WriteLine("Please input a positive numerical value.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please input a positive numerical value.");
                }
            }
            //Get opening scale
            while (openingScale <= 0)
            {
                try
                {
                    Console.Write(" Cell Opening Scale ");
                    openingScale = Convert.ToInt32(Console.ReadLine());
                    if (openingScale <= 0)
                    {
                        Console.WriteLine("Please input a positive numerical value.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please input a positive numerical value.");
                }
            }

            Initialize:
            //Initialize maze, or world, or both, depending
            World.Initialize(dInfo, interiorScale, boundScale, openingScale);

            //Wait to start
            Console.WriteLine("\nPress [enter] to begin.\n");
            while (ConsoleKey.Enter != Console.ReadKey().Key)
            {
                //Spinning while input not [enter]
            }
        }

        /// <summary>
        /// Gets count of dimensions from input and creates array to store dimension information
        /// </summary>
        /// <returns>Array to store dimension information</returns>
        static int[] GetDCount()
        {
            int _dCount = 0;
            //Get number of dimensions
            while (_dCount < 3 || _dCount > 16)
            {
                try
                {
                    Console.Write(" Number of dimensions (3 - 16): ");
                    _dCount = Convert.ToInt32(Console.ReadLine());
                    if (_dCount < 3 || _dCount > 16)
                    {
                        Console.WriteLine("Please input an integer from 3 to 16.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please input an integer from 3 to 16.");
                }
            }

            //Return dimension information array
            return new int[_dCount];
        }

        /// <summary>
        /// Generates count of dimensions from a maximum number of cells and creates array to store dimension information
        /// </summary>
        /// <param name="_maxCells">Maximum number of cells</param>
        /// <returns>Array to store dimension information</returns>
        static int[] GenerateDCount(int _maxCells)
        {
            int _dCount = 0;

            //Get number of dimensions
            int _maxD = (int)Math.Floor(Math.Log(_maxCells, DefaultMinDSize));
            _maxD = _maxD > 16 ? 16 : (_maxD < 3 ? 3 : _maxD);
            _dCount = Randomize.RandInt(MinDCount, _maxD);

            //Return dimension information array
            return new int[_dCount];
        }

        /// <summary>
        /// Gets number of dimensions and dimension sizes through player input
        /// </summary>
        /// <returns>Dimension information</returns>
        static int[] DSizeAndCountByInput()
        {
            //Get _dInfo
            int[] _dInfo = GetDCount();

            //Tracks count of cells
            int _cellCount = 1;

            //Use input to set every value in _dInfo to 1 or more
            for (int _d = 0; _d < _dInfo.Length; _d++)
            {
                while (_dInfo[_d] < 1 || (_d == _dInfo.Length - 1 && _cellCount < 2 && _dInfo[_d] == 1))
                {
                    try
                    {
                        Console.Write(" Size of dimension {0}: ", _d);
                        _dInfo[_d] = Convert.ToInt32(Console.ReadLine());
                        if (_dInfo[_d] < 1)
                        {
                            Console.WriteLine("Please input an integer greater or equal to 1.");
                        }

                        //Check to see if last dimension and that cells in maze will be more than 1
                        if (_d == _dInfo.Length - 1 && _cellCount < 2 && _dInfo[_d] == 1)
                        {
                            Console.WriteLine("Error: There will only be one cell generated. \n Please input an integer greater than 1.");
                        }
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Please input an integer that is greater or equal to 1.");
                    }
                }
                //Update _cellCount
                _cellCount *= _dInfo[_d];
            }

            //Return _dInfo
            return _dInfo;
        }

        /// <summary>
        /// Gets dimension sizes through a player input maximum cell value and number of dimensions
        /// </summary>
        /// <returns>Dimension information</returns>
        static int[] InputMaxCellsAndCount()
        {
            //Get _dInfo
            int[] _dInfo = GetDCount();

            //Tracks count of cells
            int _cellCount = 1;
            int _cellsLeft;

            //Initializing _maxCells
            int _maxCells = 0;
            int _maxSize;
            int _minSize = 1;

            //Get maximum cell count
            while (_maxCells < 2)
            {
                try
                {
                    Console.Write(" Maximum number of cells: ");
                    _maxCells = Convert.ToInt32(Console.ReadLine());
                    if (_maxCells < 2)
                    {
                        Console.WriteLine("Please input an integer greater than 1.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please input an integer greater than 1.");
                }
            }

            //Initilize cells left
            _cellsLeft = (_maxCells / _cellCount);

            //Initialize _maxSize
            _maxSize = (int)Math.Ceiling(Math.Pow(_maxCells, (double)1 / _dInfo.Length));

            //Generate size of each dimension based on maximum number of cells
            for (int _d = 0; _d < _dInfo.Length; _d++)
            {

                _dInfo[_d] = Randomize.RandInt(_cellsLeft < _minSize ? _cellsLeft : _minSize, _cellsLeft < _maxSize ? _cellsLeft : _maxSize);
                _cellCount *= _dInfo[_d];
                _cellsLeft = (int)Math.Floor((double)_maxCells / _cellCount);
            }

            //Return _dInfo
            return _dInfo;
        }

        /// <summary>
        /// Gets dimension count and sizes through a player input maximum cell value
        /// </summary>
        /// <returns>Dimension information</returns>
        static int[] InputMaxCellsAndGenCount()
        {
            //Tracks count of cells
            int _cellCount = 1;
            int _cellsLeft;

            //Initializing _maxCells
            int _maxCells = 0;
            int _maxSize;
            int _minSize = 1;

            //Get maximum cell count
            while (_maxCells < 2)
            {
                try
                {
                    Console.Write(" Maximum number of cells: ");
                    _maxCells = Convert.ToInt32(Console.ReadLine());
                    if (_maxCells < 2)
                    {
                        Console.WriteLine("Please input an integer greater than 1.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please input an integer greater than 1.");
                }
            }

            //Get _dInfo
            int[] _dInfo = GenerateDCount(_maxCells);

            //Initilize cells left
            _cellsLeft = (_maxCells / _cellCount);

            //Initialize _maxSize
            _maxSize = (int)Math.Ceiling(Math.Pow(_maxCells, (double)1 / _dInfo.Length));

            //Generate size of each dimension based on maximum number of cells
            for (int _d = 0; _d < _dInfo.Length; _d++)
            {

                _dInfo[_d] = Randomize.RandInt(_cellsLeft < _minSize ? _cellsLeft : _minSize, _cellsLeft < _maxSize ? _cellsLeft : _maxSize);
                _cellCount *= _dInfo[_d];
                _cellsLeft = (int)Math.Floor((double)_maxCells / _cellCount);
            }

            //Return _dInfo
            return _dInfo;
        }

        /// <summary>
        /// Gets dimension sizes through a player input dimension count and the default maximum number of cells
        /// </summary>
        /// <returns>Dimension information</returns>
        static int[] InputDCount()
        {
            //Get _dInfo
            int[] _dInfo = GetDCount();

            //Tracks count of cells
            int _cellCount = 1;
            int _cellsLeft;

            //Initializing _maxCells
            int _maxSize;
            int _minSize = 1;

            //Initilize cells left
            _cellsLeft = (DefaultMaxCells / _cellCount);

            //Initialize _maxSize
            _maxSize = (int)Math.Ceiling(Math.Pow(DefaultMaxCells, (double)1 / _dInfo.Length));

            //Generate size of each dimension based on maximum number of cells
            for (int _d = 0; _d < _dInfo.Length; _d++)
            {

                _dInfo[_d] = Randomize.RandInt(_cellsLeft < _minSize ? _cellsLeft : _minSize, _cellsLeft < _maxSize ? _cellsLeft : _maxSize);
                _cellCount *= _dInfo[_d];
                _cellsLeft = (int)Math.Floor((double)DefaultMaxCells / _cellCount);
            }

            //Return _dInfo
            return _dInfo;
        }

        /// <summary>
        /// Generates dimension sizes using a generated dimension count and the default maximum number of cells
        /// </summary>
        /// <returns>Dimension information</returns>
        static int[] GenDCountAndSizes()
        {
            //Get _dInfo
            int[] _dInfo = GenerateDCount(DefaultMaxCells);

            //Tracks count of cells
            int _cellCount = 1;
            int _cellsLeft;

            //Initializing _maxCells
            int _maxSize;
            int _minSize = 1;

            //Initilize cells left
            _cellsLeft = (DefaultMaxCells / _cellCount);

            //Initialize _maxSize
            _maxSize = (int)Math.Ceiling(Math.Pow(DefaultMaxCells, (double)1 / _dInfo.Length));

            //Generate size of each dimension based on maximum number of cells
            for (int _d = 0; _d < _dInfo.Length; _d++)
            {

                _dInfo[_d] = Randomize.RandInt(_cellsLeft < _minSize ? _cellsLeft : _minSize, _cellsLeft < _maxSize ? _cellsLeft : _maxSize);
                _cellCount *= _dInfo[_d];
                _cellsLeft = (int)Math.Floor((double)DefaultMaxCells / _cellCount);
            }

            //Return _dInfo
            return _dInfo;
        }

        /// <summary>
        /// Gets generation option from user
        /// </summary>
        /// <returns>Generation option</returns>
        public static GenOption GetGenOption()
        {
            GenOption _selected = GenOption.predefined;

            Console.Clear();
            Console.ResetColor();

            /*Console.SetCursorPosition(0, 0);
            Console.Write("Choose generation method:");
            Console.SetCursorPosition(0, OptionLinesDisplayed + 2);
            Console.Write("Use arrow keys to select option. [Enter] to confirm selection.");*/

            while (true)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.Write("Choose generation method:");
                Console.SetCursorPosition(0, OptionLinesDisplayed + 2);
                Console.Write("Use arrow keys to select option. [Enter] to confirm selection.");

                //Write Options
                int _start = (GenOptionCount - 1) - (int)_selected < OptionLinesDisplayed ? ((GenOptionCount - 1) - OptionLinesDisplayed < 0 ? 0 : (GenOptionCount - 1) - OptionLinesDisplayed) : (int)_selected;
                int _end = (GenOptionCount - 1) - (int)_selected > OptionLinesDisplayed ? (int)_selected + OptionLinesDisplayed : (GenOptionCount - 1);

                int _lineCount = 0;

                for (int _i = (int)_selected; _i < ((int)_selected + OptionLinesDisplayed < GenOptionCount ? (int)_selected + OptionLinesDisplayed : GenOptionCount); _i++)
                {
                    if (_i == (int)_selected)
                    {
                        Console.ForegroundColor = HighlightFG;
                        Console.BackgroundColor = HighlightBG;
                    }
                    else
                    {
                        Console.ResetColor();
                    }

                    Console.SetCursorPosition(0, 1 + _i - (int)_selected);
                    Console.Write("    {0}", GenOptionString[_i]);
                    _lineCount++;
                    Console.ResetColor();
                    if (_lineCount >= OptionLinesDisplayed)
                        goto Input;

                    /*for (int _j = 0; _j <= GenOptionString[_i].Length / (Console.BufferWidth - 6); _j++)
                    {
                        Console.SetCursorPosition(0, 1 + _i + _j - (int)_selected);
                        Console.Write("    {0}", GenOptionString[_i].Substring(_j * (Console.BufferWidth - 6), (Console.BufferWidth - 6)));
                        _lineCount++;
                        if (_lineCount >= OptionLinesDisplayed)
                            goto Input;
                    }*/
                }

            Input:
                //Get input
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Enter:
                        goto Done;
                    case ConsoleKey.UpArrow:
                        if ((int)_selected != 0)
                            _selected = (GenOption)((int)_selected - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        if ((int)_selected != GenOptionCount - 1)
                            _selected = (GenOption)((int)_selected + 1);
                        break;
                }
            }

            Done:
            Console.Clear();
            Console.ResetColor();
            return _selected;
        }

        /// <summary>
        /// Gets cell scale option from user
        /// </summary>
        /// <returns>Cell scale option</returns>
        public static CellOption GetCellOption()
        {
            CellOption _selected = CellOption.predefined;

            Console.Clear();
            Console.ResetColor();

            /*Console.SetCursorPosition(0, 0);
            Console.Write("Choose cell scaling method:");
            Console.SetCursorPosition(0, OptionLinesDisplayed + 2);
            Console.Write("Use arrow keys to select option. [Enter] to confirm selection.");*/

            while (true)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.Write("Choose cell scaling method:");
                Console.SetCursorPosition(0, OptionLinesDisplayed + 2);
                Console.Write("Use arrow keys to select option. [Enter] to confirm selection.");

                //Write Options
                int _start = (CellOptionCount - 1) - (int)_selected < OptionLinesDisplayed ? ((CellOptionCount - 1) - OptionLinesDisplayed < 0 ? 0 : (CellOptionCount - 1) - OptionLinesDisplayed) : (int)_selected;
                int _end = (CellOptionCount - 1) - (int)_selected > OptionLinesDisplayed ? (int)_selected + OptionLinesDisplayed : (CellOptionCount - 1);

                int _lineCount = 0;

                for (int _i = (int)_selected; _i < ((int)_selected + OptionLinesDisplayed < CellOptionCount ? (int)_selected + OptionLinesDisplayed : CellOptionCount); _i++)
                {
                    if (_i == (int)_selected)
                    {
                        Console.ForegroundColor = HighlightFG;
                        Console.BackgroundColor = HighlightBG;
                    }
                    else
                    {
                        Console.ResetColor();
                    }

                    Console.SetCursorPosition(0, 1 + _i - (int)_selected);
                    Console.Write("    {0}", CellOptionString[_i]);
                    _lineCount++;
                    Console.ResetColor();
                    if (_lineCount >= OptionLinesDisplayed)
                        goto Input;

                    /*for (int _j = 0; _j <= CellOptionString[_i].Length / (Console.BufferWidth - 6); _j++)
                    {
                        Console.SetCursorPosition(0, 1 + _i + _j - (int)_selected);
                        Console.Write("    {0}", CellOptionString[_i].Substring(_j * (Console.BufferWidth - 6), (Console.BufferWidth - 6)));
                        _lineCount++;
                        if (_lineCount >= OptionLinesDisplayed)
                            goto Input;
                    }*/
                }

            Input:
                //Get input
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Enter:
                        goto Done;
                    case ConsoleKey.UpArrow:
                        if ((int)_selected != 0)
                            _selected = (CellOption)((int)_selected - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        if ((int)_selected != CellOptionCount - 1)
                            _selected = (CellOption)((int)_selected + 1);
                        break;
                }
            }

            Done:
            Console.Clear();
            Console.ResetColor();
            return _selected;
        }

    }
}
