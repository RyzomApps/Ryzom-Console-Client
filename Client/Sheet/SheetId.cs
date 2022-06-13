///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Diagnostics;
using API;
using Client.Network;

namespace Client.Sheet
{
    /// <summary>
    /// SheetId
    /// </summary>
    /// <author>Stephane Coutelas</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    /// <remarks>
    /// This class is case unsensitive. It means that you can call build() and 
    /// buildIdVector() with string with anycase, it'll work.
    /// </remarks>
    public class SheetId
    {
        static bool _initialised = false;
        static bool _removeUnknownSheet = true;
        static bool _dontHaveSheetKnowledge = false;

        public static SheetId Unknown = new SheetId(0);

        private uint _id;

        /// <summary>
        /// Constructor
        /// </summary>
        public SheetId()
        {

        }

        /// <summary>
        /// create an SheetId from a numeric reference
        /// </summary>
        public SheetId(uint sheetRef)
        {
            _id = sheetRef;
        }

        /// <summary>
        /// Constructor
        /// </summary>

        public SheetId(in string sheetName)
        {
            if (!BuildSheetId(sheetName, sheetName.Length))
            {
                if (string.IsNullOrEmpty(sheetName))
                {
                    //_client.GetLogger().Warn("SHEETID: Try to create an CSheetId with empty name. TODO: check why.");
                }
                else
                {
                    //_client.GetLogger().Warn("SHEETID: The sheet '%s' is not in sheet_id.bin, setting it to Unknown", sheetName);
                }

                //TODO: this = Unknown;
            }
        }

        private bool BuildSheetId(in string sheetName, in int sheetNameLength)
        {
            return false;
        }

        /// <summary>
        /// Return the **whole** sheet id (id+type)
        /// </summary>
        public uint AsInt()
        {
            return _id;
        }

        /// <summary>
        /// Load the association sheet ref / sheet name
        /// </summary>
        internal static void Init(bool removeUnknownSheet, IClient _client)
        {
            // allow multiple calls to init in case libraries depending on sheetid call this init from their own
            if (_initialised)
            {
                if (_dontHaveSheetKnowledge)
                    _client.GetLogger().Info("SHEETID: CSheetId is already initialized without sheet_id.bin");

                return;
            }

            //	CFile::addFileChangeCallback ("sheet_id.bin", cbFileChange);

            _removeUnknownSheet = removeUnknownSheet;

            LoadSheetId();
            _initialised = true;
        }

        /// <summary>
        /// Load sheet_id.bin file
        /// </summary>
        private static void LoadSheetId()
        {

        }

        /// <summary>
        /// Serial
        /// </summary>
        public void Serial(BitMemoryStream f)
        {
            Debug.Assert(!_dontHaveSheetKnowledge);
    
            f.Serial(ref _id);
        }
    }
}