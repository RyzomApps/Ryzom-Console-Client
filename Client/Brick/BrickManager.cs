///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Client.Sheet;

namespace Client.Brick
{
    /// <summary>
    /// Manager of Sabrina Bricks.
    /// </summary>
    /// <author>Lionel Berenguier</author>
    /// <author>Nevrax France</author>
    /// <date>2003</date>
    public class BrickManager(RyzomClient ryzomClient)
    {
        private readonly RyzomClient _ryzomClient = ryzomClient;
        private BrickSheet[] _brickVector;
        private readonly List<List<SheetId>> _sheetsByFamilies = [];
        private readonly List<int> _nbBricksPerFamily = [];

        public void Init()
        {
            // Read the Bricks from the SheetMngr.
            var brickSheets = _ryzomClient.GetSheetManager().GetSheets();

            // Clear existing bricks and preallocate space for performance
            _brickVector = new BrickSheet[16 * 1024];

            foreach (var brickSheet in brickSheets)
            {
                if (brickSheet.Value.Sheet is not BrickSheet)
                    continue;

                var shid = brickSheet.Key.GetShortId();

                // Resize the array if needed
                if (shid >= _brickVector.Length)
                {
                    Array.Resize(ref _brickVector, (int)shid + 1); // Resize the array
                }

                _brickVector[(int)shid] = (BrickSheet)brickSheet.Value.Sheet; // Add the brick sheet to the vector
            }

            // Initialize _sheetsByFamilies and _nbBricksPerFamily based on _brickVector
            foreach (var brickSheet in _brickVector)
            {
                if ((int)brickSheet.BrickFamily >= _sheetsByFamilies.Count)
                {
                    // Expand if necessary
                    for (var i = _sheetsByFamilies.Count; i <= (int)brickSheet.BrickFamily; i++)
                    {
                        _sheetsByFamilies.Add([]);
                        _nbBricksPerFamily.Add(0);
                    }
                }

                var indexInFamily = brickSheet.IndexInFamily - 1; // Assuming index in family starts at 1

                if (indexInFamily >= _nbBricksPerFamily[(int)brickSheet.BrickFamily])
                {
                    for (var j = _nbBricksPerFamily[(int)brickSheet.BrickFamily]; j <= indexInFamily; j++)
                    {
                        _sheetsByFamilies[(int)brickSheet.BrickFamily].Add(new SheetId(_ryzomClient.GetSheetIdFactory()));
                    }
                    _nbBricksPerFamily[(int)brickSheet.BrickFamily] = indexInFamily + 1; // Set new count
                }

                _sheetsByFamilies[(int)brickSheet.BrickFamily][indexInFamily] = brickSheet.Id;
            }
        }

        /// <summary>
        /// Initialize by loading bricks done at init time
        /// </summary>
        public void InitInGame()
        {
            // Load known bricks from the server or database
            // This would be the equivalent of fetching known bricks
            // and setting up any necessary observers.
        }

        /// <summary>
        /// Init the rest done at initInGame time
        /// </summary>
        public void InitTitles()
        {
            // Initialization logic for titles
        }

        /// <summary>
        /// Get a brick from its sheetId
        /// </summary>
        /// <param name="id">The id of the sheet</param>
        public BrickSheet GetBrick(SheetId id)
        {
            var shid = id.GetShortId();
            return shid < _brickVector.Length ? _brickVector[(int)shid] : null;
        }

        public SheetId GetBrickSheet(uint family, uint index)
        {
            if (family >= _sheetsByFamilies.Count || index >= _nbBricksPerFamily[(int)family])
                return new SheetId(_ryzomClient.GetSheetIdFactory(), 0);

            return _sheetsByFamilies[(int)family][(int)index];
        }

        public void FilterKnownBricks(List<SheetId> bricks)
        {
            var knownBricks = new List<SheetId>();
            foreach (var brick in bricks)
            {
                if (IsBrickKnown(brick))
                {
                    knownBricks.Add(brick);
                }
            }
            bricks.Clear();
            bricks.AddRange(knownBricks);
        }

        private bool IsBrickKnown(SheetId id)
        {
            var brick = GetBrick(id);
            return brick != null && IsBrickKnown(brick);
        }

        private bool IsBrickKnown(BrickSheet brick)
        {
            // Implement logic to determine if a brick is known.
            // This could involve checking against a known brick database or list.
            return false;
        }
    }
}