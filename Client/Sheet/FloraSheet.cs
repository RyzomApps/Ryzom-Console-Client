///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Client.Stream;

namespace Client.Sheet
{
    /// <summary>
    /// Info about flora, read from a .flora sheet
    /// </summary>
    public class FloraSheet : EntitySheet
    {
        private readonly SheetIdFactory _sheetIdFactory;
        private readonly List<CPlantInfo> _Plants = new List<CPlantInfo>();
        private ulong _TotalWeight;

        public float MicroLifeThreshold; // 0 -> every tile has micro-life > 1 -> no tile has micro-life

        /// <summary>
        /// ctor
        /// </summary>
        public FloraSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory)
        {
            _sheetIdFactory = sheetIdFactory;
        }

        ///// <summary>Build the sheet from an external script.</summary>
        //virtual void build(in NLGEORGES::UFormElm item);

        /// <summary>
        /// Get total weight of plant infos
        /// </summary> 
        public ulong getPlantInfoTotalWeight()
        {
            return _TotalWeight;
        }

        ////** Get plant info from weighted index
        //  *
        //  * e.g : we have 3 .plant in the .flora:
        //  * .plant a.plant, weighted 4
        //  * .plant b.plant, weighted 2
        //  * .plant c.plant, weighted 1
        //  *
        //  * getPlantInfoWeight(0) -> a.plant
        //  * getPlantInfoWeight(1) -> a.plant
        //  * getPlantInfoWeight(2) -> a.plant
        //  * getPlantInfoWeight(3) -> a.plant (4 occurences)
        //  * getPlantInfoWeight(4) -> b.plant
        //  * getPlantInfoWeight(5) -> b.plant (2 occurences)
        //  * getPlantInfoWeight(6) -> c.plant (1 occurences)
        //  *
        //  */
        //const CPlantInfo *getPlantInfoFromWeightedIndex(uint64 index) const;

        /// <summary>
        /// Plant info access
        /// </summary>
        public uint GetNumPlantInfos()
        {
            return (uint)_Plants.Count;
        }

        public CPlantInfo GetPlantInfo(uint index)
        {
            return _Plants[(int)index];
        }

        public override void Serial(BitMemoryStream f)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serialize plant sheet into binary data file.
        /// </summary>
        public override void Serial(BitStreamFile s)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Infos about a .plant contained in a .flora
        /// </summary>
        public class CPlantInfo
        {
            /// <summary>
            /// for sorting by weights
            /// </summary>
            public ulong CumulatedWeight;
            public string SheetName = "";
            public uint Weight;

            //virtual void build(in NLGEORGES::UFormElm item);

            public static bool operator <(in CPlantInfo lhs, in CPlantInfo rhs)
            {
                return lhs.CumulatedWeight < rhs.CumulatedWeight;
            }

            public static bool operator >(in CPlantInfo lhs, in CPlantInfo rhs)
            {
                return lhs.CumulatedWeight > rhs.CumulatedWeight;
            }
        }
    }
}