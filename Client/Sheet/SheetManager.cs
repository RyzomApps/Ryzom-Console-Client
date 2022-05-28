///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace Client.Sheet
{
    public class SheetManager : IDisposable
    {
        private readonly RyzomClient _client;

        protected int _NbEyesColor;
        protected int _NbHairColor;

        /// Return the number of color available for the eyes.
        public int nbEyesColor()
        {
            return _NbEyesColor;
        }

        /// Return the number of color available for the hair.
        public int nbHairColor()
        {
            return _NbHairColor;
        }


        /// Get all sheets (useful for other managers (skill, brick, ...))
        public SortedDictionary<SheetId, SheetManagerEntry> getSheets()
        {
            return _EntitySheetContainer;
        }

        /// Set output data path
        public void setOutputDataPath(string dataPath)
        {
            _OutputDataPath = dataPath;
        }

        /// Return output data path
        public string getOutputDataPath()
        {
            return _OutputDataPath;
        }

        //private List<List<ItemSheet>> _VisualSlots = new List<List<CtemSheet>>();

        // directory where to create .packed_sheets
        private string _OutputDataPath;

        /// this structure is fill by the loadForm() function and will contain all the sheets needed
        protected SortedDictionary<SheetId, SheetManagerEntry> _EntitySheetContainer = new SortedDictionary<SheetId, SheetManagerEntry>();

        // Associate sheet to visual slots
        //protected SortedDictionary<ItemSheet, List<Tuple<EVisualSlot, uint>>> _SheetToVS = new SortedDictionary<ItemSheet, List<Tuple<EVisualSlot, uint>>>();

        private SortedDictionary<string, ushort> computeVS_ProcessedItem = new SortedDictionary<string, ushort>();

        //-----------------------------------------------
        // CSheetManager :
        // Constructor.
        //-----------------------------------------------
        public SheetManager(RyzomClient client)
        {
            _client = client;

            _NbEyesColor = 0; // Default is no color available for the eyes.

            //// Slot 0 is invalid.
            //for (uint i = 0; i < NB_SLOT; ++i)
            //{
            //    ItemVector slotList = new ItemVector();
            //    slotList.push_back(0);
            //    _VisualSlots.push_back(slotList);
            //}
        }

        //-----------------------------------------------
        // CSheetManager :
        // Destructor.
        //-----------------------------------------------
        public void Dispose()
        {
            release();
        }

        public void release()
        {
            //_VisualSlots.Clear();

            _EntitySheetContainer.Clear();

            //_SheetToVS.Clear();
        }

        /// <summary>
        /// Load all sheets.
        /// </summary>
        public void Load(object callBack, bool updatePackedSheet, bool needComputeVS, bool dumpVSIndex)
        {
            // Initialize the Sheet DB.
            LoadAllSheet(callBack, updatePackedSheet, needComputeVS, dumpVSIndex);

            // Optimize memory taken by all strings of all sheets
            //ClientSheetsStrings.memoryCompress();

            return;
        }

        /// <summary>
        /// Load all sheets.
        /// </summary>
        public void LoadAllSheet(object callBack, bool updatePackedSheet, bool needComputeVS, bool dumpVSIndex, bool forceRecompute = false, List<string> userExtensions = null)
        {
            //callBack.progress(0);
            //callBack.pushCropedValues(0, 0.5f);

            // Get some information from typ files.
            //loadTyp();

            // prepare a list of sheets extension to load.
            List<string> extensions = new List<string>();

            //uint sizeTypeVersion = sizeof(TypeVersion);
            //uint sizeCTypeVersion = sizeof(CTypeVersion);
            //
            //uint nb = sizeTypeVersion / sizeCTypeVersion;
            //{
            //    if (userExtensions == null)
            //    {
            //        _EntitySheetContainer.Clear();
            //    }
            //
            //    EntitySheetMap entitySheetContainer = new EntitySheetMap();
            //
            //    for (uint i = 0; i < nb; ++i)
            //    {
            //        // see if extension is wanted
            //        bool found = false;
            //        if (userExtensions != null)
            //        {
            //            for (int l = 0; l < userExtensions.Count; ++l)
            //            {
            //                if (string.Compare(userExtensions[l], TypeVersion[i].Type.c_str(), true) == 0)
            //                {
            //                    found = true;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            found = true;
            //        }
            //        if (found)
            //        {
            //            entitySheetContainer.clear();
            //            extensions.Clear();
            //            extensions.Add(TypeVersion[i].Type);
            //            SheetManagerEntry.setVersion(TypeVersion[i].Version);
            //            string path = Path.lookup(TypeVersion[i].Type + ".packed_sheets", false);
            //
            //            if (forceRecompute && !string.IsNullOrEmpty(path))
            //            {
            //                // delete previous packed sheets
            //                NLMISC.CFile.deleteFile(path);
            //                path = "";
            //            }
            //            if (string.IsNullOrEmpty(path))
            //            {
            //                path = Path.standardizePath(_OutputDataPath) + TypeVersion[i].Type + ".packed_sheets";
            //            }
            //            global::loadForm(extensions, path, entitySheetContainer, updatePackedSheet);
            //
            //            EntitySheetMap.iterator it = entitySheetContainer.begin();
            //            while (it != entitySheetContainer.end())
            //            {
            //                _EntitySheetContainer[it.first] = it.second;
            //                it.second.EntitySheet = 0;
            //                // Next
            //                ++it;
            //            }
            //        }
            //    }
            //}

            // Re-compute Visual Slot
            if (needComputeVS)
            {
                computeVS();
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

            //
            //callBack.popCropedValues();
        }

        //-----------------------------------------------
        // computeVS :
        // compute Visual Slots for this sheet.
        //-----------------------------------------------
        public void LoadAllSheetNoPackedSheet(object callBack, List<string> extensions, string wildcardFilter)
        {
            //callBack.progress(0);
            //callBack.pushCropedValues(0, 0.5f);

            //  load all forms
            //global::loadFormNoPackedSheet(extensions, _EntitySheetContainer, wildcardFilter);

            //
            //callBack.popCropedValues();
        }

        public void computeVS()
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

        ////-----------------------------------------------
        //// processSheet :
        //// Porcessing the sheet.
        //// \param sheet : sheet to process.
        //public void processSheet(EntitySheet sheet)
        //{
        //    // For now: no op
        //}
        //
        //public VisualSlotItemArray getVSItems(ItemSheet sheet)
        //{
        //    ItemSheet2SlotItemArray.const_iterator it = _SheetToVS.find(sheet);
        //
        //    if (it == _SheetToVS.end())
        //    {
        //        return null;
        //    }
        //    return &(it.second);
        //}
        //
        //public int getVSIndex(string itemName, VisualSlot slot)
        //{
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
        //    return -1;
        //}

        ////-----------------------------------------------
        //// get :
        //// Get a sheet from its number.
        //// \param uint32 num : sheet number.
        //// \return EntitySheet * : pointer on the sheet according to the param or 0 if any pb.
        //public EntitySheet get(SheetId num)
        //{
        //    EntitySheetMap.iterator it = _EntitySheetContainer.find(num);
        //    if (it != _EntitySheetContainer.end())
        //    {
        //        return it.second.EntitySheet;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //} // get //
        //
        //public uint getNumItem(SLOTTYPE.EVisualSlot slot)
        //{
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
        //        return 0;
        //    }
        //}
        //
        ////-----------------------------------------------
        //// getItem :
        //// Get the real.
        ////-----------------------------------------------
        //public ItemSheet getItem(SLOTTYPE.EVisualSlot slot, uint index)
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
        //} // getItem //
        //
        //
        ////-----------------------------------------------
        //// loadTyp :
        //// Get Some information from 'typ' files.
        ////-----------------------------------------------
        //public void loadTyp()
        //{
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
        //} // initTyp //
        //
        //public void dumpVisualSlots()
        //{
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
        //}
        //
        //public void dumpVisualSlotsIndex()
        //{
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
        //}
    }
}
