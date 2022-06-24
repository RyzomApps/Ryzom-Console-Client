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
using API;

namespace Client.Sheet
{
    public class SheetIdFactory
    {
        private readonly RyzomClient _client;

        // Use 24 bits id and 8 bits file types
        private const int NlSheetIdIdBits = 24;
        private const int NlSheetIdTypeBits = 32 - NlSheetIdIdBits;

        private bool _initialised;

        bool _removeUnknownSheet = true;
        bool _dontHaveSheetKnowledge = false;

        private string[] _fileExtensions;
        private Dictionary<uint, string> _sheetIdToName;
        private Dictionary<string, uint> _sheetNameToId;

        /// <summary>
        /// constructor
        /// </summary>
        public SheetIdFactory(RyzomClient ryzomClient)
        {
            _client = ryzomClient;
        }

        /// <summary>
        /// create an SheetId from a numeric reference
        /// </summary>
        public SheetId SheetId(uint sheetRef)
        {
            return new SheetId { _id = sheetRef };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SheetId SheetId(string sheetName)
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
        /// Load the association sheet ref / sheet name
        /// </summary>
        internal void Init(bool removeUnknownSheet, string fileName)
        {
            // allow multiple calls to init in case libraries depending on sheetid call this init from their own
            if (_initialised)
            {
                if (_dontHaveSheetKnowledge)
                    _client.GetLogger().Warn("SHEETID: SheetIdManager is already initialized without sheet_id.bin");

                return;
            }

            //	CFile::addFileChangeCallback ("sheet_id.bin", cbFileChange);

            _removeUnknownSheet = removeUnknownSheet;

            LoadSheetIds(fileName);

            _initialised = true;
        }

        /// <summary>
        /// Load sheet_id.bin file
        /// </summary>
        private void LoadSheetIds(string path)
        {
            //H_AUTO(CSheetIdInit);
            _client.GetLogger().Info($"Loading sheet IDs from {path}");

            // Open the sheet id to sheet file name association

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                var fileBytes = File.ReadAllBytes(path);

                //fileBytes = fileBytes.Reverse().ToArray();

                // clear entries
                _fileExtensions = new string[0];
                _sheetIdToName = new Dictionary<uint, string>();
                _sheetNameToId = new Dictionary<string, uint>();

                // reserve space for the vector of file extensions
                Array.Resize(ref _fileExtensions, 1 << NlSheetIdTypeBits);

                // Get the map from the file
                var tempMap = new Dictionary<uint, string>();

                // workaround file.SerialCont(tempMap);
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
                // workaround end

                if (_removeUnknownSheet)
                {
                    //    uint removednbfiles = 0;
                    //    uint nbfiles = (uint)tempMap.Count;
                    //
                    //    // now we remove all files that not available
                    //    SortedDictionary<uint, string>.Enumerator itStr2;
                    //    for (itStr2 = tempMap.GetEnumerator(); itStr2.MoveNext();)
                    //    {
                    //        if (Path.exists(itStr2.Current.Value))
                    //        {
                    //        }
                    //        else
                    //        {
                    //            SortedDictionary<uint, string>.Enumerator olditStr = itStr2;
                    //            //_client.GetLogger().Debug ("Removing file '%s' from CSheetId because the file not exists", (*olditStr).second.c_str ());
                    //            tempMap.Remove(olditStr);
                    //            removednbfiles++;
                    //        }
                    //    }
                    //
                    //    _client.GetLogger().Info("SHEETID: Removed %d files on %d from CSheetId because these files don't exist", removednbfiles, nbfiles);
                }

                {
                    // Convert the map to one big string and 1 static map (id to name)
                    // Get the number and size of all strings
                    //var tempVec = new List<string>(); // Used to initialise the first map
                    //var nSize = 0;
                    //
                    //var nNb = 0;
                    //foreach (var it in tempMap)
                    //{
                    //    nSize += it.Value.Length + 1;
                    //    nNb++;
                    //}

                    // Make the big string (composed of all strings) and a vector referencing each string
                    //tempVec.Resize(nNb);

                    //_AllStrings.Ptr = new char[nSize];
                    //it = tempMap.GetEnumerator();
                    //nSize = 0;
                    //nNb = 0;
                    //while (it.MoveNext())
                    //{
                    //    tempVec[nNb].Ptr = _AllStrings.Ptr + nSize;
                    //    _AllStrings.Ptr + nSize = it.Current.Value.c_str();
                    //    toLowerAscii(_AllStrings.Ptr + nSize);
                    //    nSize += (uint)it.Current.Value.Length + 1;
                    //    nNb++;
                    //}

                    // Finally build the static map (id to name)
                    //_sheetIdToName.reserve(tempVec.Count);

                    var nNb = 0;
                    foreach (var it2 in tempMap)
                    {
                        _sheetIdToName.Add(it2.Key, it2.Value/*tempVec[nNb]*/);

                        nNb++;
                    }

                    // The vector of all small string is not needed anymore we have all the info in
                    // the static map and with the pointer AllStrings referencing the beginning.
                }

                {
                    // Build the invert map (Name to Id) & file extension vector
                    var nSize = _sheetIdToName.Count;
                    _sheetNameToId = new Dictionary<string, uint>(nSize);

                    foreach (var itStr in _sheetIdToName)
                    {
                        // add entry to the inverse map
                        _sheetNameToId.Add(itStr.Value, itStr.Key);

                        // work out the type value for this entry in the map
                        //var sheetId = new SheetId(this) {Id = itStr.Key};

                        //uint type = sheetId.IdInfos.Type;
                        //
                        //// check whether we need to add an entry to the file extensions vector
                        //if (_fileExtensions[type].empty())
                        //{
                        //    // find the file extension part of the given file name
                        //    _fileExtensions[type] = toLowerAscii(CFile.getExtension(itStr.second.Ptr));
                        //}
                    }
                }
            }
            else
            {
                _client.GetLogger().Error("SheetIdManager: Can't open the file sheet_id.bin");
            }

            _client.GetLogger().Debug($"Finished loading sheet_id.bin: {_sheetIdToName.Count} entries read");
        }

