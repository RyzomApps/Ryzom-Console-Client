///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics.CodeAnalysis;
using API.Sheet;
using Client.Stream;

namespace Client.Sheet
{
    /// <summary>
    /// SheetId
    /// </summary>
    /// <author>Stephane Coutelas</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    /// <remarks>
    /// This class is case insensitive. It means that you can call build() and 
    /// buildIdVector() with string with anycase, it'll work.
    /// </remarks>
    public class SheetId : IComparable<SheetId>, ISheetId
    {
        private readonly SheetIdFactory _sheetIdFactory;

        private readonly IdInfos _id = new IdInfos();

        public string Name => ToString();

        /// <summary>
        /// Constructor
        /// </summary>
        public SheetId(SheetIdFactory sheetIdFactory)
        {
            _sheetIdFactory = sheetIdFactory;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SheetId(SheetIdFactory sheetIdFactory, uint id)
        {
            _sheetIdFactory = sheetIdFactory;
            _id.Id = id;
        }

        /// <summary>
        /// Return the sheet type (sub part of the sheetid)
        /// </summary>
        public uint GetSheetType()
        {
            return _id.Type;
        }

        /// <summary>
        /// Return the **whole** sheet id (id+type)
        /// </summary>
        public uint AsInt()
        {
            return _id.Id;
        }

        /// <summary>
        /// Return the sheet sub id (sub part of the sheetid)
        /// </summary>
        public uint GetShortId()
        {
            return _id.ShortId;
        }

        /// <summary>
        /// Serial
        /// </summary>
        public void Serial(BitMemoryStream f)
        {
            uint idId = 0;

            if (!f.IsReading())
                idId = _id.Id;

            f.Serial(ref idId);

            if (f.IsReading())
                _id.Id = idId;
        }

        /// <summary>
        /// Serial
        /// </summary>
        internal void Serial(BitStreamFile s)
        {
            s.Serial(out uint idId);
            _id.Id = idId;
        }

        public int CompareTo([AllowNull] SheetId other)
        {
            return other != null ? AsInt().CompareTo(other.AsInt()) : 0;
        }

        public void BuildSheetId(int shortId, SheetType type)
        {
            // TODO: BuildSheetId implementation!
            _id.ShortId = (uint)shortId;
            _id.Type = (uint)type;

            //_sheetIdFactory.SheetId(shortId);
        }

        /// <summary>
        /// Return the sheet id as a string
        /// If the sheet id is not found, then:
        /// - if 'ifNotFoundUseNumericId==false' the returned string is "<Sheet %d not found in sheet_id.bin>" with the id in %d
        /// - if 'ifNotFoundUseNumericId==tue'   the returned string is "#%u" with the id in %u
        /// </summary>
        public override string ToString()
        {
            return _sheetIdFactory.ToString(this, false);
        }

        /// <inheritdoc cref="ToString()"/>
        public string ToString(bool ifNotFoundUseNumericId)
        {
            return _sheetIdFactory.ToString(this, ifNotFoundUseNumericId);
        }
    }
}