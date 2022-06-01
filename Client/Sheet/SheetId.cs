///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API;

namespace Client.Sheet
{
    // TODO: make SheetId non static
    public class SheetId
    {
        static bool _Initialised = false;
        static bool _RemoveUnknownSheet = true;
        static bool _DontHaveSheetKnowledge = false;

        public static SheetId Unknown = new SheetId(0);

        private uint _id;

        public SheetId()
        {

        }

        public SheetId(uint sheetRef)
        {
            _id = sheetRef;
        }

        /// <summary>Return the **whole** sheet id (id+type)</summary>
        public uint AsInt()
        {
            return _id;
        }

        internal static void init(bool removeUnknownSheet, IClient _client)
        {
            // allow multiple calls to init in case libraries depending on sheetid call this init from their own
            if (_Initialised)
            {
                if (_DontHaveSheetKnowledge)
                    _client.GetLogger().Info("SHEETID: CSheetId is already initialized without sheet_id.bin");

                return;
            }

            //	CFile::addFileChangeCallback ("sheet_id.bin", cbFileChange);

            _RemoveUnknownSheet = removeUnknownSheet;

            loadSheetId();
            _Initialised = true;
        }

        private static void loadSheetId()
        {

        }
    }
}