        /// <summary>
        /// build from a SubSheetId and a type
        /// </summary>
        internal SheetId BuildSheetId(string sheetName)
        {
            var ret = new SheetId();

            Debug.Assert(_initialised);

            //// When no sheet_id.bin is loaded, use dynamically assigned IDs.
            //if (_dontHaveSheetKnowledge)
            //{
            //    string sheetNameLc = sheetName.ToLower();
            //
            //    SortedDictionary<string, uint>.Enumerator it = _DevSheetNameToId.find(sheetNameLc);
            //    if (it == _DevSheetNameToId.end())
            //    {
            //        // Create a new dynamic sheet ID.
            //        // nldebug("SHEETID: Creating a dynamic sheet id for '%s'", sheetName.c_str());
            //
            //        string sheetType = Path.GetExtension(sheetNameLc);
            //        sheetName = Path.GetFileNameWithoutExtension(sheetNameLc);
            //
            //        SortedDictionary<string, uint>.Enumerator tit = _DevTypeNameToId.find(sheetType);
            //
            //        uint typeId;
            //
            //        if (tit == _DevTypeNameToId.end())
            //        {
            //            _FileExtensions.push_back(sheetType);
            //            _DevSheetIdToName.push_back(new List<string>());
            //            typeId = (uint)_FileExtensions.size() - 1;
            //            _DevTypeNameToId[sheetType] = typeId;
            //            string unknownNewType = "unknown." + sheetType;
            //            _DevSheetIdToName[typeId].push_back(unknownNewType);
            //            _Id.IdInfos.Type = typeId;
            //            _Id.IdInfos.Id = _DevSheetIdToName[typeId].size() - 1;
            //            _DevSheetNameToId[unknownNewType] = _Id.Id;
            //            if (sheetName == "unknown")
            //            {
            //                return ret; // Return with the unknown sheet id of this type
            //            }
            //        }
            //        else
            //        {
            //            ret._type = tit.Value;
            //        }
            //
            //        // Add a new sheet name to the type
            //        _DevSheetIdToName[typeId].Add(sheetNameLc);
            //        _Id.IdInfos.Id = _DevSheetIdToName[typeId].size() - 1;
            //        // nldebug("SHEETID: Type %i, id %i, sheetid %i", _Id.IdInfos.Type, _Id.IdInfos.Id, _Id.Id);
            //        _DevSheetNameToId[sheetNameLc] = _Id.Id;
            //        return ret;
            //    }
            //
            //    ret._id = it.Value;
            //    return ret;
            //}

            // try looking up the sheet name in _SheetNameToId
            if (_sheetNameToId.ContainsKey(sheetName.ToLower()))
            {
                ret._id = _sheetNameToId[sheetName];

                // store debug info
                //_DebugSheetName = sheetName;

                return ret;
            }

            // failed to find the sheet name in the sheetname map so see if the string is numeric
            if (sheetName[0] == '#' && sheetName.Length > 1)
            {
                if (uint.TryParse(sheetName[1..], out var numericId))
                {
                    ret._id = numericId;
                    return ret;
                }
            }

            return null;
        }
    }
}
