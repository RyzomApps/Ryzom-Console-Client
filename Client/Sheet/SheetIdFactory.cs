///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using API.Sheet;

namespace Client.Sheet
{
    public class SheetIdFactory : ISheetIdFactory
    {
        private readonly RyzomClient _client;

        // Use 24 bits id and 8 bits file types
        private const int NlSheetIdIdBits = 24;
        private const int NlSheetIdTypeBits = 32 - NlSheetIdIdBits;

        private bool _initialised;

        private bool _removeUnknownSheet = true;
        private bool _dontHaveSheetKnowledge = false;

        private string[] _fileExtensions;
        private Dictionary<uint, string> _sheetIdToName;
        private Dictionary<string, uint> _sheetNameToId;

        public SheetId Unknown;

        /// <summary>
        /// constructor
        /// </summary>
        public SheetIdFactory(RyzomClient ryzomClient)
        {
            Unknown ??= new SheetId(this);

            _client = ryzomClient;
        }

        /// <summary>
        /// create an SheetId from a numeric reference
        /// </summary>
        public ISheetId SheetId(uint sheetRef)
        {
            return new SheetId(this) { _id = sheetRef };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ISheetId SheetId(string sheetName)
        {
            var ret = BuildSheetId(sheetName);

            if (ret == null)
            {
                if (string.IsNullOrEmpty(sheetName))
                {
                    _client.GetLogger().Warn("SHEETID: Try to create an CSheetId with empty name. TODO: check why.");
                }
                else
                {
                    _client.GetLogger().Warn($"SHEETID: The sheet '{sheetName}' is not in sheet_id.bin, setting it to Unknown");
                }

                //TODO: this = Unknown;
            }

            return ret;
        }

        /// <summary>
        /// Convert between file extensions and numeric sheet types
        /// note: fileExtension *not* include the '.' eg "bla" and *not* ".bla"
        /// </summary>
        internal uint TypeFromFileExtension(string fileExtension)
        {
            if (!_initialised)
            {
                Init(false);
            }

            uint i;
            for (i = 0; i < _fileExtensions.Length; i++)
            {
                if (fileExtension.ToLower() == _fileExtensions[i])
                {
                    return i;
                }
            }

            return uint.MaxValue;

        }

        /// <summary>
        /// Return the sheet id as a string
        /// If the sheet id is not found, then:
        /// - if 'ifNotFoundUseNumericId==false' the returned string is "<Sheet %d not found in sheet_id.bin>" with the id in %d
        /// - if 'ifNotFoundUseNumericId==tue'   the returned string is "#%u" with the id in %u
        /// </summary>
        internal string ToString(SheetId sheetId, bool ifNotFoundUseNumericId)
        {
            if (!_initialised)
                Init(false);

            if (_dontHaveSheetKnowledge)
                throw new NotImplementedException();

            if (_sheetIdToName.ContainsKey(sheetId.Id))
                return _sheetIdToName[sheetId.Id];

            return ifNotFoundUseNumericId ? $"#{sheetId.Id}" : $"<Sheet {sheetId.Id} not found in sheet_id.bin>";
        }

        /// <summary>
        /// Load the association sheet ref / sheet name
        /// </summary>
        internal void Init(bool removeUnknownSheet)
        {
            // allow multiple calls to initialize in case libraries depending on SheetId call this initialization from their own
            if (_initialised)
            {
                if (_dontHaveSheetKnowledge)
                    _client.GetLogger().Warn("SHEETID: SheetIdManager is already initialized without sheet_id.bin");

                return;
            }

            //	CFile::addFileChangeCallback ("sheet_id.bin", cbFileChange);

            _removeUnknownSheet = removeUnknownSheet;

            LoadSheetIds();

            _initialised = true;
        }

        /// <summary>
        /// Load sheet_id.bin file
        /// </summary>
        private void LoadSheetIds()
        {
            var path = Constants.SheetsIdBinPath;

            _client.GetLogger().Info($"Loading sheet IDs from {path}");

            // Open the sheet id to sheet file name association
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                var fileBytes = File.ReadAllBytes(path);

                // clear entries
                _fileExtensions = new string[0];
                _sheetIdToName = new Dictionary<uint, string>();
                _sheetNameToId = new Dictionary<string, uint>();

                // reserve space for the vector of file extensions
                Array.Resize(ref _fileExtensions, 1 << NlSheetIdTypeBits);

                // Get the map from the file
                var tempMap = new Dictionary<uint, string>();

                #region workaround file.SerialCont(tempMap);
                var filePointer = 0;

                var tmp = new byte[4];
                Array.Copy(fileBytes, filePointer, tmp, 0, tmp.Length);
                var len = BitConverter.ToInt32(tmp);
                filePointer += 4;

                for (var i = 0; i < len; i++)
                {
                    tmp = new byte[4];
                    Array.Copy(fileBytes, filePointer, tmp, 0, tmp.Length);
                    var k = BitConverter.ToUInt32(tmp);
                    filePointer += 4;

                    tmp = new byte[4];
                    Array.Copy(fileBytes, filePointer, tmp, 0, tmp.Length);
                    var contLen = BitConverter.ToInt32(tmp);
                    filePointer += 4;

                    tmp = new byte[contLen];
                    Array.Copy(fileBytes, filePointer, tmp, 0, tmp.Length);
                    var cont = System.Text.Encoding.UTF8.GetString(tmp);
                    filePointer += contLen;

                    tempMap.Add(k, cont);
                }
                #endregion workaround end

                if (_removeUnknownSheet)
                {
                    uint removednbfiles = 0;
                    var nbfiles = (uint)tempMap.Count;

                    // now we remove all files that not available
                    var toRemove = new List<uint>();

                    foreach (var (key, value) in tempMap)
                    {
                        if (!File.Exists(value))
                            toRemove.Add(key);
                    }

                    foreach (var olditStr in toRemove)
                    {
                        //_client.GetLogger().Debug ("Removing file '%s' from CSheetId because the file not exists", (*olditStr).second.c_str ());
                        tempMap.Remove(olditStr);
                        removednbfiles++;
                    }

                    _client.GetLogger().Debug($"SHEETID: Removed {removednbfiles} files on {nbfiles} from CSheetId because these files don't exist");
                }

                // Build the static map (id to name)
                foreach (var (key, value) in tempMap)
                {
                    _sheetIdToName.Add(key, value);
                }

                // Build the invert map (Name to Id) & file extension vector
                var nSize = _sheetIdToName.Count;
                _sheetNameToId = new Dictionary<string, uint>(nSize);

                foreach (var (key, value) in _sheetIdToName)
                {
                    // add entry to the inverse map
                    _sheetNameToId.Add(value, key);

                    // work out the type value for this entry in the map
                    var sheetId = SheetId(key);

                    var type = sheetId.Type;

                    // check whether we need to add an entry to the file extensions vector
                    if (type < _fileExtensions.Length && _fileExtensions[type] != null)
                    {
                        // find the file extension part of the given file name
                        _fileExtensions[type] = Path.GetExtension(value)?[1..];
                    }
                }
            }
            else
            {
                _client.GetLogger().Error($"SheetIdManager: Can't open the file {path}");
            }

            _client.GetLogger().Debug($"Finished loading sheet_id.bin: {_sheetIdToName.Count} entries read");
        }

        /// <summary>
        /// build from a SubSheetId and a type
        /// </summary>
        internal SheetId BuildSheetId(string sheetName)
        {
            var ret = new SheetId(this);

            Debug.Assert(_initialised);

            // When no sheet_id.bin is loaded, use dynamically assigned IDs.
            if (_dontHaveSheetKnowledge)
            {
                throw new NotImplementedException();
            }

            // try looking up the sheet name in _SheetNameToId
            if (_sheetNameToId.ContainsKey(sheetName.ToLower()))
            {
                ret._id = _sheetNameToId[sheetName];

                // store debug info
                //_DebugSheetName = sheetName;

                return ret;
            }

            // failed to find the sheet name in the sheet name map so see if the string is numeric
            if (sheetName[0] != '#' || sheetName.Length <= 1)
                return null;

            if (!uint.TryParse(sheetName[1..], out var numericId))
                return null;

            ret._id = numericId;

            return ret;
        }
    }
}
