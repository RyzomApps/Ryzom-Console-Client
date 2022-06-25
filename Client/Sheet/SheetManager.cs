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
using Client.Stream;

namespace Client.Sheet
{
    public class TypeVersion
    {
        public string Type;
        public uint Version;

        public TypeVersion(string type, uint version)
        {
            Type = type;
            Version = version;
        }
    }

    /// <summary>
    /// Class to manage all sheets.
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public class SheetManager : IDisposable
    {
        private readonly RyzomClient _client;

        // if you change these values please rebuild the packed_sheets with an updated sheets_packer binary.
        //   This is the only way to have correct version in both client and packed_sheets
        public static TypeVersion[] TypeVersion =
        {
            new TypeVersion("creature", 17),
            new TypeVersion("fx", 0),
            new TypeVersion("building", 2),
            new TypeVersion("sitem", 44),
            new TypeVersion("item", 44),
            new TypeVersion("plant", 5),
            new TypeVersion("death_impact", 0),
            new TypeVersion("race_stats", 3),
            new TypeVersion("light_cycle", 0),
            new TypeVersion("weather_setup", 1),
            new TypeVersion("continent", 12),
            new TypeVersion("world", 1),
            new TypeVersion("weather_function_params", 2),
            new TypeVersion("mission_icon", 0),
            new TypeVersion("sbrick", 33),
            new TypeVersion("sphrase", 4),
            new TypeVersion("skill_tree", 5),
            new TypeVersion("titles", 1),
            new TypeVersion("succes_chances_table", 1),
            new TypeVersion("automaton_list", 23),
            new TypeVersion("animset_list", 25),
            new TypeVersion("animation_fx", 4),
            new TypeVersion("id_to_string_array", 1),
            new TypeVersion("emot", 1),
            new TypeVersion("forage_source", 2),
            new TypeVersion("flora", 0),
            new TypeVersion("animation_fx_set", 3),
            new TypeVersion("attack_list", 9),
            new TypeVersion("text_emotes", 1),
            new TypeVersion("sky", 5),
            new TypeVersion("outpost", 0),
            new TypeVersion("outpost_building", 1),
            new TypeVersion("outpost_squad", 1),
            new TypeVersion("faction", 0)
        };

        /// <summary>
        /// Get all sheets (useful for other managers (skill, brick, ...))
        /// </summary>
        public SortedDictionary<SheetId, SheetManagerEntry> GetSheets()
        {
            return _entitySheetContainer;
        }

        /// <summary>
        /// Set output data path
        /// </summary>
        public void SetOutputDataPath(string dataPath)
        {
            _outputDataPath = dataPath;
        }

        /// <summary>
        /// Return output data path
        /// </summary>
        public string GetOutputDataPath()
        {
            return _outputDataPath;
        }

        //private List<List<ItemSheet>> _VisualSlots = new List<List<CtemSheet>>();

        /// <summary>
        /// directory where to create .packed_sheets
        /// </summary>
        private string _outputDataPath;

        /// <summary>
        /// this structure is fill by the loadForm() function and will contain all the sheets needed
        /// </summary>
        private readonly SortedDictionary<SheetId, SheetManagerEntry> _entitySheetContainer = new SortedDictionary<SheetId, SheetManagerEntry>();

        // Associate sheet to visual slots
        //protected SortedDictionary<ItemSheet, List<Tuple<EVisualSlot, uint>>> _SheetToVS = new SortedDictionary<ItemSheet, List<Tuple<EVisualSlot, uint>>>();

        //private SortedDictionary<string, ushort> _computeVsProcessedItem = new SortedDictionary<string, ushort>();

        /// <summary>
        /// Constructor
        /// </summary>
        public SheetManager(RyzomClient client)
        {
            _client = client;

            //// Slot 0 is invalid.
            //for (uint i = 0; i < NB_SLOT; ++i)
            //{
            //    ItemVector slotList = new ItemVector();
            //    slotList.push_back(0);
            //    _VisualSlots.push_back(slotList);
            //}
        }

        /// <summary>
        /// Destructor
        /// </summary>
        public void Dispose()
        {
            Release();
        }

        /// <summary>
        /// Release memory
        /// </summary>
        public void Release()
        {
            //_VisualSlots.Clear();

            _entitySheetContainer.Clear();

            //_SheetToVS.Clear();
        }

        /// <summary>
        /// Load all sheets
        /// </summary>
        public void Load(object callBack, bool updatePackedSheet, bool needComputeVS, bool dumpVSIndex, SheetIdFactory sheetIdFactory)
        {
            // Initialize the Sheet DB.
            LoadAllSheet(callBack, updatePackedSheet, sheetIdFactory, needComputeVS, dumpVSIndex);

            // Optimize memory taken by all strings of all sheets
            //ClientSheetsStrings.memoryCompress();

            return;
        }

        /// <summary>
        /// Load all sheets
        /// </summary>
        public void LoadAllSheet(object callBack, bool updatePackedSheet, SheetIdFactory sheetIdFactory, bool needComputeVS, bool dumpVSIndex, bool forceRecompute = false, List<string> userExtensions = null)
        {
            //callBack.progress(0);
            //callBack.pushCropedValues(0, 0.5f);

            // Get some information from typ files.
            //loadTyp();

            // prepare a list of sheets extension to load.
            var extensions = new List<string>();

            //uint sizeTypeVersion = sizeof(TypeVersion);
            //uint sizeTypeVersion = sizeof(TypeVersion);

            //uint nb = sizeTypeVersion / sizeTypeVersion;
            uint nb = 34; // todo size of type version

            {
                if (userExtensions == null)
                {
                    _entitySheetContainer.Clear();
                }

                var entitySheetContainer = new SortedDictionary<SheetId, SheetManagerEntry>();

                for (uint i = 0; i < nb; ++i)
                {
                    // see if extension is wanted
                    var found = false;

                    if (userExtensions != null)
                    {
                        foreach (var ext in userExtensions)
                        {
                            if (string.Compare(ext, TypeVersion[i].Type, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                found = true;
                            }
                        }
                    }
                    else
                    {
                        found = true;
                    }

                    if (found)
                    {
                        entitySheetContainer.Clear();
                        extensions.Clear();
                        extensions.Add(TypeVersion[i].Type);

                        //SheetManagerEntry.setVersion(TypeVersion[i].Version);

                        var path = TypeVersion[i].Type + ".packed_sheets";

                        Debug.Print(path);

                        //if (forceRecompute && !string.IsNullOrEmpty(path))
                        //{
                        //    // delete previous packed sheets
                        //    NLMISC.CFile.deleteFile(path);
                        //    path = "";
                        //}
                        //if (string.IsNullOrEmpty(path))
                        //{
                        //    path = Path.standardizePath(_OutputDataPath) + TypeVersion[i].Type + ".packed_sheets";
                        //}

                        if (path.Contains("sbrick.packed_sheets") || path.Contains("sphrase.packed_sheets") || path.Contains("forage_source.packed_sheets"))
                            LoadForm(extensions, path, ref entitySheetContainer, sheetIdFactory, updatePackedSheet);

                        foreach (var entitySheet in entitySheetContainer)
                        {
                            _entitySheetContainer[entitySheet.Key] = entitySheet.Value;
                            //entitySheet.Value.EntitySheet = null; //_sheetIdFactory.EntitySheet(0);
                        }
                    }
                }
            }

            // Re-compute Visual Slot
            if (needComputeVS)
            {
                ComputeVS();
            }

            //// Compute Visual Slots
            //for (uint i = 0; i < SLOTTYPE.NB_SLOT; ++i)
            //{
            //    _VisualSlots[i].resize(VisualSlotManager.getInstance().getNbIndex((SLOTTYPE.EVisualSlot)i) + 1, 0); // Nb Index +1 because index 0 is reserve for empty.
            //}
            //
            //EntitySheetMap.iterator it = _EntitySheetContainer.begin();
            //
            //while (it != _EntitySheetContainer.end())
            //{
            //    List<VisualSlotManager.TIdxbyVS> result = new List<VisualSlotManager.TIdxbyVS>();
            //    VisualSlotManager.getInstance().sheet2Index(it.first, result);
            //
            //    for (uint i = 0; i < result.Count; ++i)
            //    {
            //        if (it.second.EntitySheet as ItemSheet != null)
            //        {
            //            _SheetToVS[it.second.EntitySheet as ItemSheet].push_back(Tuple.Create(result[i].VisualSlot, result[i].Index));
            //            _VisualSlots[result[i].VisualSlot][result[i].Index] = it.second.EntitySheet as ItemSheet;
            //        }
            //    }
            //
            //    ++it;
            //}

            //// Dump visual slots
            //// nb : if a new visual_slot.tab has just been generated don't forget
            //// to move it in data_common before dump.
            //if (dumpVSIndex)
            //{
            //    dumpVisualSlotsIndex();
            //}

            //callBack.popCropedValues();
        }

        private void LoadForm(in List<string> sheetFilters, string packedFilename, ref SortedDictionary<SheetId, SheetManagerEntry> container, SheetIdFactory _sheetIdFactory, bool updatePackedSheet = true, bool errorIfPackedSheetNotGood = true)
        {
            List<string> dictionnary = new List<string>();
            SortedDictionary<string, uint> dictionnaryIndex = new SortedDictionary<string, uint>();
            SortedDictionary<SheetId, List<uint>> dependencies = new SortedDictionary<SheetId, List<uint>>();
            List<uint> dependencyDates = new List<uint>();

            // check the extension (i know that file like "foo.packed_sheetsbar" will be accepted but this check is enough...)
            Debug.Assert(packedFilename.IndexOf(".packed_sheets", StringComparison.Ordinal) != -1);

            string packedFilenamePath = "./data/" + packedFilename; //Path.lookup(File.getFilename(packedFilename), false, false);

            if (string.IsNullOrEmpty(packedFilenamePath))
            {
                packedFilenamePath = packedFilename;
            }

            // make sure the SheetId singleton has been properly initialised
            //_sheetIdFactory.Init(updatePackedSheet);

            // load the packed sheet if exists
            try
            {
                var ifile = new BitStreamFile();
                //ifile.SetCacheFileOnOpen(true);

                if (!ifile.Open(packedFilenamePath))
                {
                    throw new IOException($"Can't open PackedSheet '{packedFilenamePath}'.");
                }

                //// an exception will be launch if the file is not the good version or if the file is not found

                ////nlinfo ("loadForm(): Loading packed file '%s'", packedFilename.c_str());

                // read the header
                const uint packedSheetHeader = 1347113800;
                ifile.SerialCheck(packedSheetHeader);

                const uint packedSheetVersion = 5;
                ifile.SerialCheck(packedSheetVersion);

                const uint packedSheetVersionCompatible = 0;
                ifile.SerialVersion(packedSheetVersionCompatible);

                // Read depend block size
                ifile.Serial(out uint dependBlockSize);

                //// Read the dependencies only if update packed sheet
                //if (updatePackedSheet)
                //{
                //    {
                //        // read the dictionnary
                //        ifile.SerialCont(dictionnary);
                //    }
                //    {
                //        // read the dependency data
                //        uint depSize;
                //        ifile.serial(depSize);
                //        for (uint i = 0; i < depSize; ++i)
                //        {
                //            SheetId sheetId = new SheetId();
                //
                //            // Avoid copy, use []
                //            ifile.serial(sheetId);
                //            ifile.serialCont(dependencies[sheetId]);
                //        }
                //    }
                //}
                //// else dummy read one big block => no heavy reallocation / free
                //else
                if (dependBlockSize > 0)
                {
                    //byte[] bigBlock = new byte[dependBlockSize];
                    ifile.SerialBuffer(out byte[] bigBlock, dependBlockSize);
                }


                // read the packed sheet data
                ifile.Serial(out uint nbEntries);
                ifile.Serial(out uint ver);

                //if (ver != T.getVersion())
                //{
                //    throw Exception("The packed sheet version in stream is different of the code");
                //}

                ifile.SerialCont(out container, _sheetIdFactory, this);
                ifile.Close();
            }
            catch (Exception e)
            {
                // clear the container because it can contains partially loaded sheet so we must clean it before continue
                container.Clear();

                //if (!updatePackedSheet)
                //{
                if (errorIfPackedSheetNotGood)
                {
                    _client.GetLogger().Error($"loadForm(): Exception during reading the packed file and can't reconstruct them ({e.Message})");
                }
                else
                {
                    _client.GetLogger().Info($"loadForm(): Exception during reading the packed file and can't reconstruct them ({e.Message})");
                }

                return;
                //}
                //else
                //{
                //    _client.GetLogger().Info($"loadForm(): Exception during reading the packed file, I'll reconstruct it ({e.Message})");
                //}
            }

            // if we don't want to update packed sheet, we have nothing more to do
            if (!updatePackedSheet)
            {
                //nlinfo ("Don't update the packed sheet with real sheet");
                return;
            }

            //{
            //    // retreive the date of all dependency file
            //    for (uint i = 0; i < dictionnary.Count; ++i)
            //    {
            //        string p = Path.lookup(dictionnary[i], false, false);
            //        if (!string.IsNullOrEmpty(p))
            //        {
            //            uint d = File.getFileModificationDate(p);
            //            dependencyDates.Add(d);
            //        }
            //        else
            //        {
            //            // file not found !
            //            // write a future date to invalidate any file dependent on it
            //            _client.GetLogger().Debug("Can't find dependent file %s !", dictionnary[i]);
            //            dependencyDates.Add(0xffffffff);
            //        }
            //    }
            //}
            //
            //// build a vector of the sheetFilters sheet ids (ie: "item")
            //List<SheetId> sheetIds = new List<SheetId>();
            //List<string> filenames = new List<string>();
            //for (uint i = 0; i < sheetFilters.Count; i++)
            //{
            //    SheetId.buildIdVector(sheetIds, filenames, sheetFilters[i]);
            //}
            //
            //// if there's no file, nothing to do
            //if (sheetIds.Count == 0)
            //{
            //    return;
            //}
            //
            //// set up the current sheet in container to remove sheet that are in the container and not in the directory anymore
            //SortedDictionary<SheetId,
            //bool> sheetToRemove = new SortedDictionary<SheetId,
            //bool>();
            //for (typename SortedDictionary < SheetId, T > .Enumerator it = container.GetEnumerator();
            //it != container.end();
            //it++) {
            //    sheetToRemove.Add(it.Key, true);
            //}
            //
            //// check if we need to create a new .pitems or just read it
            //uint packedFiledate = File.getFileModificationDate(packedFilenamePath);
            //
            //bool containerChanged = false;
            //
            //UFormLoader formLoader = null;
            //
            //List<uint> NeededToRecompute = new List<uint>();
            //
            //for (uint k = 0; k < filenames.Count; k++)
            //{
            //    string p = Path.lookup(filenames[k], false, false);
            //    if (string.IsNullOrEmpty(p))
            //    {
            //        continue;
            //    }
            //
            //    uint d = File.getFileModificationDate(p);
            //
            //    // no need to remove this sheet
            //    sheetToRemove[sheetIds[k]] = false;
            //
            //    if (d > packedFiledate || !container.ContainsKey(sheetIds[k]))
            //    {
            //        NeededToRecompute.Add(k);
            //    }
            //    else
            //    {
            //        // check the date of each parent
            //        Debug.Assert(dependencies.ContainsKey(sheetIds[k]));
            //        List<uint> depends = dependencies[sheetIds[k]];
            //
            //        for (uint i = 0; i < depends.Count; ++i)
            //        {
            //            if (dependencyDates[depends[i]] > packedFiledate)
            //            {
            //                _client.GetLogger().Debug("Dependency on %s for %s not up to date !", dictionnary[depends[i]], sheetIds[k].ToString().c_str());
            //                NeededToRecompute.Add(k);
            //                break;
            //            }
            //        }
            //    }
            //}
            //
            //_client.GetLogger().Info("%d sheets checked, %d need to be recomputed", filenames.Count, NeededToRecompute.Count);
            //
            //TTime last = CTime.getLocalTime();
            //TTime start = CTime.getLocalTime();
            //
            //SmartPtr<UForm> form = new SmartPtr<UForm>();
            //List<SmartPtr<UForm>> cacheFormList = new List<SmartPtr<UForm>>();
            //using System;
            //using System.Collections.Generic;
            //
            //for (uint j = 0; j < NeededToRecompute.Length; j++)
            //{
            //    if (CTime.getLocalTime() > last + 5000)
            //    {
            //        last = CTime.getLocalTime();
            //        if (j > 0)
            //        {
            //            _client.GetLogger().Info("%.0f%% completed (%d/%d), %d seconds remaining", (float)j * 100.0 / NeededToRecompute.Length, j, NeededToRecompute.Length, (NeededToRecompute.Length - j) * (last - start) / j / 1000);
            //        }
            //    }
            //
            //    // create the georges loader if necessary
            //    if (formLoader == null)
            //    {
            //        WarningLog.addNegativeFilter("CFormLoader: Can't open the form file");
            //        formLoader = UFormLoader.createLoader();
            //    }
            //
            //    //	cache used to retain information (to optimize time).
            //    if (form)
            //    {
            //        cacheFormList.Add(form);
            //    }
            //
            //    // Load the form with given sheet id
            //    form = formLoader.loadForm(sheetIds[NeededToRecompute[j]].ToString().c_str());
            //    if (form)
            //    {
            //        {
            //            // build the dependency data
            //            List<uint> depends = new List<uint>();
            //            SortedSet<string> dependFiles = new SortedSet<string>();
            //            form.getDependencies(dependFiles);
            //            Debug.Assert(dependFiles.find(sheetIds[NeededToRecompute[j]].ToString()) != dependFiles.end());
            //            // remove the sheet itself from the container
            //            dependFiles.erase(sheetIds[NeededToRecompute[j]].ToString());
            //
            //            SortedSet<string>.Enumerator first = new SortedSet<string>.Enumerator(dependFiles.GetEnumerator());
            //            SortedSet<string>.Enumerator last = new SortedSet<string>.Enumerator(dependFiles.end());
            //            for (; first != last; ++first)
            //            {
            //
            //                string filename = File.getFilename(first);
            //                SortedDictionary<string,
            //                uint>.Enumerator findDicIt = dictionnaryIndex.find(filename);
            //
            //                if (findDicIt != dictionnaryIndex.end())
            //                {
            //
            //                    depends.Add(findDicIt.Value);
            //                    continue;
            //                }
            //
            //                string p = Path.lookup(first, false, false);
            //                if (!string.IsNullOrEmpty(p))
            //                {
            //                    uint dicIndex;
            //                    // add a new dictionnary entry
            //                    dicIndex = (uint)dictionnary.Length;
            //                    dictionnaryIndex.insert(Tuple.Create(filename, (uint)dictionnary.Length));
            //                    dictionnary.Add(filename);
            //
            //                    // add the dependecy index
            //                    depends.Add(dicIndex);
            //                }
            //            }
            //
            //            // store the dependency list with the sheet ID
            //            dependencies[sheetIds[NeededToRecompute[j]]] = depends;
            //        }
            //
            //        // add the new creature, it could be already loaded by the packed sheets but will be overwritten with the new one
            //        //Tuple < typename SortedDictionary<SheetId, T>.Enumerator,bool > res = container.insert(Tuple.Create(sheetIds[NeededToRecompute[j]], T()));
            //
            //        res.Item1.Value.readGeorges(form, sheetIds[NeededToRecompute[j]]);
            //        containerChanged = true;
            //    }
            //}
            //
            //if (!NeededToRecompute.Length == 0)
            //{
            //    _client.GetLogger().Info("%d seconds to recompute %d sheets", (uint)(CTime.getLocalTime() - start) / 1000, NeededToRecompute.Length);
            //}
            //
            //// free the georges loader if necessary
            //if (formLoader != null)
            //{
            //    UFormLoader.releaseLoader(formLoader);
            //    WarningLog.removeFilter("CFormLoader: Can't open the form file");
            //}
            //
            //// we have now to remove sheets that are in the container and not exist anymore in the sheet directories
            //for (SortedDictionary<SheetId, bool>.Enumerator it2 = sheetToRemove.begin(); it2.MoveNext();)
            //{
            //    if (it2.Current.Value)
            //    {
            //        _client.GetLogger().Info("the sheet '%s' is not in the directory, remove it from container", it2.Current.Key.ToString().c_str());
            //        container.find(it2.Current.Key).Value.removed();
            //        container.erase(it2.Current.Key);
            //        containerChanged = true;
            //        dependencies.erase(it2.Current.Key);
            //    }
            //}
            //
            //// now, save the new container in the packedfile
            //try
            //{
            //    if (containerChanged)
            //    {
            //        COFile ofile = new COFile();
            //        ofile.open(packedFilenamePath);
            //
            //        // write the header.
            //        ofile.serialCheck(PACKED_SHEET_HEADER);
            //        ofile.serialCheck(PACKED_SHEET_VERSION);
            //        ofile.serialVersion(PACKED_SHEET_VERSION_COMPATIBLE);
            //
            //        // Write a dummy block size for now
            //        SInt32 posBlockSize = ofile.getPos();
            //        uint dependBlockSize = 0;
            //        ofile.serial(dependBlockSize);
            //
            //        // write the dictionnary
            //        ofile.serialCont(dictionnary);
            //
            //        // write the dependencies data
            //        uint depSize = (uint)dependencies.Length;
            //        ofile.serial(depSize);
            //        SortedDictionary<SheetId,
            //        List<uint>>.Enumerator first = new SortedDictionary<SheetId,
            //        List<uint>>.Enumerator(dependencies.begin());
            //        SortedDictionary<SheetId,
            //        List<uint>>.Enumerator last = new SortedDictionary<SheetId,
            //        List<uint>>.Enumerator(dependencies.end());
            //        for (; first != last; ++first)
            //        {
            //
            //            SheetId si = first.Key;
            //            ofile.serial(si);
            //
            //            ofile.serialCont(first.Value);
            //        }
            //
            //        // Then get the dictionary + dependencies size, and write it back to posBlockSize
            //        SInt32 endBlockSize = ofile.getPos();
            //        dependBlockSize = (endBlockSize - posBlockSize) - 4;
            //        ofile.seek(posBlockSize, IStream.begin);
            //        ofile.serial(dependBlockSize);
            //        ofile.seek(endBlockSize, IStream.begin);
            //
            //        // write the sheet data
            //        uint nbEntries = (uint)sheetIds.Length;
            //        uint ver = T.getVersion();
            //        ofile.serial(nbEntries);
            //        ofile.serial(ver);
            //        ofile.serialCont(container);
            //        ofile.close();
            //    }
            //}
            //catch (Exception e)
            //{
            //    _client.GetLogger().Info("loadForm(): Exception during saving the packed file, it will be recreated next launch (%s)", e.what());
            //}
            //
            //// housekeeping
            //sheetIds.Clear();
            //filenames.Clear();
        }


        /// <summary>
        /// compute Visual Slots for this sheet.
        /// </summary>
        public void LoadAllSheetNoPackedSheet(object callBack, List<string> extensions, string wildcardFilter)
        {
            //callBack.progress(0);
            //callBack.pushCropedValues(0, 0.5f);

            //  load all forms
            //global::loadFormNoPackedSheet(extensions, _EntitySheetContainer, wildcardFilter);

            //callBack.popCropedValues();
        }

        /// <summary>
        /// Compute Visual Slots for this sheet
        /// </summary>
        public void ComputeVS()
        {
            ////	static ClassicMap< string, ushort > ProcessedItem;
            //SortedDictionary<string, ushort>.Enumerator it;
            //
            //VisualSlotManager.TVisualSlot vs = new VisualSlotManager.TVisualSlot();
            //vs.resize(NB_SLOT);
            //
            ////
            //EntitySheetMap.iterator itS = _EntitySheetContainer.Begin();
            //
            //while (itS != _EntitySheetContainer.end())
            //{
            //    // Visual Slots are only valid for Items.
            //    ItemSheet item = itS.second.EntitySheet as ItemSheet;
            //
            //    if (item != null && itS.first.getSheetType() == SheetId.TypeFromFileExtension("sitem"))
            //    {
            //        for (uint j = 0; j < NB_SLOT_TYPE; ++j)
            //        {
            //            SlotType slotType = (SlotType)j;
            //
            //            if (item.hasSlot(slotType))
            //            {
            //                VisualSlot visualSlot = convertTypeToVisualSlot(slotType);
            //
            //                if (visualSlot != HIDDEN_SLOT)
            //                {
            //                    string currentSheet = (itS.first).toString();
            //
            //                    VisualSlotManager.TElement vsElmt = new VisualSlotManager.TElement();
            //
            //                    string sheetName = toString("%s%d", currentSheet, visualSlot);
            //
            //                    it.CopyFrom(computeVS_ProcessedItem.find(sheetName));
            //
            //                    // Insert if not found
            //                    if (it == computeVS_ProcessedItem.end())
            //                    {
            //                        uint itemNumber = new uint();
            //                        if (vs[visualSlot].Element.empty())
            //                        {
            //                            itemNumber = 1;
            //                        }
            //                        else
            //                        {
            //                            itemNumber = vs[visualSlot].Element[vs[visualSlot].Element.size() - 1].Index + 1;
            //                        }
            //
            //                        // Item Processed
            //                        computeVS_ProcessedItem.Add(sheetName, itemNumber);
            //
            //                        vsElmt.Index = itemNumber;
            //                    }
            //                    else
            //                    {
            //                        vsElmt.Index = it.second;
            //                    }
            //
            //                    vsElmt.SheetId = itS.first;
            //                    vs[visualSlot].Element.push_back(vsElmt);
            //                }
            //            }
            //        }
            //    }
            //
            //    // Next Sheet
            //    ++itS;
            //}
            //
            //// Open the file.
            //NLMISC.COFile f = new NLMISC.COFile();

            //if (f.open("visual_slot.tab"))
            //{
            //    // Dump entities.
            //    f.serialCont(vs);
            //
            //    // Close the File.
            //    f.close();
            //}
            //else
            //{
            //    _client.GetLogger().Warn("SheetMngr:load: cannot open/create the file 'visual_slot.tab'.");
            //}
        }

        /// <summary>
        /// Processing the sheet
        /// </summary>
        /// <param name="sheet">sheet to process</param>
        public void ProcessSheet(EntitySheet sheet)
        {
            // For now: no op
        }

        ///// <summary>
        ///// Get a pair of visual slots / index from a CItemSheet pointer.
        ///// </summary>
        ///// <param name="sheet"></param>
        ///// <returns></returns>
        //public VisualSlotItemArray getVSItems(ItemSheet sheet)
        //{
        //    //    ItemSheet2SlotItemArray.const_iterator it = _SheetToVS.find(sheet);
        //    //
        //    //    if (it == _SheetToVS.end())
        //    //    {
        //    //        return null;
        //    //    }
        //    //    return &(it.second);
        //}

        /// <summary>
        /// From an item name and a slot, get its item, or -1 if not found
        /// </summary>
        public int GetVSIndex(string itemName, VisualSlot slot)
        {
            //    SheetId si = new SheetId();
            //
            //    if (!si.buildSheetId(itemName))
            //    {
            //        _client.GetLogger().Warn("<CSheetManager::getVSIndex> : cannot build id from item %s for the slot %d.", itemName, slot);
            //        return -1;
            //    }
            //
            //    EntitySheetMap.iterator it = _EntitySheetContainer.find(si);
            //
            //    if (it == _EntitySheetContainer.end())
            //    {
            //        _client.GetLogger().Warn("<CSheetManager::getVSIndex> : cannot find %s for the slot %d.", itemName, slot);
            //        return -1;
            //    }
            //    if (it.second.EntitySheet == 0 || it.second.EntitySheet.type() != EntitySheet.ITEM)
            //    {
            //        _client.GetLogger().Warn("<CSheetManager::getVSIndex> : %s is not an item for the slot %d.", itemName, slot);
            //        return -1;
            //    }
            //
            //    ItemSheet @is = (ItemSheet)it.second.EntitySheet;
            //
            //    VisualSlotItemArray ia = getVSItems(@is);
            //    if (ia == null)
            //    {
            //        _client.GetLogger().Warn("<CSheetManager::getVSIndex> : no items for the slot %d. while looking for %s", slot, itemName);
            //        return -1;
            //    }
            //
            //    VisualSlotItemArray.const_iterator first = new VisualSlotItemArray.const_iterator(ia.begin());
            //    VisualSlotItemArray.const_iterator last = new VisualSlotItemArray.const_iterator(ia.end());
            //
            //    for (; first != last; ++first)
            //    {
            //        if (first.first == slot)
            //        {
            //            return first.second;
            //        }
            //    }
            //
            //    _client.GetLogger().Warn("<CSheetManager::getVSIndex> : cannot find %s for the slot %d.", itemName, slot);
            return -1;
        }

        /// <summary>
        /// Get a sheet from its number
        /// </summary>
        /// <param name="num">sheet number</param>
        /// <returns>pointer on the sheet according to the param or 0 if any pb</returns>
        public EntitySheet Get(SheetId num)
        {
            return _entitySheetContainer.ContainsKey(num) ? _entitySheetContainer[num].EntitySheet : null;
        }

        /// <summary>
        /// Get the number of available items for the given visual slot
        /// </summary>
        public uint GetNumItem(VisualSlot slot)
        {
            //    // The slot is not a visible one.
            //    if (slot == SLOTTYPE.HIDDEN_SLOT)
            //    {
            //        return 0;
            //    }
            //    // Convert into an uint to remove warnings.
            //    uint s = (uint)slot;
            // 
            //    // Check slot.
            //    if (s < _VisualSlots.size())
            //    {
            //        return (uint)_VisualSlots[s].size();
            //    }
            //    else
            //    {
            //        _client.GetLogger().Warn("CSheetManager::getNumItem : invalid slot %d.", slot);
            return 0;
            //    }
        }

        //-----------------------------------------------
        // getItem :
        // Get the real.
        //-----------------------------------------------
        //public ItemSheet getItem(VisualSlot slot, uint index)
        //{
        //    // The slot is not a visible one.
        //    if (slot == SLOTTYPE.HIDDEN_SLOT)
        //    {
        //        return 0;
        //    }
        //
        //    // Convert into an uint to remove warnings.
        //    uint s = (uint)slot;
        //
        //    // Check slot.
        //    if (s < _VisualSlots.size())
        //    {
        //        // Check index.
        //        if (index < _VisualSlots[s].size())
        //        {
        //            // Not the default Item.
        //            if (index != 0)
        //            {
        //                return _VisualSlots[s][index];
        //            }
        //            // Default Item.
        //            else
        //            {
        //                return null;
        //            }
        //        }
        //        // Bad index.
        //        else
        //        {
        //            //_client.GetLogger().Warn("CSheetManager::getItem : invalid index %d for the slot %d.", index, slot);
        //            return null;
        //        }
        //    }
        //    // Bad slot.
        //    else
        //    {
        //        _client.GetLogger().Warn("CSheetManager::getItem : invalid slot %d.", slot);
        //        return null;
        //    }
        //}


        /// <summary>
        /// Get Some information from 'typ' files.
        /// </summary>
        public void LoadTyp()
        {
            //    // Read the Eyes Color 'typ'
            //    NLMISC.CSmartPtr<NLGEORGES.UType> smartPtr = FormLoader.loadFormType("_creature_3d_eyes_color.typ");
            //    if (smartPtr != null)
            //    {
            //        string maxStr = smartPtr.getMax();
            //        fromString(maxStr, _NbEyesColor);
            //
            //        if (_NbEyesColor <= 0)
            //        {
            //            _client.GetLogger().Warn("CSheetManager::loadTyp: There no is Color available for the eyes.");
            //        }
            //    }
            //    else
            //    {
            //        _client.GetLogger().Warn("CSheetManager::loadTyp: Cannot load the '_creature_3d_eyes_color.typ' file.");
            //    }
            //
            //    // Read the Hair Color 'typ'
            //    smartPtr = FormLoader.loadFormType("_creature_3d_hair_color.typ");
            //    if (smartPtr != null)
            //    {
            //        string maxStr = smartPtr.getMax();
            //        fromString(maxStr, _NbHairColor);
            //        if (_NbHairColor <= 0)
            //        {
            //            _client.GetLogger().Warn("CSheetManager::loadTyp: There is no Color available for the hair.");
            //        }
            //    }
            //    else
            //    {
            //        _client.GetLogger().Warn("CSheetManager::loadTyp: Cannot load the '_creature_3d_hair_color.typ' file.");
            //    }
        } // initTyp //

        /// <summary>
        /// Dump the visual slots
        /// </summary>
        public void DumpVisualSlots()
        {
            //    for (uint k = 0; k < _VisualSlots.size(); ++k)
            //    {
            //        ItemVector iv = _VisualSlots[k];
            //        for (uint l = 0; l < iv.size(); ++l)
            //        {
            //            if (iv[l])
            //            {
            //                nlinfo("Slot %d, item %d = %s", (int)k, (int)l, iv[l].Id.toString().c_str());
            //            }
            //        }
            //    }
        }

        /// <summary>
        /// Dump all visual slots indexes in a file
        /// </summary>
        public void DumpVisualSlotsIndex()
        {
            //    FILE vsIndexFile = nlfopen(getLogDirectory() + "vs_index.txt", "w");
            //    if (vsIndexFile != null)
            //    {
            //        for (uint i = 0; i < SLOTTYPE.NB_SLOT; ++i)
            //        {
            //            fprintf(vsIndexFile, "VISUAL SLOT : %d\n", i);
            //            ItemVector rVTmp = _VisualSlots[i];
            //            for (uint j = 0; j < rVTmp.size(); ++j)
            //            {
            //                ItemSheet pIS = rVTmp[j];
            //                if (pIS != null)
            //                {
            //                    fprintf(vsIndexFile, "%d : %s\n", j, pIS.Id.toString().c_str());
            //                }
            //                //nlSleep(100);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        _client.GetLogger().Warn("<CSheetManager::loadAllSheet> Can't open file to dump VS index");
            //    }
            //
            //    fclose(vsIndexFile);
        }
    }
}
