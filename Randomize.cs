using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDMazeGeneration
{
    /// <summary>
    /// Class to generate random values
    /// </summary>
    public static class Randomize
    {
        private static Random r = new Random();

        /// <summary>
        /// Generates a random non-negative integer
        /// </summary>
        /// <returns>A 32-bit signed integer greater than or equal to zero and less than MaxValue</returns> 
        public static int RandInt()
        {
            return r.Next();
        }

        /// <summary>
        /// Generates a random non-negative integer, does not return specified integers
        /// </summary>
        /// <param name="_not">Integers not to return</param>
        /// <returns>A 32-bit signed integer greater than or equal to zero, less than MaxValue, and is not included in _not</returns>
        public static int RandInt(int[] _not)
        {
            //Loops for return value to ensure value is returnable
            int _return;
            do
            {
                _return = r.Next();
            } while (_not.Contains(_return));
            return _return;
        }

        /// <summary>
        /// Generates a random non-negative integer with an inclusive upper bound
        /// </summary>
        /// <param name="_ceiling">The inclusive upper bound of the integer to be generated</param>
        /// <returns>A 32-bit signed integer greater than or equal to zero and less than or equal to _ceiling</returns>
        public static int RandInt(int _ceiling)
        {
            return r.Next(_ceiling + 1);
        }

        /// <summary>
        /// Generates a random non-negative integer with an inclusive upper bound, does not return specified integers
        /// Returns 0 if all numbers in range are specified as non-returnable
        /// </summary>
        /// <param name="_ceiling">The inclusive upper bound of the integer to be generated</param>
        /// <param name="_not">Integers not to return</param>
        /// <returns>A 32-bit signed integer greater than or equal to zero, less than or equal to _ceiling, and is not included in _not</returns>
        public static int RandInt(int _ceiling, int[] _not)
        {
            // Check to ensure that infinite loop does not occur
            bool allContained = true;
            for (int i = 0; i <= _ceiling; i++)
            {
                allContained = _not.Contains(i);
                if (!allContained)
                {
                    break;
                }
            }
            if (allContained)
            {
                return 0;
            }
            //Loops for return value to ensure value is returnable
            int _return;
            do
            {
                _return = r.Next(_ceiling + 1);
            } while (_not.Contains(_return));
            return _return;
        }

        /// <summary>
        /// Generates a random non-negative integer with inclusive upper and lower bounds
        /// </summary>
        /// <param name="_floor">The inclusive lower bound of the integer to be generated</param>
        /// <param name="_ceiling">The inclusive upper bound of the integer to be generated</param>
        /// <returns>A 32-bit signed integer greater than or equal to _floor and less than or equal to _ceiling</returns>
        public static int RandInt(int _floor, int _ceiling)
        {
            return r.Next(_floor, _ceiling + 1);
        }

        /// <summary>
        /// Generates a random non-negative integer with inclusive upper and lower bounds, does not return specified integers
        /// Returns 0 if all numbers in range are specified as non-returnable
        /// </summary>
        /// <param name="_floor">The inclusive lower bound of the integer to be generated</param>
        /// <param name="_ceiling">The inclusive upper bound of the integer to be generated</param>
        /// <param name="_not">Integers not to return<</param>
        /// <returns>A 32-bit signed integer greater than or equal to _floor, less than or equal to _ceiling, and is not included in _not</returns>
        public static int RandInt(int _floor, int _ceiling, int[] _not)
        {
            // Check to ensure that infinite loop does not occur
            bool allContained = true;
            for (int i = _floor; i <= _ceiling; i++)
            {
                allContained = _not.Contains(i);
                if (!allContained)
                {
                    break;
                }
            }
            if (allContained)
            {
                return 0;
            }
            //Loops for return value to ensure value is returnable
            int _return;
            do
            {
                _return = r.Next(_floor, (_ceiling + 1));
            } while (_not.Contains(_return));
            return _return;
        }

        /// <summary>
        /// Generates a random boolean value with a 1 in 2 chance of getting true
        /// </summary>
        /// <returns>True or False</returns>
        public static bool RandBool()
        {
            return ((r.Next(2) % 2) == 0);
        }

        /// <summary>
        /// Generates a random boolean value with a specified chance (1 in _chanceMod) of getting true
        /// </summary>
        /// <param name="_chanceMod">Chance modifier</param>
        /// <returns>True or False</returns>
        public static bool RandBool(int _chanceMod)
        {
            //Avoid dividing by zero
            if (_chanceMod == 0)
            {
                return true;
            }
            return ((r.Next(_chanceMod) % _chanceMod) == 0);
        }

        /// <summary>
        /// Generates a cell coordinate value for an opening that borders on an outside wall given dimensions and dimension sizes
        /// </summary>
        /// <param name="_dInfo">Dimensions and dimension sizes</param>
        /// <returns>Opening coordinate</returns>
        public static int[] RandOpening(int[] _dInfo)
        {
            int[] _openingCoor = new int[_dInfo.Length];
            int _outsideEdges = 0;
            bool _allCoorDefined = false;
            int _d = 0;

            //While coordiantes are undefined or no outside edges
            while (!_allCoorDefined || _outsideEdges == 0)
            {
                //Randomize cooridinate values
                _openingCoor[_d] = RandInt(_dInfo[_d] - 1);
                //If outside edge increment _outside edges
                if (_openingCoor[_d] == 0 || _openingCoor[_d] == _dInfo[_d] - 1)
                    _outsideEdges++;

                //Increment _d and keep _d from exceeding _dInfo.Length
                _d = (_d + 1) % _dInfo.Length;

                //If _d is 0, all coordinates have been defined, set _allCoorDefined to true
                if (_d == 0)
                    _allCoorDefined = true;
            }

            //return coordinate values for opening
            return _openingCoor;
        }

        /// <summary>
        /// Generates a cell coordinate value for an opening that borders on an outside wall given dimensions and dimension sizes
        /// Avoids placing new opening on outside walls shared with given coordinates of previously existing opening
        /// </summary>
        /// <param name="_dInfo">Dimensions and dimension sizes</param>
        /// <param name="_iOpening">Previously existing opening</param>
        /// <returns>Opening coordinates</returns>
        public static int[] RandOpening(int[] _dInfo, int[] _iOpening)
        {
            int[] _openingCoor = new int[_dInfo.Length];
            int _outsideEdges = 0;
            bool _allCoorDefined = false;
            int _d = 0;

            //While coordiantes are undefined or no outside edges
            while (!_allCoorDefined || _outsideEdges == 0)
            {
                //Randomize cooridinate values
                //Avoid placing opening on same outside bounds as _iOpening
                _openingCoor[_d] = _iOpening[_d] == 0 ? RandInt(1, _dInfo[_d] - 1) : (_iOpening[_d] == _dInfo[_d] - 1 ? RandInt(_dInfo[_d] - 2) : RandInt(_dInfo[_d] - 1));
                //If outside edge increment _outside edges
                if (_openingCoor[_d] == 0 || _openingCoor[_d] == _dInfo[_d] - 1)
                    _outsideEdges++;

                //Increment _d and keep _d from exceeding _dInfo.Length
                _d = (_d + 1) % _dInfo.Length;

                //If _d is 0, all coordinates have been defined, set _allCoorDefined to true
                if (_d == 0)
                    _allCoorDefined = true;
            }

            //return coordinate values for opening
            return _openingCoor;
        }

        /// <summary>
        /// Generates a 'random' center point coordinate in a cell based on given values
        /// </summary>
        /// <param name="_constant">Given constant from cell</param>
        /// <param name="_var">Variable</param>
        /// <param name="_modI">Mod</param>
        /// <param name="_iScale">Interior cell scale</param>
        /// <param name="_oScale">Opening scale, or the width of whatever the center point is being generated for</param>
        /// <returns>'Random' center point coordinate in cell</returns>
        public static int OpeningCoor(int _constant, int _var, int _modI, int _iScale, int _oScale)
        {
            //modI is 1 for X, 3 for Y

            //Get section value
            int _section;
            _section = (_var - _modI) % 4 == 0 ? 0 : Math.Abs((_var - _modI) % 8) > 4 ? (((Math.Abs(_var - _modI) % 8) + 1) % 2) + 1 : (((Math.Abs(_var - _modI) % 8) % 2) + 1);

            //Get center coordinate

            //Get scale of possible centers
            //Section: 0 - both     1 - first half      2 - second half
            int _scale = (_iScale / (_section == 0 ? 1 : 2)) - _oScale + 1;

            //Get amount to add back
            int _addBack = (int)Math.Ceiling((double)((_iScale / 2) * (_section == 0 ? 0 : _section - 1)) + (_oScale / 2));

            //Get 'random' value within scale
            int _random = (int)Math.Abs(Math.Floor((double)(((_constant * _var) + _var) % _scale)));

            //Add _addBack to _random to get _coor
            int _coor = _random + _addBack;

            //return coordinate
            return _coor;
        }
    }
}
